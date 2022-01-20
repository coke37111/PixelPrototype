using Assets.Scripts.Util;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class Monster : MonoBehaviour
    {
        private Transform target;
        private MonsterAttackCollision atkColl;
        private Transform trImage;
        private SkeletonMecanim skelMecanim;
        private Animator skelAnim;

        public float speed = 3f;
        public float knockbackPower = 100f;

        // Use this for initialization
        void Start()
        {
            skelMecanim = GetComponentInChildren<SkeletonMecanim>();
            skelAnim = skelMecanim.GetComponent<Animator>();
            atkColl = GetComponentInChildren<MonsterAttackCollision>();
            trImage = skelMecanim.transform;

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
                Vector3 dist = target.position - transform.position;
                if(dist.magnitude >= .5f)
                {
                    dir = dist.normalized;

                    transform.Translate(Vector3.right * dir.x * speed * Time.deltaTime);
                }
            }

            skelAnim.SetBool("isMove", dir != Vector3.zero);

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

        public void PlayAttackAnim()
        {
            skelAnim.SetTrigger("isAtk");
        }
    }
}