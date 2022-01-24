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
            Log.Print($"Enter {name} {other.transform.position} {other.contactOffset}");
            CheckCollider(other);
        }

        #endregion

        public void SetBombCallback(UnityAction bombCallback)
        {
            callback = bombCallback;
        }

        private void CheckCollider(Collider other)
        {
            if (other.tag == "Bomb-Range")
                return;

            if (other.tag == "Bomb")
            {
                other.GetComponent<CollisionEventListener>().Raise("Explosion");
                return;
            }

            callback?.Invoke();
        }
    }
}