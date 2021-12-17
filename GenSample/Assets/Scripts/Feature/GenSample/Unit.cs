using Assets.Scripts.Managers;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class Unit : MonoBehaviour
    {
        public enum UNIT_STATE
        {
            IDLE,
            MOVE,
        }

        private UNIT_STATE unitState;

        private readonly float MAX_IDLE_TIME = 1f;
        private float curIdleTime;
        private float idleDelay;

        protected bool isLeftDir;
        private Vector3 spawnPos;
        protected Vector3 targetPos;
        public LayerMask ignoreClickLayer;
        public LayerMask knockbackLayer;

        public SpriteRenderer srHair2;
        public SpriteRenderer srSkin;
        public SpriteRenderer srHair1;
        public SpriteRenderer srCos;
        public SpriteRenderer srHat;
        public SpriteRenderer srWpShild;
        public SpriteRenderer srWp;

        protected Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public virtual void Init()
        {
            unitState = UNIT_STATE.IDLE;
            curIdleTime = GetIdleTime();

            isLeftDir = Random.Range(0f, 1f) >= .5f;
            SetDir();
        }

        protected virtual void Update()
        {
            switch (unitState)
            {
                case UNIT_STATE.IDLE:
                    {
                        if (idleDelay >= curIdleTime)
                        {
                            targetPos = transform.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                            unitState = UNIT_STATE.MOVE;

                            idleDelay = 0f;
                            curIdleTime = GetIdleTime();
                        }
                        else
                        {
                            idleDelay += Time.deltaTime;
                        }
                        break;
                    }
                case UNIT_STATE.MOVE:
                    {
                        Vector3 nextPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime);
                        if (Vector3.Distance(transform.position, targetPos) <= .1f || OutOfRangePos(nextPos))
                        {
                            unitState = UNIT_STATE.IDLE;
                        }
                        else
                        {
                            isLeftDir = transform.position.x > targetPos.x;
                            SetDir();

                            transform.position = nextPos;
                        }
                        break;
                    }
            }
        }

        private bool OutOfRangePos(Vector3 pos)
        {
            return Vector3.Distance(spawnPos, pos) > 1f;
        }

        private float GetIdleTime()
        {
            return Random.Range(0f, MAX_IDLE_TIME);
        }

        public void SetSprite(Dictionary<string, string> unitPartList)
        {
            foreach (string unitPartName in unitPartList.Keys)
            {
                string resPath = unitPartList[unitPartName];

                Sprite resultSprite = ResourceManager.LoadAsset<Sprite>(resPath);
                switch (unitPartName)
                {
                    case "cos":
                        {
                            srCos.sprite = resultSprite;
                            break;
                        }
                    case "hair1":
                        {
                            srHair1.sprite = resultSprite;
                            break;
                        }
                    case "hair2":
                        {
                            srHair2.sprite = resultSprite;
                            break;
                        }
                    case "skin":
                        {
                            srSkin.sprite = resultSprite;
                            break;
                        }
                    case "hat":
                        {
                            srHat.sprite = resultSprite;
                            break;
                        }
                    case "wp_1":
                        {
                            srWp.sprite = resultSprite;
                            break;
                        }
                    case "wp_shild":
                        {
                            srWpShild.sprite = resultSprite;
                            break;
                        }
                }
            }
        }

        public virtual void ResetSpawnPos(float posX, float posY, float posZ)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            spawnPos = new Vector3(posX, posY, posZ);
            transform.position = spawnPos;

            unitState = UNIT_STATE.IDLE;
            curIdleTime = GetIdleTime();

            isLeftDir = Random.Range(0f, 1f) >= .5f;
            SetDir();
        }

        public Vector3 GetSpawnPos()
        {
            return spawnPos;
        }

        public void SetDir()
        {
            srHair2.flipX = !isLeftDir;
            srSkin.flipX = !isLeftDir;
            srHair1.flipX = !isLeftDir;
            srCos.flipX = !isLeftDir;
            srHat.flipX = !isLeftDir;
            srWpShild.flipX = !isLeftDir;
            srWp.flipX = !isLeftDir;
        }

        public virtual void Knockback(float centerX, float centerZ)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10000f, knockbackLayer))
            {
                if (hit.collider.tag == "Indicator")
                {
                    Vector3 diffPos = transform.position - new Vector3(centerX, transform.position.y, centerZ);
                    Vector3 dir = diffPos.normalized;

                    rb.AddForce(dir * 1000f);
                }
            }         
        }
    }
}