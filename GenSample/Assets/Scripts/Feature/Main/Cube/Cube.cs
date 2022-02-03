using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Feature.Main.Cube
{
    public class Cube : MonoBehaviour
    {
        private UnityAction destroyCallback;

        public void SetDestroyCallback(UnityAction callback)
        {
            destroyCallback = callback;
        }

        protected void DestroyCube()
        {
            destroyCallback?.Invoke();
        }
    }
}