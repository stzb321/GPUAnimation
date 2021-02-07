using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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

        GenThings(selector);
    }

    public static void GenThings(Object selector)
    {
        var path = AssetDatabase.GetAssetPath(selector);
        var dir = Path.GetDirectoryName(path);
        var newPrefab = (GameObject)GameObject.Instantiate(selector);
        var animator = newPrefab.GetComponent<Animator>();
        if(animator == null)
        {
            GameObject.DestroyImmediate(newPrefab);
            Debug.LogError("selection prefab does not have Animator compoment!");
            return;
        }

        var skin = newPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
        var modelMesh = new Mesh();
        skin.BakeMesh(modelMesh);
        var parentFolder = Path.Combine(dir, "GPU");
        var subFolder = AssetDatabase.CreateFolder(parentFolder, newPrefab.name);
        var savePath = Path.Combine(subFolder, string.Format("{0}_mesh.asset", newPrefab.name));

        AssetDatabase.CreateAsset(modelMesh, savePath);

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

        Texture2D texture = new Texture2D(texWidth, totalFrame, TextureFormat.RGB24, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        animator.speed = 0;  //因为靠代码手动Update，所以speed设置为0.
        float boundMin = 0;
        float boundMax = 0;
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
                skin.BakeMesh(modelMesh);

                for (int z = 0; z < modelMesh.vertexCount; z++)
                {
                    Vector3 vertex = modelMesh.vertices[z];
                    Color col = new Color(vertex.x, vertex.y, vertex.z);
                    pos.Add(col);
                    boundMin = Mathf.Min(boundMin, vertex.x, vertex.y, vertex.z);
                    boundMax = Mathf.Max(boundMax, vertex.x, vertex.y, vertex.z);
                }
                pixels.Add(pos);
            }
            
        }

        GameObject.DestroyImmediate(newPrefab);
        AssetDatabase.Refresh();
    }
}
