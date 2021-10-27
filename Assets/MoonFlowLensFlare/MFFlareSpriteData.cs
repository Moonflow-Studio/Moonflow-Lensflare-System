using System;
using UnityEngine;


[Serializable]
public struct MFFlareSpriteData
{
    public float useLightColor;
    public bool useRotation;
    public int index;
    public Rect block;
    public float scale;
    [Range(-1.5f,1)]public float offset;
    [ColorUsage(true,true)]public Color color;
}
