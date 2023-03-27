using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Animation anim;
        anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/Mao_na_frente_mcp.bvh");
        //anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/RArmRotTest.bvh");
        draw_skeleton.Draw(anim);
        draw_skeleton.DrawFrame(0, anim);
        StartCoroutine(DrawWaiting(anim, 0.5f));
        


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator DrawWaiting(Animation anim, float wait)
    {
        for (int i = 0; i < anim.frames; i++)
        {
            Debug.Log(i);
            draw_skeleton.DrawFrame(i, anim);
            yield return new WaitForSeconds(wait);
        }
    }

}
