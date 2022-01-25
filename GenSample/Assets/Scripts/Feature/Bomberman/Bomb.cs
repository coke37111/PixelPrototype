using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.PxpCraft;
using Assets.Scripts.Util;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class Bomb : BomberManBlock, IPunInstantiateMagicCallback
    {
        public GameObject basePrefab;
        private BombCollision bombColl;
        private BombermanMapController mapCtrl;
        private BombermanManager manager;

        private float time;
        private float curTime;
        private bool completeBuild = false;
        private int power;
        private bool isExplosion = false;

        #region UNITY

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!completeBuild)
                return;

            if (!isExplosion)
            {
                if (curTime >= time)
                {
                    Explosion();
                }
                else
                {
                    curTime += Time.deltaTime;
                }
            }
        }

        #endregion

        #region PUN_METHOD

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            Transform unitContainer = FindObjectOfType<UnitContainer>().transform;
            transform.SetParent(unitContainer);

            int bombPower = (int)info.photonView.InstantiationData[0];
            float bombTime = (float)info.photonView.InstantiationData[1];

            SetMapCtrl(FindObjectOfType<BombermanMapController>());
            Build(bombPower, bombTime);

            mapCtrl.RegisterBlock(this);
        }

        #endregion

        private void Init()
        {
            manager = FindObjectOfType<BombermanManager>();

            basePrefab.SetActive(true);
            bombColl = GetComponentInChildren<BombCollision>();
            curTime = 0f;
            isExplosion = false;            
        }

        public void SetMapCtrl(BombermanMapController mapCtrl)
        {
            this.mapCtrl = mapCtrl;
        }

        public void Build(int power, float time)
        {
            Init();

            this.power = power;
            this.time = time;

            completeBuild = true;
        }

        public void Explosion()
        {
            if (manager.IsEndGame())
                return;

            if (isExplosion)
                return;

            isExplosion = true;

            basePrefab.SetActive(false);
            bombColl.GetComponent<BoxCollider>().enabled = false;

            Vector2Int bombPos = GetPosition();
            mapCtrl.MakeExplosion(bombPos);
            mapCtrl.MakeExplosion(bombPos, power);

            mapCtrl.UnregisterBlock(this);

            Destroy(gameObject);
        }
    }
}