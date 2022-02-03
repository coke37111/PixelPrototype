using Assets.Scripts.Feature.Main;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Settings;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class TemplateManager : MonoBehaviour
    {
        private CubeContainer cubeContainer;
        private BombermanCameraController camCtrl;

        public SandboxMapDataSO mapData;

        // Use this for initialization
        void Start()
        {
            // TODO : 임시, Spine에서 UnitSpinePart 컴포넌트 다 떼야함
            RoomSettings.roomType = RoomSettings.ROOM_TYPE.Bomberman;

            // GenerateMap By Data
            {
                cubeContainer = FindObjectOfType<CubeContainer>();
                if(cubeContainer != null)
                    cubeContainer.GenerateCubes(mapData);
            }

            // Make Player
            {
                GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>(PrefabPath.PlayerPath);
                if (pfPlayer != null)
                {
                    GameObject goPlayer = Instantiate(pfPlayer, Vector3.up, Quaternion.identity);

                    camCtrl = FindObjectOfType<BombermanCameraController>();
                    if (camCtrl != null)
                        camCtrl.SetTarget(goPlayer.transform);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}