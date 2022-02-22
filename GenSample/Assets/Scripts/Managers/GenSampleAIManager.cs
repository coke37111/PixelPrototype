using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Assets.Scripts.Settings.RoomSettings;

namespace Assets.Scripts.Managers
{
    public class GenSampleAIManager : MonoBehaviour
    {
        public LayerMask ignoreClickLayer;

        [Range(1, 100)]
        public int unitGenCount = 1;

        private GenSampleManager genSampleManager;
        private List<UnitAI> unitAIList = new List<UnitAI>();
        private Transform unitContainer;
        private UnitLocalPlayer unitLocalPlayer;
        private MobController mobCtrl;

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
                ResetMobHp();
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
            GameObject pfUnit = ResourceManager.LoadAsset<GameObject>("Prefab/Unit/AIUnit");
            UnitAI unitComp;

            for (int i = 0; i < unitGenCount; i++)
            {
                if (unitAIList.Count <= i)
                {
                    GameObject goUnit = Instantiate(pfUnit, unitContainer);
                    unitComp = goUnit.GetComponent<UnitAI>();

                    if (unitComp != null)
                        unitAIList.Add(unitComp);
                }
                else
                {
                    unitComp = unitAIList[i];
                }

                if (unitComp == null)
                {
                    Log.Error($"UnitAI Component cannot find!");
                    return;
                }

                unitComp.Init();

                PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);                
                if(!UnitSettings.useSpine())
                {
                    //Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict(playerUnitSetting.GetUnitType());
                    //unitComp.SetSprite(selectUnitParts);
                }
                else
                    unitComp.MakeSpine(playerUnitSetting.GetSpinePath());
            }
        }

        private void DestroyAllAIUnit()
        {
            foreach (UnitAI unit in unitAIList)
            {
                Destroy(unit.gameObject);
            }
            unitAIList.Clear();
        }

        private void ResetAIUnitPos()
        {
            Vector2 spawnAreaX = genSampleManager.GetSpawnAreaX();
            Vector2 spawnAreaZ = genSampleManager.GetSpawnAreaZ();

            foreach (UnitAI unit in unitAIList)
            {
                float spawnX = UnityEngine.Random.Range(spawnAreaX.x, spawnAreaX.y);
                float spawnZ = UnityEngine.Random.Range(spawnAreaZ.x, spawnAreaZ.y);

                unit.ResetSpawnPos(new Vector3(spawnX, initSpawnHeight, spawnZ));
            }
        }

        private void ResetPlayerPos()
        {
            Vector3 initPos = new Vector3(0, initSpawnHeight, -1f);
            unitLocalPlayer.ResetSpawnPos(initPos);
        }
        public void SpawnPlayer()
        {
            Vector3 initPos = new Vector3(0, initSpawnHeight, -1f);

            PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);            

            GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>("Prefab/Unit/LocalPlayer");
            GameObject goPlayer = Instantiate(pfPlayer, initPos, Quaternion.identity, unitContainer);
            unitLocalPlayer = goPlayer.GetComponent<UnitLocalPlayer>();
            unitLocalPlayer.Init();
            if (!UnitSettings.useSpine())
            {
                //Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict(playerUnitSetting.GetUnitType());
                //unitLocalPlayer.SetSprite(selectUnitParts);
            }
            else
                unitLocalPlayer.MakeSpine(playerUnitSetting.GetSpinePath());

            //CameraController.Instance.SetOwner(unitLocalPlayer);
        }
        private void GenerateMob()
        {
            if (RoomSettings.roomType != ROOM_TYPE.Raid)
                return;

            Vector3 initPos = new Vector3(0f, 1f, 0f);
            string pfMobPath = "Prefab/Mob";

            GameObject pfMob = ResourceManager.LoadAsset<GameObject>(pfMobPath);
            GameObject goMob = Instantiate(pfMob, initPos, Quaternion.identity);
            mobCtrl = goMob.GetComponent<MobController>();
            mobCtrl.Init();
        }

        private void ResetMobHp()
        {
            mobCtrl.ResetHp();
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
            //indicator.RegisterKnockbackListener(Knockback);
        }
        public void Knockback(Vector3 center)
        {
            unitLocalPlayer.Knockback(center.x, center.z);
            foreach (UnitAI unit in unitAIList)
            {
                unit.Knockback(center.x, center.z);
            }
        }
    }
}