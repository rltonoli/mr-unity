using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class draw_skeleton : MonoBehaviour
{
    public static float Scale = 1/150f;
    public static List<GameObject> draw_listofjoints = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private static void TestJoints(Animation anim)
    {
        int n = draw_listofjoints.Count;
        for (int i=0; i<n; i++)
        {
            if (draw_listofjoints[i].name != anim.listofjoints[i].name)
            {
                Debug.Log("The list of joints drawn as GameObjects does not match the list of joints read in the bvh file");
                Debug.Log(draw_listofjoints[i].name);
                Debug.Log(anim.listofjoints[i].name);
            }
        }
    }

    public static void Draw(Animation anim)
    {

        // Create an empty object to accomodate the skeleton
        GameObject empty_holder = new GameObject("Input skeleton");
        empty_holder.transform.position = new Vector3(0, 0, 0);
        

        // Create the root
        GameObject rootObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rootObj.name = anim.root.name;
        rootObj.transform.SetParent(empty_holder.transform);
        rootObj.transform.localPosition = anim.root.localTranslation[0];
        draw_listofjoints.Add(rootObj);

        // Start to iterate creating every other joint
        foreach (Joint child in anim.root.children)
        {
            GameObject dummy = createObjects(rootObj, child);
        }

        //empty_holder.transform.localScale = new Vector3(Scale, Scale, Scale);

        TestJoints(anim);

    }

    private static GameObject createObjects(GameObject parent, Joint child)
    {
        //Receives the GameObject that represents the parent joint and the Joint child


        //Creates a sphere and sets its local position (given the child joint's local translation)
        GameObject childObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        draw_listofjoints.Add(childObj);
        childObj.name = child.name;
        childObj.transform.SetParent(parent.transform);
        childObj.transform.localPosition = child.localTranslation[0];
        //Adds a reference to the joint GameObject in the respective Joint
        child.gameobject_joint = childObj;

        //Creates the bone GameObject given the parent's and the child's sphere GameObjects
        GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bone.name = "_bone" + parent.name;
        bone.transform.SetParent(parent.transform);
        float boneSize = Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), childObj.transform.localPosition);
        bone.transform.localPosition = child.localTranslation[0] / 2;
        bone.transform.localScale = new Vector3(1, boneSize / 2, 1);
        //Adds a reference to the bone GameObject to a list called "bones" in the parent's Joint refenrece.
        child.parent.bones.Add(bone);

        //Rotates the bone GameObject so that it is aligned with the line that passes through 
        bone.transform.Rotate(
            Vector3.Cross(new Vector3(0.0f, 1.0f, 0.0f), parent.transform.position - childObj.transform.position),
            Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), parent.transform.position - childObj.transform.position)
            );

        
        //Calls this functon recursevely for each registered child joint (each children of Joint child)
        if (child.children.Count > 0)
        {
            foreach (Joint respective_child in child.children)
            {
                createObjects(childObj, respective_child);
            }
        }


        return childObj;
    }

    public static void DrawFrame(int frame, Animation anim)
    {
        int n = draw_listofjoints.Count;
        for (int i = 0; i < n; i++)
        {
            //(draw_listofjoints[i].name != anim.listofjoints[i].name)
            //draw_listofjoints[i].transform.localRotation = Quaternion.Euler(anim.listofjoints[i].localEulerAngles[frame]);
            draw_listofjoints[i].transform.localEulerAngles = anim.listofjoints[i].localEulerAngles[frame];
        }

        Debug.Log(anim.GetJoint("RightArm").gameobject_joint.transform.localEulerAngles);
        Debug.Log(anim.GetJoint("RightArm").localEulerAngles[frame]);
    }

}
