using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

public class SkeletonMap
{
    public Animation anim;
    private bool modelflag = false;
    public Dictionary<string, Joint> keyValuePairs = new Dictionary<string, Joint>();
    public List<string> keynames = new List<string>
                                          { "Root",
                                            "Hips",
                                            "Spine", "Spine1","Spine2", "Spine3",
                                            "Neck","Neck1","Head",
                                            "RightShoulder","RightArm", "RightForeArm", "RightHand",
                                            "LeftShoulder","LeftArm", "LeftForeArm","LeftHand",
                                            "RightUpLeg", "RightLeg", "RightFoot",
                                            "LeftUpLeg","LeftLeg", "LeftFoot"};

    public SkeletonMap(Animation anim) 
    {
        this.anim = anim;
    }

    public SkeletonMap(Animation anim, string model) 
    { 
        this.anim = anim;
        _ = SetSkeletonModel(model);
    }

    public bool SetSkeletonModel(string model)
    {
        List<string> jointnames = new List<string>();
        switch (model)
        {
            case ("Vicon"):
                jointnames = new List<string> 
                                          { "Hips",
                                            "Hips",
                                            "Spine","Spine1","Spine2","Spine3",
                                            "Neck", "Neck1","Head",
                                            "RightShoulder","RightArm","RightForeArm","RightHand",
                                            "LeftShoulder","LeftArm","LeftForeArm","LeftHand",
                                            "RightUpLeg","RightLeg","RightFoot",
                                            "LeftUpLeg","LeftLeg","LeftFoot"};
                break;
            case ("Talita"):
                jointnames = new List<string>
                                          { "root",
                                            "spine05",
                                            "spine04","spine03","Spine02","Spine01",
                                            "neck01", "neck02", "head",
                                            "clavicle.R", "upperarm01.R", "lowerarm01.R", "wrist.R",
                                            "clavicle.L", "upperarm01.L", "lowerarm01.L", "wrist.L",
                                            "upperleg01.R", "lowerleg01.R", "foot.R",
                                            "upperleg01.L", "lowerleg01.L", "foot.L"};
                break;
            case ("MoBu"):
                break;
            //case ("SomeCustomSkeleton"):
            //
            //    /* Define your skeleton here */
            //
            //    break;
            default: return false;
        }

        // Populates the dictionary with the KNOWN joint names and the Joints from Animation anim
        for (int i = 0; i < this.keynames.Count; i++)
        {
            Joint joint = this.anim.GetJoint(jointnames[i]);
            if (joint != null)
                this.keyValuePairs.Add(this.keynames[i], joint);
            else
                Debug.Log("Could not find joint: " + this.keynames[i]);
        }

        this.modelflag = true;
        return this.modelflag;
    }


    public Joint GetJoint(string jointname)
    {
        if (this.keyValuePairs.Count > 0)
        {
            Joint joint = this.keyValuePairs[jointname];
            if (joint != null)
                return joint;
            else
                Debug.Log("Joint " + jointname + "was not found.");
        }
        else
            Debug.Log("Joint map was not found. Please use SetSkeltonModel() first.");
        return null;
    }
    
}



