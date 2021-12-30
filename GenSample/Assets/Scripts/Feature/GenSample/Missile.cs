using Assets.Scripts.Util;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class Missile : MonoBehaviour
    {
        public float speed = 5f;

        public UnitLocalPlayer Owner {get; private set;}

        public void Start()
        {
            Destroy(gameObject, 3.0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            MobController targetMob = other.GetComponent<MobController>();
            if(targetMob != null)
            {
                targetMob.AttackBy(Owner);
            }

            UnitBase targetUnit = other.GetComponent<UnitBase>();
            if(targetUnit != null)
            {
                targetUnit.AttackBy(Owner);
            }

            if(targetMob != null || targetUnit != null)
            {
                Destroy(gameObject);
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
    }
}