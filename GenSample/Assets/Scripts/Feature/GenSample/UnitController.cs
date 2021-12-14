using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitController : Unit
    {
        public float speed = 1f;        

        protected override void Update()
        {
            if (Input.GetMouseButtonUp(1))
            {                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 10000f))
                {
                    targetPos = hit.point;
                }
            }

            if(Vector3.Distance(transform.position, targetPos) > .1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, 0f, targetPos.z), Time.deltaTime * speed);
            }
        }
    }
}