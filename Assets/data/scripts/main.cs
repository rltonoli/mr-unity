using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{

    public GameObject characterModel;

    // Start is called before the first frame update
    void Start()
    {
        Animation anim;
        anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/Mao_na_frente_mcp.bvh");
        //anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/RArmRotTest.bvh");
        SkeletonMap skelmap_anim = new SkeletonMap(anim, "Vicon");

        Animation anim_talita;
        if (characterModel != null)
        {
            anim_talita = model.GenerateFromModel("Talita", characterModel);
            SkeletonMap skelmap_talita = new SkeletonMap(anim_talita, "Talita");
        }


        GameObject srcSkeletonObj = new GameObject("Source Skeleton");
        draw_skeleton srcSkeleton = srcSkeletonObj.AddComponent<draw_skeleton>();
        srcSkeleton.Draw(anim);

        // draw_skeleton srcSkeleton = new draw_skeleton(anim);

        StartCoroutine(DrawWaiting(srcSkeleton, 0.01f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator DrawWaiting(draw_skeleton skel, float wait)
    {
        for (int i = 0; i < skel.anim.frames; i++)
        {
            Debug.Log(i);
            skel.DrawFrame(i);
            yield return new WaitForSeconds(wait);
        }
    }

}
