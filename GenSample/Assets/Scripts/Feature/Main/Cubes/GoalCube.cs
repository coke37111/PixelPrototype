using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class GoalCube : Cube
    {
        public bool isHideGuide = false;

        #region UNITY

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                if (PlayerSettings.IsConnectNetwork())
                {
                    PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.CheckCoopEvent, Photon.Realtime.ReceiverGroup.All, CooperateClearEvent.Goal, true);
                }
            }
        }

        #endregion

        public void HideGuide()
        {
            if (isHideGuide)
                transform.Find("Guide").gameObject.SetActive(false);
        }
    }
}