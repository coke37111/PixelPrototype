using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class PlayerAttackCollision : MonoBehaviour
    {
        private Player player;
        private List<CollisionEventListener> targetListenerList;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Mob-Hit")
            {
                CollisionEventListener collEvent = collision.GetComponent<CollisionEventListener>();
                if (collEvent != null)
                {
                    if (targetListenerList == null)
                        targetListenerList = new List<CollisionEventListener>();

                    if (targetListenerList.Contains(collEvent))
                        return;

                    targetListenerList.Add(collEvent);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if(collision.tag == "Mob-Hit")
            {
                CollisionEventListener collEvent = collision.GetComponent<CollisionEventListener>();
                if (collEvent != null)
                {
                    if (targetListenerList == null)
                        return;

                    if (targetListenerList.Contains(collEvent))
                        targetListenerList.Remove(collEvent);
                }
            }
        }

        public void SetParent(Player player)
        {
            this.player = player;
        }

        public void Raise()
        {
            if(targetListenerList == null || targetListenerList.Count <= 0)
            {
                return;
            }

            targetListenerList[0].Raise("AttackBy", player);
        }
    }
}