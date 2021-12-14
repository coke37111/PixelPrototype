using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyManager : MonoBehaviour
{
    public DummyPlayer _playerPrefab;

    private void Awake()
    {
    }

    void Start()
    {
        var root = GetComponent<Canvas>().gameObject;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var obj = GameObject.Instantiate<GameObject>(_playerPrefab.gameObject, root.transform);
            obj.GetComponent<DummyPlayer>().Player = player;
        }
    }
        
    void Update()
    {
    }
}
