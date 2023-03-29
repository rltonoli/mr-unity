using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonMap : MonoBehaviour
{
    public Animation anim;
    public string model;
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

    public Dictionary<string, List<Joint>> bones = new Dictionary<string, List<Joint>>();
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
                                            "spine04","spine03","spine02","spine01",
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
        GenerateBones();

        this.modelflag = true;
        this.model = model;
        return this.modelflag;
    }

    private void GenerateBones()
    {
        // Generate the bones to be aligned
        // TODO: There should be an alignment for the hips, the facing direction (eg, imagine the line from LeftUpLeg to RightUpLeg)

        bones.Add("Spine" , new List<Joint> { keyValuePairs["Hips"], keyValuePairs["Spine"] } );
        bones.Add("Spine1", new List<Joint> { keyValuePairs["Spine"], keyValuePairs["Spine1"] });
        bones.Add("Spine2", new List<Joint> { keyValuePairs["Spine1"], keyValuePairs["Spine2"] });
        bones.Add("Spine3", new List<Joint> { keyValuePairs["Spine2"], keyValuePairs["Spine3"] });

        bones.Add("Neck", new List<Joint> { keyValuePairs["Spine3"], keyValuePairs["Neck"] });
        bones.Add("Neck1", new List<Joint> { keyValuePairs["Neck"], keyValuePairs["Neck1"] });
        bones.Add("Head", new List<Joint> { keyValuePairs["Neck1"], keyValuePairs["Head"] });

        //bones.Add("RightClavicle", new List<Joint> { keyValuePairs["RightShoulder"], keyValuePairs["RightArm"] });
        bones.Add("RightArm", new List<Joint> { keyValuePairs["RightArm"], keyValuePairs["RightForeArm"] });
        bones.Add("RightForeArm", new List<Joint> { keyValuePairs["RightForeArm"], keyValuePairs["RightHand"] });

        //bones.Add("LeftClavicle", new List<Joint> { keyValuePairs["LeftShoulder"], keyValuePairs["LeftArm"] });
        bones.Add("LeftArm", new List<Joint> { keyValuePairs["LeftArm"], keyValuePairs["LeftForeArm"] });
        bones.Add("LeftForeArm", new List<Joint> { keyValuePairs["LeftForeArm"], keyValuePairs["LeftHand"] });

        bones.Add("RightUpLeg", new List<Joint> { keyValuePairs["RightUpLeg"], keyValuePairs["RightLeg"] });
        bones.Add("RightLeg", new List<Joint> { keyValuePairs["RightLeg"], keyValuePairs["RightFoot"] });

        bones.Add("LeftUpLeg", new List<Joint> { keyValuePairs["LeftUpLeg"], keyValuePairs["LeftLeg"] });
        bones.Add("LeftLeg", new List<Joint> { keyValuePairs["LeftLeg"], keyValuePairs["LeftFoot"] });
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



