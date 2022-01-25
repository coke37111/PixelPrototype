using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class Bomb : BomberManBlock
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

        public override void Init()
        {
            base.Init();

            manager = FindObjectOfType<BombermanManager>();

            basePrefab.SetActive(true);
            bombColl = GetComponentInChildren<BombCollision>();
            curTime = 0f;
            isExplosion = false;
            canExplosion = true;
            canPenetrate = true;
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

        public override void Explosion()
        {
            base.Explosion();

            if (manager.IsEndGame())
                return;

            if (isExplosion)
                return;

            isExplosion = true;

            basePrefab.SetActive(false);
            bombColl.GetComponent<BoxCollider>().enabled = false;

            Vector2Int bombPos = GetPosition();
            mapCtrl.MakeExplosion(bombPos, "EffExplosion");
            mapCtrl.MakeExplosion(bombPos, power, "EffExplosion");

            mapCtrl.UnregisterBlock(this);
            Destroy(gameObject);
        }
    }
}