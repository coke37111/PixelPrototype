using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Settings.SO
{
    [CreateAssetMenu(fileName = "IndicatorSetting", menuName = "SO/Setting/Indicator")]
    public class IndicatorSettingSO : ScriptableObject
    {
        public float minSpawnTime;
        public float maxSpawnTime;

        public static string path = "Setting/IndicatorSetting";
    }
}