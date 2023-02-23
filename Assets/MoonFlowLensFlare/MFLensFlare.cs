using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum FadeState
{
    Render = 0,
    FadeIn = 1,
    FadeOut = 2,
    Unrendered = 3
}
public class FlareStatusData
{
    public Vector3 sourceCoordinate;
    public Vector3[] flareWorldPosCenter;
    public float flareScale;
    public bool isIn;
    public FadeState fadeState;
    public Vector4 sourceScreenPos;
    public Mesh flareMesh;
    public Vector3[] vertices;
    public Vector2[] uv;
    public Color[] vertColor;
    public int[] triangle;
}


public class MFLensFlare : MonoBehaviour
{
    public bool DebugMode;
    [Space(10)]
    public Material material;
    public float fadeoutTime;
    public ComputeShader cs_PrepareLightOcclusion;
    public Dictionary<MFFlareLauncher, FlareStatusData> FlareDict => _flareDict;
    
    private Dictionary<MFFlareLauncher, FlareStatusData> _flareDict;
    private Camera _camera;
    private Vector2 _halfScreen;
    private MaterialPropertyBlock _propertyBlock;
    private Vector3 _screenCenter;
    private Queue<Mesh> _meshPool;
    private static readonly int STATIC_FLARESCREENPOS = Shader.PropertyToID("_FlareScreenPos");
    private static readonly int STATIC_BaseMap = Shader.PropertyToID("_MainTex");
    
