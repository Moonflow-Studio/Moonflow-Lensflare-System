using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(URPFlareAsset))]
public class URPFlareAssetModelEditor : Editor
{
    private URPFlareAsset _targetAsset;
    private URPFlareAssetModelEditor _ins;
    private List<bool> _tablelist;

    private static readonly List<Rect>[] STATIC_FlareRectModel = new[]
    {
        new List<Rect>()    //2x2
        {
            new Rect(0, 0.5f, 0.5f, 0.5f),
            new Rect(0.5f, 0.5f, 0.5f, 0.5f),
            new Rect(0, 0, 0.5f, 0.5f),
            new Rect(0.5f, 0, 0.5f, 0.5f)
        },
        new List<Rect>()    //4x4
        {
            new Rect(0, 0.75f, 0.25f,  0.25f),
            new Rect(0.25f, 0.75f, 0.25f,  0.25f),
            new Rect(0.5f, 0.75f, 0.25f,  0.25f),
            new Rect(0.75f, 0.75f, 0.25f,  0.25f),
            new Rect(0, 0.5f, 0.25f,  0.25f),
            new Rect(0.25f, 0.5f, 0.25f,  0.25f),
            new Rect(0.5f, 0.5f, 0.25f,  0.25f),
            new Rect(0.75f, 0.5f, 0.25f,  0.25f),
            new Rect(0, 0.25f, 0.25f,  0.25f),
            new Rect(0.25f, 0.25f, 0.25f,  0.25f),
            new Rect(0.5f, 0.25f, 0.25f,  0.25f),
            new Rect(0.75f, 0.25f, 0.25f,  0.25f),
            new Rect(0, 0, 0.25f,  0.25f),
            new Rect(0.25f, 0, 0.25f,  0.25f),
            new Rect(0.5f, 0, 0.25f,  0.25f),
            new Rect(0.75f, 0, 0.25f,  0.25f)
        },
        new List<Rect>()    //Mega
        {
            new Rect(0, 0.5f, 0.5f,  0.5f),
            new Rect(0.5f, 0.75f, 0.25f,  0.25f),
            new Rect(0.75f, 0.75f, 0.25f,  0.25f),
            new Rect(0.5f, 0.5f, 0.25f,  0.25f),
            new Rect(0.75f, 0.5f, 0.25f,  0.25f),
            new Rect(0, 0, 0.5f,  0.5f),
            new Rect(0.5f, 0.25f, 0.25f,  0.25f),
            new Rect(0.75f, 0.5f, 0.25f,  0.25f),
            new Rect(0.75f, 0.25f, 0.25f,  0.25f),
            new Rect(0.75f, 0.125f, 0.25f,  0.125f),
            new Rect(0.75f, 0.0625f, 0.25f,  0.0625f),
            new Rect(0.75f, 0.03125f, 0.25f,  0.03125f),
            new Rect(0.75f, 0, 0.25f,  0.03125f)
        },
        new List<Rect>() //1L4S
        {
            new Rect(0, 0.5f, 1,  0.5f),
            new Rect(0, 0.25f, 0.5f,  0.25f),
            new Rect(0.5f, 0.25f, 0.5f,  0.25f),
            new Rect(0, 0, 0.5f,  0.25f),
            new Rect(0.5f, 0, 0.5f,  0.25f)
        }, 
        new List<Rect>() //1L2M8S
        {
            new Rect(0, 0.5f, 1,  0.5f),
            new Rect(0, 0.25f, 0.5f,  0.25f),
            new Rect(0.5f, 0.375f, 0.25f,  0.125f),
            new Rect(0.75f, 0.375f, 0.25f,  0.125f),
            new Rect(0.5f, 0.25f, 0.25f,  0.125f),
            new Rect(0.75f, 0.25f, 0.25f,  0.125f),
            new Rect(0, 0f, 0.5f,  0.25f),
            new Rect(0.5f, 0.125f, 0.25f,  0.125f),
            new Rect(0.75f, 0.125f, 0.25f,  0.125f),
            new Rect(0.5f, 0f, 0.25f,  0.125f),
            new Rect(0.75f, 0f, 0.25f,  0.125f)
        }
    };
    
