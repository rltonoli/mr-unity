using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class EgoCoordSystem : MonoBehaviour
{
    public SkeletonMap skeletonMap;
    public Surface surface;
    public EgoCoord rightHandEgoCoord;
    public EgoCoord leftHandEgoCoord;

    public GameObject EgoCoordsHolder;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Configure()
    {
        this.skeletonMap = GetComponent<SkeletonMap>();
        this.surface = GetComponent<Surface>();
        if (this.skeletonMap == null)
            Debug.Log("Could not find component the SkeletonMap component in the SrcEgoCoord for " + this.gameObject.name);
        if (this.surface == null)
            Debug.Log("Could not find component the Surface component in the SrcEgoCoord for " + this.gameObject.name);
        Joint rhand = this.skeletonMap.keyValuePairs["RightHand"];
        Joint lhand = this.skeletonMap.keyValuePairs["LeftHand"];
        this.rightHandEgoCoord = new EgoCoord(this, rhand);
        this.leftHandEgoCoord = new EgoCoord(this, lhand);
    }

    public void Compute()
    {
        this.rightHandEgoCoord.Compute();
        this.leftHandEgoCoord.Compute();
    }

    public void DrawEgoCoord( EgoCoord coord )
    {
        if (coord == null)
        {
            Debug.Log("DrawEgoCoord must receive an EgoCoord object to draw. Null received.");
            return;
        }
        else
        {
            if (this.EgoCoordsHolder == null)
            {
                this.EgoCoordsHolder = new GameObject("EgoCoordsHolder");
                this.EgoCoordsHolder.transform.parent = this.gameObject.transform;

            }
            coord.DrawRefPoints(this.EgoCoordsHolder);
            coord.DrawDispVectors(this.EgoCoordsHolder);
            coord.DrawImportance();
        }
        
    }

}

public class EgoCoord
{

    public EgoCoordSystem sys;
    public SkeletonMap skeletonMap;
    public Surface surface;
    public Joint joint;
    public string name;

    public List<Vector3> referencePoints = new List<Vector3>();
    public List<Vector3> displacementVectors = new List<Vector3>();
    public List<float> importance = new List<float>();

    private List<GameObject> objReferencePoints = new List<GameObject>();
    private List<GameObject> objdisplacementVectors = new List<GameObject>();
    private List<GameObject> objimportance = new List<GameObject>();


    public EgoCoord(EgoCoordSystem sys, Joint joint)
    {
        this.sys = sys;
        this.skeletonMap = this.sys.skeletonMap;
        this.surface = this.sys.surface;
        this.joint = joint;
        this.name = joint.name;
        if (this.joint.name == this.skeletonMap.GetJoint("RightHand").name)
            this.name = "RightHand";
        if (this.joint.name == this.skeletonMap.GetJoint("LeftHand").name)
            this.name = "LeftHand";

    }

    public void Compute()
    {
        Get_ReferencePoint();
        Get_DisplacementVector();
        Get_ImportanceFactor();
    }

    private void Get_ReferencePoint()
    {
        this.referencePoints.Clear();
        // Get reference point (centroid)
        foreach (MeshTriangle mtriangle in this.surface.headMesh)
        {
            this.referencePoints.Add(mtriangle.GetCentroid());
        }
        foreach (MeshTriangle mtriangle in this.surface.bodyMesh)
        {
            this.referencePoints.Add(mtriangle.GetCentroid());
        }
        foreach (SurfacePoint limb in this.surface.limbs)
        {
            if (!CheckIfSameLimb(limb))
                this.referencePoints.Add(limb.Object.GetComponent<Collider>().ClosestPoint(this.joint.Object.transform.position));
        }
    }

    private void Get_DisplacementVector()
    {
        this.displacementVectors.Clear();
        for (int i = 0; i < referencePoints.Count; i++)
        {
            this.displacementVectors.Add(this.joint.Object.transform.position - this.referencePoints[i]);
        }
    }

    private void Get_ImportanceFactor()
    {
        Vector3 n;
        float cos_theta;
        int count = 0;
        float proximity;
        float orthogonality;
        Joint aux_limbjoint1;
        Joint aux_limbjoint2;

        this.importance.Clear();

        foreach (MeshTriangle mtriangle in this.surface.headMesh)
        {
            n = mtriangle.GetNormalVector();
            cos_theta = Mathf.Cos(Vector3.Angle(n, this.displacementVectors[count]) * Mathf.PI / 180);
            float mag = this.displacementVectors[count].magnitude;
            Assert.AreNotEqual(mag, 0f);
            proximity = 1 / mag;
            orthogonality = (cos_theta + 1) / 2;
            this.importance.Add(proximity * orthogonality);
            count += 1;
        }
        foreach (MeshTriangle mtriangle in this.surface.bodyMesh)
        {
            n = mtriangle.GetNormalVector();
            cos_theta = Mathf.Cos(Vector3.Angle(n, this.displacementVectors[count]) * Mathf.PI / 180);
            float mag = this.displacementVectors[count].magnitude;
            Assert.AreNotEqual(mag, 0f);
            proximity = 1 / mag;
            orthogonality = (cos_theta + 1) / 2;
            this.importance.Add(proximity * orthogonality);
            count += 1;
        }
        foreach (SurfacePoint limb in this.surface.limbs)
        {
            if (!CheckIfSameLimb(limb)) {
                aux_limbjoint1 = this.skeletonMap.bones[limb.attachedJointName][0];
                aux_limbjoint2 = this.skeletonMap.bones[limb.attachedJointName][1];
                //cos_theta = 1 - Mathf.Cos(Vector3.Angle(aux_limbjoint2.Object.transform.position - aux_limbjoint1.Object.transform.position, this.displacementVectors[count]) * Mathf.PI / 180);
                cos_theta = Mathf.Cos(Vector3.Angle(aux_limbjoint2.Object.transform.position - aux_limbjoint1.Object.transform.position, this.displacementVectors[count]) * Mathf.PI / 180);
                float mag = this.displacementVectors[count].magnitude;
                Assert.AreNotEqual(mag, 0f);
                proximity = 1 / mag;
                orthogonality = (cos_theta + 1) / 2;
                this.importance.Add(proximity * orthogonality);
                count += 1;
            }
        }

        float sum = this.importance.Sum();
        
        for (int i = 0; i < this.importance.Count; i++)
        {
            this.importance[i] /= sum;
        }

    }

