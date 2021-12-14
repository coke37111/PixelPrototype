using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DummyManager : MonoBehaviour
{
    void Start()
    {
        var data = new List<object>();
        data.Add(Random.Range(0, 1.0f));
        data.Add(Random.Range(0, 1.0f));
        data.Add(Random.Range(0, 1.0f));
        PhotonNetwork.Instantiate(Path.Combine("Prefab", "DummyPlayer"), Vector3.zero, Quaternion.identity, 0, data.ToArray());
    }
}
