using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.PxpCraft;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using Photon.Pun;
using Photon.Realtime;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;
using PunHashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Feature.Bomberman.Unit
{
    public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable, IOnEventCallback
    {
        public int bombPower = 6;
        public float bombTime = 3f;

        private BombermanMapController mapCtrl;
        private BombermanManager manager;
        private BombermanCameraController camCtrl;

        private Transform trSpine;
        private Animator anim;
        private GameObject pfBomb;
        private bool _isMove;
        private bool isMove {
            get => _isMove;
            set {
                _isMove = value;
                if (anim != null)
                    anim.SetBool("isMove", value);
            }
        }
        private bool _isLeftDir;
        private bool isLeftDir {
            get => _isLeftDir;
            set {
                _isLeftDir = value;
                ChangeDir(value);
            }
        }
        private PhotonView photonView;
        private bool raiseDieCall = false;
        private bool isControllable;

        private readonly string bombPath = "Prefab/BomberMan/Block/Bomb/BombRoot";

        private PlayerUnitSettingSO playerUnitSetting;
        private Rigidbody rb;
        private bool canJump;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            if (PlayerSettings.IsConnectNetwork())
                return;

            PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
            string spinePath = playerUnitSetting.GetSpinePath();
            MakeSpine(spinePath);

            Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerSettings.IsConnectNetwork() && !photonView.IsMine)
                return;

            if (!isControllable)
                return;

            Move();
            Jump();
            MakeBomb();
        }
        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnCollisionEnter(Collision coll)
        {
            if (coll.gameObject.tag == "Cube")
            {
                canJump = true;
            }
        }

        #endregion

        #region PUN_METHOD

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            string spinePath = info.photonView.InstantiationData[0].ToString();
            MakeSpine(spinePath);

            Init();

            SetBomberManMapController(FindObjectOfType<BombermanMapController>());
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(isLeftDir);
                stream.SendNext(isMove);
            }
            else
            {
                // Network player, receive data
                this.isLeftDir = (bool)stream.ReceiveNext();
                this.isMove = (bool)stream.ReceiveNext();
            }
        }

        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;
            object[] data = (photonEvent.CustomData != null) ? photonEvent.CustomData as object[] : null;

            switch (eventCodeType)
            {
                case EventCodeType.MakeBomb:
                    {
                        int senderViewId = (int)data[0];
                        if (photonView.ViewID != senderViewId)
                            return;

                        Vector3 bombPos = (Vector3)data[1];
                        int power = (int)data[2];
                        float time = (float)data[3];

                        GameObject goBomb = Instantiate(pfBomb, bombPos, Quaternion.identity);
                        Bomb bomb = goBomb.GetComponent<Bomb>();
                        bomb.SetMapCtrl(mapCtrl);
                        bomb.Build(power, time);

                        mapCtrl.RegisterBlock(bomb);
                        break;
                    }
            }
        }

        #endregion

        private void Init()
        {
            manager = FindObjectOfType<BombermanManager>();

            SkeletonMecanim skelM = GetComponentInChildren<SkeletonMecanim>();
            trSpine = skelM.transform;
            anim = skelM.GetComponent<Animator>();

            pfBomb = ResourceManager.LoadAsset<GameObject>(bombPath);

            photonView = GetComponent<PhotonView>();
            isControllable = true;

            camCtrl = FindObjectOfType<BombermanCameraController>();
            if (camCtrl != null && photonView.IsMine)
                camCtrl.SetTarget(transform);

            playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
            rb = GetComponent<Rigidbody>();
            canJump = true;
        }

        public void SetBomberManMapController(BombermanMapController mapCtrl)
        {
            this.mapCtrl = mapCtrl;
        }

        private void Move()
        {
            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                dir += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                dir = Vector3.back;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                dir += Vector3.right;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                dir += Vector3.left;
            }

            transform.Translate(dir * playerUnitSetting.speed * Time.deltaTime);

            // TODO : 좌우전환
            {
                if(dir.x > 0f || dir.x < 0f)
                {
                    isLeftDir = dir.x < 0f;
                }
            }

            // TODO : animation
            {
                isMove = dir != Vector3.zero;
            }            
        }

        private void ChangeDir(bool isLeftDir)
        {
            Vector3 spineScale = trSpine.localScale;
            if (!isLeftDir)
            {
                spineScale.x = -Mathf.Abs(spineScale.x);
            }
            else if (isLeftDir)
            {
                spineScale.x = Mathf.Abs(spineScale.x);
            }
            trSpine.localScale = spineScale;
        }

        private void MakeBomb()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Vector3 bombPos = new Vector3(Mathf.RoundToInt(transform.position.x), 0.5f, Mathf.RoundToInt(transform.position.z));

                if (mapCtrl.GetBlockInPos(new Vector2Int((int)bombPos.x, (int)bombPos.z)) != null)
                {
                    return;
                }

                if (PlayerSettings.IsConnectNetwork())
                {
                    PhotonEventManager.RaiseEvent(EventCodeType.MakeBomb, ReceiverGroup.All, new object[]
                    {
                        photonView.ViewID, bombPos, bombPower, bombTime
                    });                    
                }
                else
                {
                    GameObject goBomb = Instantiate(pfBomb, bombPos, Quaternion.identity);
                    Bomb bomb = goBomb.GetComponent<Bomb>();
                    bomb.SetMapCtrl(mapCtrl);
                    bomb.Build(bombPower, bombTime);

                    mapCtrl.RegisterBlock(bomb);
                }
            }
        }

        public void HitExplosion(params object[] param)
        {
            bool isConnectServer = PlayerSettings.IsConnectNetwork();

            if (manager.IsEndGame())
                return;

            if (isConnectServer && !photonView.IsMine)
                return;

            isControllable = false;

            if (isConnectServer)
                RaiseDie();
            else
                Destroy(gameObject);

            camCtrl.ResetPos();
        }

        private void RaiseDie()
        {
            if (raiseDieCall)
                return;
            raiseDieCall = true;

            // 게임 종료 체크 처리를 위한 호출(=>GenSampleManager)
            PunHashtable props = new PunHashtable
                    {
                        { PlayerSettings.PLAYER_DIE, true },
                    };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            if(photonView.IsMine)
                PhotonNetwork.Destroy(photonView);
        }

        public void MakeSpine(string spinePath)
        {
            GameObject spineBase = ResourceManager.LoadAsset<GameObject>(spinePath);
            GameObject goSpine = Instantiate(spineBase, transform);
            Vector3 scale = goSpine.transform.localScale;
            scale.x *= 2f;
            scale.y *= 3f;
            goSpine.transform.localScale = scale;
        }

        private void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                canJump = false;
                rb.AddForce(Vector3.up * playerUnitSetting.jumpPower);
            }
        }

        public void SetControllable(bool flag)
        {
            isControllable = flag;
        }
    }
}