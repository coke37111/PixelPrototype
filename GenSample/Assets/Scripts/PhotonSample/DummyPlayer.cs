using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    private Player _player;
    private PhotonView _photonView;

    public PhotonView PhotonView
    {
        get
        {
            if (_photonView == null)
                _photonView = GetComponent<PhotonView>();
            return _photonView;
        }
    }
    public Player Player 
    { 
        get => _player; 
        set
        {
            _player = value;
            PhotonView.ViewID = _player.ActorNumber;
        }
    }
    
    public const float _moveSpeed = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonView.AmOwner == false)
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
