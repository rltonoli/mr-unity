using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


public class Surface : MonoBehaviour
{

    public List<SurfacePoint> surfacePoints = new List<SurfacePoint>();
    public SkeletonMap skeletonMap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReadSourceSurfaceFile(string filePath)
    {
        if (!(File.Exists(filePath)))
        {
            Debug.Log("Surface file not found");
            return;
        }

        this.skeletonMap = GetComponent<SkeletonMap>();
        if (this.skeletonMap == null)
        {

            Debug.Log("Could not find a skeleton map for " + this.gameObject.name);
            return;
        }

        CreateSurfacePoints();

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

        //Debug.Log(linedContent.Count);
        //Debug.Log(this.surfacePoints.Count);
        //Debug.Log(linedContent[0].Length);

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
                //Debug.Log(this.surfacePoints[i].matrix[0, 0] + " " + this.surfacePoints[i].matrix[0, 1] + " " + this.surfacePoints[i].matrix[0, 2] + " " + this.surfacePoints[i].matrix[0, 3]);
                //Debug.Log(this.surfacePoints[i].matrix[1, 0] + " " + this.surfacePoints[i].matrix[1, 1] + " " + this.surfacePoints[i].matrix[1, 2] + " " + this.surfacePoints[i].matrix[1, 3]);
                //Debug.Log(this.surfacePoints[i].matrix[2, 0] + " " + this.surfacePoints[i].matrix[2, 1] + " " + this.surfacePoints[i].matrix[2, 2] + " " + this.surfacePoints[i].matrix[2, 3]);
                //Debug.Log(this.surfacePoints[i].matrix[3, 0] + " " + this.surfacePoints[i].matrix[3, 1] + " " + this.surfacePoints[i].matrix[3, 2] + " " + this.surfacePoints[i].matrix[3, 3]);
                if (this.surfacePoints[i].matrix.ValidTRS() == false)
                {
                    Debug.Log("Something went wrong reading the transformation matrix of surface point " + this.surfacePoints[i].name);
                }
                //Debug.Log(this.surfacePoints[i].matrix.lossyScale);

            }
            else
            {
                this.surfacePoints[i].radius = System.Convert.ToSingle(linedContent[i][0], System.Globalization.CultureInfo.InvariantCulture);
            } 
        }


    }

    public void DrawSurface()
    {
        Joint joint;
        Joint childJoint;
        GameObject pointObj;
        float limbSize;

        foreach (SurfacePoint point in this.surfacePoints)
        {
            joint = this.skeletonMap.GetJoint(point.attachedJointName);
            if (point.pointType == "mesh")
            {
                pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pointObj.name = point.name;
                pointObj.transform.SetParent(joint.Object.transform);
                pointObj.transform.localPosition = point.matrix.GetPosition();
                pointObj.transform.localRotation = point.matrix.rotation;
                point.Object = pointObj;
            }
            else if (point.pointType == "limb")
            {
                // Grab the other joint that compose the limb. E.g., joint is RightUpLeg, so it gets the second joint from the RightUpLeg *bone* from skeleton map
                childJoint = this.skeletonMap.bones[joint.name][1];
                pointObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pointObj.name = point.name;
                pointObj.transform.SetParent(joint.Object.transform);
                limbSize = Vector3.Distance(new Vector3(0.0f, 0.0f, 0.0f), childJoint.Object.transform.localPosition);

                pointObj.transform.localPosition = (childJoint.Object.transform.position - joint.Object.transform.position)/ 2;

                pointObj.transform.localScale = new Vector3(point.radius, limbSize/2, point.radius);

                //Rotates the bone GameObject so that it is aligned with the line that passes through 
                pointObj.transform.Rotate(
                    Vector3.Cross(new Vector3(0.0f, 1.0f, 0.0f), joint.Object.transform.position - childJoint.Object.transform.position),
                    Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), joint.Object.transform.position - childJoint.Object.transform.position)
                    );

                point.Object = pointObj;

            }
        }
    }

    public void DrawFrame()
    {
        
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

}

public class SurfacePoint
{
    public string name;
    public string pointType;
    public string attachedJointName;
    public Matrix4x4 matrix = new Matrix4x4();
    public float radius;
    public GameObject Object;

    public SurfacePoint(string name, string pointType, string attachedJointName)
    {
        this.name = name;
        this.pointType = pointType;
        this.attachedJointName = attachedJointName;
    }
}
