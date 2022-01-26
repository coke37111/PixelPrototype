using Assets.Scripts.Feature.Sandbox.Cube;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox
{
    public class CubeContainer : MonoBehaviour
    {
        public List<CubeRoot> GetAllCubes()
        {
            List<CubeRoot> results = new List<CubeRoot>();

            for(int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<CubeRoot>())
                {
                    results.Add(child.GetComponent<CubeRoot>());
                }
            }

            return results;
        }

        public void DestroyAllCubes()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<CubeRoot>())
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}