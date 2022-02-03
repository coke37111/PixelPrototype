﻿using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Main.nsCube;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Photon.Pun;
using Photon.Realtime;
using Spine.Unity;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;
using PunHashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Feature.Main.Player
{
    public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable, IOnEventCallback
    {
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

        public LayerMask cubeLayer;
        private Cube belowCube;

        private Vector3 moveDir;
        private Vector3 accDir;

        private float _curHp;
        private float curHp {
            get => _curHp;
            set {
                _curHp = value;
                if (hpBar != null)
                    hpBar.SetGauge(curHp / playerUnitSetting.hp);
            }
        }
        private HpBar hpBar;

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
            GetBelowCube();
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
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(isLeftDir);
                stream.SendNext(isMove);
                stream.SendNext(curHp);
            }
            else
            {
                // Network player, receive data
                this.isLeftDir = (bool)stream.ReceiveNext();
                this.isMove = (bool)stream.ReceiveNext();
                this.curHp = (float)stream.ReceiveNext();
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

                        Transform parent = FindObjectOfType<CubeContainer>().transform;
                        GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                        GameObject goCubeRoot = Instantiate(pfCubeRoot, bombPos, Quaternion.identity, parent);
                        EditCube cubeRoot = goCubeRoot.GetComponent<EditCube>();
                        cubeRoot.Build("BombCube");
                        break;
                    }
            }
        }

        #endregion

        private void Init()
        {
            SkeletonMecanim skelM = GetComponentInChildren<SkeletonMecanim>();
            trSpine = skelM.transform;
            anim = skelM.GetComponent<Animator>();

            pfBomb = ResourceManager.LoadAsset<GameObject>(bombPath);

            photonView = GetComponent<PhotonView>();
            isControllable = true;

            playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
            rb = GetComponent<Rigidbody>();
            canJump = true;

            moveDir = Vector3.zero;
            accDir = Vector3.zero;

            curHp = playerUnitSetting.hp;

            hpBar = GetComponentInChildren<HpBar>();
            hpBar.SetGauge(curHp / playerUnitSetting.hp);
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

            if(belowCube != null)
            {
                if (belowCube.GetComponent<IceCube>())
                {
                    accDir = Vector3.Lerp(accDir, dir, belowCube.GetComponent<IceCube>().dampRatio);
                    transform.position += accDir * Time.deltaTime;
                }
            }            

            moveDir = dir;

            transform.Translate(moveDir * playerUnitSetting.speed * Time.deltaTime);

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
                Vector3 bombPos = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.CeilToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));                
                    
                if (PlayerSettings.IsConnectNetwork())
                {
                    PhotonEventManager.RaiseEvent(EventCodeType.MakeBomb, ReceiverGroup.All, new object[]
                    {
                        photonView.ViewID, bombPos
                    });                    
                }
                else
                {
                    Transform parent = FindObjectOfType<CubeContainer>().transform;
                    GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                    GameObject goCubeRoot = Instantiate(pfCubeRoot, bombPos, Quaternion.identity, parent);
                    EditCube cubeRoot = goCubeRoot.GetComponent<EditCube>();
                    cubeRoot.Build("BombCube");
                }
            }
        }

        public void HitExplosion(params object[] param)
        {
            bool isConnectServer = PlayerSettings.IsConnectNetwork();

            if (isConnectServer && !photonView.IsMine)
                return;

            float damage = (float)param[0];
            curHp -= damage;
            if (curHp <= 0f)
            {
                curHp = 0f;

                isControllable = false;

                if (isConnectServer)
                    RaiseDie();
                else
                    Destroy(gameObject);
            }

            hpBar.SetGauge(curHp / playerUnitSetting.hp);
        }

        private void RaiseDie()
        {
            if (raiseDieCall)
                return;
            raiseDieCall = true;

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
            Transform root = transform.Find("UnitBase/SpineRoot");
            GameObject spineBase = ResourceManager.LoadAsset<GameObject>(spinePath);
            Instantiate(spineBase, root);
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

        private void GetBelowCube()
        {
            CapsuleCollider coll = GetComponent<CapsuleCollider>();

            Vector3 rayOrg = transform.position + GetComponent<CapsuleCollider>().center;
            Vector3 rayDir = Vector3.down;
            float rayDist = coll.height / 2f;

            RaycastHit hit;
            if (Physics.Raycast(rayOrg, rayDir, out hit, rayDist, cubeLayer))
            {
                Cube cube = hit.collider.GetComponent<Cube>();
                if (cube != null)
                {
                    if(belowCube == null || !belowCube.Equals(cube))
                    {
                        belowCube = cube;

                        if (belowCube.GetComponent<IceCube>())
                        {
                            if(accDir == Vector3.zero)
                                accDir = moveDir * playerUnitSetting.speed;
                        }
                        else
                        {
                            accDir = Vector3.zero;
                        }
                    }
                }

                //CubeBase collCube = hit.collider.GetComponent<CubeBase>();
                //if (belowCube == null ||
                //    belowCube.GetCubeType() != collCube.GetCubeType())
                //{
                //    if (belowCube != null && belowCube.GetCubeType() == CUBE_TYPE.DamageCube)
                //        belowCube.GetComponent<DamageCube>().UnregisterDamageListener(AttackByCube);

                //    belowCube = collCube;
                //    if (belowCube.GetCubeType() == CUBE_TYPE.IceCube)
                //    {
                //        accDelta = moveDir * playerUnitSetting.speed;
                //    }
                //    else if (belowCube.GetCubeType() == CUBE_TYPE.DamageCube)
                //    {
                //        belowCube.GetComponent<DamageCube>().RegisterDamageListener(AttackByCube);
                //    }
                //    else if (belowCube.GetCubeType() == CUBE_TYPE.BreakCube)
                //    {
                //        belowCube.GetComponent<BreakCube>().CheckBreak();
                //    }
                //    else
                //    {
                //        accDelta = Vector3.zero;
                //    }
                //}
            }
            else
            {
                //if (belowCube != null && belowCube.GetCubeType() == CUBE_TYPE.DamageCube)
                //    belowCube.GetComponent<DamageCube>().UnregisterDamageListener(AttackByCube);

                belowCube = null;
            }
        }
    }
}