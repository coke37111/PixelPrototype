using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitAI : UnitBase
    {
        public LayerMask knockbackLayer;

        public enum UNIT_STATE
        {
            IDLE,
            MOVE,
        }
        private UNIT_STATE unitState;

        private readonly float MAX_IDLE_TIME = 1f;

        private float curIdleTime;
        private float idleDelay;
        private Vector3 spawnPos;
        private Vector3 targetPos;

        protected override void Attack()
        {

        }

        protected override void Jump()
        {

        }

        protected override void Move()
        {
            switch (unitState)
            {
                case UNIT_STATE.IDLE:
                    {
                        if (idleDelay >= curIdleTime)
                        {
                            targetPos = transform.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                            unitState = UNIT_STATE.MOVE;

                            idleDelay = 0f;
                            curIdleTime = GetIdleTime();
                        }
                        else
                        {
                            idleDelay += Time.deltaTime;
                        }
                        break;
                    }
                case UNIT_STATE.MOVE:
                    {
                        Vector3 nextPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime);
                        if (Vector3.Distance(transform.position, targetPos) <= .1f || OutOfRangePos(nextPos))
                        {
                            unitState = UNIT_STATE.IDLE;
                        }
                        else
                        {
                            isLeftDir = transform.position.x > targetPos.x;

                            transform.position = nextPos;
                        }
                        break;
                    }
            }
        }

        public override void Init()
        {
            base.Init();

            unitState = UNIT_STATE.IDLE;
            curIdleTime = GetIdleTime();
            controlable = true;

            isLeftDir = Random.Range(0f, 1f) >= .5f;
        }

        private bool OutOfRangePos(Vector3 pos)
        {
            return Vector3.Distance(spawnPos, pos) > 1f;
        }

        private float GetIdleTime()
        {
            return Random.Range(0f, MAX_IDLE_TIME);
        }

        public override void ResetSpawnPos(Vector3 pos)
        {
            base.ResetSpawnPos(pos);

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            spawnPos = pos;
            transform.position = spawnPos;
        }

        public override void Knockback(float centerX, float centerZ)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10000f, knockbackLayer))
            {
                if (hit.collider.tag == "Indicator")
                {
                    Vector3 diffPos = transform.position - new Vector3(centerX, transform.position.y, centerZ);
                    Vector3 dir = diffPos.normalized;

                    rb.AddForce(dir * 1000f);
                }
            }
        }
    }
}