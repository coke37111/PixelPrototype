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
using PunHashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Feature.Bomberman.Unit
{
    public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable, IOnEventCallback
    {
        public float speed = 3f;
        public int bombPower = 6;
        public float bombTime = 3f;

        private BombermanMapController mapCtrl;
        private BombermanManager manager;
        private BombermanCameraController camCtrl;

        private Transform trSpine;
        private Animator anim;
        private GameObject pfBomb;
        private Transform unitContainer; // TODO : SetParent를 위한 Component
        private CollisionEventListener collListener;
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

        #endregion

        #region PUN_METHOD

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            string spinePath = info.photonView.InstantiationData[0].ToString();
            MakeSpine(spinePath);

            Init();

            transform.SetParent(unitContainer);

            SetBomberManMapController(FindObjectOfType<BombermanMapController>());

            camCtrl = FindObjectOfType<BombermanCameraController>();
            if (camCtrl != null && photonView.IsMine)
                camCtrl.SetTarget(transform);
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

        }

        #endregion

        private void Init()
        {
            manager = FindObjectOfType<BombermanManager>();

            SkeletonMecanim skelM = GetComponentInChildren<SkeletonMecanim>();
            trSpine = skelM.transform;
            anim = skelM.GetComponent<Animator>();

            pfBomb = ResourceManager.LoadAsset<GameObject>(bombPath);
            unitContainer = FindObjectOfType<UnitContainer>().transform;

            collListener = GetComponentInChildren<CollisionEventListener>();
            collListener.RegisterListner("HitExplosion", HitExplosion);

            photonView = GetComponent<PhotonView>();
            isControllable = true;
        }

        public void SetBomberManMapController(BombermanMapController mapCtrl)
        {
            this.mapCtrl = mapCtrl;
        }

        private void Move()
        {
            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                dir += Vector3.left;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                dir += Vector3.right;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                dir += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                dir = Vector3.back;
            }            

            transform.Translate(dir * speed * Time.deltaTime);

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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 bombPos = new Vector3(Mathf.RoundToInt(transform.position.x), 0.5f, Mathf.RoundToInt(transform.position.z));

                if (mapCtrl.GetBlockInPos(new Vector2Int((int)bombPos.x, (int)bombPos.z)) != null)
                {
                    return;
                }

                if (PlayerSettings.IsConnectNetwork())
                {
                    var data = new List<object>();
                    data.Add(bombPower);
                    data.Add(bombTime);

                    PhotonNetwork.Instantiate(bombPath, bombPos, Quaternion.identity, 0, data.ToArray());
                }
                else
                {
                    GameObject goBomb = Instantiate(pfBomb, bombPos, Quaternion.identity, unitContainer);
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
    }
}