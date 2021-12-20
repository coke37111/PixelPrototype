﻿using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Managers.GenSampleManager;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitController : Unit, IPunInstantiateMagicCallback, IOnEventCallback
    {
        public float speed = 1f;

        public PhotonView photonView;

        private bool isConnected = false;
        private bool isDie = false;
        private bool canJump = true;

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
                if (Input.GetMouseButtonUp(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 10000f, clickLayer))
                    {
                        Move(hit.point);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space) && canJump)
                {
                    canJump = false;
                    rb.AddForce(Vector3.up * 150f);
                }
            }

            // ACTION
            {
                Vector3 delta = Vector3.zero;
                if (Input.GetKey(KeyCode.W))
                    targetPos.z += (speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.S))
                    targetPos.z -= (speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.A))
                    targetPos.x -= (speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.D))
                    targetPos.x += (speed * Time.deltaTime);

                isLeftDir = targetPos.x <= transform.position.x;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.z), Time.deltaTime * speed);

                OnChnagePosition?.Invoke(transform.position);

                if (isConnected && transform.localPosition.y <= -5f)
                {
                    Hashtable props = new Hashtable
                    {
                        { PLAYER_LIVES, 0 }
                    };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                }
            }
        }

        void OnCollisionEnter(Collision coll)
        {
            if(coll.gameObject.tag == "Ground")
            {
                canJump = true;
            }
        }

        #endregion


        #region PUN_CALLBACK

        private void RaiseEvent(EventCodeType eventCodeType, params object[] objs)
        {
            List<object> content = new List<object>() { photonView.ViewID };
            content.AddRange(objs);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent((byte)eventCodeType, content.ToArray(), raiseEventOptions, sendOptions);
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
                        if (photonView.ViewID != senderViewId)
                            return;

                        isLeftDir = (bool)data[1];
                        SetDir();
                        break;
                    }
            }
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
        }

        protected override void OnChangeDir(bool isLeft)
        {
            base.OnChangeDir(isLeft);
            SetDir();

            if (isConnected)
                RaiseEvent(EventCodeType.Move, isLeftDir);
        }

        public void Move(Vector3 targetPos)
        {
            this.targetPos = targetPos;

            isLeftDir = targetPos.x <= transform.position.x;            
        }

        public override void Knockback(float centerX, float centerZ)
        {            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10000f, knockbackLayer))
            {
                if(hit.collider.tag == "Indicator")
                {
                    Vector3 diffPos = transform.position - new Vector3(centerX, transform.position.y, centerZ);
                    Vector3 dir = diffPos.normalized;

                    if (canJump)
                        rb.AddForce(dir * 1000f);
                }
            }
        }

        public void Die()
        {
            isDie = true;
        }

        public void ResetSpawnPos(Vector3 pos)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = pos;
            targetPos = pos;
        }

        public event Action<Vector3> OnChnagePosition;

    }
}