using Assets.Scripts.Feature.Bomberman.Unit;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombermanExplosion : MonoBehaviour
    {
        private float damage;

        public void SetDamage(float damage)
        {
            this.damage = damage;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                    player.HitExplosion(damage);
            }
        }
    }
}