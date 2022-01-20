using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class MonsterAttackCollision : MonoBehaviour
    {
        private Monster monster;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                CollisionEventListener collEvent = collision.GetComponent<CollisionEventListener>();
                if(collEvent != null)
                {
                    monster.PlayAttackAnim();
                    collEvent.Raise("AttackBy", monster);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                CollisionEventListener collEvent = collision.GetComponent<CollisionEventListener>();
                if (collEvent != null)
                {
                    monster.PlayAttackAnim();
                    collEvent.Raise("AttackBy", monster);
                }
            }
        }

        public void SetParent(Monster monster)
        {
            this.monster = monster;
        }
    }
}