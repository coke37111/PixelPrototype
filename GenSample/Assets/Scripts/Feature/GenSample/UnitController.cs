﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitController : Unit, IPunInstantiateMagicCallback, IOnEventCallback
    {
        public LayerMask ignoreClickLayer;

        public enum EventCodeType
        {
            Jump,
        }

        public float speed = 1f;

        public PhotonView photonView;

        private bool canJump = true;

        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void Init()
        {
            base.Init();

            canJump = true;
        }

        protected override void Update()
        {
            if (!photonView.IsMine)
                return;

            if (Input.GetMouseButtonUp(1))
            {                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 10000f, ~ignoreClickLayer))
                {                    
                    targetPos = hit.point;

                    isLeftDir = targetPos.x <= transform.position.x;
                    SetDir();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                RaiseEvent(EventCodeType.Jump);
            }
            
            if (Vector3.Distance(transform.position, targetPos) > .1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.z), Time.deltaTime * speed);
            }

            if(!canJump && GetComponent<Rigidbody>().velocity.y <= 0f)
            {
                RaycastHit hit;
                if(Physics.Raycast(transform.position, Vector3.down, out hit, .25f))
                {
                    if(hit.collider.tag == "Ground")
                    {
                        canJump = true;
                    }
                }
            }
        }

        private void RaiseEvent(EventCodeType eventCodeType, object[] content = null)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent((byte)eventCodeType, content, raiseEventOptions, sendOptions);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            info.photonView.transform.SetParent(FindObjectOfType<UnitContainer>().transform);

            Dictionary<string, string> unitPartList = (Dictionary<string, string>)info.photonView.InstantiationData[0];
            SetSprite(unitPartList);

            Init();
        }

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;

            switch (eventCodeType)
            {
                case EventCodeType.Jump:
                    {
                        canJump = false;
                        GetComponent<Rigidbody>().AddForce(Vector3.up * 100f);
                        break;
                    }
            }
        }
    }
}