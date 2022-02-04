using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.Feature.Main
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