    // [MenuItem("Assets/Create URPFlareData split by model")]
    // static void CreateFlareDataModel()
    // {
    //     string path = "";
    //     Object obj = Selection.activeObject;
    //     path = obj == null ? "Assets" : AssetDatabase.GetAssetPath(obj.GetInstanceID());
    //
    //     ScriptableObject flareData = CreateInstance<URPFlareAssetModel>();
    //     string t = path + "//" + "FlareByModel.asset";
    //     if (!Directory.Exists(t)) 
    //     {
    //         Debug.Log("Create Asset " + t);
    //         AssetDatabase.CreateAsset(flareData, t);
    //     }
    //     else
    //     {
    //         LoopCreateFlareAssetModel(1, path);
    //         return;
    //     }
    //     AssetDatabase.Refresh();
    // }
    //
    // static void LoopCreateFlareAssetModel(int serial, string path)
    // {
    //     string t = path + "//" + "FlareByModel("+serial+").asset";
    //     Debug.Log("Create Asset " + t);
    //     ScriptableObject flareData = CreateInstance<URPFlareAssetModel>();
    //     if (!Directory.Exists(t)) 
    //     {
    //         AssetDatabase.CreateAsset(flareData, t);
    //     }
    //     else
    //     {
    //         LoopCreateFlareAssetModel(serial + 1, path);
    //         return;
    //     }
    //     AssetDatabase.Refresh();
    // }

    private void Awake()
    {
        _targetAsset = target as URPFlareAsset;
        _tablelist = new List<bool>();
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        // base.OnInspectorGUI();
        _targetAsset.flareSprite = (Texture2D)EditorGUILayout.ObjectField("Texture", _targetAsset.flareSprite, typeof(Texture2D),true);
        // _targetAsset.directionalLight = EditorGUILayout.Toggle("Directional Light", _targetAsset.directionalLight);
        // _targetAsset.useLightIntensity = EditorGUILayout.Toggle("Use Light Intensity", _targetAsset.useLightIntensity);
        _targetAsset.fadeWithScale = EditorGUILayout.Toggle("Fade With Scale", _targetAsset.fadeWithScale);
        _targetAsset.fadeWithAlpha = EditorGUILayout.Toggle("Fade With Alpha", _targetAsset.fadeWithAlpha);
        _targetAsset.flareTexModel = (FlareTexModel)EditorGUILayout.EnumPopup("Texture Layout", _targetAsset.flareTexModel);
        PaintTable();
    
        
        for (int i = 0; i < _tablelist.Count; i++)
        {
            if (_tablelist[i])
            {
                _targetAsset.spriteBlocks.Add(new SpriteData()
                {
                    useLightColor = false,
                    useRotation = false,
                    index = i,
                    block = STATIC_FlareRectModel[(int) _targetAsset.flareTexModel][i],
                    scale = 1,
                    offset = 0,
                    color = Color.white
                });
            }
        }
        
        for (int i = 0; i < _targetAsset.spriteBlocks.Count;)
        {
            EditorGUILayout.Space(5);
            SpriteData data = _targetAsset.spriteBlocks[i];
            Rect t = EditorGUILayout.BeginHorizontal(); 
            EditorGUILayout.LabelField(" ",new[] {GUILayout.Height(60), GUILayout.Width(60)});
            EditorGUILayout.BeginVertical();
            data.index = Mathf.Clamp(data.index, 0, STATIC_FlareRectModel[(int) _targetAsset.flareTexModel].Count-1);
            data.block = STATIC_FlareRectModel[(int) _targetAsset.flareTexModel][data.index];
            GUI.DrawTextureWithTexCoords(new Rect(t.position + new Vector2(0,30 * (1- data.block.height/data.block.width)),new Vector2(60,60 * data.block.height / data.block.width)), _targetAsset.flareSprite, data.block);
            data.index = EditorGUILayout.IntSlider("Index", data.index, 0, STATIC_FlareRectModel[(int) _targetAsset.flareTexModel].Count-1);
            data.useRotation = EditorGUILayout.Toggle("Rotation", data.useRotation);
            data.useLightColor = EditorGUILayout.Toggle("LightColor", data.useLightColor);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            data.offset = EditorGUILayout.Slider("Offset",data.offset, -1.5f, 1f);
            data.color = EditorGUILayout.ColorField("Color", data.color);
            data.scale = EditorGUILayout.FloatField("Scale", data.scale);

            if (GUILayout.Button("Remove"))
            {
                _targetAsset.spriteBlocks.RemoveAt(i);
            }
            else
            {
                _targetAsset.spriteBlocks[i] = data;
                i++;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            AssetDatabase.SaveAssets();
        }
        Undo.RecordObject(_targetAsset, "Change Flare Asset Data");
    }
    public void PaintTable()
    {
        if(_tablelist!=null)_tablelist.Clear();
        if(_targetAsset.spriteBlocks == null)_targetAsset.spriteBlocks = new List<SpriteData>();
        int texIndex = (int) _targetAsset.flareTexModel;
        switch (_targetAsset.flareTexModel)
        {
            case FlareTexModel._2x2:
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < 2; i++)
                {
                    Rect r = (Rect)EditorGUILayout.BeginHorizontal();
                    _tablelist.Add(GUILayout.Button("0" + i + 1, new[] {GUILayout.Height(125), GUILayout.Width(125)}));
                    _tablelist.Add(GUILayout.Button("0" + i + 2, new[] {GUILayout.Height(125), GUILayout.Width(125)}));
                    EditorGUILayout.EndHorizontal();
                    if (_targetAsset.flareSprite)
                    {
                        GUI.DrawTextureWithTexCoords(new Rect(r.position.x , r.position.y, 124, 124), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 2 + 0]);
                        GUI.DrawTextureWithTexCoords(new Rect(r.position.x + 126, r.position.y, 124, 124), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 2 + 1]);
                    }
                }
                EditorGUILayout.EndVertical();
                break;
        
