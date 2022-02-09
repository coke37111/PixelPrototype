using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class EditCube : MonoBehaviour, IPunInstantiateMagicCallback, IOnEventCallback
    {
        private string cubeId;
        private List<Collider> collObjs = new List<Collider>();
        private PhotonView photonView;

        #region UNITY

        private void OnTriggerEnter(Collider other)
        {
            if (!collObjs.Contains(other))
                collObjs.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (collObjs.Contains(other))
                collObjs.Remove(other);
        }

        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #endregion

        #region PUN_METHOD

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            CubeContainer cubeContainer = FindObjectOfType<CubeContainer>();
            transform.SetParent(cubeContainer.transform);

            string buildCubeId = info.photonView.InstantiationData[0].ToString();
            Build(buildCubeId);
        }

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;
            object[] data = (photonEvent.CustomData != null) ? photonEvent.CustomData as object[] : null;

            switch (eventCodeType)
            {
                case EventCodeType.DestroyNormalBlock:
                    {
                        int senderViewId = (int)data[0];
                        if (photonView.ViewID != senderViewId)
                            return;

                        if (!photonView.IsMine)
                            return;

                        PhotonNetwork.Destroy(photonView);
                        break;
                    }
            }
        }

        #endregion

        public void Build(string cubeId)
        {
            photonView = GetComponent<PhotonView>();

            this.cubeId = cubeId;

            GameObject pfRealCube = ResourceManager.LoadAsset<GameObject>($"Prefab/Main/Cube/{cubeId}");
            GameObject goRealCube = Instantiate(pfRealCube, transform.position, Quaternion.identity, transform);

            Cube realCube = goRealCube.GetComponent<Cube>();
            realCube.SetDestroyCallback(DestroyCube);
        }

        public string GetCubeId()
        {
            return cubeId;
        }

        public void MakeRealCube()
        {
            if (!CanMakeRealCube())
                return;

            Transform parent = FindObjectOfType<CubeContainer>().transform;
            GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
            GameObject goCubeRoot = Instantiate(pfCubeRoot, GetPosition(), Quaternion.identity, parent);
            EditCube cubeRoot = goCubeRoot.GetComponent<EditCube>();
            cubeRoot.Build(cubeId);
        }

        private bool CanMakeRealCube()
        {
            return collObjs.Count <= 0;
        }

        public void ClearCollObjs()
        {
            collObjs.Clear();
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void DestroyCube()
        {
            if(cubeId == "NormalCube")
            {
                MakeItem();

                if (PlayerSettings.IsConnectNetwork())
                {
                    PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.DestroyNormalBlock, ReceiverGroup.All, new object[]
                    {
                    photonView.ViewID
                    });

                    return;
                }
            }

            Destroy(gameObject);
        }

        private void MakeItem()
        {
            string itemPath = Random.Range(0f, 1f) > .5f ? PrefabPath.ItemPowerPath : PrefabPath.ItemRangePath;
            if (PlayerSettings.IsConnectNetwork())
            {
                PhotonEventManager.RaiseEvent(EventCodeType.MakeItem, ReceiverGroup.MasterClient, new object[]{
                    GetPosition(), itemPath
                });
            }
            else
            {
                GameObject pfItem = ResourceManager.LoadAsset<GameObject>(itemPath);
                Instantiate(pfItem, GetPosition(), Quaternion.identity, null);
            }
        }
    }
}