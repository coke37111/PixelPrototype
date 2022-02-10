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
                PlayerController target = other.GetComponent<PlayerController>();
                if(target != player)
                {
                    target.RaiseAttackBy(player.GetAtk());
                    Destroy(gameObject);
                }
            }else if(other.tag == "Cube")
            {
                other.GetComponent<Cube>().Hit(player.GetAtk());
                Destroy(gameObject);
            }
            //bool isDestroy = false;

            //MobController targetMob = other.GetComponent<MobController>();
            //if(targetMob != null)
            //{
            //    targetMob.AttackBy(Owner);
            //    isDestroy = true;
            //}
            
            //if(RoomSettings.roomType == RoomSettings.ROOM_TYPE.Pvp)
            //{
            //    UnitBase targetUnit = other.GetComponent<UnitBase>();
            //    if (targetUnit != null && !Owner.IsSameTeam(targetUnit.teamNum))
            //    {
            //        targetUnit.AttackBy(Owner);
            //        isDestroy = true;
            //    }
            //}

            //if(isDestroy)
            //{
            //    Destroy(gameObject);
            //}
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