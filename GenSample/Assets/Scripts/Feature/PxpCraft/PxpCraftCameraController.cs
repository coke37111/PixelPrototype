using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class PxpCraftCameraController : MonoBehaviour
    {
        public Transform target;
        public float height = 2f;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (target == null)
                return;

            Vector3 curPos = transform.position;
            curPos.x = target.position.x;
            curPos.y = target.position.y + height;
            transform.position = curPos;
        }
    }
}