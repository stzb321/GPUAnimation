using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUAnimatiorController : MonoBehaviour
{
    public AnimInfo[] animInfos;
    public float speed = 1.0f;

    MaterialPropertyBlock materialPropertyBlock;
    MeshRenderer meshRenderer;
    AnimInfo _curAnimInfo;
    Dictionary<string, AnimInfo> animInfoDict;
    float _playTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(materialPropertyBlock, 0);
        SetDefaultAnimInfo();
    }

    // Update is called once per frame
    void Update()
    {
        float t = (_playTime % _curAnimInfo.animLen) / _curAnimInfo.animLen;
        float index = (t * _curAnimInfo.animFrameNum + _curAnimInfo.animFrameOffset);
        float curFrameIndex = (index / _curAnimInfo.totalFrames);
        _playTime += (Time.deltaTime * speed);

        materialPropertyBlock.SetFloat("_CurFrameIndex", curFrameIndex);
        meshRenderer.SetPropertyBlock(materialPropertyBlock, 0);
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
        materialPropertyBlock.SetFloat("_BoundMax", animInfo.m_boundMax);
        materialPropertyBlock.SetFloat("_BoundMin", animInfo.m_boundMin);
        meshRenderer.SetPropertyBlock(materialPropertyBlock, 0);
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

    void SetDefaultAnimInfo()
    {
        for (int i = 0; i < animInfos.Length; i++)
        {
            if(animInfos[i].isDefault)
            {
                _curAnimInfo = animInfos[i];
                break;
            }
        }
    }
}
