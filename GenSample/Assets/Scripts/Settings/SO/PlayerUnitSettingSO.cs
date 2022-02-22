using UnityEngine;

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
        [Header("- 점프 파워")]
        public float jumpPower;
        [Header("- 근거리 넉백 파워")]
        public Vector2 meleeKnockbackPower;
        [Header("- 미사일 넉백 파워")]
        public Vector2 missileKnockbackPower;
        public Vector3 unitScale;

        public bool canMakeBomb;
        public bool canAttack;

        //[Header("- 스킨목록")]
        //public string[] unitTypes;
        [Header("- 스파인목록")]
        public string[] spineTypes;

        public static string path = "Setting/PlayerUnitSetting";

        //public string GetUnitType()
        //{
        //    if (unitTypes.Length <= 0)
        //        return null;

        //    int selectIdx = Random.Range(0, unitTypes.Length);
        //    return unitTypes[selectIdx];
        //}

        public string GetSpinePath()
        {
            if (spineTypes.Length <= 0)
                return null;

            int selectIdx = Random.Range(0, spineTypes.Length);
            return spineTypes[selectIdx];
        }
    }
}