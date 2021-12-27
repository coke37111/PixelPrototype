using UnityEngine;

namespace Assets.Scripts.Settings
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

        public static string path = "Setting/PlayerUnitSetting";
    }
}