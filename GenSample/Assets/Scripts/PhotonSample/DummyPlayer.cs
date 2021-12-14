using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DummyPlayer : MonoBehaviour, IPunInstantiateMagicCallback
{
    public PhotonView PhotonView
    {
        get => gameObject.GetComponent<PhotonView>();
    }

    public const float _moveSpeed = 100.0f;

    void Update()
    {
        if (PhotonView.Owner != PhotonNetwork.LocalPlayer)
            return;

        Vector3 delta = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            delta.y += (_moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            delta.y -= (_moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            delta.x -= (_moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            delta.x += (_moveSpeed * Time.deltaTime);

        transform.position += delta;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {        
        info.photonView.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
        info.photonView.transform.localPosition = Vector3.zero;
        GetComponent<Image>().color = (Color)info.photonView.InstantiationData[0];
    }
}
