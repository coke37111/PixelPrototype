using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.GenSample
{
    [RequireComponent(typeof(PhotonView))]
    public class UnitNetworkPlayer : UnitLocalPlayer, IPunInstantiateMagicCallback, IPunObservable, IOnEventCallback
    {
        private PhotonView photonView;
        private bool raiseDieCall = false;

        #region UNITY

        protected override void Update()
        {
            if (!photonView.IsMine)
                return;

            base.Update();

            CheckFall();
        }

        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #endregion

        #region PUN_IMPL

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            Init();

            Dictionary<string, string> unitPartList = (Dictionary<string, string>)info.photonView.InstantiationData[0];
            SetSprite(unitPartList);

            if(RoomSettings.roomType == RoomSettings.ROOM_TYPE.Pvp)
                teamNum = (int)info.photonView.InstantiationData[1];
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(isLeftDir);
                stream.SendNext(curHp);
            }
            else
            {
                // Network player, receive data
                this.isLeftDir = (bool)stream.ReceiveNext();
                this.curHp = (float)stream.ReceiveNext();
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;
            object[] data = (photonEvent.CustomData != null) ? photonEvent.CustomData as object[] : null;

            switch (eventCodeType)
            {                
                case EventCodeType.PlayerDie:
                    {
                        int senderViewId = (int)data[0];
                        if (photonView.AmOwner && photonView.ViewID == senderViewId)
                        {
                            Die();
                        }
                        break;
                    }
                case EventCodeType.MobDie:
                case EventCodeType.Fail:
                    {
                        controlable = false;
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
                case EventCodeType.Knockback:
                    {
                        float centerX = (float)data[0];
                        float centerZ = (float)data[1];

                        Knockback(centerX, centerZ);
                        break;
                    }
                case EventCodeType.Hit:
                    {
                        int senderViewId = (int)data[0];
                        float atk = (float)data[1];

                        if (photonView.ViewID != senderViewId)
                            return;

                        MakeHitEffect();

                        Log.Print($"{senderViewId} attacked");
                        curHp -= GetFinalDamage(atk, GetDef());
                        if (curHp <= 0f && photonView.AmOwner)
                        {
                            RaiseDie();
                        }
                        break;
                    }
            }
        }

        [PunRPC]
        public void MakeMissileRPC(Vector3 position, Vector3 moveDir,  PhotonMessageInfo info)
        {
            float lag = (float)(PhotonNetwork.Time - info.SentServerTime);
            Vector3 initPos = position + (moveDir + Vector3.up) * .25f;
            GameObject pfBullet = ResourceManager.LoadAsset<GameObject>("Prefab/Missile");
            GameObject goBullet = Instantiate(pfBullet, initPos, Quaternion.identity, transform);
            Missile bullet = goBullet.GetComponent<Missile>();
            bullet.InitializeBullet(this, moveDir, Mathf.Abs(lag));
        }

        #endregion

        public override void Init()
        {
            base.Init();

            photonView = GetComponent<PhotonView>();
            raiseDieCall = false;
        }

        protected override void ShowAtkEff()
        {
            SetAtkEffColor();
            PhotonEventManager.RaiseEvent(EventCodeType.MakeAtkEff, ReceiverGroup.All, photonView.ViewID, curEffColor);
        }

        private void CheckFall()
        {
            if (transform.localPosition.y <= -5f)
            {
                RaiseDie();
            }
        }

        private void RaiseDie()
        {
            if (raiseDieCall)
                return;

            raiseDieCall = true;

            // 게임 종료 체크 처리를 위한 호출(=>GenSampleManager)
            Hashtable props = new Hashtable
                    {
                        { PlayerSettings.PLAYER_DIE, true },
                    };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            // 오브젝트 Destroy를 위한 호출
            PhotonEventManager.RaiseEvent(EventCodeType.PlayerDie, ReceiverGroup.All, photonView.ViewID);
        }

        private void Die()
        {
            PhotonNetwork.Destroy(photonView);
        }

        private void SetAtkEffColor(string effColor)
        {
            if (!string.IsNullOrEmpty(curEffColor))
                return;

            curEffColor = effColor;
        }

        public override void AttackBy(UnitLocalPlayer unitNetworkPlayer)
        {
            if (!photonView.IsMine)
                return;

            PhotonEventManager.RaiseEvent(EventCodeType.Hit, ReceiverGroup.All, photonView.ViewID, unitNetworkPlayer.GetAtk());
        }

        protected override void MakeMissile()
        {
            photonView.RPC("MakeMissileRPC", RpcTarget.AllViaServer, rb.position, moveDir);
        }
    }
}