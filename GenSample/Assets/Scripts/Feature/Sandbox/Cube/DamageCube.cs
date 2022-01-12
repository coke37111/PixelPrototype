using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class DamageCube : CubeBase
    {
        protected override CUBE_TYPE cubeType => CUBE_TYPE.DamageCube;

        public class DamageListenerData
        {
            public float delay;
            public UnityAction<DamageCube> listener;

            public DamageListenerData(float delay, UnityAction<DamageCube> listener)
            {
                this.delay = delay;
                this.listener = listener;
            }
        }

        public float damage = 5.0f;
        public float damageDelay = 0.5f;

        private List<DamageListenerData> damageListenerList;

        #region UNITY

        private void Update()
        {
            if (damageListenerList == null || damageListenerList.Count <= 0)
                return;

            for(int i = 0; i < damageListenerList.Count; i++)
            {
                DamageListenerData dlData = damageListenerList[i];
                if (dlData.delay >= damageDelay)
                {
                    dlData.delay = 0f;
                    dlData.listener(this);
                }
                else
                {
                    dlData.delay += Time.deltaTime;
                }
            }
        }

        #endregion

        public void RegisterDamageListener(UnityAction<DamageCube> listener)
        {
            if (damageListenerList == null)
                damageListenerList = new List<DamageListenerData>();

            DamageListenerData dlData = damageListenerList.Find(e => e.listener == listener);
            if (dlData != null)
                return;

            damageListenerList.Add(new DamageListenerData(0f, listener));
        }

        public void UnregisterDamageListener(UnityAction<DamageCube> listener)
        {
            if (damageListenerList == null)
                return;

            DamageListenerData dlData = damageListenerList.Find(e => e.listener == listener);
            if (dlData != null)
                damageListenerList.Remove(dlData);
        }
    }
}