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
        protected Vector3 targetPos;

        public SpriteRenderer srHair2;
        public SpriteRenderer srSkin;
        public SpriteRenderer srHair1;
        public SpriteRenderer srCos;
        public SpriteRenderer srHat;
        public SpriteRenderer srWpShild;
        public SpriteRenderer srWp;

        public void Init()
        {
            srHair2.sprite = SetSprite("Image/Unit/human_m/imgs/hair2");
            srSkin.sprite = SetSprite("Image/Unit/human_m/imgs/skin");
            srHair1.sprite = SetSprite("Image/Unit/human_m/imgs/hair1");
            srCos.sprite = SetSprite("Image/Unit/human_m/imgs/cos");
            srHat.sprite = SetSprite("Image/Unit/human_m/imgs/hat");
            srWpShild.sprite = SetSprite("Image/Unit/human_m/imgs/wp_shild");
            srWp.sprite = SetSprite("Image/Unit/human_m/imgs/wp_1");

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

        public void ResetSpawnPos(float posX, float posZ)
        {
            spawnPos = new Vector3(posX, 0f, posZ);
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
            srHair2.flipX = !isLeftDir;
            srSkin.flipX = !isLeftDir;
            srHair1.flipX = !isLeftDir;
            srCos.flipX = !isLeftDir;
            srHat.flipX = !isLeftDir;
            srWpShild.flipX = !isLeftDir;
            srWp.flipX = !isLeftDir;
        }
    }
}