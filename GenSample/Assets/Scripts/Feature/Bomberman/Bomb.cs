using Assets.Scripts.Feature.PxpCraft;
using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class Bomb : MonoBehaviour
    {
        public GameObject basePrefab;

        public Transform centerCollider;
        public Transform leftCollider;
        public Transform rightCollider;
        public Transform upCollider;
        public Transform downCollider;

        private float time;
        private float curTime;
        private bool isBuild = false;
        private Vector2 power;
        private bool isExplosion = false;
        private bool endExpR, endExpL, endExpU, endExpD = false;
        private Coroutine corExpL, corExpR, corExpU, corExpD;
        private CollisionEventListener collListener;

        #region UNITY

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!isBuild)
                return;

            if(!isExplosion)
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

            if (endExpL && endExpR && endExpU && endExpD)
            {
                Destroy(gameObject);
            }
        }

        public void Explosion(params object[] param)
        {
            collListener.UnregisterListener("Explosion");
            basePrefab.SetActive(false);

            isExplosion = true;

            centerCollider.gameObject.SetActive(true);
            corExpL = StartCoroutine(ExplosionLeft());
            corExpR = StartCoroutine(ExplosionRight());
            corExpU = StartCoroutine(ExplosionUp());
            corExpD = StartCoroutine(ExplosionDown());
        }

        #endregion

        private void Init()
        {
            collListener = GetComponentInChildren<CollisionEventListener>();
            collListener.RegisterListner("Explosion", Explosion);

            basePrefab.SetActive(true);

            centerCollider.gameObject.SetActive(false);
            leftCollider.gameObject.SetActive(false);
            rightCollider.gameObject.SetActive(false);
            upCollider.gameObject.SetActive(false);
            downCollider.gameObject.SetActive(false);

            leftCollider.GetComponent<BombCollision>().SetBomb(EndExpL);
            rightCollider.GetComponent<BombCollision>().SetBomb(EndExpR);
            upCollider.GetComponent<BombCollision>().SetBomb(EndExpU);
            downCollider.GetComponent<BombCollision>().SetBomb(EndExpD);

            curTime = 0f;
            isExplosion = false;
            endExpL = endExpR = endExpU = endExpD = false;
        }

        public void Build(Vector2 power, float time)
        {
            Init();

            this.power = power;
            this.time = time;

            isBuild = true;
        }

        private IEnumerator ExplosionLeft()
        {
            leftCollider.gameObject.SetActive(true);
            Vector3 colScale = leftCollider.localScale;
            while(colScale.x < power.x * .98f)
            {
                colScale.x = Mathf.Lerp(colScale.x, power.x, Time.deltaTime / 0.1f);
                leftCollider.localScale = colScale;
                yield return null;
            }

            endExpL = true;
            yield return null;
        }

        private IEnumerator ExplosionRight()
        {
            rightCollider.gameObject.SetActive(true);
            Vector3 colScale = rightCollider.localScale;
            while (colScale.x < power.x * .98f)
            {
                colScale.x = Mathf.Lerp(colScale.x, power.x, Time.deltaTime / 0.1f);
                rightCollider.localScale = colScale;
                yield return null;
            }

            endExpR = true;
            yield return null;
        }

        private IEnumerator ExplosionUp()
        {
            upCollider.gameObject.SetActive(true);
            Vector3 colScale = upCollider.localScale;
            while (colScale.x < power.x * .98f)
            {
                colScale.x = Mathf.Lerp(colScale.x, power.x, Time.deltaTime / 0.1f);
                upCollider.localScale = colScale;
                yield return null;
            }

            endExpU = true;
            yield return null;
        }

        private IEnumerator ExplosionDown()
        {
            downCollider.gameObject.SetActive(true);
            Vector3 colScale = downCollider.localScale;
            while (colScale.x < power.x * .98f)
            {
                colScale.x = Mathf.Lerp(colScale.x, power.x, Time.deltaTime / 0.1f);
                downCollider.localScale = colScale;
                yield return null;
            }

            endExpD = true;
            yield return null;
        }

        public void EndExpL()
        {
            StopCoroutine(corExpL);
            endExpL = true;
        }

        public void EndExpR()
        {
            StopCoroutine(corExpR);
            endExpR = true;
        }

        public void EndExpU()
        {
            StopCoroutine(corExpU);
            endExpU = true;
        }

        public void EndExpD()
        {
            StopCoroutine(corExpD);
            endExpD = true;
        }
    }
}