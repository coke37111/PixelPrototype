using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class BombCube : CubeBase
    {
        protected override CUBE_TYPE cubeType => CUBE_TYPE.BombCube;
                
        [Header("- 폭발 범위(X-Z)")]
        public float range;        
        public float time;        
        public float power;
        [Header("- 폭발 범위 감지 Layer")]
        public LayerMask explosionCheckLayer;

        private float curTime;
        private bool isExplosion = false;

        private bool endExpL = false;
        private bool endExpR = false;
        private bool endExpF = false;
        private bool endExpB = false;

        private GameObject baseBomb;

        #region UNITY

        private void Start()
        {
            baseBomb = transform.Find("bomb").gameObject;
        }

        private void Update()
        {
            if (!isExplosion)
            {
                if (curTime >= time)
                {
                    Explosion();
                }
                else
                {
                    curTime += Time.deltaTime;
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                isExplosion = false;
                Explosion();
            }

            if (endExpL && endExpR && endExpF && endExpB)
                Destroy(gameObject);
        }

        #endregion

        public void Explosion()
        {
            if (isExplosion)
                return;

            isExplosion = true;

            if (baseBomb != null)
                baseBomb.SetActive(false);

            MakeExplosionEff(transform.position, "EffExplosion");
            StartCoroutine(StartExplosion(Vector3.left));
            StartCoroutine(StartExplosion(Vector3.right));
            StartCoroutine(StartExplosion(Vector3.forward));
            StartCoroutine(StartExplosion(Vector3.back));
        }

        private IEnumerator StartExplosion(Vector3 dir)
        {
            Vector3 orgPos = transform.position;
            float newRange = range;
            RaycastHit[] hits = Physics.RaycastAll(orgPos, dir, newRange, explosionCheckLayer);
            if (hits.Length > 0)
            {
                foreach(RaycastHit hit in hits)
                {
                    if (hit.Equals(this))
                    {
                        continue;
                    }

                    if(dir == Vector3.left || dir == Vector3.right)
                    {
                        newRange = Mathf.Abs(hit.point.x - transform.position.x);
                    }
                    else if(dir == Vector3.forward || dir == Vector3.back)
                    {
                        newRange = Mathf.Abs(hit.point.z - transform.position.z);
                    }
                    
                    if (newRange <= 0)
                        newRange = 0;

                    break;
                }                
            }

            if (newRange > 0)
            {
                int rangeToInt = (int)newRange;

                for (int i = 1; i <= rangeToInt; i++)
                {
                    MakeExplosionEff(orgPos + dir * i * .25f, "EffExplosion");
                    yield return new WaitForSeconds(.05f);
                }
            }

            yield return null;

            if (dir == Vector3.left) endExpL = true;
            else if (dir == Vector3.right) endExpR = true;
            else if (dir == Vector3.forward) endExpF = true;
            else if (dir == Vector3.back) endExpB = true;
        }

        private void MakeExplosionEff(Vector3 pos, string effId)
        {
            GameObject pfExpEff = ResourceManager.LoadAsset<GameObject>($"Prefab/BomberMan/Effect/{effId}");
            GameObject goExpEff = Instantiate(pfExpEff, pos, Quaternion.identity, null);
            Destroy(goExpEff, 1f);
        }
    }
}