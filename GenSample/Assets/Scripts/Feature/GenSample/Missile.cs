using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class Missile : MonoBehaviour
    {
        public float speed = 5f;

        private PlayerController player;

        public UnitLocalPlayer Owner {get; private set;}

        public void Start()
        {
            Destroy(gameObject, 3.0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                if (RoomSettings.roomType == RoomSettings.ROOM_TYPE.Raid)
                    return;

                PlayerController target = other.GetComponent<PlayerController>();
                if(target != player)
                {
                    if (target.GetTeamNum() >= 0 &&
                        target.GetTeamNum() == player.GetTeamNum())
                        return;

                    target.RaiseAttackBy(player.GetAtk());
                    target.Knockback(player.transform.position, player.GetMissileKnockbackPower());
                    Destroy(gameObject);
                }
            }else if(other.tag == "Cube")
            {
                other.GetComponent<Cube>().Hit(player.GetAtk());
                Destroy(gameObject);
            }else if(other.tag == "Mob")
            {
                MobController targetMob = other.GetComponent<MobController>();
                if (targetMob != null)
                {
                    targetMob.AttackBy(player.GetAtk());
                    Destroy(gameObject);
                }
            }
        }

        public void InitializeBullet(UnitLocalPlayer owner, Vector3 originalDirection, float lag)
        {
            Owner = owner;

            transform.forward = originalDirection;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = originalDirection * speed;
            rigidbody.position += rigidbody.velocity * lag;
        }

        public void InitializeBullet(PlayerController player, Vector3 originalDirection, float lag)
        {
            this.player = player;
            transform.forward = originalDirection;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = originalDirection * speed;
            rigidbody.position += rigidbody.velocity * lag;
        }
    }
}