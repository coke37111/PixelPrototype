using Assets.Scripts.Feature.PxpCraft;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Feature.Main
{
    public class BombCollision : MonoBehaviour
    {
        private UnityAction callback;

        private List<Collider> colls;

        #region UNITY

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                if (colls == null)
                    colls = new List<Collider>();

                if(!colls.Contains(other))
                    colls.Add(other);
            }

            //CheckCollider(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (colls != null && colls.Contains(other))
            {
                colls.Remove(other);
            }

            if(colls == null || colls.Count <= 0)
                GetComponent<BoxCollider>().isTrigger = false;
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