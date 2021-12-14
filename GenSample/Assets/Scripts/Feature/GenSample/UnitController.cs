using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitController : Unit
    {
        public float speed = 1f;

        protected override void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
                
                isLeftDir = true;
                SetDir();
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

                isLeftDir = false;
                SetDir();
            }
            
            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
            }
            
            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
            }
        }
    }
}