using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


public class Surface : MonoBehaviour
{

    public List<SurfacePoint> surfacePoints = new List<SurfacePoint>();

    public List<MeshTriangle> headMesh = new List<MeshTriangle>();
    public List<MeshTriangle> bodyMesh = new List<MeshTriangle>();

    public SkeletonMap skeletonMap;

    private bool CheckFile(string filePath)
    {
        if (!(File.Exists(filePath)))
        {
            Debug.Log("Surface file not found");
            return false;
        }
        return true;
    }

    private bool CheckSkeletonMap()
    {
        this.skeletonMap = GetComponent<SkeletonMap>();
        if (this.skeletonMap == null)
        {
            Debug.Log("Could not find a skeleton map for " + this.gameObject.name);
            return false;
        }
        return true;
    }

    private List<string[]> ReadFile(string filePath)
    {
        // Read file
        List<string[]> linedContent = new List<string[]>();
        using (StreamReader streamReader = new StreamReader(filePath))
        {
            //readContents = streamReader.ReadToEnd();
            string fileline;
            while ((fileline = streamReader.ReadLine()) != null)
            {
                linedContent.Add(fileline.Split(","));
            }

        }
        return linedContent;
    }

    public void ReadSourceSurfaceFile(string filePath)
    {
        if (!CheckFile(filePath))
            return;
        if (!CheckSkeletonMap())
            return;
        CreateSurfacePoints();
        List<string[]> linedContent = ReadFile(filePath);

        for (int i = 0; i < linedContent.Count; i++)
        {
            if (this.surfacePoints[i].pointType == "mesh")
            {
                for (int j = 0; j < 4; j++)
                {
                    this.surfacePoints[i].matrix.SetRow(j, new Vector4(System.Convert.ToSingle(linedContent[i][j * 4], System.Globalization.CultureInfo.InvariantCulture),
                                                                       System.Convert.ToSingle(linedContent[i][j * 4 + 1], System.Globalization.CultureInfo.InvariantCulture),
                                                                       System.Convert.ToSingle(linedContent[i][j * 4 + 2], System.Globalization.CultureInfo.InvariantCulture),
                                                                       System.Convert.ToSingle(linedContent[i][j * 4 + 3], System.Globalization.CultureInfo.InvariantCulture)));
                }
                if (this.surfacePoints[i].matrix.ValidTRS() == false)
                {
                    Debug.Log("Something went wrong reading the transformation matrix of surface point " + this.surfacePoints[i].name);
                }
            }
            else
            {
                this.surfacePoints[i].radius = System.Convert.ToSingle(linedContent[i][0], System.Globalization.CultureInfo.InvariantCulture);
            } 
        }

        CreateSurfaceMeshes();
    }

    public void rescale(int factor)
    {
        for (int i = 0; i < this.surfacePoints.Count; i++)
        {
            if (this.surfacePoints[i].pointType == "mesh")
            {
                this.surfacePoints[i].matrix[0, 3] /= factor;
                this.surfacePoints[i].matrix[1, 3] /= factor;
                this.surfacePoints[i].matrix[2, 3] /= factor;
            }
            else
                this.surfacePoints[i].radius /= factor;
        }
    }

    public void ReadTargetSurfaceFile(string filePath)
    {
        if (!CheckFile(filePath))
            return;
        if (!CheckSkeletonMap())
            return;
        CreateSurfacePoints();

        List<string[]> linedContent = ReadFile(filePath);

        Vector3 reference = new Vector3(System.Convert.ToSingle(linedContent[linedContent.Count - 1][0], System.Globalization.CultureInfo.InvariantCulture),
                                                                System.Convert.ToSingle(linedContent[linedContent.Count - 1][1], System.Globalization.CultureInfo.InvariantCulture),
                                                                System.Convert.ToSingle(linedContent[linedContent.Count - 1][2], System.Globalization.CultureInfo.InvariantCulture));

        for (int i = 0; i < linedContent.Count-1; i++) // "-1" because the last line of the target surface data file was used in the (original) python version of this code
        {
            if (this.surfacePoints[i].pointType == "mesh")
            {
                this.surfacePoints[i].lposition = new Vector3(System.Convert.ToSingle(linedContent[i][0], System.Globalization.CultureInfo.InvariantCulture),
                                                                System.Convert.ToSingle(linedContent[i][1], System.Globalization.CultureInfo.InvariantCulture),
                                                                System.Convert.ToSingle(linedContent[i][2], System.Globalization.CultureInfo.InvariantCulture));
                this.surfacePoints[i].lposition -= reference;
            }
            else
                this.surfacePoints[i].radius = System.Convert.ToSingle(linedContent[i][0], System.Globalization.CultureInfo.InvariantCulture);
        }
        
        

        CreateSurfaceMeshes();
    }

