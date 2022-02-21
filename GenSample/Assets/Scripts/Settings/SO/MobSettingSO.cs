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
        [Header("- 0보다 작으면 1로 고정")]
        public float minAtkDelay;
        [Header("- 0보다 작으면 1로 고정")]
        public float maxAtkDelay;
        [Header("- Mob 위치 기준 공격이 생성되는 위치 범위")]
        public Vector3 atkPos;
        [Header("- 생성된 공격 크기(X-Z)")]
        public Vector2 atkScale;
        [Header("- 공격이 유지되는 시간")]
        public float atkLimitTime;

        public static string path = "Setting/MobSetting";
    }
}