using UnityEngine;

#region ENUM

public enum CameraMoveType
{
    Fix,
    FollowPlayer
}

#endregion


namespace Assets.Scripts.Settings.SO
{
    [CreateAssetMenu(fileName = "CameraViewSetting", menuName = "SO/Setting/CameraViewSetting")]
    public class CameraViewSettingSO : ScriptableObject
    {
        public CameraMoveType cameraMoveType;
        public Vector3 rotate;
        public float dist;
        public float height;
    }
}