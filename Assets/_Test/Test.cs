using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public URPFlareLauncher launcher;

    public Material material;

    private MaterialPropertyBlock _propertyBlock;
    // Start is called before the first frame update
    void Start()
    {
        _propertyBlock = new MaterialPropertyBlock();
        material.SetTexture("_BaseMap", Resources.Load("Lensflare/FlareStar"/*+launcher.asset.flareSprite.name*/) as Texture2D);
    }

    private void OnDisable()
    {
        material.SetTexture("_BaseMap", Texture2D.blackTexture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
