using Assets.Scripts.Feature.Bomberman.Unit;
using Assets.Scripts.Feature.Main.Cube;
using Assets.Scripts.Util;
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
            if(other.tag == "Cube")
            {                
                BombCube bombCube = other.GetComponent<BombCube>();
                if (bombCube != null)
                {
                    bombCube.Explosion();
                }
            }
        }
    }
}