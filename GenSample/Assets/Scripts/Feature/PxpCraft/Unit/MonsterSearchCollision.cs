using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class MonsterSearchCollision : MonoBehaviour
    {
        private Transform target;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player-Hit")
            {
                target = collision.transform;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (target == null)
            {
                if (collision.tag == "Player-Hit")
                {
                    target = collision.transform;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.tag == "Player-Hit")
            {
                target = null;
            }
        }

        public Transform GetTarget()
        {
            return target;
        }
    }
}