using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DummyManager : MonoBehaviour
{
    public DummyPlayer _playerPrefab;
    
    private void Awake()
    {
    }

    void Start()
    {
        var obj = PhotonNetwork.Instantiate(Path.Combine("Prefab", "DummyPlayer"), Vector3.zero, Quaternion.identity, 0);
        

        //obj.GetComponent<DummyPlayer>().Player = PhotonNetwork.LocalPlayer;

        //var root = GetComponent<Canvas>().gameObject;
        //foreach (var player in PhotonNetwork.PlayerList)
        //{
            

        //    var obj = GameObject.Instantiate<GameObject>(_playerPrefab.gameObject, root.transform);            
            
        //}
    }
        
    void Update()
    {
    }
}
