using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Managers.GenSampleManager;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitController : Unit, IPunInstantiateMagicCallback, IOnEventCallback
    {
        public LayerMask ignoreClickLayer;

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
                    if (Physics.Raycast(ray, out hit, 10000f, ~ignoreClickLayer))
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
                if (Vector3.Distance(transform.position, targetPos) > .1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.z), Time.deltaTime * speed);
                }
                
                if(isConnected && transform.localPosition.y <= -5f)
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
            targetPos = transform.position;
        }

        public void Move(Vector3 targetPos)
        {
            this.targetPos = targetPos;

            isLeftDir = targetPos.x <= transform.position.x;
            SetDir();

            if (isConnected)
                RaiseEvent(EventCodeType.Move, isLeftDir);
        }

        public override void Knockback(float hitX, float hitZ)
        {
            Vector3 diffPos = transform.position - new Vector3(hitX, transform.position.y, hitZ);
            Vector3 dir = diffPos.normalized;

            if (canJump)
                rb.AddForce(dir * 1000f);
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

    }
}