    public void DrawSurface(float scale)
    {
        Joint joint;
        Joint childJoint;
        Joint root;
        GameObject pointObj;

        foreach (SurfacePoint point in this.surfacePoints)
        {
            joint = this.skeletonMap.GetJoint(point.attachedJointName);
            root = this.skeletonMap.GetJoint("Hips");
            if (point.pointType == "mesh")
            {
                pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pointObj.name = point.name;
                pointObj.transform.SetParent(joint.Object.transform);
                
                if (point.lposition.magnitude == 0f) // for source surface points
                {
                    pointObj.transform.localPosition = point.matrix.GetPosition();
                    pointObj.transform.localRotation = point.matrix.rotation;
                }
                else // for target surface points
                    pointObj.transform.localPosition = point.lposition + root.Object.transform.position - joint.Object.transform.position;
                point.Object = pointObj;
                pointObj.transform.localScale = new Vector3(scale, scale, scale);
            }
            else if (point.pointType == "limb")
            {
                // Grab the other joint that compose the limb. E.g., joint is RightUpLeg, so it gets the second joint from the RightUpLeg *bone* from skeleton map
                childJoint = this.skeletonMap.bones[point.attachedJointName][1];
                pointObj = SurfaceUtils.DrawPrimitive(PrimitiveType.Capsule, point.name, joint.Object, childJoint.Object, point.radius);
                point.Object = pointObj;

            }
        }

        DrawMeshes();

    }
 

    private void DrawMeshes()
    {
        int count = 0;
        foreach (MeshTriangle mt in headMesh)
        {
            mt.Draw(count);
            count++;
        }
        foreach (MeshTriangle mt in bodyMesh)
        {
            mt.Draw(count);
            count++;
        }
    }



    public void UpdateMeshes()
    {
        foreach (MeshTriangle mt in headMesh)
            mt.Update();
        foreach (MeshTriangle mt in bodyMesh)
            mt.Update();
    }



    private void CreateSurfacePoints()
    {
        List<string> names = new List<string>               { "chestRight"  ,"chestLeft", "abdomenRight","abdomenLeft"  ,"hipRight" ,"hipLeft"  ,"thightRight"  ,"thightLeft"   ,"shinRight","shinLeft" ,"abdomenUp","armRight" ,"foreRight","armLeft","foreLeft","headRight","headLeft","earRight","earLeft","chinRight","chinLeft","cheekRight","cheekLeft","mouth","foreHead","backHeadRight","backHeadLeft","backHead","loinRight","loinLeft","loinUp"};
        List<string> pointTypes = new List<string>          { "mesh"        , "mesh"    , "mesh"        , "mesh"        , "mesh"    , "mesh"    , "limb"        , "limb"        , "limb"    , "limb"    , "mesh"    , "limb"    , "limb", "limb", "limb", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh", "mesh" };
        List<string> attachedJointNames = new List<string>  { "Spine3"      , "Spine3"  , "Spine"       , "Spine"       , "Hips"    , "Hips"    , "RightUpLeg"  , "LeftUpLeg"   , "RightLeg", "LeftLeg" , "Spine2"  , "RightArm", "RightForeArm", "LeftArm", "LeftForeArm", "Head", "Head", "Head", "Head", "Head", "Head", "Head", "Head", "Head", "Head", "Head", "Head", "Head", "Hips", "Hips", "Spine1", };
        for (int i = 0; i < names.Count; i++)
        {
            this.surfacePoints.Add( new SurfacePoint(names[i], pointTypes[i], attachedJointNames[i]) );
        }

    }

    private SurfacePoint GetSurfPoint(string name)
    {
        foreach (SurfacePoint sp in this.surfacePoints)
        {
            if (sp.name == name)
                return sp;
        }
        Debug.Log("Surface point " + name + " not found.");
        return null;
    }

    private MeshTriangle CreateMesh(string point1, string point2, string point3)
    {
        SurfacePoint p1 = GetSurfPoint(point1);
        SurfacePoint p2 = GetSurfPoint(point2);
        SurfacePoint p3 = GetSurfPoint(point3);
        return new MeshTriangle(p1, p2, p3);
    }

    private void CreateSurfaceMeshes()
    {

        this.headMesh.Add(CreateMesh("headRight", "foreHead", "headLeft"));
        this.headMesh.Add(CreateMesh("headRight", "earRight", "cheekRight"));
        this.headMesh.Add(CreateMesh("earRight", "chinRight", "cheekRight"));
        this.headMesh.Add(CreateMesh("headLeft", "cheekLeft", "earLeft"));
        this.headMesh.Add(CreateMesh("earLeft", "cheekLeft", "chinLeft"));
        this.headMesh.Add(CreateMesh("headRight", "cheekRight", "foreHead"));
        this.headMesh.Add(CreateMesh("headLeft", "foreHead", "cheekLeft"));
        this.headMesh.Add(CreateMesh("chinRight", "chinLeft", "mouth"));
        this.headMesh.Add(CreateMesh("chinRight", "chinLeft", "foreHead"));
        this.headMesh.Add(CreateMesh("backHeadRight", "backHeadLeft", "backHead"));
        this.headMesh.Add(CreateMesh("headRight", "backHeadRight", "earRight"));
        this.headMesh.Add(CreateMesh("headLeft", "earLeft", "backHeadLeft"));

        this.bodyMesh.Add(CreateMesh("chestRight", "abdomenUp", "chestLeft"));
        this.bodyMesh.Add(CreateMesh("chestRight", "abdomenRight", "abdomenUp"));
        this.bodyMesh.Add(CreateMesh("chestLeft", "abdomenUp", "abdomenLeft"));
        this.bodyMesh.Add(CreateMesh("abdomenRight", "abdomenLeft", "abdomenUp"));
        this.bodyMesh.Add(CreateMesh("abdomenRight", "hipRight", "hipLeft"));
        this.bodyMesh.Add(CreateMesh("abdomenLeft", "hipRight", "hipLeft"));
        this.bodyMesh.Add(CreateMesh("loinUp", "loinLeft", "loinRight"));
        this.bodyMesh.Add(CreateMesh("abdomenRight", "loinRight", "hipRight"));
        this.bodyMesh.Add(CreateMesh("abdomenLeft", "hipLeft", "loinLeft"));

    }

}

public class SurfacePoint
{
    public string name;
    public string pointType;
    public string attachedJointName;
    public Matrix4x4 matrix = new Matrix4x4(); //For source surface
    public Vector3 lposition; // For target surface
    public float radius;
    public GameObject Object;

