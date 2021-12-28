﻿using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitController : Unit, IPunInstantiateMagicCallback, IOnEventCallback
    {
        public readonly string[] effColorList = new string[] {
            "blue",
            "green",
            "purple",
            "red",
            "yellow"
        };

        public PhotonView photonView;
        public Transform effContainerL;
        public Transform effContainerR;

        private PlayerUnitSettingSO playerUnitSetting;

        public event Action<Vector3> OnChnagePosition;

        private bool isConnected = false;
        private bool isDie = false;
        private bool canJump = true;

        private bool canAtk;
        private MobController targetMob;
        private float curAtkDelay;

        private GameObject atkEffectL;
        private GameObject atkEffectR;
        private string curEffColor;
        

        #region UNITY

        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        protected override void Update()
        {
            if (isConnected)
            {
                if (!photonView.IsMine)
                    return;
            }

            if (isDie)
                return;

            // INPUT
            {
                // 키보드 wasd 이동으로
                //if (Input.GetMouseButtonUp(1))
                //{
                //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //    RaycastHit hit;
                //    if (Physics.Raycast(ray, out hit, 10000f, clickLayer))
                //    {
                //        Move(hit.point);
                //    }
                //}

                if (Input.GetKeyDown(KeyCode.Space) && canJump && Controlable)
                {
                    canJump = false;
                    rb.AddForce(Vector3.up * 150f);
                }
            }

            // ACTION
            {
                MoveByKeyboard();
                Attack();

                if (isConnected && transform.localPosition.y <= -5f)
                {
                    Hashtable props = new Hashtable
                    {
                        { PLAYER_DIE, true },
                    };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                    RaiseEvent(EventCodeType.PlayerDie, ReceiverGroup.All);
                }
            }
        }

        void OnCollisionEnter(Collision coll)
        {
            if(coll.gameObject.tag == "Ground")
            {
                canJump = true;
            }
            else if(coll.gameObject.tag == "Mob")
            {
                canAtk = true;
                targetMob = coll.gameObject.GetComponent<MobController>();
            }
        }

        void OnCollisionExit(Collision coll)
        {
            if(coll.gameObject.tag == "Mob")
            {
                canAtk = false;
                targetMob = null;
            }
        }

        #endregion

        #region PUN_CALLBACK

        private void RaiseEvent(EventCodeType eventCodeType, ReceiverGroup receiveGroup, params object[] objs)
        {
            List<object> content = new List<object>() { photonView.ViewID };
            content.AddRange(objs);

            PhotonEventManager.RaiseEvent(eventCodeType, receiveGroup, content.ToArray());
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            Dictionary<string, string> unitPartList = (Dictionary<string, string>)info.photonView.InstantiationData[0];
            SetSprite(unitPartList);

            Init(true);
        }

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;
            object[] data = (photonEvent.CustomData != null) ? photonEvent.CustomData as object[]: null;

            switch (eventCodeType)
            {
                case EventCodeType.Move:
                    {
                        int senderViewId = (int)data[0];
                        if (photonView.ViewID != senderViewId || photonView.AmOwner)
                            return;

                        isLeftDir = (bool)data[1];
                        SetDir();
                        break;
                    }
                case EventCodeType.Knockback:
                    {
                        float centerX = (float)data[1];
                        float centerZ = (float)data[2];

                        Knockback(centerX, centerZ);
                        break;
                    }
                case EventCodeType.MakeAtkEff:
                    {
                        int senderViewId = (int)data[0];
                        string effColor = data[1].ToString();

                        if (photonView.ViewID != senderViewId)
                        {
                            return;
                        }

                        SetAtkEffColor(effColor);
                        MakeAtkEffect();
                        break;
                    }
                case EventCodeType.PlayerDie:
                    {
                        int senderViewId = (int)data[0];
                        if(photonView.ViewID == senderViewId)
                        {
                            Die();
                        }
                        break;
                    }
            }
        }

        #endregion

        #region FOR_TEST

        public void ResetSpawnPos(Vector3 pos)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = pos;
        }

        #endregion

        public void Init(bool isConnected)
        {
            base.Init();

            this.isConnected = isConnected;

            isDie = false;
            canJump = true;

            transform.SetParent(FindObjectOfType<UnitContainer>().transform);
            targetPos = new Vector3( transform.position.x, 0.0f, transform.position.z);

            playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);

            curAtkDelay = 0f;
        }

        protected override void OnChangeDir(bool isLeft)
        {
            base.OnChangeDir(isLeft);
            SetDir();

            if (isConnected)
                RaiseEvent(EventCodeType.Move, ReceiverGroup.Others, isLeftDir);
        }

        // TODO : 마우스 클릭으로 이동 시에 사용되던 함수
        //public void Move(Vector3 targetPos)
        //{
        //    this.targetPos = targetPos;

        //    isLeftDir = targetPos.x <= transform.position.x;            
        //}

        private void MoveByKeyboard()
        {
            Vector3 delta = Vector3.zero;

            if (Controlable)
            {                
                if (Input.GetKey(KeyCode.W))
                    delta.z += (playerUnitSetting.speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.S))
                    delta.z -= (playerUnitSetting.speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.A))
                    delta.x -= (playerUnitSetting.speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.D))
                    delta.x += (playerUnitSetting.speed * Time.deltaTime);
            }

            if (delta.x != 0)
                isLeftDir = delta.x < 0;

            if (delta != Vector3.zero)
                transform.position += delta;

            OnChnagePosition?.Invoke(transform.position);
        }

        public override void Knockback(float centerX, float centerZ)
        {
            var distance = Vector3.Distance(new Vector3(centerX, transform.position.y, centerZ), transform.position);

            if (canJump && distance <= 1.3f)
            {                
                Vector3 diffPos = transform.position - new Vector3(centerX, transform.position.y, centerZ);
                Vector3 dir = diffPos.normalized;
                rb.AddForce(dir * 300f + Vector3.up * 150f);
            }
        }

        public void Die()
        {
            isDie = true;

            if(PlayerSettings.IsConnectNetwork() && photonView.AmOwner)
                PhotonNetwork.Destroy(photonView);
        }

        private void Attack()
        {
            if (canAtk && targetMob != null)
            {
                if (curAtkDelay >= playerUnitSetting.atkDelay)
                {
                    curAtkDelay = 0f;

                    if (targetMob.IsDie())
                    {
                        return;
                    }

                    targetMob.AttackBy(this);
                    CheckAtkEffect();
                }
                else
                {
                    curAtkDelay += Time.deltaTime;
                }
            }
        }

        private void CheckAtkEffect()
        {
            SetAtkEffColor();

            if (PhotonNetwork.IsConnected)
            {
                List<object> content = new List<object>() { photonView.ViewID, curEffColor };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                SendOptions sendOptions = new SendOptions { Reliability = true };
                PhotonNetwork.RaiseEvent((byte)EventCodeType.MakeAtkEff, content, raiseEventOptions, sendOptions);
            }
            else
            {
                MakeAtkEffect();
            }
        }

        public void SetAtkEffColor()
        {
            if (!string.IsNullOrEmpty(curEffColor))
                return;

            int colorIdx = UnityEngine.Random.Range(0, effColorList.Length);
            curEffColor = effColorList[colorIdx];
        }

        public void SetAtkEffColor(string effColor)
        {
            if (!string.IsNullOrEmpty(curEffColor))
                return;

            curEffColor = effColor;
        }

        public void MakeAtkEffect()
        {
            if (isLeftDir)
            {
                if(atkEffectL == null)
                {
                    GameObject pfAtkEff =
                        ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_{curEffColor}_L");
                    atkEffectL = Instantiate(pfAtkEff, effContainerL);
                    atkEffectL.transform.localPosition = Vector3.zero;
                }

                atkEffectL.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                if (atkEffectR == null)
                {
                    GameObject pfAtkEff =
                        ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_{curEffColor}_R");
                    atkEffectR = Instantiate(pfAtkEff, effContainerR);
                    atkEffectR.transform.localPosition = Vector3.zero;
                }

                atkEffectR.GetComponent<ParticleSystem>().Play();
            }
        }

        private bool Controlable
        {
            get
            {
                if(PlayerSettings.IsConnectNetwork())
                {
                    object value;
                    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomSettings.GAME_END, out value))
                        return ((bool)value) == false;

                    return false;
                }

                return true;
            }
        }

        public float GetAtk()
        {
            return playerUnitSetting.atk;
        }
    }
}