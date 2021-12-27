using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Settings.SO
{
    [CreateAssetMenu(fileName = "MobSetting", menuName = "SO/Setting/Mob")]
    public class MobSettingSO : ScriptableObject
    {
        [Header("- 체력(x유저수 비례해 증가)")]
        public float hp;
        [Header("- 체력 회복량(x100%)")]
        public float regenHpRatio;
        [Header("- 체력 회복 딜레이")]
        public float regenHpDelay;

        public static string path = "Setting/MobSetting";
    }
}