﻿using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Managers;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman.Unit
{
    public class PlayerController : MonoBehaviour
    {
        public float speed = 3f;

        private Transform trSpine;
        private Animator anim;
        private GameObject pfBomb;
        private Transform unitContainer; // TODO : SetParent를 위한 Component
        
        #region UNITY

        // Use this for initialization
        void Start()
        {
            SkeletonMecanim skelM = GetComponentInChildren<SkeletonMecanim>();
            trSpine = skelM.transform;
            anim = skelM.GetComponent<Animator>();

            pfBomb = ResourceManager.LoadAsset<GameObject>("Prefab/BomberMan/Bomb/Bomb");
            unitContainer = FindObjectOfType<UnitContainer>().transform;
        }

        // Update is called once per frame
        void Update()
        {
            Move();
            MakeBomb();
        }

        #endregion

        private void Move()
        {
            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                dir += Vector3.left;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                dir += Vector3.right;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                dir += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                dir = Vector3.back;
            }            

            transform.Translate(dir * speed * Time.deltaTime);

            // TODO : 좌우전환
            {
                Vector3 spineScale = trSpine.localScale;
                if (dir.x > 0)
                {
                    spineScale.x = -Mathf.Abs(spineScale.x);
                }
                else if (dir.x < 0)
                {
                    spineScale.x = Mathf.Abs(spineScale.x);
                }
                trSpine.localScale = spineScale;
            }

            // TODO : animation
            {
                bool isMove = dir != Vector3.zero;
                anim.SetBool("isMove", isMove);
            }            
        }

        private void MakeBomb()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {                
                Vector3 bombPos = new Vector3(Mathf.RoundToInt(transform.position.x), 0.75f, Mathf.RoundToInt(transform.position.z));
                GameObject goBomb = Instantiate(pfBomb, bombPos, Quaternion.identity, unitContainer);
                Bomb bomb = goBomb.GetComponent<Bomb>();
                bomb.Build(new Vector2(6f, 6f), 3f);
            }
        }
    }
}