            case FlareTexModel._4x4:
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < 4; i++)
                {
                    Rect r = (Rect)EditorGUILayout.BeginHorizontal();
                    for (int j = 0; j < 4; j++)
                    {
                        _tablelist.Add(GUILayout.Button("0"+ (i + 1) * (j + 1), new[] {GUILayout.Height(60), GUILayout.Width(60)}));
                    }
                    EditorGUILayout.EndHorizontal();
                    if (_targetAsset.flareSprite)
                    {
                        GUI.DrawTextureWithTexCoords(new Rect(r.position.x , r.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 4 + 0]);
                        GUI.DrawTextureWithTexCoords(new Rect(r.position.x + 63, r.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 4 + 1]);
                        GUI.DrawTextureWithTexCoords(new Rect(r.position.x + 125, r.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 4 + 2]);
                        GUI.DrawTextureWithTexCoords(new Rect(r.position.x + 190, r.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 4 + 3]);
                    }
                }
                EditorGUILayout.EndVertical();
                break;
        
            case FlareTexModel._Mega:
                Rect m1 = (Rect)EditorGUILayout.BeginHorizontal();
                _tablelist.Add(GUILayout.Button("1", new[] {GUILayout.Height(125), GUILayout.Width(125)}));
            
                EditorGUILayout.BeginVertical();
                {
                    
                    Rect m3 = (Rect)EditorGUILayout.BeginHorizontal();
                    _tablelist.Add(GUILayout.Button("2", new []{GUILayout.Height(60), GUILayout.Width(60)}));
                    _tablelist.Add(GUILayout.Button("3", new []{GUILayout.Height(60), GUILayout.Width(60)}));
                    EditorGUILayout.EndHorizontal();
                    Rect m4 = (Rect)EditorGUILayout.BeginHorizontal();
                    _tablelist.Add(GUILayout.Button("4", new []{GUILayout.Height(60), GUILayout.Width(60)}));
                    _tablelist.Add(GUILayout.Button("5",new []{GUILayout.Height(60), GUILayout.Width(60)}));
                    EditorGUILayout.EndHorizontal();
                    if (_targetAsset.flareSprite)
                    {
                        GUI.DrawTextureWithTexCoords(new Rect(m1.position.x , m1.position.y, 124, 124), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][0]);
                        GUI.DrawTextureWithTexCoords(new Rect(m3.position.x , m3.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][1]);
                        GUI.DrawTextureWithTexCoords(new Rect(m3.position.x  + m3.height * 1.05f, m3.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][2]);
                        GUI.DrawTextureWithTexCoords(new Rect(m4.position.x , m4.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][3]);
                        GUI.DrawTextureWithTexCoords(new Rect(m4.position.x + m4.height * 1.05f , m4.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][4]);
                    }
                }
                EditorGUILayout.EndVertical();
            
                EditorGUILayout.EndHorizontal();
                Rect m2 = (Rect)EditorGUILayout.BeginHorizontal();
                _tablelist.Add(GUILayout.Button("6", new[] {GUILayout.Height(125), GUILayout.Width(125)}));
            
                EditorGUILayout.BeginVertical();
                {
                    Rect m5 = (Rect)EditorGUILayout.BeginHorizontal();
                    _tablelist.Add(GUILayout.Button("7", new []{GUILayout.Height(60), GUILayout.Width(60)}));
                    _tablelist.Add(GUILayout.Button("8",new []{GUILayout.Height(60), GUILayout.Width(60)}));
                    EditorGUILayout.EndHorizontal();
                    Rect m6 = (Rect)EditorGUILayout.BeginHorizontal();
                    _tablelist.Add(GUILayout.Button("9" ,new []{GUILayout.Height(60), GUILayout.Width(60)}));
                    Rect m10 = (Rect)EditorGUILayout.BeginVertical();
                    {
                        _tablelist.Add(GUILayout.Button("10", new []{GUILayout.Height(30), GUILayout.Width(60)}));
                        _tablelist.Add(GUILayout.Button("11", new []{GUILayout.Height(13f), GUILayout.Width(60)}));
                        _tablelist.Add(GUILayout.Button("12", new []{GUILayout.Height(7.5f), GUILayout.Width(60)}));
                        _tablelist.Add(GUILayout.Button("13", new []{GUILayout.Height(7.5f), GUILayout.Width(60)}));
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    if (_targetAsset.flareSprite)
                    {
                        GUI.DrawTextureWithTexCoords(new Rect(m2.position.x , m2.position.y, 124, 124), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][5]);
                        
                        GUI.DrawTextureWithTexCoords(new Rect(m5.position.x , m5.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][6]);
                        GUI.DrawTextureWithTexCoords(new Rect(m5.position.x  + m5.height * 1.05f, m5.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][7]);
                        GUI.DrawTextureWithTexCoords(new Rect(m6.position.x , m6.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][8]);
                        
                        GUI.DrawTextureWithTexCoords(new Rect(m6.position.x + m6.height, m10.position.y, 59, 29), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][9]);
                        GUI.DrawTextureWithTexCoords(new Rect(m6.position.x + m6.height, m10.position.y + m6.height * 0.5f, 59, 12), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][10]);
                        GUI.DrawTextureWithTexCoords(new Rect(m6.position.x + m6.height, m10.position.y + m6.height * 0.75f, 59, 7), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][11]);
                        GUI.DrawTextureWithTexCoords(new Rect(m6.position.x + m6.height, m10.position.y + m6.height * 0.9f, 59, 7), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][12]);
                    }
                }
                EditorGUILayout.EndVertical();
            
                EditorGUILayout.EndHorizontal();
                break;
        
            case FlareTexModel._1L4S:
            
                Rect r31 = (Rect)EditorGUILayout.BeginVertical();
                _tablelist.Add(GUILayout.Button("1", new[] {GUILayout.Height(125), GUILayout.Width(125)}));
                Rect r32 = (Rect)EditorGUILayout.BeginHorizontal();
                _tablelist.Add(GUILayout.Button("2", new []{GUILayout.Height(60), GUILayout.Width(60)}));
                _tablelist.Add(GUILayout.Button("3", new []{GUILayout.Height(60), GUILayout.Width(60)}));
                EditorGUILayout.EndHorizontal();
                Rect r33 = (Rect)EditorGUILayout.BeginHorizontal();
                _tablelist.Add(GUILayout.Button("4", new []{GUILayout.Height(60), GUILayout.Width(60)}));
                _tablelist.Add(GUILayout.Button("5", new[] {GUILayout.Height(60), GUILayout.Width(60)}));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                if (_targetAsset.flareSprite)
                {
                    GUI.DrawTextureWithTexCoords(new Rect(r31.position.x, r31.position.y, 124, 124), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][0]);
                    GUI.DrawTextureWithTexCoords(new Rect(r32.position.x, r32.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][1]);
                    GUI.DrawTextureWithTexCoords(new Rect(r32.position.x + r32.height + 2, r32.position.y, 60, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][2]);
                    GUI.DrawTextureWithTexCoords(new Rect(r33.position.x, r33.position.y, 59, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][3]);
                    GUI.DrawTextureWithTexCoords(new Rect(r33.position.x + r33.height + 2, r33.position.y, 60, 59), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][4]);
                }
                break;
        
            case FlareTexModel._1L2M8S:
                Rect r41 = (Rect)EditorGUILayout.BeginVertical();
                _tablelist.Add(GUILayout.Button("1", new[] {GUILayout.Height(125), GUILayout.Width(125)}));
                if (_targetAsset.flareSprite)
                {
                    GUI.DrawTextureWithTexCoords(new Rect(r41.position.x, r41.position.y, 124, 124), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][0]);
                }

                for (int i = 0; i < 2; i++)
                {
                
                    Rect r42 = (Rect)EditorGUILayout.BeginHorizontal();
                    _tablelist.Add(GUILayout.Button((i * 5 + 2).ToString(), new[] {GUILayout.Height(60), GUILayout.Width(60)}));
                    EditorGUILayout.BeginVertical();
                    {
                        Rect r43 = (Rect)EditorGUILayout.BeginHorizontal();
                        _tablelist.Add(GUILayout.Button((i * 5 + 3).ToString(), new[] {GUILayout.Height(30), GUILayout.Width(30)}));
                        _tablelist.Add(GUILayout.Button((i * 5 + 4).ToString(), new[] {GUILayout.Height(30), GUILayout.Width(30)}));
                        EditorGUILayout.EndHorizontal();
                        Rect r45 = (Rect)EditorGUILayout.BeginHorizontal();
                        _tablelist.Add(GUILayout.Button((i * 5 + 5).ToString(), new[] {GUILayout.Height(30), GUILayout.Width(30)}));
                        _tablelist.Add(GUILayout.Button((i * 5 + 6).ToString(), new[] {GUILayout.Height(30), GUILayout.Width(30)}));
                        EditorGUILayout.EndHorizontal();
                        if (_targetAsset.flareSprite)
                        {
                            GUI.DrawTextureWithTexCoords(new Rect(r42.position.x, r42.position.y, 59, 60), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 5 + 1]);
                            GUI.DrawTextureWithTexCoords(new Rect(r43.position.x, r43.position.y, 30, 29), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 5 + 2]);
                            GUI.DrawTextureWithTexCoords(new Rect(r43.position.x + r43.height + 1, r43.position.y, 30, 29), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 5 + 3]);
                            GUI.DrawTextureWithTexCoords(new Rect(r45.position.x, r45.position.y, 30, 29), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 5 + 4]);
                            GUI.DrawTextureWithTexCoords(new Rect(r45.position.x + r45.height + 1, r45.position.y, 30, 29), _targetAsset.flareSprite, STATIC_FlareRectModel[texIndex][i * 5 + 5]);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                break;
            default: break;
        }
    }
}
