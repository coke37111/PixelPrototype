using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyManager : MonoBehaviour
{
    public GameObject _controlTarget;
    public const float _moveSpeed = 100.0f;

    private void Awake()
    {
    }
    void Start()
    {
    }
        
    void Update()
    {

        Vector3 delta = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            delta.y += (_moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            delta.y -= (_moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            delta.x -= (_moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            delta.x += (_moveSpeed * Time.deltaTime);

        _controlTarget.transform.position += delta;
    }
}
