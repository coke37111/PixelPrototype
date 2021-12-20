using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class ObjController : MonoBehaviour
    {
        private Transform trImageRoot;

        private void Awake()
        {
            trImageRoot = transform.Find("ImageRoot");
        }
        // Use this for initialization
        void Start()
        {
            if (trImageRoot == null)
            {
                return;
            }

            Quaternion quaUnit = trImageRoot.rotation;
            float camRotX = Camera.main.transform.rotation.x;
            quaUnit.x = camRotX;
            trImageRoot.rotation = quaUnit;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}