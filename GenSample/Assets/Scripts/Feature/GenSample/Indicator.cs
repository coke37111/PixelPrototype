using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
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
        [Header("x=밀어내는power/y=띄우는power")]
        public Vector2 knockbackPower = new Vector2(300f, 150f);

        private float timeLimit;

        private float curTime;

        private UnityAction<object[]> knockbackCB;
        private bool _tryDestroyed;

        #region UNITY

        void Update()
        {
            if(curTime >= timeLimit)
            {
                //curTime = 0f;

                DestroyIndicator();
            }
            else
            {
                curTime += Time.deltaTime;
                SetTextAlert();
            }            
        }

        #endregion

        #region PUN_CALLBACKS

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;
            Init((float)data[0], (float)data[1], (float)data[2]);
        }

        #endregion

        public void Init(float timeLimit, float scaleX, float scaleZ)
        {
            this.timeLimit = timeLimit;
            _tryDestroyed = false;
            curTime = 0f;

            SetTextAlert();
            trArea.localScale = new Vector3(scaleX, trArea.localScale.y, scaleZ);
        }

        public void RegisterKnockbackListener(UnityAction<object[]> knockbackCB)
        {
            this.knockbackCB = knockbackCB;
        }

        private void SetTextAlert()
        {
            textAlert.text = (Mathf.Max(0, timeLimit - curTime + 1f)).ToString("N0");
        }

        private void DestroyIndicator()
        {
            Vector3 trPos = transform.position;
            float radius = trArea.localScale.x * 0.5f;

            PlayerController[] players = FindObjectsOfType<PlayerController>();
            if (players != null)
            {
                foreach (PlayerController player in players)
                {
                    player.Knockback(trPos, radius, knockbackPower);
                }
            }
            Destroy(gameObject);
        }
    }
}