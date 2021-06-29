using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GpuAnimatiorController : MonoBehaviour
{
    public AnimInfo[] animInfos;
    public float speed = 1.0f;

    AnimInfo _curAnimInfo;
    Dictionary<string, AnimInfo> animInfoDict;
    Material mat;
    float _playTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        float t = (_playTime % _curAnimInfo.animLen) / _curAnimInfo.animLen;
        float index = (t * _curAnimInfo.animFrameNum + _curAnimInfo.animFrameOffset);
        float curFrameIndex = (index / _curAnimInfo.totalFrames);
        _playTime += (Time.deltaTime * speed);

        mat.SetFloat("_CurFrameIndex", curFrameIndex);
    }

    public void Play(string animName)
    {
        if(_curAnimInfo != null && _curAnimInfo.animName == animName)
        {
            return;
        }

        AnimInfo animInfo = GetAnimInfo(animName);
        if(animInfo == null)
        {
            return;
        }

        _curAnimInfo = animInfo;
        mat.SetFloat("_BoundMax", animInfo.m_boundMax);
        mat.SetFloat("_BoundMin", animInfo.m_boundMin);
        _playTime = 0;
    }

    AnimInfo GetAnimInfo(string animName)
    {
        if(animInfoDict == null)
        {
            animInfoDict = new Dictionary<string, AnimInfo>();
            for (int i = 0; i < animInfos.Length; i++)
            {
                animInfoDict.Add(animInfos[i].animName, animInfos[i]);
            }
        }

        AnimInfo animInfo = null;
        animInfoDict.TryGetValue(animName, out animInfo);
        return animInfo;
    }
}
