using System;
using System.Collections.Generic;
using UnityEngine;

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
public class MFFlareAssetModel : MFFlareAsset
{
   [SerializeField]public FlareTexModel flareTexModel;
}


