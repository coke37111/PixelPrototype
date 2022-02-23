using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Settings.SO.Editor
{
    [CustomEditor(typeof(GameModeSettingSO))]
    [CanEditMultipleObjects]
    public class GameModeSettingSOEditor : UnityEditor.Editor
    {
        SerializedProperty survivalCount;
        SerializedProperty limitTime;
        SerializedProperty dieHeight;

        void OnEnable()
        {
            survivalCount = serializedObject.FindProperty("survivalCount");
            limitTime = serializedObject.FindProperty("limitTime");
            dieHeight = serializedObject.FindProperty("dieHeight");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            GameModeSettingSO settingSO = (GameModeSettingSO)target;

            settingSO.gameMode = (GameMode)EditorGUILayout.EnumPopup("GameMode", settingSO.gameMode);
            switch (settingSO.gameMode)
            {
                case GameMode.Survival:
                    {
                        settingSO.matchType = (MatchType)EditorGUILayout.EnumPopup("MatchType", settingSO.matchType);
                        if(settingSO.matchType == MatchType.Free)
                        {
                            EditorGUILayout.PropertyField(survivalCount);
                        }
                        break;
                    }
                case GameMode.Cooperate:
                    {
                        settingSO.coopClearEvent = (CooperateClearEvent)EditorGUILayout.EnumPopup("CoopClearEvent", settingSO.coopClearEvent);
                        break;
                    }
                case GameMode.Sandbox:
                    {
                        break;
                    }
            }

            EditorGUILayout.PropertyField(limitTime);

            settingSO.canFallDie = EditorGUILayout.Toggle("CanFallDie", settingSO.canFallDie);
            if (settingSO.canFallDie)
            {
                EditorGUILayout.PropertyField(dieHeight);
            }

            settingSO.cameraViewSetting = (CameraViewSettingSO)EditorGUILayout.ObjectField("CameraViewSetting", settingSO.cameraViewSetting, typeof(CameraViewSettingSO), false);
            settingSO.playerUnitSetting = (PlayerUnitSettingSO)EditorGUILayout.ObjectField("PlayerUnitSetting", settingSO.playerUnitSetting, typeof(PlayerUnitSettingSO), false);
            settingSO.mapData = (SandboxMapDataSO)EditorGUILayout.ObjectField("MapData", settingSO.mapData, typeof(SandboxMapDataSO), false);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(settingSO);
            }
        }
    }
}