using Assets.Scripts.Managers;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.UI
{
    public class CubeSlotController : MonoBehaviour
    {
        public int slotCnt = 8;
        public Transform slotContainer;

        private readonly Dictionary<string, string> tileNameBySlotDict = 
            new Dictionary<string, string>();

        private SandboxManager sbManager;
        private List<string> existCubeNames = new List<string>();
        private List<CubeSlot> curCubeSlots = new List<CubeSlot>();
        private int curPage;
        private int totalPage;
        private bool activeSlotUI;
        private CubeTileSettingSO cubeTileSetting;

        #region UNITY

        private void Start()
        {
            curPage = 0;            
        }

        private void Update()
        {
            if (!activeSlotUI)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectSlot(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SelectSlot(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SelectSlot(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SelectSlot(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SelectSlot(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SelectSlot(7);
            }else if (Input.GetKeyDown(KeyCode.Tab))
            {
                ChangePage();
            }
        }

        #endregion

        public void Build(SandboxManager sbManager)
        {
            this.sbManager = sbManager;
            cubeTileSetting = ResourceManager.LoadAsset<CubeTileSettingSO>($"Setting/CubeTileSetting");
            foreach(CubeTileData cubeTileData in cubeTileSetting.cubeTileData)
            {
                if (tileNameBySlotDict.ContainsKey(cubeTileData.cubeName))
                {
                    Log.Error($"Already Exist Cube in Dict : {cubeTileData.cubeName}");
                    continue;
                }

                tileNameBySlotDict.Add(cubeTileData.cubeName, cubeTileData.tileName);
            }

            ShowSlotUI();

            CheckExistCubes();

            MakeEmptySlots();
            RefreshSlots();
        }

        private void CheckExistCubes()
        {
            GameObject[] pfCubes = ResourceManager.LoadAssets<GameObject>("Prefab/Sandbox/Cube");
            foreach (GameObject pfCube in pfCubes)
            {
                if (tileNameBySlotDict.ContainsKey(pfCube.name))
                {
                    existCubeNames.Add(pfCube.name);
                }
            }
            totalPage = Mathf.Max(0, (existCubeNames.Count - 1) / slotCnt);
        }

        private void MakeEmptySlots()
        {
            GameObject pfCubeSlot = ResourceManager.LoadAsset<GameObject>("Prefab/UI/Sandbox/CubeSlot");
            for (int i = 0; i < slotCnt; i++)
            {
                GameObject goCubeSlot = Instantiate(pfCubeSlot, slotContainer);
                goCubeSlot.SetActive(false);
                CubeSlot cubeSlot = goCubeSlot.GetComponent<CubeSlot>();
                curCubeSlots.Add(cubeSlot);
            }
        }

        private void RefreshSlots()
        {
            curCubeSlots.ForEach(e =>
            {
                e.DeselectSlot();
                e.gameObject.SetActive(false);
            });

            int curNum = 0;
            for (int i = curPage * slotCnt; i < existCubeNames.Count; i++)
            {
                if (curNum >= slotCnt)
                    break;

                if (curNum >= curCubeSlots.Count)
                    break;

                string cubeName = existCubeNames[i];
                CubeSlot cubeSlot = curCubeSlots[curNum];
                cubeSlot.gameObject.SetActive(true);
                cubeSlot.Build(curNum, tileNameBySlotDict[cubeName]);
                cubeSlot.RegisterSelectListener(cubeName, (cubeName) =>
                {
                    curCubeSlots.ForEach(e => e.DeselectSlot());
                    sbManager.SetNextCube(cubeName);
                });
                if (curNum == 0)
                    cubeSlot.SelectSlot();

                curNum++;
            }
        }

        private void SelectSlot(int num)
        {
            if (num >= curCubeSlots.Count)
                return;

            if (curCubeSlots[num] == null)
                return;

            if (!curCubeSlots[num].gameObject.activeSelf)
                return;

            curCubeSlots.ForEach(e => e.DeselectSlot());
            curCubeSlots[num].SelectSlot();
        }

        private void ChangePage()
        {
            curPage++;
            if (curPage > totalPage)
                curPage = 0;

            RefreshSlots();
        }

        public void ShowSlotUI()
        {
            if (activeSlotUI)
                return;

            activeSlotUI = true;
            gameObject.SetActive(activeSlotUI);
        }

        public void HideSlotUI()
        {
            if (!activeSlotUI)
                return;

            activeSlotUI = false;
            gameObject.SetActive(activeSlotUI);
        }
    }
}