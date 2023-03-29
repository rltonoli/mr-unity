using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Aqui talvez tenha uma mudan�a de perspectiva:
// Eu acredito que preciso (sempre que poss�vel) trabalhar em cima dos dados contidos na Unity - dentro do *3D*
// Raz�es a favor disso:
// 1. S� assim vou poder me aproveitar de m�todos tipo .GetPosition() do que estou observando no 3D
// 2. O modelo/esqueleto/dados target (tgt) est�o presentes no 3D e N�O est�o dispon�veis no sistema de coordenadas do BVH, portanto vou ter que "traduzir" eles pro sistemas do BVH e a� conseguir calcular tudo
// 3. Todo update e c�lculo que eu fizer no sistema do BVH terei que traduzir para o sistema do Unity, seja dos dados de source quanto dos dados de target.
//
// Como posso fazer isso:
// 1. Na classe Joint posso adicionar uma refer�ncia para o GameObject criado para representar essa junta;
//  1.1 dessa forma, o SkeletonMap deveria retornar refer�ncias para o GameObject da junta ao inv�s do Joint (question�vel?)
//  OU
//  1.2 SkeletonMap continuaria a retornar refer�ncias de Joint mas toda vez eu utilizo a refer�ncia ao GameObject, tanto do target quanto do source
//  1.2.1 O GameObject iria se mover de maneira "independente" ao longo dos frames 
//
//  Raz�es a favor disso:
//      - Pouqu�ssima altera��o no c�digo ou na metodologia. Basta eu trocar joint.method() por joint.Object.method()
//      - 
//
// 2. (Jeito maluco - exerc�cio) N�o utilizar nada da classe Joint
//  - Acho que n�o � vi�vel pois eu perco a camada de abstra��o que consegue "caminhar" pela estrutura.
//  - Al�m disso, eu perco a "base" que j� est� criada para que, no futuro, eu fa�a a tradu��o Unity-BVH e tenha possibilidade de salvar o BVH.
//
//
// Observa��es aleat�rias:
// - Toda hora estou pensando que eu quero usar o draw_skeleton para um modelo (target) mas acho que isso n�o deveria acontecer;
//   n�o tem raz�o para fazer isso, o modelo est� ali j� com os GameObjects criados e nas posi��es corretas, � poss�vel imprimir tudo que eu quero a partir do modelo.
//   O que faz muito mais sentido do que criar um esqueleto separado para representar passos intermedi�rios do processo.
// - O draw_skeleton, para targets, deve funcionar como um adendo/feature e n�o como a refer�ncia principal dos c�lculos.
// - O draw_skeleton, para o source, �, de fato, o BVH. � o BVH traduzido para o Unity e com ele devo trabalhar. N�O SE ESQUECER.


public class main : MonoBehaviour
{

    public GameObject talitaModel;

    // Start is called before the first frame update
    void Start()
    {
        Animation anim;
        anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/Mao_na_frente_mcp.bvh");
        //anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/RArmRotTest.bvh");
        GameObject objMappedSrcSkeleton = new GameObject("Mapped Source Skeleton");
        SkeletonMap mapSrcSkeleton = objMappedSrcSkeleton.AddComponent<SkeletonMap>();
        mapSrcSkeleton.anim = anim;
        mapSrcSkeleton.SetSkeletonModel("Vicon");

        // Draw is responsible to "translate" the BVH coordinate system to Unity's system
        GameObject objSrcSkeleton = new GameObject("Source Skeleton");
        draw_skeleton drawSrcSkeleton = objSrcSkeleton.AddComponent<draw_skeleton>();
        drawSrcSkeleton.Draw(anim);


        Animation anim_talita;
        if (talitaModel == null)
        {
            Debug.Log("No Talita model assigned");
            return;
        }
        
        anim_talita = model.GenerateFromModel("Talita", talitaModel);
        SkeletonMap mapTalita = talitaModel.AddComponent<SkeletonMap>();
        mapTalita.anim = anim_talita;
        mapTalita.SetSkeletonModel("Talita");
        retargeting mrTalita = talitaModel.AddComponent<retargeting>();
        //mrTalita.BoneRetargeting(mapSrcSkeleton, mapTalita);

        // draw_skeleton srcSkeleton = new draw_skeleton(anim);

        anim.printHierarchy();

        StartCoroutine(DrawWaiting(drawSrcSkeleton, mrTalita, mapSrcSkeleton, mapTalita, 0.01f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator DrawWaiting(draw_skeleton skel, retargeting mr_talita, SkeletonMap mapSrcSkeleton, SkeletonMap mapTalita, float wait)
    {
        for (int i = 0; i < skel.anim.frames; i++)
        {
            Debug.Log(i);
            skel.DrawFrame(i);
            mr_talita.WorldRotRetargeting(mapSrcSkeleton, mapTalita, i);
            yield return new WaitForSeconds(wait);
        }
    }

}