    public SurfacePoint(string name, string pointType, string attachedJointName)
    {
        this.name = name;
        this.pointType = pointType;
        this.attachedJointName = attachedJointName;
    }
}


public class MeshTriangle
{
    public List<SurfacePoint> vertices = new List<SurfacePoint>();
    public List<GameObject> edges = new List<GameObject>();

    public MeshTriangle( SurfacePoint p1, SurfacePoint p2, SurfacePoint p3)
    {
        this.vertices.Add(p1);
        this.vertices.Add(p2);
        this.vertices.Add(p3);
    }

    public void Draw(int i)
    {
        this.edges.Add(SurfaceUtils.DrawPrimitive(PrimitiveType.Cylinder, "meshline_" + this.vertices[0].Object.name + "_" + this.vertices[1].Object.name, this.vertices[0].Object, this.vertices[1].Object, 0.2f));
        this.edges.Add(SurfaceUtils.DrawPrimitive(PrimitiveType.Cylinder, "meshline_" + this.vertices[1].Object.name + "_" + this.vertices[2].Object.name, this.vertices[1].Object, this.vertices[2].Object, 0.2f));
        this.edges.Add(SurfaceUtils.DrawPrimitive(PrimitiveType.Cylinder, "meshline_" + this.vertices[2].Object.name + "_" + this.vertices[0].Object.name, this.vertices[2].Object, this.vertices[0].Object, 0.2f));
    }

    public void Update()
    {
        SurfaceUtils.DrawPrimitive(this.edges[0], this.vertices[0].Object, this.vertices[1].Object);
        SurfaceUtils.DrawPrimitive(this.edges[1], this.vertices[1].Object, this.vertices[2].Object);
        SurfaceUtils.DrawPrimitive(this.edges[2], this.vertices[2].Object, this.vertices[0].Object);
    }

}

public static class SurfaceUtils
{
    public static GameObject DrawPrimitive(PrimitiveType type, string name, GameObject parent, GameObject child, float radius)
    {
        // Creates an object of the specified type starting at parent and ending at child
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent.transform);
        float size = Vector3.Distance(parent.transform.position, child.transform.position);
        Vector3 axis = Vector3.Cross(new Vector3(0.0f, 1.0f, 0.0f), parent.transform.position - child.transform.position);
        float rotAngle = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), parent.transform.position - child.transform.position);
        if ((rotAngle > 0) && (axis.magnitude == 0f))
            obj.transform.Rotate(Vector3.forward, 180f);
        else
            obj.transform.Rotate(axis, rotAngle);
        obj.transform.localPosition = new Vector3(0f, 0f, 0f);
        obj.transform.position -= obj.transform.up * size / 2;
        obj.transform.localScale = new Vector3(radius, size / 2, radius);
        return obj;
    }

    public static GameObject DrawPrimitive(GameObject obj, GameObject parent, GameObject child)
    {
        // Creates an object of the specified type starting at parent and ending at child
        float size = Vector3.Distance(parent.transform.position, child.transform.position);
        Vector3 axis = Vector3.Cross(new Vector3(0.0f, 1.0f, 0.0f), parent.transform.position - child.transform.position);
        float rotAngle = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), parent.transform.position - child.transform.position);
        obj.transform.rotation = Quaternion.identity;
        obj.transform.Rotate(axis, rotAngle);
        obj.transform.localPosition = new Vector3(0f, 0f, 0f);
        obj.transform.position -= obj.transform.up * size / 2;
        obj.transform.localScale = new Vector3(obj.transform.localScale.x, size / 2, obj.transform.localScale.z);
        return obj;
    }
}
