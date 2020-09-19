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
[CreateAssetMenu(fileName = "FlareAsset", menuName = "Create FlareAsset split by model")]
[Serializable]
public class URPFlareAsset : ScriptableObject
{
   [SerializeField]public Texture2D flareSprite;
   [SerializeField]public bool fadeWithScale;
   [SerializeField]public bool fadeWithAlpha;
   [SerializeField]public List<SpriteData> spriteBlocks;
   [SerializeField]public FlareTexModel flareTexModel;
}


/*
[Serializable]
public class URPFlareAssetFree : ScriptableObject
{
   public Texture2D flareSprite;
   public bool fadeWithScale;
   public bool fadeWithAlpha;
   public List<SpriteData> spriteBlocks;
   public FlareTexModel flareTexModel;
   
   [MenuItem("Assets/Create URPFlareData split by SpriteEditor")]
   static void CreateFlareDataFree()
   {
      string path = "";
      Object obj = Selection.activeObject;
      path = obj == null ? "Assets" : AssetDatabase.GetAssetPath(obj.GetInstanceID());

      ScriptableObject flareData = CreateInstance<URPFlareAssetFree>();
      string t = path + "//" + "FlareBySpriteEditor.asset";
      if (!Directory.Exists(t)) 
      {
         Debug.Log("Create Asset " + t);
         AssetDatabase.CreateAsset(flareData, t);
      }
      else
      {
         LoopCreateFlareAssetFree(1, path);
         return;
      }
      AssetDatabase.Refresh();
   }
   
   static void LoopCreateFlareAssetFree(int serial, string path)
   {
      string t = path + "//" + "FlareBySpriteEditor("+serial+").asset";
      Debug.Log("Create Asset " + t);
      ScriptableObject flareData = CreateInstance<URPFlareAssetFree>();
      if (!Directory.Exists(t)) 
      {
         AssetDatabase.CreateAsset(flareData, t);
      }
      else
      {
         LoopCreateFlareAssetFree(serial + 1, path);
         return;
      }
      AssetDatabase.Refresh();
   }
}
*/
