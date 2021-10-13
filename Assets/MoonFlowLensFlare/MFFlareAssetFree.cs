using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

[Serializable]
public class MFFlareAssetFree : ScriptableObject
{
    public Texture2D flareSprite;
    public bool fadeWithScale;
    public bool fadeWithAlpha;
    public List<MFFlareSpriteData> spriteBlocks;
}