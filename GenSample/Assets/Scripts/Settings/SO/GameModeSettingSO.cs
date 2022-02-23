using System.Collections;
using System.Collections.Generic;
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

public enum CooperateClearEvent
{
    MobDie,
    Goal,
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
        [HideInInspector]
        public CooperateClearEvent coopClearEvent;
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

        private List<CooperateClearEvent> curClearEvent;

        public bool IsClear(CooperateClearEvent coopClearEvent)
        {
            if (gameMode != GameMode.Cooperate)
                return false;

            if(this.coopClearEvent == coopClearEvent)
            {
                if (curClearEvent == null)
                    curClearEvent = new List<CooperateClearEvent>();

                curClearEvent.Add(coopClearEvent);
                return true;
            }
            return false;
        }

        public bool IsAllClear()
        {
            if (gameMode != GameMode.Cooperate)
                return true;

            if (curClearEvent == null)
                return false;

            return curClearEvent.Contains(coopClearEvent);
        }
    }
}