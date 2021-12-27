using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class GenSampleAIManager : MonoBehaviour
    {
        public LayerMask ignoreClickLayer;

        [Range(1, 100)]
        public int unitGenCount = 1;

        private GenSampleManager genSampleManager;
        private List<Unit> unitList = new List<Unit>();
        private Transform unitContainer;
        private UnitController unitCtrl;

        private readonly float initSpawnHeight = 1f;

        public void Build(GenSampleManager genSampleManager)
        {
            this.genSampleManager = genSampleManager;
            unitContainer = FindObjectOfType<UnitContainer>().transform;

            DestroyAllAIUnit();
            GenerateAIUnit();
            ResetAIUnitPos();

            SpawnPlayer();
            GenerateMob();
        }

        public void Proc()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ResetAIUnitPos();
                ResetPlayerPos();
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                DestroyAllAIUnit();
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                GenerateAIUnit();
                ResetAIUnitPos();
            }

            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10000f, ~ignoreClickLayer))
                {
                    MakeIndicator(hit.point);
                }
            }
        }

        private void GenerateAIUnit()
        {
            GameObject pfUnit = ResourceManager.LoadAsset<GameObject>("Prefab/AIUnit");
            Unit unitComp;

            for (int i = 0; i < unitGenCount; i++)
            {
                if (unitList.Count <= i)
                {
                    GameObject goUnit = Instantiate(pfUnit, unitContainer);
                    unitComp = goUnit.GetComponent<Unit>();

                    if (unitComp != null)
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
                unitComp.SetSprite(UnitSettings.GetSelectUnitPartDict());
            }
        }

        private void DestroyAllAIUnit()
        {
            foreach (Unit unit in unitList)
            {
                Destroy(unit.gameObject);
            }
            unitList.Clear();
        }

        private void ResetAIUnitPos()
        {
            Vector2 spawnAreaX = genSampleManager.GetSpawnAreaX();
            Vector2 spawnAreaZ = genSampleManager.GetSpawnAreaZ();

            foreach (Unit unit in unitList)
            {
                float spawnX = UnityEngine.Random.Range(spawnAreaX.x, spawnAreaX.y);
                float spawnZ = UnityEngine.Random.Range(spawnAreaZ.x, spawnAreaZ.y);

                unit.ResetSpawnPos(spawnX, initSpawnHeight, spawnZ);
            }
        }

        private void ResetPlayerPos()
        {
            Vector3 initPos = new Vector3(0, initSpawnHeight, -1f);
            unitCtrl.ResetSpawnPos(initPos);
        }
        public void SpawnPlayer()
        {
            Vector3 initPos = new Vector3(0, initSpawnHeight, -1f);
            Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict();

            GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>("Prefab/Player");
            GameObject goPlayer = Instantiate(pfPlayer, initPos, Quaternion.identity, unitContainer);
            unitCtrl = goPlayer.GetComponent<UnitController>();
            unitCtrl.SetSprite(selectUnitParts);
            unitCtrl.Init(false);

            CameraController.Instance.SetOwner(unitCtrl);
        }
        private void GenerateMob()
        {
            Vector3 initPos = new Vector3(0f, 1f, 0f);
            string pfMobPath = "Prefab/Mob";

            GameObject pfMob = ResourceManager.LoadAsset<GameObject>(pfMobPath);
            GameObject goMob = Instantiate(pfMob, initPos, Quaternion.identity);
            goMob.GetComponent<MobController>().Init();
        }

        public void MakeIndicator(Vector3 hitPoint)
        {
            string pfPath = Path.Combine("Prefab", "Indicator");
            Vector3 initPos = new Vector3(hitPoint.x, 0f, hitPoint.z);

            float limitTime = 2f;
            float scaleX = 3f;
            float scaleZ = 3f;

            GameObject pfIndicator = ResourceManager.LoadAsset<GameObject>(pfPath);
            GameObject goIndicator = Instantiate(pfIndicator, initPos, Quaternion.identity, unitContainer);
            Indicator indicator = goIndicator.GetComponent<Indicator>();
            indicator.Init(limitTime, scaleX, scaleZ);
            indicator.RegisterKnockbackListener(Knockback);
        }
        public void Knockback(Vector3 center)
        {
            unitCtrl.Knockback(center.x, center.z);
            foreach (Unit unit in unitList)
            {
                unit.Knockback(center.x, center.z);
            }
        }
    }
}