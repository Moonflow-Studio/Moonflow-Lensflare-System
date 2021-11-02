
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MFFlareAssetSlicer))]
public class MFFlareFreeAssetEditor : Editor
{
    private MFFlareAssetSlicer _targetAssetSlicer;
    private MFFlareCellAssetEditor _ins;
    private List<bool> _tablelist;
    private Texture2D _tmp;
    private Sprite[] _tmpSprites;
    
    // [MenuItem("Assets/Create/MFLensflare/Create MFFlareData split by SpriteEditor")]
    // static void CreateFlareDataFree()
    // {
    //     string path = "";
    //     UnityEngine.Object obj = Selection.activeObject;
    //     path = obj == null ? "Assets" : AssetDatabase.GetAssetPath(obj.GetInstanceID());
    //
    //     ScriptableObject flareData = CreateInstance<MFFlareAssetFree>();
    //     string t = path + "//" + "FlareBySpriteEditor.asset";
    //     if (!Directory.Exists(t)) 
    //     {
    //         Debug.Log("Create Asset " + t);
    //         AssetDatabase.CreateAsset(flareData, t);
    //     }
    //     else
    //     {
    //         LoopCreateFlareAssetFree(1, path);
    //         return;
    //     }
    //     AssetDatabase.Refresh();
    // }
    //
    // static void LoopCreateFlareAssetFree(int serial, string path)
    // {
    //     string t = path + "//" + "FlareBySpriteEditor("+serial+").asset";
    //     Debug.Log("Create Asset " + t);
    //     ScriptableObject flareData = CreateInstance<MFFlareAssetFree>();
    //     if (!Directory.Exists(t)) 
    //     {
    //         AssetDatabase.CreateAsset(flareData, t);
    //     }
    //     else
    //     {
    //         LoopCreateFlareAssetFree(serial + 1, path);
    //         return;
    //     }
    //     AssetDatabase.Refresh();
    // }

    private void Awake()
    {
        _targetAssetSlicer = target as MFFlareAssetSlicer;
        _tablelist = new List<bool>();
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Save"))
        {
            AssetDatabase.SaveAssets();
        }
        EditorGUI.BeginChangeCheck();
        if(_tablelist!=null)_tablelist.Clear();
        _targetAssetSlicer.fadeWithScale = EditorGUILayout.Toggle("Fade With Scale", _targetAssetSlicer.fadeWithScale);
        _targetAssetSlicer.fadeWithAlpha = EditorGUILayout.Toggle("Fade With Alpha", _targetAssetSlicer.fadeWithAlpha);
        _targetAssetSlicer.flareSprite = (Texture2D)EditorGUILayout.ObjectField("Texture", _targetAssetSlicer.flareSprite, typeof(Texture2D),true);
        if (_targetAssetSlicer.flareSprite!=null)
        {
            if (!_targetAssetSlicer.flareSprite.Equals(_tmp))
            {
                _tmp = _targetAssetSlicer.flareSprite;
                var guid = AssetDatabase.FindAssets(_tmp.name)[0];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var targetLoader = AssetDatabase.LoadAllAssetsAtPath(path);
                _tmpSprites = new Sprite[targetLoader.Length - 1];
                for (int i = 1; i < targetLoader.Length; i++)
                {
                    _tmpSprites[i-1] = targetLoader[i] as Sprite;
                }
            }
        }
        else
        {
            _tmp = null;
            _tmpSprites = null;
        }

        if (_tmpSprites == null) return;
        Vector2 wh = new Vector2(_targetAssetSlicer.flareSprite.width, _targetAssetSlicer.flareSprite.height);
        for (int i = 0; i < Mathf.Ceil((float)_tmpSprites.Length / 5); i++)
        {
            using (new EditorGUILayout.HorizontalScope(new GUIStyle(){fixedHeight = 60,stretchHeight = false,fixedWidth = 320, stretchWidth = false}))
            {
                Rect t = EditorGUILayout.BeginHorizontal(); 
                for (int j = 5 * i; j < 5 * i + 5; j++)
                {
                    if (j < _tmpSprites.Length)
                    {
                        _tablelist.Add(GUILayout.Button((j + 1).ToString(), new[] {GUILayout.Height(60), GUILayout.Width(60)}));
                        Rect r = new Rect(_tmpSprites[j].rect.x / wh.x, _tmpSprites[j].rect.y/wh.y, _tmpSprites[j].rect.width/wh.x, _tmpSprites[j].rect.height / wh.y);
                        GUI.DrawTextureWithTexCoords(
                            new Rect(t.position + new Vector2(63 * (j - 5 * i)+1,1/*60 * (1- r.height/r.width)*/),
                                new Vector2(58, 58)), 
                            _targetAssetSlicer.flareSprite, 
                            r);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.Space(30);
        for (int i = 0; i < _tablelist.Count; i++)
        {
            if (_tablelist[i])
            {
                _targetAssetSlicer.spriteBlocks.Add(new MFFlareSpriteData()
                {
                    useLightColor = 0,
                    useRotation = false,
                    index = i,
                    block = _tmpSprites[i].rect,
                    scale = 1,
                    offset = 0,
                    color = Color.white
                });
            }
        }

        if (_tmpSprites == null || _tmpSprites.Length == 0) return;
        for (int i = 0; i < _targetAssetSlicer.spriteBlocks.Count;)
        {
            EditorGUILayout.Space(5);
            MFFlareSpriteData data = _targetAssetSlicer.spriteBlocks[i];
            Rect t = EditorGUILayout.BeginHorizontal(); 
            EditorGUILayout.LabelField(" ",new[] {GUILayout.Height(60), GUILayout.Width(60)});
            EditorGUILayout.BeginVertical();
            data.index = Mathf.Clamp(data.index, 0, _tmpSprites.Length-1);
            data.block = new Rect(_tmpSprites[data.index].rect.x / wh.x, _tmpSprites[data.index].rect.y/wh.y, _tmpSprites[data.index].rect.width/wh.x, _tmpSprites[data.index].rect.height / wh.y);
            GUI.DrawTextureWithTexCoords(
                new Rect(t.position,
                    new Vector2(60,60)), 
                _targetAssetSlicer.flareSprite, 
                data.block);
            data.index = EditorGUILayout.IntSlider("Index", data.index, 0, _tmpSprites.Length-1);
            data.useRotation = EditorGUILayout.Toggle("Rotation", data.useRotation);
            data.useLightColor = EditorGUILayout.Slider("LightColor", data.useLightColor, 0, 1);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            data.offset = EditorGUILayout.Slider("Offset",data.offset, -1.5f, 1f);
            data.color = EditorGUILayout.ColorField("Color", data.color);
            data.scale = EditorGUILayout.FloatField("Scale", data.scale);

            if (GUILayout.Button("Remove"))
            {
                _targetAssetSlicer.spriteBlocks.RemoveAt(i);
            }
            else
            {
                _targetAssetSlicer.spriteBlocks[i] = data;
                i++;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_targetAssetSlicer);
        }
        Undo.RecordObject(_targetAssetSlicer, "Change Flare Asset Data");
    }
}
