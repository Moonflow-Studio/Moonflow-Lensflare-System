using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


[Serializable]
public struct SpriteData
{
   public bool useLightColor;
   public bool useRotation;
   public int index;
   public Rect block;
   public float scale;
   [Range(-1.5f,1)]public float offset;
   [ColorUsage(true,true)]public Color color;
}
public enum FlareTexModel
{
   _2x2 = 0,
   _4x4 = 1,
   _Mega = 2,
   _1L4S = 3,
   _1L2M8S = 4,
}
[Serializable]
public class URPFlareAsset : ScriptableObject
{
   public Texture2D flareSprite;
   public bool fadeWithScale;
   public bool fadeWithAlpha;
   public List<SpriteData> spriteBlocks;
   public FlareTexModel flareTexModel;
   
   [MenuItem("Assets/Create URPFlareData")]
   static void CreateFlareData()
   {
      string path = "";
      Object obj = Selection.activeObject;
      path = obj == null ? "Assets" : AssetDatabase.GetAssetPath(obj.GetInstanceID());

      ScriptableObject flareData = CreateInstance<URPFlareAsset>();
      string t = path + "//" + "Flare.asset";
      if (!Directory.Exists(t)) 
      {
         Debug.Log("Create Asset " + t);
         AssetDatabase.CreateAsset(flareData, t);
      }
      else
      {
         LoopCreateFlareAsset(1, path);
         return;
      }
      AssetDatabase.Refresh();
   }
   
   static void LoopCreateFlareAsset(int serial, string path)
   {
      string t = path + "//" + "Flare("+serial+").asset";
      Debug.Log("Create Asset " + t);
      ScriptableObject flareData = CreateInstance<URPFlareAsset>();
      if (!Directory.Exists(t)) 
      {
         AssetDatabase.CreateAsset(flareData, t);
      }
      else
      {
         LoopCreateFlareAsset(serial + 1, path);
         return;
      }
      AssetDatabase.Refresh();
   }
   
}
