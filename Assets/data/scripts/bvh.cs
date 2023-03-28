using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class bvh : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        //Animation anim;
        //anim = ReadBVHFile(Application.dataPath+"/data/input/Mao_na_frente_mcp.bvh");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static Animation ReadBVHFile(string path)
    {
        if (!(File.Exists(path))) {
            Debug.Log("BVH file not found");
            return null;
        }
        return GetBVHDataFromFile(path);

    }


    private static Animation GetBVHDataFromFile(string path)
    {
        bool flagMotionDataBegin = false;
        Animation anim = new Animation("BVHFile");

        // Read file
        List<string> linedContent = new List<string>();
        using (StreamReader streamReader = new StreamReader(path))
        {
            //readContents = streamReader.ReadToEnd();
            string fileline;
            while ((fileline = streamReader.ReadLine()) != null)
            {
                linedContent.Add(fileline);
            }
            
        }

        Debug.Log(linedContent);
        

        int numLines = linedContent.Count;
        string line;

        // Goes through file string line by line
        int lineCount = 0;
        while (lineCount < numLines)
        {
            line = linedContent[lineCount];

            // During header enter here
            if (!flagMotionDataBegin)
            {
                if (line.Contains("ROOT"))
                {
                    lineCount = RegisterJoints(linedContent, lineCount, null, anim);
                    flagMotionDataBegin = true;
                }
                
            }
            else
            {
                if (line.Contains("MOTION"))
                {
                    
                    string[] aux_line = linedContent[lineCount + 1].Split('\t', System.StringSplitOptions.RemoveEmptyEntries);
                    // Check for different characters - blank spaces or \t's
                    if (aux_line.Length == 1) aux_line = linedContent[lineCount + 1].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                    anim.frames = System.Convert.ToInt32(aux_line[1]);
                    aux_line = linedContent[lineCount + 2].Split('\t', System.StringSplitOptions.RemoveEmptyEntries);
                    // Again check for different characters - blank spaces or \t's
                    if (aux_line.Length == 1)
                    {
                        aux_line = linedContent[lineCount + 2].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                        anim.fps = 1 / System.Convert.ToSingle(aux_line[2], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                        anim.fps = 1 / System.Convert.ToSingle(aux_line[1], System.Globalization.CultureInfo.InvariantCulture);
                    lineCount += 3;
                }
                RegisterFrame(linedContent, lineCount, anim);
            }

            lineCount += 1;
        }
        
        Debug.Log("Got data from BVH");
        

        return anim;
    }

    private static void RegisterFrame(List<string> content, int lineCount, Animation anim)
    {
        //TODO: CHECK THIS: https://gamedev.stackexchange.com/questions/140579/euler-right-handed-to-quaternion-left-handed-conversion-in-unity
        //TODO: CURRENTLY ONLY CONSIDERING THE CASE Xposition Yposition Zposition Zrotation Xrotation Yrotation
        string[] line = content[lineCount].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        int item = 0;
        foreach (Joint joint in anim.GetJoints())
        {
            joint.appendLocalTranslation(new Vector3(System.Convert.ToSingle(line[item], System.Globalization.CultureInfo.InvariantCulture), 
                                                     System.Convert.ToSingle(line[item + 1], System.Globalization.CultureInfo.InvariantCulture), 
                                                     System.Convert.ToSingle(line[item + 2], System.Globalization.CultureInfo.InvariantCulture)));
            joint.appendLocalRotation(new Vector3(System.Convert.ToSingle(line[item + 4], System.Globalization.CultureInfo.InvariantCulture),
                                                  System.Convert.ToSingle(line[item + 5], System.Globalization.CultureInfo.InvariantCulture),
                                                  System.Convert.ToSingle(line[item + 3], System.Globalization.CultureInfo.InvariantCulture)));
            item += 6;
        }
    }

    private static int RegisterJoints(List<string> content, int lineCount, Joint parent, Animation anim){
        string name = content[lineCount].Split(' ', System.StringSplitOptions.RemoveEmptyEntries)[1];
        int depth = (content[lineCount + 1].Split(' ').Length - 1)/2;
        Joint joint = new Joint(name, anim);
        joint.depth = depth;
        string[] aux_line = content[lineCount + 2].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        joint.setOffset(new Vector3(System.Convert.ToSingle(aux_line[1], System.Globalization.CultureInfo.InvariantCulture),
                                    System.Convert.ToSingle(aux_line[2], System.Globalization.CultureInfo.InvariantCulture),
                                    System.Convert.ToSingle(aux_line[3], System.Globalization.CultureInfo.InvariantCulture)));
        aux_line = content[lineCount + 3].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        joint.setChannels(System.Convert.ToInt32(aux_line[1]), aux_line[2], aux_line[3], aux_line[4], aux_line[5], aux_line[6], aux_line[7]);

        if (parent is null)
            anim.setRoot(joint);
        else
        {
            joint.parent = parent;
            parent.children.Add(joint);
        }

        lineCount += 3;
        bool done = false;

        while (!done)
        {
            lineCount += 1;
            //Goes line by line looking for the joint's closure
            if (content[lineCount].Contains("}"))
            {
                done = true;
            }
            else if (content[lineCount].Contains("JOINT"))
            {
                lineCount = RegisterJoints(content, lineCount, joint, anim);
            }
            else if (content[lineCount].Contains("End Site"))
            {
                //Debug.Log(name + " endsite registered");
                aux_line = content[lineCount + 2].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                joint.setEndSite(new Vector3(System.Convert.ToSingle(aux_line[1]), System.Convert.ToSingle(aux_line[2]), System.Convert.ToSingle(aux_line[3])));
                lineCount += 3;
            }
            
        }

        return lineCount;
    }

}


