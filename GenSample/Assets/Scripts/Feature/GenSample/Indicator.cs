using Assets.Scripts.Managers;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Feature.GenSample
{
    public class Indicator : MonoBehaviour, IPunInstantiateMagicCallback
    {
        public Text textAlert;
        public Transform trArea;

        private float timeLimit;

        private float curTime;

        private bool isConnected;

        private UnityAction<Vector3> knockbackCB;

        #region UNITY

        void Update()
        {
            if(curTime >= timeLimit)
            {
                curTime = 0f;

                DestroyIndicator();
            }
            else
            {
                curTime += Time.deltaTime;
            }

            SetTextAlert();
        }

        #endregion

        #region PUN_CALLBACKS

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;
            Init((float)data[0], (float)data[1], (float)data[2], true);
        }

        #endregion

        public void Init(float timeLimit, float scaleX, float scaleZ, bool isConnected = false)
        {
            this.isConnected = isConnected;

            this.timeLimit = timeLimit;

            curTime = 0f;

            SetTextAlert();
            trArea.localScale = new Vector3(scaleX, trArea.localScale.y, scaleZ);
        }

        public void SetGenSampleManager(UnityAction<Vector3> knockbackCB)
        {
            this.knockbackCB = knockbackCB;
        }

        private void SetTextAlert()
        {
            textAlert.text = (timeLimit - curTime + 1f).ToString("N0");
        }

        private void DestroyIndicator()
        {
            if (isConnected)
            {
                Vector3 trPos = transform.position;

                PhotonView photonView = GetComponent<PhotonView>();

                List<object> content = new List<object>() { photonView.ViewID, trPos.x, trPos.y, trPos.z };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                SendOptions sendOptions = new SendOptions { Reliability = true };
                PhotonNetwork.RaiseEvent((byte)GenSampleManager.EventCodeType.Knockback, content.ToArray(), raiseEventOptions, sendOptions);
            }
            else
            {
                knockbackCB?.Invoke(transform.position);
                Destroy(gameObject);
            }
        }
    }
}