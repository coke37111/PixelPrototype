using Assets.Scripts.Util;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class Player : MonoBehaviour
    {
        private Animator skelAnim;
        private Rigidbody2D rBody;

        public float jumpPower = 100f;
        public float speed = 5f;

        // Use this for initialization
        void Start()
        {
            skelAnim = GetComponentInChildren<SkeletonMecanim>()
                .GetComponent<Animator>();
            rBody = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            Move();
            Jump();
            Attack();
        }

        private void Move()
        {
            float axisX = Input.GetAxis("Horizontal");

            transform.Translate(Vector3.right * axisX * speed * Time.deltaTime);

            skelAnim.SetBool("isMove", axisX != 0f);

            Vector3 trScale = transform.localScale;
            trScale.x = axisX >= 0f ? -Mathf.Abs(trScale.x) : Mathf.Abs(trScale.x);
            transform.localScale = trScale;
        }

        private void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rBody.AddForce(Vector3.up * jumpPower);
            }
        }

        private void Attack()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                skelAnim.SetTrigger("isAtk");
            }
        }
    }
}