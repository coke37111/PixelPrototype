using Assets.Scripts.Feature.Bomberman;
using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cube
{
    public class BombCube : Cube
    {
        public float expTime;
        public Vector3 expRange;
        public float expDamage;
        public LayerMask rayCastLayer;

        private float curExpTime;
        private bool isExplosion;

        private BoxCollider coll;

        private bool endExpF, endExpB, endExpU, endExpD, endExpL, endExpR = false;
        private List<Collider> enterColl = new List<Collider>();

        #region UNITY

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (curExpTime >= expTime)
            {
                Explosion();                
            }
            else
            {
                curExpTime += Time.deltaTime;
            }

            if (endExpF && endExpB &&
                endExpU && endExpD &&
                endExpL && endExpR)
            {
                DestroyCube();
            }

            // TODO : FOR Test
            //if (Input.GetKeyDown(KeyCode.A))
            //{
            //    isExplosion = false;
            //    Explosion();
            //}
            //int rayCnt = 4;
            //Vector3 collCenter = coll.bounds.center;
            //Vector3 collExtent = coll.bounds.extents * 0.85f;
            //for (int i = 0; i < rayCnt; i++)
            //{
            //    if (i == 0 || i == rayCnt - 1)
            //        continue;

            //    float rayStartX = coll.bounds.min.x + coll.bounds.size.x / (rayCnt - 1) * i;
            //    float rayStartZ = coll.bounds.min.z + coll.bounds.size.z / (rayCnt - 1) * i;

            //    Vector3 rayOrgF = new Vector3(rayStartX, collCenter.y, collCenter.z + collExtent.z);
            //    Vector3 rayOrgB = new Vector3(rayStartX, collCenter.y, collCenter.z - collExtent.z);
            //    Vector3 rayOrgU = new Vector3(rayStartX, collCenter.y + collExtent.y, collCenter.z);
            //    Vector3 rayOrgD = new Vector3(rayStartX, collCenter.y - collExtent.y, collCenter.z);
            //    Vector3 rayOrgL = new Vector3(collCenter.x - collExtent.x, collCenter.y, rayStartZ);
            //    Vector3 rayOrgR = new Vector3(collCenter.x + collExtent.x, collCenter.y, rayStartZ);

            //    Debug.DrawRay(rayOrgF, Vector3.forward * expRange.z, Color.red);
            //    Debug.DrawRay(rayOrgB, Vector3.back * expRange.z, Color.red);
            //    Debug.DrawRay(rayOrgU, Vector3.up * expRange.y, Color.red);
            //    Debug.DrawRay(rayOrgD, Vector3.down * expRange.y, Color.red);
            //    Debug.DrawRay(rayOrgL, Vector3.left * expRange.x, Color.red);
            //    Debug.DrawRay(rayOrgR, Vector3.right * expRange.x, Color.red);
            //}
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                if (!enterColl.Contains(other))
                    enterColl.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.tag == "Player")
            {
                if (enterColl.Contains(other))
                    enterColl.Remove(other);

                if (enterColl.Count <= 0)
                    coll.isTrigger = false;
            }
        }

        #endregion

        private void Init()
        {
            coll = GetComponent<BoxCollider>();

            curExpTime = 0f;
            isExplosion = false;
            coll.isTrigger = true;
        }

        public void Explosion()
        {
            if (isExplosion)
            {
                return;
            }
            isExplosion = true;

            MakeExpEff(transform.position);
            StartCoroutine(StartExplosion(Vector3.forward));
            StartCoroutine(StartExplosion(Vector3.back));
            StartCoroutine(StartExplosion(Vector3.up));
            StartCoroutine(StartExplosion(Vector3.down));
            StartCoroutine(StartExplosion(Vector3.left));
            StartCoroutine(StartExplosion(Vector3.right));
        }

        private IEnumerator StartExplosion(Vector3 dir, int rayCnt = 6)
        {
            if (rayCnt >= 3)
            {
                float newRange = GetCollisionRange(dir, rayCnt);

                int rangeToInt = (int)newRange;
                for (int i = 0; i < rangeToInt; i++)
                {
                    MakeExpEff(transform.position + dir * (i + 1));
                    yield return new WaitForSeconds(.05f);
                }
            }

            if (dir == Vector3.forward) endExpF = true;
            else if (dir == Vector3.back) endExpB = true;
            else if (dir == Vector3.up) endExpU = true;
            else if (dir == Vector3.down) endExpD = true;
            else if (dir == Vector3.right) endExpR = true;
            else if (dir == Vector3.left) endExpL = true;
        }

        private void MakeExpEff(Vector3 pos)
        {
            GameObject pfExpEff = ResourceManager.LoadAsset<GameObject>($"Prefab/BomberMan/Effect/EffExplosion");
            GameObject goExpEff = Instantiate(pfExpEff, pos, Quaternion.identity, null);
            BombermanExplosion bExp = goExpEff.GetComponent<BombermanExplosion>();
            bExp.SetDamage(expDamage);

            Destroy(goExpEff, 1f);
        }

        private float GetCollisionRange(Vector3 dir, int rayCnt)
        {
            float newRange = 0f;

            Vector3 collCenter = coll.bounds.center;
            Vector3 collExtent = coll.bounds.extents * 0.85f;
            for (int i = 0; i < rayCnt; i++)
            {
                if (i == 0 || i == rayCnt - 1)
                    continue;

                float rayStart = 0f;
                if (dir == Vector3.forward || dir == Vector3.back ||
                    dir == Vector3.up || dir == Vector3.down)
                {
                    rayStart = coll.bounds.min.x + coll.bounds.size.x / (rayCnt - 1) * i;
                }
                else if (dir == Vector3.left || dir == Vector3.right)
                {
                    rayStart = coll.bounds.min.z + coll.bounds.size.z / (rayCnt - 1) * i;
                }

                Vector3 rayOrg = Vector3.zero;
                float curExpRange = 0f;
                if (dir == Vector3.forward)
                {
                    rayOrg = new Vector3(rayStart, collCenter.y, collCenter.z + collExtent.z);
                    curExpRange = expRange.z;
                }
                else if (dir == Vector3.back)
                {
                    rayOrg = new Vector3(rayStart, collCenter.y, collCenter.z - collExtent.z);
                    curExpRange = expRange.z;
                }
                else if (dir == Vector3.up)
                {
                    rayOrg = new Vector3(rayStart, collCenter.y + collExtent.y, collCenter.z);
                    curExpRange = expRange.y;
                }
                else if (dir == Vector3.down)
                {
                    rayOrg = new Vector3(rayStart, collCenter.y - collExtent.y, collCenter.z);
                    curExpRange = expRange.y;
                }
                else if (dir == Vector3.right)
                {
                    rayOrg = new Vector3(collCenter.x + collExtent.x, collCenter.y, rayStart);
                    curExpRange = expRange.x;
                }
                else if (dir == Vector3.left)
                {
                    rayOrg = new Vector3(collCenter.x - collExtent.x, collCenter.y, rayStart);
                    curExpRange = expRange.x;
                }

                newRange = curExpRange;
                if (Physics.Raycast(rayOrg, dir, out RaycastHit hit, curExpRange, rayCastLayer))
                {
                    if(hit.collider.GetComponent<BombCube>())
                    {
                        continue;
                    }

                    if (dir == Vector3.forward || dir == Vector3.back)
                    {
                        newRange = Mathf.Abs(hit.point.z - transform.position.z);
                    }
                    else if (dir == Vector3.up || dir == Vector3.down)
                    {
                        newRange = Mathf.Abs(hit.point.y - transform.position.y);
                    }
                    else if (dir == Vector3.right || dir == Vector3.left)
                    {
                        newRange = Mathf.Abs(hit.point.x - transform.position.x);
                    }
                    break;
                }
            }

            return newRange;
        }
    }
}