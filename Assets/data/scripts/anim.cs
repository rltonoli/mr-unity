using UnityEngine;
using System.Collections.Generic;

public class Animation
{
    public string name;
    public Joint root;
    public List<Joint> listofjoints = new List<Joint>();
    public int frames;
    public float fps;

    public Animation(string name)
    {
        this.name = name;
    }

    public void setRoot(Joint joint)
    {
        this.root = joint;
    }

    public void printHierarchy()
    {
        auxPrint(this.root);
    }

    private Joint auxPrint(Joint joint)
    {
        Debug.Log(joint.name.PadLeft(joint.depth*4));
        if (joint.children.Count > 0)
        {
            foreach (Joint child in joint.children)
            {
                auxPrint(child);
            }
        }
        return joint;
    }

    public Joint GetJoint(string name) { 
        foreach (Joint joint in listofjoints)
        {
            if (joint.name == name) return joint;
        }
        return null;
    }

    public IEnumerable<Joint> GetJoints()
    {
        foreach (Joint joint in this.listofjoints)
        {
            yield return joint;
        }
    }


}

public class Joint
{
    public string name;
    public List<Joint> children = new List<Joint>();
    public Joint parent;
    public Vector3 offset;
    public int n_channels;
    public string c_first, c_second, c_third, c_fourth, c_fifth, c_sixth;
    public bool endsite = false;
    public Vector3 endsite_offset;
    public int depth;
    public Animation anim;
    public List<Vector3> localEulerAngles = new List<Vector3>();
    public List<Vector3> localTranslation = new List<Vector3>();

    // Used in draw_skeleton
    public List<GameObject> bones = new List<GameObject>();

    public GameObject Object = null;

    public Quaternion previousRotation; //From transform

    public Joint(string name, Animation anim)
    {
        this.name = name;
        this.anim = anim;
        this.anim.listofjoints.Add(this);
    }

    public void setOffset(Vector3 offset)
    {
        this.offset = offset;
    }

    public void setChannels(int n_channels, string c_first, string c_second, string c_third, string c_fourth, string c_fifth, string c_sixth)
    {
        this.n_channels = n_channels;
        this.c_first = c_first;
        this.c_second = c_second;
        this.c_third = c_third;
        this.c_fourth = c_fourth;
        this.c_fifth = c_fifth;
        this.c_sixth = c_sixth;
    }

    public void setEndSite(Vector3 endsite_offset)
    {
        this.endsite = true;
        this.endsite_offset = endsite_offset;
    }

    public void appendLocalRotation(Vector3 lRotation)
    {
        this.localEulerAngles.Add(lRotation);
    }

    public void appendLocalTranslation(Vector3 lTranslation)
    {
        this.localTranslation.Add(lTranslation);
    }

}