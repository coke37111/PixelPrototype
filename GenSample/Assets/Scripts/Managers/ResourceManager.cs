using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public static class ResourceManager
    {
        public static T LoadAsset<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public static T[] LoadAssets<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }
    }
}