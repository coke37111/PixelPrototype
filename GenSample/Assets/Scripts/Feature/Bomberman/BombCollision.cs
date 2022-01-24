using Assets.Scripts.Feature.PxpCraft;
using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombCollision : MonoBehaviour
    {
        private UnityAction callback;

        #region UNITY

        private void OnTriggerEnter(Collider other)
        {
            CheckCollider(other);
        }

        #endregion

        public void SetBombCallback(UnityAction bombCallback)
        {
            callback = bombCallback;
        }

        private void CheckCollider(Collider other)
        {
            if (other.tag == "Cube")
                callback?.Invoke();

            if (other.tag == "Bomb")
            {
                callback?.Invoke();
                other.GetComponent<CollisionEventListener>().Raise("Explosion");
                return;
            }

            if(other.tag == "Player")
            {
                other.GetComponent<CollisionEventListener>().Raise("HitExplosion");
            }
        }
    }
}