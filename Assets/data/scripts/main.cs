using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Aqui talvez tenha uma mudança de perspectiva:
// Eu acredito que preciso (sempre que possível) trabalhar em cima dos dados contidos na Unity - dentro do *3D*
// Razões a favor disso:
// 1. Só assim vou poder me aproveitar de métodos tipo .GetPosition() do que estou observando no 3D
// 2. O modelo/esqueleto/dados target (tgt) estão presentes no 3D e NÃO estão disponíveis no sistema de coordenadas do BVH, portanto vou ter que "traduzir" eles pro sistemas do BVH e aí conseguir calcular tudo
// 3. Todo update e cálculo que eu fizer no sistema do BVH terei que traduzir para o sistema do Unity, seja dos dados de source quanto dos dados de target.
//
// Como posso fazer isso:
// 1. Na classe Joint posso adicionar uma referência para o GameObject criado para representar essa junta;
//  1.1 dessa forma, o SkeletonMap deveria retornar referências para o GameObject da junta ao invés do Joint (questionável?)
//  OU
//  1.2 SkeletonMap continuaria a retornar referências de Joint mas toda vez eu utilizo a referência ao GameObject, tanto do target quanto do source
//  1.2.1 O GameObject iria se mover de maneira "independente" ao longo dos frames 
//
//  Razões a favor disso:
//      - Pouquíssima alteração no código ou na metodologia. Basta eu trocar joint.method() por joint.Object.method()
//      - 
//
// 2. (Jeito maluco - exercício) Não utilizar nada da classe Joint
//  - Acho que não é viável pois eu perco a camada de abstração que consegue "caminhar" pela estrutura.
//  - Além disso, eu perco a "base" que já está criada para que, no futuro, eu faça a tradução Unity-BVH e tenha possibilidade de salvar o BVH.
//
//
// Observações aleatórias:
// - Toda hora estou pensando que eu quero usar o draw_skeleton para um modelo (target) mas acho que isso não deveria acontecer;
//   não tem razão para fazer isso, o modelo está ali já com os GameObjects criados e nas posições corretas, é possível imprimir tudo que eu quero a partir do modelo.
//   O que faz muito mais sentido do que criar um esqueleto separado para representar passos intermediários do processo.
// - O draw_skeleton, para targets, deve funcionar como um adendo/feature e não como a referência principal dos cálculos.
// - O draw_skeleton, para o source, É, de fato, o BVH. É o BVH traduzido para o Unity e com ele devo trabalhar. NÃO SE ESQUECER.


public class main : MonoBehaviour
{

    public GameObject talitaModel;

    // Start is called before the first frame update
    void Start()
    {

        // Check if a model was assigned as the target to motion retargeting
        Animation anim_talita;
        if (talitaModel == null)
        {
            Debug.Log("No Talita model assigned");
            return;
        }

        // Reads a BVH to be used as source to motion retargeting
        Animation anim;
        anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/Mao_na_frente_mcp.bvh");
        //anim = bvh.ReadBVHFile(Application.dataPath + "/data/input/RArmRotTest.bvh");


        // Draw is responsible to "translate" the BVH coordinate system to Unity's system
        // Creates a skeleton that represents the source animation (the BVH file)
        GameObject objSrcSkeleton = new GameObject("Source Skeleton");
        objSrcSkeleton.transform.position = Vector3.zero;
        draw_skeleton drawSrcSkeleton = objSrcSkeleton.AddComponent<draw_skeleton>();
        drawSrcSkeleton.Draw(anim, objSrcSkeleton);

        // Creates the mapping of the source skeleton
        //GameObject objMappedSrcSkeleton = new GameObject("Mapped Source Skeleton");
        SkeletonMap mapSrcSkeleton = objSrcSkeleton.AddComponent<SkeletonMap>();
        mapSrcSkeleton.anim = anim;
        mapSrcSkeleton.SetSkeletonModel("Vicon");

        // Reads the surface data of the source animation
        Surface srcSurface = objSrcSkeleton.AddComponent<Surface>();
        srcSurface.ReadSourceSurfaceFile(Application.dataPath + "/data/surface/mocap_surface_rodolfo.csv");
        srcSurface.DrawSurface();


        // Instantiate the model as an Animation object ("reads" the model/character)
        anim_talita = model.GenerateFromModel("Talita", talitaModel);
        // Creates the mapping of the target skeleton
        SkeletonMap mapTalita = talitaModel.AddComponent<SkeletonMap>();
        mapTalita.anim = anim_talita;
        mapTalita.SetSkeletonModel("Talita");
        // Adds the retargeting component
        retargeting mrTalita = talitaModel.AddComponent<retargeting>();



        //StartCoroutine(DrawWaiting(drawSrcSkeleton, mrTalita, mapSrcSkeleton, mapTalita, 0.01f));
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
