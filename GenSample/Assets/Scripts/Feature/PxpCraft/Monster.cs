using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class Monster : MonoBehaviour
    {
        private Transform target;
        private MonsterAttackCollision atkColl;
        private Transform trImage;

        public float speed = 3f;
        public float knockbackPower = 100f;

        // Use this for initialization
        void Start()
        {
            atkColl = GetComponentInChildren<MonsterAttackCollision>();
            trImage = GetComponentInChildren<SpriteRenderer>()
                .transform;

            atkColl.SetParent(this);
        }

        // Update is called once per frame
        void Update()
        {
            Move();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                target = collision.transform;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if(target == null)
            {
                if (collision.tag == "Player")
                {
                    target = collision.transform;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if(collision.tag == "Player")
            {
                target = null;
            }
        }

        private void Move()
        {
            Vector3 dir = Vector3.zero;

            if (target == null)
            {
                // TODO : AI
            }
            else
            {
                dir = target.position - transform.position;

                transform.Translate(Vector3.right * dir.normalized.x * speed * Time.deltaTime);
            }

            Vector3 trScale = trImage.localScale;
            if (dir.x > 0f)
            {
                trScale.x = -Mathf.Abs(trScale.x);
            }
            else if (dir.x < 0f)
            {
                trScale.x = Mathf.Abs(trScale.x);
            }
            trImage.localScale = trScale;
        }
    }
}