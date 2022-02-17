using System.Collections;
using UnityEngine;

#region ENUM

public enum GameMode
{
    Survival,
    Cooperate,
    Sandbox,
}

public enum MatchType
{
    Free,
    Team,
}

#endregion

namespace Assets.Scripts.Settings.SO
{
    [CreateAssetMenu(fileName = "GameModeSetting", menuName = "SO/Setting/GameModeSetting")]
    public class GameModeSettingSO : ScriptableObject
    {
        [HideInInspector]
        public GameMode gameMode;
        [HideInInspector]
        public MatchType matchType;
        [HideInInspector]
        public int survivalCount;
        [Header("- 0 이거나 0보다 작을 시 제한시간 없음"), HideInInspector]
        public float limitTime;
        [HideInInspector]
        public bool canFallDie;
        [HideInInspector]
        public float dieHeight;
        [HideInInspector]
        public CameraViewSettingSO cameraViewSetting;
        [HideInInspector]
        public PlayerUnitSettingSO playerUnitSetting;
        [HideInInspector]
        public SandboxMapDataSO mapData;
    }
}