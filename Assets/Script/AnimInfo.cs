using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimInfo
{
    public bool isDefault = false;
    public string animName;
    public float m_boundMin = 0;
    public float m_boundMax = 0;
    public Texture2D m_posTex;
    public bool loop;
    public float animLen;
    public string nextAnimName;
    public int totalFrames = 0;
    public int animFrameNum = 0;
    public int animFrameOffset = 0;
}
