﻿using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature
{
    public class BomberManBlock : MonoBehaviour
    {

        #region UNITY

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        public Vector2Int GetPosition()
        {
            Vector3 pos = transform.position;
            return new Vector2Int((int)pos.x, (int)pos.z);
        }

    }
}