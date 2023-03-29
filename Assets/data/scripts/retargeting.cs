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

    public void BoneRetargeting(SkeletonMap srcMap, SkeletonMap tgtMap)
    {
        this.srcMap = srcMap;
        this.tgtMap = tgtMap;
        foreach (string key in tgtMap.bones.Keys)
        {
            //Debug.Log("Bone: " + key);
            //Debug.Log("Src: " + this.srcMap.bones[key][0].name + this.srcMap.bones[key][1].name);
            //Debug.Log("Tgt: " + this.tgtMap.bones[key][0].name + this.tgtMap.bones[key][1].name);
            //Vector3 srcBone = this.srcMap.bones[key][0]
        }
    }

}
