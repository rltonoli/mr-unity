using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

//TODO: Maybe change this name to bvh_skeleton
public class draw_skeleton : MonoBehaviour
{
    public List<GameObject> draw_listofjoints = new List<GameObject>();
    public Animation anim;
    public SkeletonMap skeletonMap;


    private void TestJoints(Animation anim)
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

    public void Draw(Animation anim, GameObject holder, int scale)
    {

        if (this.anim != null)
        {
            Debug.Log("Skeleton already drawing an animation. Please instantiate another skeleton.");
            return;
        }

        this.anim = anim;

        // Create an empty object to accomodate the skeleton
        //GameObject empty_holder = new GameObject("Input skeleton");
        //empty_holder.transform.position = new Vector3(0, 0, 0);
        

        // Create the root
        GameObject rootObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rootObj.name = this.anim.root.name;
        rootObj.transform.SetParent(holder.transform);
        //rootObj.transform.localPosition = anim.root.localTranslation[0];
        rootObj.transform.localPosition = this.anim.root.offset * scale;
        //rootObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        this.anim.root.Object = rootObj;
        draw_listofjoints.Add(rootObj);

        // Start to iterate creating every other joint
        foreach (Joint child in this.anim.root.children)
        {
            GameObject dummy = createObjects(rootObj, child, scale);
        }

        //empty_holder.transform.localScale = new Vector3(Scale, Scale, Scale);

        TestJoints(this.anim);
    }

    private GameObject createObjects(GameObject parent, Joint child, int scale)
    {
        //Receives the GameObject that represents the parent joint and the Joint child


        //Creates a sphere and sets its local position (given the child joint's local translation)
        GameObject childObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        draw_listofjoints.Add(childObj);
        childObj.name = child.name;
        childObj.transform.SetParent(parent.transform);
        //childObj.transform.localPosition = child.localTranslation[0];
        childObj.transform.localPosition = child.offset* scale;
        //childObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f)/parent.transform.lossyScale[0];
        //Adds a reference to the joint GameObject in the respective Joint
        child.Object = childObj;

        //Creates the bone GameObject given the parent's and the child's sphere GameObjects
        GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bone.name = "_bone" + parent.name;
        bone.transform.SetParent(parent.transform);
        float boneSize = Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), childObj.transform.localPosition);
        //bone.transform.localPosition = child.localTranslation[0] / 2;
        bone.transform.localPosition = child.offset * scale / 2;

        bone.transform.localScale = new Vector3(0.2f, boneSize / 2, 0.2f);

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
                createObjects(childObj, respective_child, scale);
            }
        }


        return childObj;
    }

    public void DrawFrame(int frame)
    {
        int n = draw_listofjoints.Count;
        
        for (int i = 0; i < n; i++)
        {
            this.anim.listofjoints[i].previousRotation = draw_listofjoints[i].transform.rotation;
            draw_listofjoints[i].transform.localRotation = BVH2UnityRotation(this.anim.listofjoints[i].localEulerAngles[frame]);
        }
        draw_listofjoints[0].transform.position = this.anim.root.localTranslation[frame];
        //Debug.Log(anim.GetJoint("RightArm").gameobject_joint.transform.localEulerAngles);
        //Debug.Log(anim.GetJoint("RightArm").localEulerAngles[frame]);

        Surface surface = GetComponent<Surface>();
        if (surface != null )
        {
            surface.UpdateMeshes(this.gameObject);
        }
        EgoCoordSystem egoCoordSystem = GetComponent<EgoCoordSystem>();
        if (egoCoordSystem != null)
        {
            egoCoordSystem.Compute();
            egoCoordSystem.DrawEgoCoord( egoCoordSystem.rightHandEgoCoord );
        }


    }

    public Quaternion BVH2UnityRotation(Vector3 euler)
    {
        // BVH's x+ axis is Unity's left (x-)
        var xRot = Quaternion.AngleAxis(-euler.x, Vector3.left);
        // Unity & BVH agree on the y & z axes
        var yRot = Quaternion.AngleAxis(-euler.y, Vector3.up);
        var zRot = Quaternion.AngleAxis(-euler.z, Vector3.forward);
        //var yRot = Quaternion.AngleAxis(euler.y, Vector3.up);
        //var zRot = Quaternion.AngleAxis(euler.z, Vector3.forward);

        return zRot * xRot * yRot;
    }

}
