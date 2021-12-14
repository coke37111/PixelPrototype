using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    private Player _player;

    public Player Player 
    { 
        get => _player; 
        set
        {
            _player = value;
            _photonView.ViewID = Player.ActorNumber;
        }
    }
    private PhotonView _photonView;
    public const float _moveSpeed = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();        
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
