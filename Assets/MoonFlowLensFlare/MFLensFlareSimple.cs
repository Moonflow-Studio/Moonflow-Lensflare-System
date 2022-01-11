using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MFLensFlareSimple : MonoBehaviour
{
    public bool DebugMode;
    [Space(10)]
    public MFFlareAsset lightSourceAsset;
    public Light directionalLight;
    public bool useLightIntensity;
    public Material material;
    public float fadeoutTime;
    public FlareStatusData flareData;
    private Camera _camera;
    private Vector2 _halfScreen;
    private MaterialPropertyBlock _propertyBlock;
    private Mesh _flareMesh;
    private List<Vector3> _totalVert;
    private List<Vector2> _totalUv;
    private List<Color> _totalColor;
    private List<int> _totalTriangle;

    private static readonly int STATIC_BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int STATIC_FLARESCREENPOS = Shader.PropertyToID("_FlareScreenPos");
    private static readonly float DISTANCE = 1f;

    private void Awake()
    {
        _totalVert = new List<Vector3>();
        _totalUv = new List<Vector2>();
        _totalColor = new List<Color>();
        _totalTriangle = new List<int>();
        _flareMesh = new Mesh();
        
        _camera = GetComponent<Camera>();
        _propertyBlock = new MaterialPropertyBlock();
        AddLight();
    }
    private FlareStatusData InitFlareData(MFFlareAsset mfFlareAssetModel)
    {
        FlareStatusData statusData = new FlareStatusData
        {
            sourceCoordinate = Vector3.zero,
            flareWorldPosCenter = new Vector3[mfFlareAssetModel.spriteBlocks.Count],
            fadeoutScale = 0,
            fadeState = 1
        };
        return statusData;
    }

    public void AddLight()
    {
        flareData = InitFlareData(lightSourceAsset);
    }

    private void Update()
    {
        _halfScreen = new Vector2(_camera.scaledPixelWidth / 2 + _camera.pixelRect.xMin, _camera.scaledPixelHeight / 2 + _camera.pixelRect.yMin);

        _flareMesh.Clear();
        
        FlareStatusData flareStatusData = flareData;
        GetSourceCoordinate(ref flareStatusData);
        bool isIn = CheckIn(ref flareStatusData);
        if (flareStatusData.fadeoutScale > 0)
        {
            if (!isIn)
            {
                flareStatusData.fadeState = 2;
            }
        }
        if(flareStatusData.fadeoutScale < 1)
        {
            if (isIn)
            {
                flareStatusData.fadeState = 1;
            }
        }
        if (!isIn && flareStatusData.fadeoutScale <=0 )
        {
            if (flareStatusData.fadeState != 3)
            {
                flareStatusData.fadeState = 3;
            }
        }
        else
        {
            CalculateMeshData(ref flareStatusData);
        }

        switch (flareStatusData.fadeState)
        {
            case 1:
                flareStatusData.fadeoutScale += Time.deltaTime / fadeoutTime;
                flareStatusData.fadeoutScale = Mathf.Clamp(flareStatusData.fadeoutScale, 0, 1);
                flareData = flareStatusData;
                break;
            case 2:
                flareStatusData.fadeoutScale -= Time.deltaTime / fadeoutTime;
                flareStatusData.fadeoutScale = Mathf.Clamp(flareStatusData.fadeoutScale, 0, 1);
                flareData = flareStatusData;
                break;
            case 3:
                // RemoveLight(lightSource[i]);
                break;
            default: flareData = flareStatusData;
                break;
        }
        
        CreateMesh();

        if (DebugMode)
        {
            Debug.Log("Lens Flare From: " + directionalLight);
            DebugDrawMeshPos();
        }
    }

    bool CheckIn(ref FlareStatusData statusData)
    {
        if (statusData.sourceCoordinate.x <  _camera.pixelRect.xMin || statusData.sourceCoordinate.y < _camera.pixelRect.yMin 
            || statusData.sourceCoordinate.x > _camera.pixelRect.xMax || statusData.sourceCoordinate.y > _camera.pixelRect.yMax
            || Vector3.Dot(directionalLight.transform.position - _camera.transform.position, _camera.transform.forward) < 0.25f)
        {
            return false;
        }
        else
        {
            Vector3 screenUV = statusData.sourceCoordinate;
            screenUV.x = screenUV.x / _camera.pixelWidth;
            screenUV.y = screenUV.y / _camera.pixelHeight;
            _propertyBlock.SetVector(STATIC_FLARESCREENPOS, screenUV);
            // var camPos = _camera.transform.position;
            // var targetPos = -directionalLight.transform.forward * 10000f;
            // Ray ray = new Ray(camPos, targetPos - camPos );
            // RaycastHit hit;
            // Physics.Raycast(ray, out hit);
            // if (Vector3.Distance(hit.point, camPos) < Vector3.Distance(targetPos, camPos))
            // {
            //     if (hit.point == Vector3.zero) return true;
            //     return false;
            // }
            return true;
        }
    }

    void GetSourceCoordinate(ref FlareStatusData statusData)
    {
        Vector3 sourceScreenPos = _camera.WorldToScreenPoint(directionalLight.transform.position - directionalLight.transform.forward * 10000);
        statusData.sourceCoordinate = new Vector3(sourceScreenPos.x , sourceScreenPos.y , DISTANCE);
    }

    void CalculateMeshData(ref FlareStatusData statusData)
    {
        Vector3[] oneFlareLine = new Vector3[lightSourceAsset.spriteBlocks.Count];
        float[] useLightColor = new float[lightSourceAsset.spriteBlocks.Count];
        for (int i = 0; i < lightSourceAsset.spriteBlocks.Count; i++)
        {
            Vector2 realSourceCoordinateOffset = new Vector2(statusData.sourceCoordinate.x - _halfScreen.x, statusData.sourceCoordinate.y - _halfScreen.y);
            Vector2 realOffset = realSourceCoordinateOffset * lightSourceAsset.spriteBlocks[i].offset;
            oneFlareLine[i] = new Vector3(_halfScreen.x + realOffset.x, _halfScreen.y + realOffset.y, statusData.sourceCoordinate.z);
            useLightColor[i] = lightSourceAsset.spriteBlocks[i].useLightColor;
        }
        statusData.flareWorldPosCenter = oneFlareLine;
    }

    void CreateMesh()
    {
        if (directionalLight == null){ Debug.Log("No light source for lensflare");return;}
        
        List<Vector3> vertList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
        List<int> tri = new List<int>();
        List<Color> vertColors = new List<Color>();

        var transform1 = _camera.transform;
        var position = transform1.position;
        var center = position + transform1.forward * 0.1f;
        
        _totalColor.Clear();
        _totalTriangle.Clear();
        _totalVert.Clear();
        _totalUv.Clear();
        vertList.Clear();
        uvList.Clear();
        tri.Clear();
        vertColors.Clear();
        if (flareData.fadeoutScale > 0)
        {
            // MFFlareLauncher observer = lightSource[lightIndex];
            Texture2D tex = lightSourceAsset.flareSprite;//observer.asset.flareSprite;
            float angle = (45 +Vector2.SignedAngle(Vector2.up, new Vector2(flareData.sourceCoordinate.x - _halfScreen.x, flareData.sourceCoordinate.y - _halfScreen.y))) / 180 * Mathf.PI;
            for (int i = 0; i < lightSourceAsset.spriteBlocks.Count; i++)
            {
                Rect rect = lightSourceAsset.spriteBlocks[i].block;
                Vector2 halfSize = new Vector2(
                    tex.width * rect.width / 2 * lightSourceAsset.spriteBlocks[i].scale * (lightSourceAsset.fadeWithScale ? ( flareData.fadeoutScale * 0.5f + 0.5f) : 1), 
                    tex.height * rect.height / 2 * lightSourceAsset.spriteBlocks[i].scale * (lightSourceAsset.fadeWithScale ? ( flareData.fadeoutScale * 0.5f + 0.5f) : 1));
                Vector3 flarePos = flareData.flareWorldPosCenter[i];
                if (lightSourceAsset.spriteBlocks[i].useRotation)
                {
                    float magnitude = Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y);
                    float cos = magnitude * Mathf.Cos(angle);
                    float sin = magnitude * Mathf.Sin(angle);
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x - sin, flarePos.y + cos, flarePos.z)) - center);
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x - cos, flarePos.y - sin, flarePos.z)) - center);
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x + cos, flarePos.y + sin, flarePos.z)) - center);
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x + sin, flarePos.y - cos, flarePos.z)) - center);
                }
                else
                {
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y + halfSize.y, flarePos.z)) - center);
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y - halfSize.y, flarePos.z)) - center);
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y + halfSize.y, flarePos.z)) - center);
                    vertList.Add(_camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y - halfSize.y, flarePos.z)) - center);

                }
                uvList.Add(rect.position);
                uvList.Add(rect.position + new Vector2(rect.width, 0));
                uvList.Add(rect.position + new Vector2(0, rect.height));
                uvList.Add(rect.position + rect.size);
                
                tri.Add(i * 4);
                tri.Add(i * 4 + 3);
                tri.Add(i * 4 + 1);
                tri.Add(i * 4);
                tri.Add(i * 4 + 2);
                tri.Add(i * 4 + 3);
                
                Color vertexAddColor = lightSourceAsset.spriteBlocks[i].color;
                Color lightColor = default;
                lightColor.r = Mathf.Lerp(1, directionalLight.color.r,
                    lightSourceAsset.spriteBlocks[i].useLightColor);
                lightColor.g = Mathf.Lerp(1, directionalLight.color.g,
                    lightSourceAsset.spriteBlocks[i].useLightColor);
                lightColor.b = Mathf.Lerp(1, directionalLight.color.b,
                    lightSourceAsset.spriteBlocks[i].useLightColor);
                lightColor.a = 1;
                lightColor *= useLightIntensity ? directionalLight.intensity : 1;
                
                vertexAddColor *= new Vector4(lightColor.r, lightColor.g, lightColor.b, 
                    (1.5f - Mathf.Abs(lightSourceAsset.spriteBlocks[i].offset)) / 1.5f
                    * (1 - Mathf.Min(1, new Vector2(flarePos.x - _halfScreen.x, flarePos.y - _halfScreen.y).magnitude / new Vector2(_halfScreen.x, _halfScreen.y).magnitude))
                ) * ((lightSourceAsset.fadeWithAlpha ? flareData.fadeoutScale: 1));
                vertexAddColor = vertexAddColor.linear;
                vertColors.Add(vertexAddColor);
                vertColors.Add(vertexAddColor);
                vertColors.Add(vertexAddColor);
                vertColors.Add(vertexAddColor);
            }
            _totalVert.AddRange(vertList);
            _totalUv.AddRange(uvList);
            _totalTriangle.AddRange(tri);
            _totalColor.AddRange(vertColors);
            _flareMesh.vertices = _totalVert.ToArray();
            _flareMesh.uv = _totalUv.ToArray();
            _flareMesh.triangles = _totalTriangle.ToArray();
            _flareMesh.colors = _totalColor.ToArray();
            _propertyBlock.SetTexture(STATIC_BaseMap, lightSourceAsset.flareSprite);

            Graphics.DrawMesh(_flareMesh, center, Quaternion.identity, material, 0, _camera, 0, _propertyBlock);
        }
    }
    void DebugDrawMeshPos()
    {
        for (int i = 0; i < lightSourceAsset.spriteBlocks.Count; i++)
        {
            Debug.DrawLine(_camera.transform.position, _camera.ScreenToWorldPoint(flareData.flareWorldPosCenter[i]));
        }
    }
}
