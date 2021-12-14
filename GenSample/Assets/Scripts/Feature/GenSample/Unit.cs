using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class Unit : MonoBehaviour
    {
        public enum UNIT_STATE
        {
            IDLE,
            MOVE,
        }

        private UNIT_STATE unitState;

        private readonly float MAX_IDLE_TIME = 1f;
        private float curIdleTime;
        private float idleDelay;

        protected bool isLeftDir;
        private Vector3 spawnPos;
        private Vector3 targetPos;

        public SpriteRenderer srHead;
        public SpriteRenderer srBody;

        public void Init()
        {
            srHead.sprite = SetSprite("Image/Unit/Head");
            srBody.sprite = SetSprite("Image/Unit/Body");

            unitState = UNIT_STATE.IDLE;
            curIdleTime = GetIdleTime();

            isLeftDir = Random.Range(0f, 1f) >= .5f;
            SetDir();
        }

        protected virtual void Update()
        {
            switch (unitState)
            {
                case UNIT_STATE.IDLE:
                    {
                        if (idleDelay >= curIdleTime)
                        {
                            targetPos = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
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
                            SetDir();

                            transform.position = nextPos;
                        }
                        break;
                    }
            }
        }

        private bool OutOfRangePos(Vector3 pos)
        {
            return Vector3.Distance(spawnPos, pos) > 1f;
        }

        private float GetIdleTime()
        {
            return Random.Range(0f, MAX_IDLE_TIME);
        }

        private static Sprite SetSprite(string path)
        {
            Sprite resultSprite = null;

            Sprite[] loadedSprite = ResourceManager.LoadAssets<Sprite>(path);

            for (int i = 0; i < loadedSprite.Length; i++)
            {
                if (Random.Range(0f, 1f) > .5f || i == loadedSprite.Length - 1)
                {
                    resultSprite = loadedSprite[i];
                    break;
                }
            }

            return resultSprite;
        }

        public void ResetSpawnPos(float posX, float posY)
        {
            spawnPos = new Vector3(posX, posY, 0f);
            transform.position = spawnPos;

            unitState = UNIT_STATE.IDLE;
            curIdleTime = GetIdleTime();

            isLeftDir = Random.Range(0f, 1f) >= .5f;
            SetDir();
        }

        public Vector3 GetSpawnPos()
        {
            return spawnPos;
        }

        public void SetDir()
        {
            srHead.flipX = !isLeftDir;
            srBody.flipX = !isLeftDir;
        }
    }
}