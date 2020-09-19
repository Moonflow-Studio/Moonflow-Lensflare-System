using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
[RequireComponent(typeof(Light))]
public class URPFlareLauncher : MonoBehaviour
{
    public bool directionalLight;
    public bool useLightIntensity;
    [SerializeField]public URPFlareAsset asset;
    [HideInInspector]public Light lightSource;
    [HideInInspector]public Texture2D tex;
    private void OnEnable()
    {
        lightSource = GetComponent<Light>();
        // Add self to awake function: AddLight in URPLensFlare.cs on camera in render;
        Camera.main.GetComponent<URPLensFlare>().AddLight(this);
        tex = Resources.Load("Lensflare/"+asset.flareSprite.name) as Texture2D;
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
