using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings.SO
{
    [CreateAssetMenu(fileName = "CubeTileSetting", menuName = "SO/Setting/CubeTile")]
    public class CubeTileSettingSO : ScriptableObject
    {           
        public List<CubeTileData> cubeTileData;
    }

    [Serializable]
    public class CubeTileData
    {
        public string cubeName;
        public string tileName;
    }
}