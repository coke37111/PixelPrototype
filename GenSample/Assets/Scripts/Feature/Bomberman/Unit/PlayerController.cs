using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.PxpCraft;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
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

        private Transform trSpine;
        private Animator anim;
        private GameObject pfBomb;
        private Transform unitContainer; // TODO : SetParent를 위한 Component
        private CollisionEventListener collListener;
        private bool isMove;
        private bool isLeftDir;
        private PhotonView photonView;
        private bool raiseDieCall = false;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            if (PlayerSettings.IsConnectNetwork())
                return;

            Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerSettings.IsConnectNetwork() && !photonView.IsMine)
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
            Init();

            SetBomberManMapController(FindObjectOfType<BombermanMapController>());

            BombermanCameraController camCtrl = FindObjectOfType<BombermanCameraController>();
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
            SkeletonMecanim skelM = GetComponentInChildren<SkeletonMecanim>();
            trSpine = skelM.transform;
            anim = skelM.GetComponent<Animator>();

            pfBomb = ResourceManager.LoadAsset<GameObject>("Prefab/BomberMan/Bomb/BombRoot");
            unitContainer = FindObjectOfType<UnitContainer>().transform;

            collListener = GetComponentInChildren<CollisionEventListener>();
            collListener.RegisterListner("HitExplosion", HitExplosion);

            photonView = GetComponent<PhotonView>();
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
                Vector3 spineScale = trSpine.localScale;
                if (dir.x > 0)
                {
                    spineScale.x = -Mathf.Abs(spineScale.x);
                    isLeftDir = false;
                }
                else if (dir.x < 0)
                {
                    spineScale.x = Mathf.Abs(spineScale.x);
                    isLeftDir = true;
                }
                trSpine.localScale = spineScale;
            }

            // TODO : animation
            {
                isMove = dir != Vector3.zero;
                anim.SetBool("isMove", isMove);
            }            
        }

        private void MakeBomb()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 bombPos = new Vector3(Mathf.RoundToInt(transform.position.x), 0.5f, Mathf.RoundToInt(transform.position.z));

                if (PlayerSettings.IsConnectNetwork())
                {
                    var data = new List<object>();
                    data.Add(bombPower);
                    data.Add(bombTime);

                    PhotonNetwork.Instantiate("Prefab/BomberMan/Bomb/BombRoot", bombPos, Quaternion.identity, 0, data.ToArray());
                }
                else
                {
                    if (mapCtrl.GetBlockInPos(new Vector2Int((int)bombPos.x, (int)bombPos.z)) != null)
                    {
                        return;
                    }

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
            if (PlayerSettings.IsConnectNetwork())
                RaiseDie();
            else
                Destroy(gameObject);
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

            PhotonNetwork.Destroy(photonView);
        }
    }
}