using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class CollisionEventListener : MonoBehaviour
    {
        private Dictionary<string, UnityAction<object[]>> listenerDict;

        public void RegisterListner(string eventName, UnityAction<object[]> listener)
        {
            if(listenerDict == null)
            {
                listenerDict = new Dictionary<string, UnityAction<object[]>>();
            }

            if (listenerDict.ContainsKey(eventName))
            {
                Log.Error($"Already Reigster Event {eventName}");
                return;
            }

            listenerDict.Add(eventName, listener);
        }

        public void UnregisterListener(string eventName)
        {
            if (listenerDict == null)
                return;

            if (listenerDict.ContainsKey(eventName))
            {
                listenerDict.Remove(eventName);
            }
        }

        public void Raise(string eventName, params object[] param)
        {
            if (listenerDict == null)
            {
                return;
            }

            if (listenerDict.ContainsKey(eventName))
            {
                listenerDict[eventName].Invoke(param);
            }
        }
    }
}