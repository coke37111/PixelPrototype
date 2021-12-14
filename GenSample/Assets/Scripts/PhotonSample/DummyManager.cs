using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DummyManager : MonoBehaviour
{
    void Start()
    {
        var data = new List<System.Object>();
        data.Add(new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1)));
        PhotonNetwork.Instantiate(Path.Combine("Prefab", "DummyPlayer"), Vector3.zero, Quaternion.identity, 0, data.ToArray());
    }
}
