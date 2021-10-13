using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
[RequireComponent(typeof(Light))]

public class MFFlareLauncher : MonoBehaviour
{
    public bool directionalLight;
    public bool useLightIntensity;
    [SerializeField]public MFFlareAssetModel assetModel;
    [HideInInspector]public Light lightSource;
    [HideInInspector]public Texture2D tex;
    private void OnEnable()
    {
        lightSource = GetComponent<Light>();
        // Add self to awake function: AddLight in URPLensFlare.cs on camera in render;
        Camera.main.GetComponent<MFLensFlare>().AddLight(this);
        tex = Resources.Load("Lensflare/"+assetModel.flareSprite.name) as Texture2D;
    }
    
    private void Reset()
    {
        lightSource = GetComponent<Light>();
    }

    private void Update()
    {
    }

    private void Awake()
    {
        
    }
    
    private void OnDestroy()
    {
        // Add self to awake function: RemoveLight in URPLensFlare.cs on camera in render;
        //Camera.main.GetComponent<URPLensFlare>().RemoveLight(this);
    }
    
}
