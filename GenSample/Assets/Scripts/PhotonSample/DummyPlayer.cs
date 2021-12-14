using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    public Player Player { get; set; }
    private PhotonView _photonView;
    public const float _moveSpeed = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _photonView.ViewID = Player.ActorNumber;
    }

    // Update is called once per frame
    void Update()
    {
        if (_photonView.AmOwner == false)
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

}
