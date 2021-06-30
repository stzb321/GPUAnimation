using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.EditorCoroutines.Editor;

public class GenGPUVertexAnimaton
{
    private static float SampleFactor = 1 / 60f;
    [MenuItem("Assets/Gen")]
    public static void Gen()
    {
        var selector = UnityEditor.Selection.activeObject;
        if(selector == null)
        {
            return;
        }

        EditorCoroutineUtility.StartCoroutineOwnerless(GenThings(selector));
    }

    static IEnumerator GenThings(Object selector)
    {
        var path = AssetDatabase.GetAssetPath(selector);
        var dir = "Assets/Prefab";
        var originPrefab = (GameObject)GameObject.Instantiate(selector);
        originPrefab.name = selector.name;
        var animator = originPrefab.GetComponent<Animator>();
        if(animator == null)
        {
            GameObject.DestroyImmediate(originPrefab);
            Debug.LogError("selection prefab does not have Animator component!");
            yield break;
        }

        
        var skin = originPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
        var modelMesh = new Mesh();
        skin.BakeMesh(modelMesh);
        var uvs = new Vector2[modelMesh.vertices.Length];
        for (int k = 0; k < modelMesh.vertices.Length; k++)
        {
            uvs[k] = new Vector2(k, 0.0f);
        }
        modelMesh.uv3 = uvs;

        var folderName = "GPU";
        var parentFolder = Path.Combine(dir, folderName);   //   Assets/Prefab/GPU
        var subFolder = Path.Combine(parentFolder, originPrefab.name);  //   Assets/Prefab/GPU/name

        if (Directory.Exists(subFolder))
        {
            FileUtil.DeleteFileOrDirectory(subFolder);
            AssetDatabase.Refresh();
        }

        AssetDatabase.CreateFolder(parentFolder, originPrefab.name);
        var savePath = Path.Combine(subFolder, string.Format("{0}_mesh.asset", originPrefab.name));

        AssetDatabase.CreateAsset(modelMesh, savePath);  //保存mesh

        var vCount = skin.sharedMesh.vertexCount;
        var texWidth = vCount;
        var totalFrame = 0;
        UnityEditor.Animations.AnimatorController animatorController = (UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;
        UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            AnimationClip clip = stateMachine.states[i].state.motion as AnimationClip;
            totalFrame += (int)(clip.length / SampleFactor) + 1;
        }

        var mesh = new Mesh();
        var defaultState = stateMachine.defaultState;
        Texture2D posTex = new Texture2D(texWidth, totalFrame, TextureFormat.RGBAHalf, false);
        posTex.wrapMode = TextureWrapMode.Clamp;
        posTex.filterMode = FilterMode.Point;
        animator.speed = 0;  //因为靠代码手动Update，所以speed设置为0.
        float boundMin = 0;
        float boundMax = 0;
        int frameOffset = 0;
        List<AnimInfo> animInfos = new List<AnimInfo>();
        List<List<Color>> pixels = new List<List<Color>>();
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            UnityEditor.Animations.AnimatorState state = stateMachine.states[i].state;
            AnimationClip clip = state.motion as AnimationClip;
            var thisClipFrames = (int)(clip.length / SampleFactor) + 1;
            animator.Play(state.name);
            for (int j = 0; j < thisClipFrames; j++)
            {
                List<Color> pos = new List<Color>();
                animator.Play(state.name, 0, (float)j / thisClipFrames);
                animator.Update(Time.deltaTime);
                yield return null;
                skin.BakeMesh(mesh);

                for (int z = 0; z < mesh.vertexCount; z++)
                {
                    Vector3 vertex = mesh.vertices[z];
                    Color col = new Color(vertex.x, vertex.y, vertex.z);
                    pos.Add(col);
                    boundMin = Mathf.Min(boundMin, vertex.x, vertex.y, vertex.z);
                    boundMax = Mathf.Max(boundMax, vertex.x, vertex.y, vertex.z);
                }
                pixels.Add(pos);
            }

            AnimInfo info = new AnimInfo();
            info.animFrameOffset = frameOffset;
            info.totalFrames = totalFrame;
            info.animFrameNum = thisClipFrames;
            info.m_boundMax = boundMax;
            info.m_boundMin = boundMin;
            info.animLen = clip.length;
            info.animName = state.name;
            info.loop = clip.isLooping;
            info.isDefault = (state == defaultState);
            animInfos.Add(info);

            frameOffset += thisClipFrames;
        }

        var diff = boundMax - boundMin;
        for (int i = 0; i < pixels.Count; i++)
        {
            var item = pixels[i];
            for (int j = 0; j < item.Count; j++)
            {
                var pixel = item[j];
                pixel.r = (pixel.r - boundMin) / diff;
                pixel.g = (pixel.g - boundMin) / diff;
                pixel.b = (pixel.b - boundMin) / diff;
                posTex.SetPixel(j, i, pixel);
            }
        }

        posTex.Apply();
        AssetDatabase.CreateAsset(posTex, Path.Combine(subFolder,  string.Format("{0}_pos.asset", originPrefab.name)));  //保存位置纹理
        File.WriteAllBytes(Path.Combine(subFolder, string.Format("{0}_pos.png", originPrefab.name)), posTex.EncodeToPNG());

        Shader shader = Shader.Find("Custom/GPUAnimation");
        Material mat = new Material(shader);
        for (int i = 0; i < animInfos.Count; i++)
        {
            if(animInfos[i].isDefault)
            {
                mat.SetFloat("_BoundMax", animInfos[i].m_boundMax);
                mat.SetFloat("_BoundMin", animInfos[i].m_boundMin);
                mat.SetTexture("_PosTex", posTex);
            }
        }
        mat.enableInstancing = true;
        AssetDatabase.CreateAsset(mat, Path.Combine(subFolder, string.Format("{0}_mat.mat", originPrefab.name)));  //保存材质


        GameObject newPrefab = new GameObject(originPrefab.name);
        GameObject model = new GameObject("model");
        model.transform.parent = newPrefab.transform;
        model.transform.localScale = Vector3.one;
        model.transform.localPosition = Vector3.zero;
        model.transform.localEulerAngles = Vector3.zero;
        model.AddComponent<MeshFilter>().sharedMesh = modelMesh;
        model.AddComponent<MeshRenderer>().sharedMaterial = mat;
        GPUAnimatiorController controller = model.AddComponent<GPUAnimatiorController>();
        controller.animInfos = animInfos.ToArray();

        PrefabUtility.SaveAsPrefabAsset(newPrefab, Path.Combine(subFolder, string.Format("{0}_prefab.prefab", originPrefab.name)));

        GameObject.DestroyImmediate(originPrefab);
        GameObject.DestroyImmediate(newPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
