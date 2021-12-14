using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class GenSampleManager : MonoBehaviour
    {
        public SpriteRenderer srBg;
        public Transform unitContainer;

        [Range(1, 100)]
        public int unitGenCount = 1;

        private Vector2 spawnAreaX; // min, max
        private Vector2 spawnAreaZ; // min, max
        [Range(0f, 1f)]
        public float spawnRange;

        private List<Unit> unitList = new List<Unit>();
        public UnitController playerUnit;

        // Use this for initialization
        void Start()
        {
            Vector2 spawnArea = GetSpawnArea();
            Log.Print(spawnArea);

            spawnAreaX = new Vector2(-spawnArea.x, spawnArea.x);
            spawnAreaZ = new Vector2(-spawnArea.y, spawnArea.y);

            DestroyAllMob();
            GenerateMob();
            ResetMobPos();

            playerUnit.Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetMobPos();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                DestroyAllMob();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                GenerateMob();
                ResetMobPos();
            }
        }

        public Vector2 GetSpawnArea()
        {
            Vector2 bgPixelSize = srBg.sprite.rect.size;
            float ppu = srBg.sprite.pixelsPerUnit;

            Vector2 bgUnitSize = bgPixelSize / ppu * spawnRange;
            Vector2 spawnArea = bgUnitSize - bgUnitSize * .5f;

            return spawnArea;
        }

        public List<Unit> GetUnitList()
        {
            return unitList;
        }

        private void GenerateMob()
        {            
            GameObject pfUnit = ResourceManager.LoadAsset<GameObject>("Prefab/Unit");
            Unit unitComp;

            for (int i = 0; i < unitGenCount; i++)
            {                
                if(unitList.Count <= i)
                {                    
                    GameObject goUnit = Instantiate(pfUnit, unitContainer);
                    unitComp = goUnit.GetComponent<Unit>();
                    
                    if(unitComp != null)
                        unitList.Add(unitComp);
                }
                else
                {
                    unitComp = unitList[i];
                }

                if (unitComp == null)
                {
                    Log.Error($"Unit Component cannot find!");
                    return;
                }

                unitComp.Init();
            }
        }

        private void DestroyAllMob()
        {
            foreach (Unit unit in unitList)
            {
                Destroy(unit.gameObject);
            }
            unitList.Clear();
        }

        private void ResetMobPos()
        {
            foreach (Unit unit in unitList)
            {
                float spawnX = Random.Range(spawnAreaX.x, spawnAreaX.y);
                float spawnZ = Random.Range(spawnAreaZ.x, spawnAreaZ.y);

                unit.ResetSpawnPos(spawnX, spawnZ);
            }
        }
    }
}