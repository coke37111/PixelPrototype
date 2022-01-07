using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class DamageCube : CubeBase
    {
        public class DamageListenerData
        {
            public float delay;
            public UnityAction listener;

            public DamageListenerData(float delay, UnityAction listener)
            {
                this.delay = delay;
                this.listener = listener;
            }
        }

        public float damage;
        public float damageDelay;

        private List<DamageListenerData> damageListener;

        public void RegisterDamageListener(UnityAction listener)
        {
            if (damageListener == null)
                damageListener = new List<DamageListenerData>();

            DamageListenerData dlData = damageListener.Find(e => e.listener == listener);
            if (dlData != null)
                return;

            damageListener.Add(new DamageListenerData(0f, listener));
        }

        public void UnregisterDamageListener(UnityAction listener)
        {
            if (damageListener == null)
                return;

            DamageListenerData dlData = damageListener.Find(e => e.listener == listener);
            if (dlData != null)
                damageListener.Remove(dlData);
        }
    }
}