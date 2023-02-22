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
    public MFFlareAsset asset;
    [HideInInspector]public Light lightSource;
    // [HideInInspector]public Texture2D tex;
    private Camera _mainCam;
    private void OnEnable()
    {
        lightSource = GetComponent<Light>();
        // Add self to awake function: AddLight in URPLensFlare.cs on camera in render;
        _mainCam = Camera.main;
        _mainCam.GetComponent<MFLensFlare>().AddLight(this);
    }
    
    private void Reset()
    {
        lightSource = GetComponent<Light>();
    }

    private void OnDisable()
    {
        // Add self to awake function: RemoveLight in URPLensFlare.cs on camera in render;
        if (_mainCam != null)
        {
            _mainCam.GetComponent<MFLensFlare>().RemoveLight(this);
        }
    }
    
}
