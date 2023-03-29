using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class retargeting : MonoBehaviour
{

    public SkeletonMap srcMap;
    public SkeletonMap tgtMap;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BoneRetargeting(SkeletonMap srcMap, SkeletonMap tgtMap, int frame)
    {
        Vector3 srcBone;
        Vector3 tgtBone;
        Quaternion rotation = new Quaternion();
        

        foreach (string key in tgtMap.bones.Keys)
        {
            //Debug.Log("Bone: " + key);
            //Debug.Log("Tgt: " + this.tgtMap.bones[key][0].Object.name + this.tgtMap.bones[key][1].Object.name);
            //Debug.Log("Src: " + this.srcMap.bones[key][0].Object.name + this.srcMap.bones[key][1].Object.name);

            srcBone = srcMap.bones[key][0].Object.transform.position - srcMap.bones[key][1].Object.transform.position;
            tgtBone = tgtMap.bones[key][0].Object.transform.position - tgtMap.bones[key][1].Object.transform.position;
            rotation.SetFromToRotation(tgtBone, srcBone);
            tgtMap.bones[key][0].Object.transform.rotation = rotation * tgtMap.bones[key][0].Object.transform.rotation;
        }

        // HARD-CODED BELLOW
        // TODO: Don't make it hard-coded
        // Here I will assume that the srcSkeleton is at the TPose in the first frame (palm of the hands facing down)
        if ((tgtMap.model=="Talita") && (frame == 0)) 
        {
            Vector3 hand = tgtMap.anim.GetJoint("wrist.R").Object.transform.position;
            Vector3 finger1 = tgtMap.anim.GetJoint("finger2-1.R").Object.transform.position;
            Vector3 finger2 = tgtMap.anim.GetJoint("finger5-1.R").Object.transform.position;
            Vector3 handBone1 = finger1 - hand;
            Vector3 handBone2 = finger2 - hand;
            Vector3 handPalmDirection = Vector3.Cross(handBone1, handBone2);



        }

    }

    public void WorldRotRetargeting(SkeletonMap srcMap, SkeletonMap tgtMap, int frame)
    {
        // This retargeting assumes that the source and target bones are aligned in the previous frame
        // in order to compute the global rotations applied to the sources' joints and apply them on the targets' joints
        // So, use BoneRetargeting first (eg, use BoneRetargeting on the first frame and apply this retargeting on the rest)
        if (frame == 0)
        {
            BoneRetargeting(srcMap, tgtMap, 0);
            return;
        }
        Quaternion previousRotation;
        Quaternion currentRotation;
        Quaternion relativeRotation;


        foreach (string key in tgtMap.bones.Keys)
        {
            previousRotation = srcMap.bones[key][0].previousRotation;
            currentRotation = srcMap.bones[key][0].Object.transform.rotation;
            relativeRotation = currentRotation * Quaternion.Inverse(previousRotation);

            tgtMap.bones[key][0].Object.transform.rotation = relativeRotation * tgtMap.bones[key][0].Object.transform.rotation;
        }

         

    }



}
