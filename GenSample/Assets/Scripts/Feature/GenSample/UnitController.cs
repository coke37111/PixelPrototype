using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitController : Unit, IPunInstantiateMagicCallback, IOnEventCallback
    {
        public LayerMask ignoreClickLayer;

        public enum EventCodeType
        {
            Move,
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

            object lives;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(GenSampleManager.PLAYER_LIVES, out lives))
            {
                if ((int)lives <= 0)
                {
                    return;
                }
            }

            if (Input.GetMouseButtonUp(1))
            {                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 10000f, ~ignoreClickLayer))
                {                    
                    targetPos = hit.point;

                    isLeftDir = targetPos.x <= transform.position.x;

                    RaiseEvent(EventCodeType.Move, isLeftDir);
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
            PhotonView targetPv = info.photonView;
            targetPv.transform.SetParent(FindObjectOfType<UnitContainer>().transform);

            Dictionary<string, string> unitPartList = (Dictionary<string, string>)targetPv.InstantiationData[0];
            SetSprite(unitPartList);

            targetPos = targetPv.transform.position;
            
            Init();
        }

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;

            switch (eventCodeType)
            {
                case EventCodeType.Move:
                    {
                        object[] data = (object[])photonEvent.CustomData;
                        int senderViewId = (int)data[0];
                        isLeftDir = (bool)data[1];

                        if (photonView.ViewID != senderViewId)
                            return;

                        SetDir();
                        break;
                    }
                case EventCodeType.Jump:
                    {
                        object[] data = (object[])photonEvent.CustomData;
                        int senderViewId = (int)data[0];

                        if (photonView.ViewID != senderViewId)
                            return;

                        canJump = false;
                        GetComponent<Rigidbody>().AddForce(Vector3.up * 100f);
                        break;
                    }
            }
        }
    }
}