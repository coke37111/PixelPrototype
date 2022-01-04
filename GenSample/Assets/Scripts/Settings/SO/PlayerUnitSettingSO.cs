﻿using UnityEngine;

namespace Assets.Scripts.Settings.SO
{
    [CreateAssetMenu(fileName = "PlayerUnitSetting", menuName = "SO/Setting/PlayerUnit")]
    public class PlayerUnitSettingSO : ScriptableObject
    {
        [Header("- 이동속도")]
        public float speed;
        [Header("- 공격력")]
        public float atk;
        [Header("- 공격 딜레이")]
        public float atkDelay;
        [Header("- 방어력")]
        public float def;
        [Header("- 체력")]
        public float hp;
        [Header("- 미사일 발사 딜레이")]
        public float fireDelay;
        [Header("- 스킨목록")]
        public string[] unitTypes;

        public static string path = "Setting/PlayerUnitSetting";
        public static string spinePath = $"Spine/test_human_m/Spine_test_human_m";

        public string GetUnitType()
        {
            if (unitTypes.Length <= 0)
                return null;

            int selectIdx = Random.Range(0, unitTypes.Length);
            return unitTypes[selectIdx];
        }
    }
}