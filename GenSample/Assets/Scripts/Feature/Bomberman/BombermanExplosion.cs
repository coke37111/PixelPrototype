using Assets.Scripts.Feature.PxpCraft;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombermanExplosion : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                CollisionEventListener collListener = other.GetComponent<CollisionEventListener>();
                if (collListener != null)
                    collListener.Raise("HitExplosion");
            }
        }
    }
}