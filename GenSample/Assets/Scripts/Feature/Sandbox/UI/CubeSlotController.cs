using Assets.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.UI
{
    public class CubeSlotController : MonoBehaviour
    {
        public int slotCnt = 8;
        public Transform slotContainer;

        private readonly Dictionary<string, string> tileNameBySlotDict = new Dictionary<string, string>
        {
            {"ClimbCube", "Tile_grass_01"},
            //{"Cube_dirt_01", "Tile_dirt_01"},
            //{"Cube_dirt_02", "Tile_dirt_02"},
            //{"Cube_grass_01", "Tile_grass_01"},
            //{"Cube_grass_02", "Tile_grass_02"},
            //{"Cube_grass_03", "Tile_grass_03"},
            //{"Cube_grass_04", "Tile_grass_04"},
            //{"Cube_rock_01", "Tile_rock_01"},
            //{"Cube_rock_02", "Tile_rock_02"},
            {"DamageCube", "Tile_grass_01"},
            {"GroundCube", "Tile_grass_01"},
            {"IceCube", "tile_ice_01"},
        };

        private SandboxManager sbManager;
        private List<CubeSlot> curCubeSlots = new List<CubeSlot>();

        #region UNITY

        private void Update()
        {
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
            }
        }

        #endregion

        public void Build(SandboxManager sbManager)
        {
            this.sbManager = sbManager;

            GameObject[] pfCubes = ResourceManager.LoadAssets<GameObject>("Prefab/Sandbox/Cube");
            GameObject pfCubeSlot = ResourceManager.LoadAsset<GameObject>("Prefab/UI/Sandbox/CubeSlot");

            int curNum = 0;
            foreach(GameObject pfCube in pfCubes)
            {
                if (curNum >= slotCnt)
                    break;

                if (tileNameBySlotDict.ContainsKey(pfCube.name))
                {
                    GameObject goCubeSlot = Instantiate(pfCubeSlot, slotContainer);
                    CubeSlot cubeSlot = goCubeSlot.GetComponent<CubeSlot>();
                    cubeSlot.Build(curNum, tileNameBySlotDict[pfCube.name]);
                    cubeSlot.RegisterSelectListener(pfCube.name, sbManager.SetNextCube);
                    if (curNum == 0)
                        cubeSlot.SelectSlot();

                    curCubeSlots.Add(cubeSlot);

                    curNum++;
                }
            }
        }

        private void SelectSlot(int num)
        {
            if (num >= curCubeSlots.Count)
                return;

            if (curCubeSlots[num] == null)
                return;

            curCubeSlots.ForEach(e => e.DeselectSlot());
            curCubeSlots[num].SelectSlot();
        }
    }
}