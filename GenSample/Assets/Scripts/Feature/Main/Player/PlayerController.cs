using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using Photon.Pun;
using Photon.Realtime;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;
using PunHashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Feature.Main.Player
{
    public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable, IOnEventCallback
    {
        private Transform trSpine;
        private Animator anim;
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

        private int bombPowerLevel;
        private int bombRangeLevel;

        private bool isInvincible = false;

        private GameObject effL;
        private GameObject effR;
        private Transform effectContainerL;
        private Transform effectContainerR;
        private PlayerAttackRange playerAttackRangeL;
        private PlayerAttackRange playerAttackRangeR;

        private Vector3 fireDir;

        public enum ATK_TYPE
        {
            Melee = 0,
            Missile,
        }
        [SerializeField]
        private ATK_TYPE atkType = ATK_TYPE.Melee;

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
            SetAtkType(Random.Range(0, 2));
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
            Attack();
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

            int atkType = (int)info.photonView.InstantiationData[1];
            SetAtkType(atkType);

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
                        int bombPowerLevel = (int)data[2];
                        int bombRangeLevel = (int)data[3];

                        Transform parent = FindObjectOfType<CubeContainer>().transform;
                        GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                        GameObject goCubeRoot = Instantiate(pfCubeRoot, bombPos, Quaternion.identity, parent);
                        EditCube cubeRoot = goCubeRoot.GetComponent<EditCube>();
                        cubeRoot.Build("BombCube");

                        BombCube bombCube = cubeRoot.GetComponentInChildren<BombCube>();
                        bombCube.Init(bombPowerLevel, bombRangeLevel);
                        break;
                    }
                case EventCodeType.PlayerAttackBy:
                    {
                        int senderViewId = (int)data[0];
                        if (photonView.ViewID != senderViewId)
                            return;

                        if (!photonView.IsMine)
                            return;

                        float damage = (float)data[1];
                        AttackBy(damage);
                        break;
                    }
            }
        }

        [PunRPC]
        public void MakeMissileRPC(Vector3 position, Vector3 moveDir, PhotonMessageInfo info)
        {
            float lag = (float)(PhotonNetwork.Time - info.SentServerTime);
            Vector3 initPos = position + (moveDir + Vector3.up) * .25f;
            GameObject pfBullet = ResourceManager.LoadAsset<GameObject>("Prefab/Missile");
            GameObject goBullet = Instantiate(pfBullet, initPos, Quaternion.identity, transform);
            Missile bullet = goBullet.GetComponent<Missile>();
            bullet.InitializeBullet(this, fireDir, Mathf.Abs(lag));
        }

        #endregion

        private void Init()
        {
            SkeletonMecanim skelM = GetComponentInChildren<SkeletonMecanim>();
            trSpine = skelM.transform;
            anim = skelM.GetComponent<Animator>();

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

            bombPowerLevel = 0;
            bombRangeLevel = 0;

            isInvincible = false;

            effectContainerL = transform.Find("UnitBase/SpineRoot/Effect/L");
            effectContainerR = transform.Find("UnitBase/SpineRoot/Effect/R");
            playerAttackRangeL = effectContainerL.GetComponent<PlayerAttackRange>();
            playerAttackRangeR = effectContainerR.GetComponent<PlayerAttackRange>();

            ChangeDir(isLeftDir);
            fireDir = isLeftDir ? Vector3.left : Vector3.right;
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

            if (isMove)
                fireDir = dir;
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
                        photonView.ViewID, bombPos, bombPowerLevel, bombRangeLevel
                    });                    
                }
                else
                {
                    Transform parent = FindObjectOfType<CubeContainer>().transform;
                    GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                    GameObject goCubeRoot = Instantiate(pfCubeRoot, bombPos, Quaternion.identity, parent);
                    EditCube cubeRoot = goCubeRoot.GetComponent<EditCube>();
                    cubeRoot.Build("BombCube");

                    BombCube bombCube = cubeRoot.GetComponentInChildren<BombCube>();
                    bombCube.Init(bombPowerLevel, bombRangeLevel);
                }
            }
        }

        public void HitExplosion(params object[] param)
        {
            bool isConnectServer = PlayerSettings.IsConnectNetwork();

            if (isConnectServer && !photonView.IsMine)
                return;

            if (isInvincible)
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

        public void SetEditMode()
        {
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<CapsuleCollider>().enabled = false;
        }

        public void SetPlayMode()
        {
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<CapsuleCollider>().enabled = true;
        }

        public void BombPowerLevelup(int level)
        {
            bombPowerLevel += level;
        }
        
        public void BombRangeLevelup(int level)
        {
            bombRangeLevel += level;
        }

        public void ActiveInvincible()
        {
            isInvincible = true;
        }

        private void Attack()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                anim.SetTrigger("isAtk");

                switch (atkType)
                {
                    case ATK_TYPE.Melee:
                        {
                            MeleeAttack();
                            break;
                        }
                    case ATK_TYPE.Missile:
                        {
                            MissileAttack();
                            break;
                        }
                }

            }
        }

        private void MeleeAttack()
        {
            if (isLeftDir)
            {
                if (effL == null)
                {
                    GameObject pfAtkEff =
                   ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_red_L");
                    effL = Instantiate(pfAtkEff, effectContainerL);
                }
                effL.transform.localPosition = Vector3.zero;
                effL.GetComponent<ParticleSystem>().Play();

                List<Collider> targetList = playerAttackRangeL.GetTargetList();
                foreach (Collider coll in targetList)
                {
                    if (coll.GetComponent<PlayerController>())
                    {
                        coll.GetComponent<PlayerController>().RaiseAttackBy(playerUnitSetting.atk);
                    }
                    if (coll.GetComponent<Cube>())
                    {
                        coll.GetComponent<Cube>().Hit(playerUnitSetting.atk);
                    }
                }
            }
            else
            {
                if (effR == null)
                {
                    GameObject pfAtkEff =
                      ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_red_R");
                    effR = Instantiate(pfAtkEff, effectContainerR);
                }
                effR.transform.localPosition = Vector3.zero;
                effR.GetComponent<ParticleSystem>().Play();

                List<Collider> targetList = playerAttackRangeR.GetTargetList();
                foreach (Collider coll in targetList)
                {
                    if (coll.GetComponent<PlayerController>())
                    {
                        coll.GetComponent<PlayerController>().RaiseAttackBy(playerUnitSetting.atk);
                    }
                    if (coll.GetComponent<Cube>())
                    {
                        coll.GetComponent<Cube>().Hit(playerUnitSetting.atk);
                    }
                }
            }
        }

        private void MissileAttack()
        {
            if (PlayerSettings.IsConnectNetwork())
            {
                photonView.RPC("MakeMissileRPC", RpcTarget.AllViaServer, rb.position, moveDir);
            }
            else
            {
                Vector3 initPos = transform.position + (moveDir + Vector3.up) * .25f;
                GameObject pfBullet = ResourceManager.LoadAsset<GameObject>("Prefab/Missile");
                GameObject goBullet = Instantiate(pfBullet, initPos, Quaternion.identity, transform);
                Missile bullet = goBullet.GetComponent<Missile>();
                bullet.InitializeBullet(this, fireDir, 0f);
            }
        }

        public void RaiseAttackBy(float damage)
        {
            if (PlayerSettings.IsConnectNetwork())
            {
                PhotonEventManager.RaiseEvent(EventCodeType.PlayerAttackBy, ReceiverGroup.All, new object[]
                {
                        photonView.ViewID, damage
                });
            }
            else
            {
                AttackBy(damage);
            }
        }

        public void AttackBy(float damage)
        {
            if (isInvincible)
                return;

            curHp -= damage;
            if (curHp <= 0f)
            {
                curHp = 0f;

                isControllable = false;

                if (PlayerSettings.IsConnectNetwork())
                    RaiseDie();
                else
                    Destroy(gameObject);
            }

            hpBar.SetGauge(curHp / playerUnitSetting.hp);
        }

        private void SetAtkType(int atkType)
        {
            this.atkType = (ATK_TYPE)atkType;
            GetComponentInChildren<AtkTypeSlot>().Build(atkType);
        }

        public void FallDie()
        {
            isControllable = false;

            if (PlayerSettings.IsConnectNetwork())
                RaiseDie();
            else
                Destroy(gameObject);
        }

        public float GetAtk()
        {
            return playerUnitSetting.atk;
        }
    }
}