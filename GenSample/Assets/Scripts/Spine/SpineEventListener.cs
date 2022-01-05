using Assets.Scripts.Util;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Spine
{
    [RequireComponent(typeof(SkeletonMecanim))]
    public class SpineEventListener : MonoBehaviour
    {
        private List<UnityAction> atkListener = new List<UnityAction>();

        public void RegisterAtkListener(UnityAction callback)
        {
            if (atkListener.Contains(callback))
            {
                Log.Error($"Already Register AtkListener");
                return;
            }

            Log.Print($"register listener");
            atkListener.Add(callback);
        }

        public void UnregisterAtkListener(UnityAction callback)
        {
            if (atkListener.Contains(callback))
                atkListener.Remove(callback);

            Log.Print($"unregister listener");
        }

        void testEvent_attack_001()
        {
            foreach(UnityAction action in atkListener)
            {
                action.Invoke();
            }
        }
    }
}