using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class model : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Animation GenerateFromModel(string ModelName, GameObject rootObject)
    {
        Animation anim = new Animation(ModelName);
        RegisterJoints(null, rootObject, 0, anim);

        return anim;
    }

    private static void RegisterJoints(Joint parent, GameObject childObject, int depth, Animation anim)
    {

        Joint joint = new Joint(childObject.name, anim);
        joint.depth = depth;
        joint.Object = childObject;

        // TODO: Test this offset 
        Quaternion currentRotation = childObject.transform.localRotation;
        childObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        joint.setOffset(childObject.transform.localPosition*100);
        childObject.transform.localRotation = currentRotation;


        if (parent is null)
            anim.setRoot(joint);
        else
        {
            joint.parent = parent;
            parent.children.Add(joint);
        }

        for (int i = 0; i < childObject.transform.childCount; i++)
        {
            RegisterJoints(joint, childObject.transform.GetChild(i).gameObject, depth + 1, anim);
        }
        
    }

}