    private static readonly float DISTANCE = 1f;
    private int _csKernel;
    private Vector4 _csLightUVInt;
    private ComputeBuffer _cbLightOcclusionCheckBuffer;
    private float[] _lightSourceDepth = {1.0f};
    private static readonly int CS_LIGHT_SRC_UVX = Shader.PropertyToID("_LightSrcUVX");
    private static readonly int CS_LIGHT_SRC_UVY = Shader.PropertyToID("_LightSrcUVY");
    private static readonly int CS_IS_LIGHT_OCCLUDED = Shader.PropertyToID("_LightSourceDepth");
    private static readonly int CS_DEPTHTEX_NAME = Shader.PropertyToID("_DepthTex");
    //Use your own depth texture used in your project
    private static readonly int PIPELINE_DEPTH_TEX = Shader.PropertyToID("_CameraDepthTexture");
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _propertyBlock = new MaterialPropertyBlock();
        _flareDict = new Dictionary<MFFlareLauncher, FlareStatusData>();
        _meshPool = new Queue<Mesh>();
        _csKernel = cs_PrepareLightOcclusion.FindKernel("PrepareLightOcclusion");
        _cbLightOcclusionCheckBuffer = new ComputeBuffer(1, sizeof(float));
        RenderPipelineManager.endCameraRendering += AddRenderPass;
    }

    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= AddRenderPass;
        _cbLightOcclusionCheckBuffer?.Release();
    }

    private FlareStatusData InitFlareData(MFFlareLauncher mfFlareLauncher)
    {
        int flareCount = mfFlareLauncher.asset.spriteBlocks.Count;
        FlareStatusData statusData = new FlareStatusData
        {
            sourceCoordinate = Vector3.zero,
            flareWorldPosCenter = new Vector3[mfFlareLauncher.asset.spriteBlocks.Count],
            // edgeScale = 1,
            flareScale = 0,
            fadeState = FadeState.Render,
            sourceScreenPos = Vector4.zero,
            flareMesh = _meshPool.Count > 0 ? _meshPool.Dequeue() : new Mesh(),
            vertices = new Vector3[flareCount * 4],
            triangle = new int[flareCount * 6],
            uv = new Vector2[flareCount * 4],
            vertColor = new Color[flareCount * 4]
        };
        for (int i = 0; i < mfFlareLauncher.asset.spriteBlocks.Count; i++)
        {
            Rect rect = mfFlareLauncher.asset.spriteBlocks[i].block;
            statusData.uv[i * 4] = rect.position;
            statusData.uv[i * 4 + 1] = rect.position + new Vector2(rect.width, 0);
            statusData.uv[i * 4 + 2] = rect.position + new Vector2(0, rect.height);
            statusData.uv[i * 4 + 3] = rect.position + rect.size;
                
            statusData.triangle[i * 6] = i * 4;
            statusData.triangle[i * 6 + 1] = i * 4 + 3;
            statusData.triangle[i * 6 + 2] = i * 4 + 1;
            statusData.triangle[i * 6 + 3] = i * 4;
            statusData.triangle[i * 6 + 4] = i * 4 + 2;
            statusData.triangle[i * 6 + 5] = i * 4 + 3;
        }
        return statusData;
    }

    public void AddLight(MFFlareLauncher mfFlareLauncher)
    {
        if (DebugMode)
        {
            Debug.Log("Add Light " + mfFlareLauncher.gameObject.name + " to FlareList");
        }
        var flareData = InitFlareData(mfFlareLauncher);
        _flareDict.Add(mfFlareLauncher, flareData);
    }

    public void RemoveLight(MFFlareLauncher mfFlareLauncher)
    {
        if(DebugMode)Debug.Log("Remove Light " + mfFlareLauncher.gameObject.name + " from FlareList");
        if (_flareDict.TryGetValue(mfFlareLauncher, out FlareStatusData flareState))
        {
            flareState.flareMesh.Clear();
            _meshPool.Enqueue(flareState.flareMesh);
            _flareDict.Remove(mfFlareLauncher);
        }
    }
    private void Update()
    {
        _halfScreen = new Vector2(_camera.scaledPixelWidth / 2 + _camera.pixelRect.xMin, _camera.scaledPixelHeight / 2 + _camera.pixelRect.yMin);
        var cameraTransform = _camera.transform;
        _screenCenter = cameraTransform.position + cameraTransform.forward * 0.1f;
        foreach (var pair in _flareDict)
        {
            FlareStatusData flareStatusData = pair.Value;
            MFFlareLauncher lightSource = pair.Key;
            GetSourceCoordinate(lightSource, ref flareStatusData);
            CheckIn(lightSource, ref flareStatusData);
            if (flareStatusData.flareScale > 0)
            {
                if(flareStatusData.flareScale >= 1)
                {
                    if (!flareStatusData.isIn)
                    {
                        flareStatusData.fadeState = FadeState.FadeOut;
                    }
                    else
                    {
                        flareStatusData.fadeState = FadeState.Render;
                    }
                }
                else
                {
                    if (!flareStatusData.isIn)
                    {
                        flareStatusData.fadeState = FadeState.FadeOut;
                    }
                    else
                    {
                        flareStatusData.fadeState = FadeState.FadeIn;
                    }
                }
            }
            else
            {
                if (!flareStatusData.isIn)
                {
                    flareStatusData.fadeState = FadeState.Unrendered;
                }
                else
                {
                    flareStatusData.fadeState = FadeState.FadeIn;
                }
            }

            if (flareStatusData.fadeState != FadeState.Unrendered)
            {
                CalculateMeshData(lightSource, ref flareStatusData);
            }

            switch (flareStatusData.fadeState)
            {
                case FadeState.FadeIn:
                    flareStatusData.flareScale += Time.deltaTime / fadeoutTime;
                    flareStatusData.flareScale = Mathf.Clamp(flareStatusData.flareScale, 0, 1);
                    break;
                case FadeState.FadeOut:
                    flareStatusData.flareScale -= Time.deltaTime / fadeoutTime;
                    flareStatusData.flareScale = Mathf.Clamp(flareStatusData.flareScale, 0, 1);
                    break;
                case FadeState.Unrendered:
                    flareStatusData.flareScale = 0;
                    // RemoveLight(lightSource[i]);
                    break;
                case FadeState.Render:
                    flareStatusData.flareScale = 1;
                    break;
                default:
                    break;
            }
            CreateMesh(lightSource, ref flareStatusData);
            if(DebugMode)DebugDrawMeshPos(lightSource, flareStatusData);
        }
        if (DebugMode)
        {
            Debug.Log("Lens Flare : " + _flareDict.Count + " lights");
        }
    }
    
    void GetSourceCoordinate(MFFlareLauncher lightSource, ref FlareStatusData statusData)
    {
        var lightSourceTransform = lightSource.transform;
        Vector3 sourceScreenPos = _camera.WorldToScreenPoint(
            lightSource.directionalLight 
                ?_camera.transform.position - lightSourceTransform.forward * 10000
                :lightSourceTransform.position
        );
        statusData.sourceCoordinate = sourceScreenPos;
    }
    void CheckIn(MFFlareLauncher lightSource, ref FlareStatusData statusData)
    {
        if (statusData.sourceCoordinate.x <  _camera.pixelRect.xMin || statusData.sourceCoordinate.y < _camera.pixelRect.yMin 
            || statusData.sourceCoordinate.x > _camera.pixelRect.xMax || statusData.sourceCoordinate.y > _camera.pixelRect.yMax
            || Vector3.Dot(lightSource.directionalLight ? Vector3.Normalize(_camera.transform.position - lightSource.transform.forward * 10000f) : Vector3.Normalize(lightSource.transform.position - _camera.transform.position), _camera.transform.forward) < 0.25f)
        {
            statusData.sourceScreenPos = Vector4.zero;
            statusData.isIn = false;
        }
        else
        {
            // var camPos = _camera.transform.position;
            // var targetPos = lightSource.directionalLight
            //     ? -lightSource.transform.forward * 10000f
            //     : lightSource.transform.position;
            // Ray ray = new Ray(camPos, targetPos - camPos );
            // RaycastHit hit;
            // Physics.Raycast(ray, out hit);
            // if (Vector3.Distance(hit.point, camPos) < Vector3.Distance(targetPos, camPos))
            // {
            //     if (hit.point == Vector3.zero) return true;
            //     return false;
            // }
            Vector4 screenUV = statusData.sourceCoordinate;
            screenUV.x = screenUV.x / _camera.pixelWidth;
            screenUV.y = screenUV.y / _camera.pixelHeight;
            screenUV.w = lightSource.directionalLight ? 1 : 0;
            statusData.sourceScreenPos = screenUV;

            _csLightUVInt = new Vector4((int)statusData.sourceCoordinate.x, (int)statusData.sourceCoordinate.y, 0, 0);
            if (_lightSourceDepth[0] == 0)
            {
                statusData.isIn = true;
            }
            else
            {
                statusData.sourceScreenPos = Vector4.zero;
                statusData.isIn = false;
            }
        }
    }

    private void PrepareLightOcclusion()
    {
        cs_PrepareLightOcclusion.SetInt(CS_LIGHT_SRC_UVX, (int)_csLightUVInt.x);
        cs_PrepareLightOcclusion.SetInt(CS_LIGHT_SRC_UVY,(int)_csLightUVInt.y);
        var depthTex = Shader.GetGlobalTexture(PIPELINE_DEPTH_TEX);

        cs_PrepareLightOcclusion.SetTexture(_csKernel, CS_DEPTHTEX_NAME, depthTex);
        _cbLightOcclusionCheckBuffer.SetData(_lightSourceDepth);
        cs_PrepareLightOcclusion.SetBuffer(_csKernel, CS_IS_LIGHT_OCCLUDED, _cbLightOcclusionCheckBuffer);
        cs_PrepareLightOcclusion.Dispatch(_csKernel, 1, 1, 1);
        _cbLightOcclusionCheckBuffer.GetData(_lightSourceDepth);
        
    }

    private void AddRenderPass(ScriptableRenderContext context, Camera camera)
    {
        // foreach (var cam in camera)
        // {
        if (camera.gameObject.CompareTag("MainCamera"))
        {
                PrepareLightOcclusion();
        }
        // }
    }

    void CalculateMeshData(MFFlareLauncher lightSource, ref FlareStatusData statusData)
    {
        Vector3[] oneFlareLine = new Vector3[lightSource.asset.spriteBlocks.Count];
        float[] useLightColor = new float[lightSource.asset.spriteBlocks.Count];
        for (int i = 0; i < lightSource.asset.spriteBlocks.Count; i++)
        {
            Vector2 realSourceCoordinateOffset = new Vector2(statusData.sourceCoordinate.x - _halfScreen.x, statusData.sourceCoordinate.y - _halfScreen.y);
            Vector2 realOffset = realSourceCoordinateOffset * lightSource.asset.spriteBlocks[i].offset;
            oneFlareLine[i] = new Vector3(_halfScreen.x + realOffset.x, _halfScreen.y + realOffset.y, DISTANCE);
            useLightColor[i] = lightSource.asset.spriteBlocks[i].useLightColor;
        }
        statusData.flareWorldPosCenter = oneFlareLine;
    }

    void CreateMesh(MFFlareLauncher lightSource, ref FlareStatusData statusData)
    {
        var flareCount = lightSource.asset.spriteBlocks.Count;
        if (statusData.flareScale > 0)
        {
            Texture2D tex = lightSource.asset.flareSprite;
            float angle = (45 +Vector2.SignedAngle(Vector2.up, new Vector2(statusData.sourceCoordinate.x - _halfScreen.x, statusData.sourceCoordinate.y - _halfScreen.y))) / 180 * Mathf.PI;
            for (int i = 0; i < lightSource.asset.spriteBlocks.Count; i++)
            {
                Rect rect = lightSource.asset.spriteBlocks[i].block;
                Vector2 halfSize = new Vector2(
                    tex.width * rect.width / 2 * lightSource.asset.spriteBlocks[i].scale * (lightSource.asset.fadeWithScale ? ( statusData.flareScale * 0.5f + 0.5f) : 1), 
                    tex.height * rect.height / 2 * lightSource.asset.spriteBlocks[i].scale * (lightSource.asset.fadeWithScale ? ( statusData.flareScale * 0.5f + 0.5f) : 1));
                Vector3 flarePos = statusData.flareWorldPosCenter[i];
                if (lightSource.asset.spriteBlocks[i].useRotation)
                {
                    float magnitude = Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y);
                    float cos = magnitude * Mathf.Cos(angle);
                    float sin = magnitude * Mathf.Sin(angle);
                    statusData.vertices[i * 4] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x - sin, flarePos.y + cos, flarePos.z)) - _screenCenter;
                    statusData.vertices[i * 4 + 1] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x - cos, flarePos.y - sin, flarePos.z)) - _screenCenter;
                    statusData.vertices[i * 4 + 2] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x + cos, flarePos.y + sin, flarePos.z)) - _screenCenter;
                    statusData.vertices[i * 4 + 3] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x + sin, flarePos.y - cos, flarePos.z)) - _screenCenter;
                }
                else
                {
                    statusData.vertices[i * 4] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y + halfSize.y, flarePos.z)) - _screenCenter;
                    statusData.vertices[i * 4 + 1] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y - halfSize.y, flarePos.z)) - _screenCenter;
                    statusData.vertices[i * 4 + 2] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y + halfSize.y, flarePos.z)) - _screenCenter;
                    statusData.vertices[i * 4 + 3] = _camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y - halfSize.y, flarePos.z)) - _screenCenter;
                }
                
                Color vertexAddColor = lightSource.asset.spriteBlocks[i].color;
                Color lightColor = default;
                Light source = lightSource.GetComponent<Light>();
                lightColor.r = Mathf.Lerp(1, source.color.r,
                    lightSource.asset.spriteBlocks[i].useLightColor);
                lightColor.g = Mathf.Lerp(1, source.color.g,
                    lightSource.asset.spriteBlocks[i].useLightColor);
                lightColor.b = Mathf.Lerp(1, source.color.b,
                    lightSource.asset.spriteBlocks[i].useLightColor);
                lightColor.a = 1;
                lightColor *= lightSource.useLightIntensity ? source.intensity : 1;
                
                vertexAddColor *= new Vector4(lightColor.r, lightColor.g, lightColor.b, 
                    (1.5f - Mathf.Abs(lightSource.asset.spriteBlocks[i].offset)) / 1.5f
                    * (1 - Mathf.Min(1, new Vector2(flarePos.x - _halfScreen.x, flarePos.y - _halfScreen.y).magnitude / new Vector2(_halfScreen.x, _halfScreen.y).magnitude))
                ) * ((lightSource.asset.fadeWithAlpha ? statusData.flareScale: 1));
                vertexAddColor = vertexAddColor.linear;
                statusData.vertColor[i * 4] = _ = vertexAddColor;
                statusData.vertColor[i * 4 + 1] = vertexAddColor;
                statusData.vertColor[i * 4 + 2] = vertexAddColor;
                statusData.vertColor[i * 4 + 3] = vertexAddColor;
            }

            statusData.flareMesh.vertices = statusData.vertices;
            statusData.flareMesh.uv = statusData.uv;
            statusData.flareMesh.triangles = statusData.triangle;
            statusData.flareMesh.colors = statusData.vertColor;
            _propertyBlock.SetTexture(STATIC_BaseMap, lightSource.asset.flareSprite);
            _propertyBlock.SetVector(STATIC_FLARESCREENPOS, statusData.sourceScreenPos);
            Graphics.DrawMesh(statusData.flareMesh, _screenCenter, Quaternion.identity, material, 0, _camera, 0, _propertyBlock);
        }
        
    }
    void DebugDrawMeshPos(MFFlareLauncher lightSource, FlareStatusData statusData)
    {
        for (int i = 0; i < lightSource.asset.spriteBlocks.Count; i++)
        {
            Debug.DrawLine(_camera.transform.position, _camera.ScreenToWorldPoint(statusData.flareWorldPosCenter[i]));
        }
    }

    private void OnDestroy()
    {
        foreach (var pair in _flareDict)
        {
            _meshPool.Enqueue(pair.Value.flareMesh);
        }
        while (_meshPool.Count > 0)
        {
            Destroy(_meshPool.Dequeue());
        }
        _meshPool.Clear();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MFLensFlare))]
public class MFLensflareEditor : Editor
{
    public MFLensFlare _target;

    private void OnEnable()
    {
        _target = target as MFLensFlare;
    }
    public override void OnInspectorGUI()
    {
        if (_target.FlareDict!=null && _target.FlareDict.Count != 0)
        {
            EditorGUILayout.LabelField("Light Count: ", _target.FlareDict.Count.ToString());
        }
        base.OnInspectorGUI();
    }
}
#endif
