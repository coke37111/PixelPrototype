using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Settings.SO
{
    [CreateAssetMenu(fileName = "GameSetting", menuName = "SO/Setting/Game")]
    public class GameSettingSO : ScriptableObject
    {
        [Header("- 게임 진입~플레이까지의 딜레이")]
        public float startDelay;
        [Header("- 게임 종료~방나가기까지의 딜레이")]
        public float endDelay;
        [Header("- 클리어까지의 제한시간")]
        public float limitTime;

        public static string path = "Setting/GameSetting";
    }
}