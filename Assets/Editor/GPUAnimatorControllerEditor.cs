using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GPUAnimatiorController))]
public class GPUAnimatorControllerEditor : Editor
{
    GPUAnimatiorController controller;
    bool showAnimtions = true;

    private void OnEnable()
    {
        controller = (GPUAnimatiorController)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        showAnimtions = EditorGUILayout.Foldout(showAnimtions, "动画");
        if(showAnimtions)
        {
            for (int i = 0; i < controller.animInfos.Length; i++)
            {
                var animInfo = controller.animInfos[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(animInfo.animName);
                if (GUILayout.Button("播放"))
                {
                    controller.Play(animInfo.animName);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("speed");
        float speed = EditorGUILayout.FloatField(controller.speed);
        controller.speed = speed;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}
