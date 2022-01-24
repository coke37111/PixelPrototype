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
            if (other.tag == "Bomb-Range")
                return;

            if (other.tag == "Bomb")
            {
                other.GetComponent<CollisionEventListener>().Raise("Explosion");
                return;
            }

            callback?.Invoke();
        }

        #endregion

        public void SetBomb(UnityAction bombCallback)
        {
            callback = bombCallback;
        }
    }
}