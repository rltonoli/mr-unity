using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EgoCoordSystem : MonoBehaviour
{
    public SkeletonMap skeletonMap;
    public Surface surface;
    public EgoCoord rightHandEgoCoord;
    public EgoCoord leftHandEgoCoord;

    // Start is called before the first frame update
    void Start()
    {
        this.skeletonMap = GetComponent<SkeletonMap>();
        this.surface = GetComponent<Surface>();
        if (this.skeletonMap == null)
            Debug.Log("Could not find component the SkeletonMap component in the SrcEgoCoord for " + this.gameObject.name);
        if (this.surface == null)
            Debug.Log("Could not find component the Surface component in the SrcEgoCoord for " + this.gameObject.name);
        Joint rhand = this.skeletonMap.keyValuePairs["RightHand"];
        Joint lhand = this.skeletonMap.keyValuePairs["LeftHand"];

        rightHandEgoCoord = new EgoCoord(this, rhand);
        leftHandEgoCoord = new EgoCoord(this, lhand);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Compute()
    {
        this.rightHandEgoCoord.Compute();
        this.leftHandEgoCoord.Compute();
    }

    public void Draw( EgoCoord coord )
    {

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

    public EgoCoord(EgoCoordSystem sys, Joint joint)
    {
        this.sys = sys;
        this.skeletonMap = this.sys.skeletonMap;
        this.surface = this.sys.surface;
        this.joint = joint;
        this.name = joint.name;
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

        foreach (MeshTriangle mtriangle in this.surface.headMesh)
        {
            n = mtriangle.GetNormalVector();
            cos_theta = Mathf.Cos(Vector3.Angle(n, this.displacementVectors[count]) * Mathf.PI / 180);
            proximity = 1 / this.displacementVectors[count].magnitude;
            orthogonality = (cos_theta + 1) / 2;
            this.importance.Add(proximity * orthogonality);
            count += 1;
        }
        foreach (MeshTriangle mtriangle in this.surface.bodyMesh)
        {
            n = mtriangle.GetNormalVector();
            cos_theta = Mathf.Cos(Vector3.Angle(n, this.displacementVectors[count]) * Mathf.PI / 180);
            proximity = 1 / this.displacementVectors[count].magnitude;
            orthogonality = (cos_theta + 1) / 2;
            this.importance.Add(proximity * orthogonality);
            count += 1;
        }
        foreach (SurfacePoint limb in this.surface.limbs)
        {
            aux_limbjoint1 = this.skeletonMap.bones[limb.attachedJointName][0];
            aux_limbjoint2 = this.skeletonMap.bones[limb.attachedJointName][1];
            cos_theta = 1 - Mathf.Cos(Vector3.Angle(aux_limbjoint2.Object.transform.position - aux_limbjoint1.Object.transform.position, this.displacementVectors[count]) * Mathf.PI / 180);
            proximity = 1 / this.displacementVectors[count].magnitude;
            orthogonality = (cos_theta + 1) / 2;
            this.importance.Add(proximity * orthogonality);
            count += 1;
        }
    }



}