    private bool CheckIfSameLimb(SurfacePoint limb)
    {
        //if ((this.joint.name == this.skeletonMap.GetJoint("RightHand").name) && ((limb.name == "foreRight") || (limb.name == "armRight")))
        //    return true;
        //else if ((this.joint.name == this.skeletonMap.GetJoint("LeftHand").name) && ((limb.name == "foreLeft") || (limb.name == "armLeft")))
        //    return true;
        if ((this.name == "RightHand") && ((limb.name == "foreRight") || (limb.name == "armRight")))
            return true;
        else if ((this.name == "LeftHand") && ((limb.name == "foreLeft") || (limb.name == "armLeft")))
            return true;
        return false;
    }

    public void DrawRefPoints(GameObject holder)
    {
        GameObject auxGameObject = null;
        if (this.objReferencePoints.Count == 0) //Create objects
        {
            for (int i = 0; i < this.referencePoints.Count; i++)
            {
                auxGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                auxGameObject.name = this.referencePoints[i].ToString();
                auxGameObject.transform.parent = holder.transform;
                auxGameObject.transform.position = this.referencePoints[i];
                auxGameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                this.objReferencePoints.Add(auxGameObject);
            }
        }

        else //Update objects
        {
            for (int i = 0; i < this.referencePoints.Count; i++)
            {
                this.objReferencePoints[i].transform.position = this.referencePoints[i];
            }
        }
    }

    public void DrawDispVectors(GameObject holder)
    {
        GameObject auxGameObject = null;
        int n = this.objdisplacementVectors.Count;
        if (n == 0) //Create objects
        {
            for (int i = 0; i < this.objReferencePoints.Count; i++)
            {
                auxGameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                auxGameObject.name = "dispvec_" + this.objReferencePoints[i].name;
                auxGameObject.transform.parent = holder.transform;
                auxUpdateDispVecPrimitive(auxGameObject, this.objReferencePoints[i], this.joint.Object, 0.01f);
                this.objdisplacementVectors.Add(auxGameObject);
            }
        }

        else //Update objects
        {
            for (int i = 0; i < n; i++)
            {
                auxUpdateDispVecPrimitive(this.objdisplacementVectors[i], this.objReferencePoints[i], this.joint.Object, 0.01f);
                
            }
            this.objdisplacementVectors[0].GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    private static GameObject auxUpdateDispVecPrimitive(GameObject obj, GameObject refpoint, GameObject joint, float scale)
    {
        // Creates an object of the specified type starting at parent and ending at child
        float size = Vector3.Distance(refpoint.transform.position, joint.transform.position);
        Vector3 axis = Vector3.Cross(new Vector3(0.0f, 1.0f, 0.0f), refpoint.transform.position - joint.transform.position);
        float rotAngle = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), refpoint.transform.position - joint.transform.position);
        obj.transform.rotation = Quaternion.identity;
        obj.transform.Rotate(axis, rotAngle);
        obj.transform.position = (joint.transform.position - refpoint.transform.position) / 2 + refpoint.transform.position;
        obj.transform.localScale = new Vector3(scale, size / 2, scale);
        return obj;
    }

    public void DrawImportance()
    {
        Color imp;
        for (int i = 0; i < this.objdisplacementVectors.Count; i++) {
            //imp = new Color(this.importance[i], 0f, 0f);
            imp = new Color(1f, (1 - importance[i]*10), (1 - importance[i]*10));
            this.objdisplacementVectors[i].GetComponent<MeshRenderer>().material.color = imp;
            //Debug.Log(this.importance[i]);
        }
    }

    private List<Joint> GetKinematicPath()
    {
        List<Joint> path = new List<Joint>();

        string aux = "";
        if (this.name == "RightHand")
            aux = "Right";
        else if (this.name == "LeftHand")
            aux = "Left";
        else
            Debug.Log("Joint not yet registered. Could not get KinematicPath");
        Assert.AreNotEqual(aux, "");

        path.Add(this.skeletonMap.GetJoint(aux+"Had"));
        path.Add(this.skeletonMap.GetJoint(aux+"ForeArm"));
        path.Add(this.skeletonMap.GetJoint(aux+"Arm"));
        path.Add(this.skeletonMap.GetJoint(aux+"Shoulder"));

        foreach (MeshTriangle mtriangle in this.surface.headMesh)
        {
            
        }
        foreach (MeshTriangle mtriangle in this.surface.bodyMesh)
        {
            this.referencePoints.Add(mtriangle.GetCentroid());
        }
        foreach (SurfacePoint limb in this.surface.limbs)
        {
            if (!CheckIfSameLimb(limb))
                this.referencePoints.Add(limb.Object.GetComponent<Collider>().ClosestPoint(this.joint.Object.transform.position));
        }

        return path;
    }

}