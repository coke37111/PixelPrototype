using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.Bomberman
{    
    public class BombermanMapController : MonoBehaviour
    {
        private int mapSize = 5;
        private List<BomberManBlock> blocks = new List<BomberManBlock>();

        public void Init()
        {
            BomberManObjectContainer objContainer = FindObjectOfType<BomberManObjectContainer>();
            objContainer.SetMapController(this);

            objContainer.RegisterBlocks();
        }

        public void RegisterBlock(BomberManBlock block)
        {
            if(blocks.Find(e => e.GetPosition() == block.GetPosition()) != null)
            {
                Log.Error($"Cannot Register, Already Exist Block in {block.GetPosition()}");
                return;
            }

            blocks.Add(block);
        }

        public void UnregisterBlock(BomberManBlock block)
        {
            if(blocks.Find(e=>e.GetPosition() == block.GetPosition()) != null)
            {
                blocks.Remove(block);
            }
        }

        public BomberManBlock GetBlockInPos(Vector2Int pos)
        {
            BomberManBlock block = blocks.Find(e => e.GetPosition() == pos);
            return block;
        }

        public bool isEndOfMap(Vector2Int pos)
        {
            return Mathf.Abs(pos.x) >= mapSize || Mathf.Abs(pos.y) >= mapSize;
        }

        public void MakeExplosion(Vector2Int pos, string effId)
        {
            MakeExplosionEff(pos, effId);
        }

        public void MakeExplosion(Vector2Int pos, int power, string effId)
        {
            StartCoroutine(StartExplosion(pos, Vector2Int.left, power, effId));
            StartCoroutine(StartExplosion(pos, Vector2Int.up, power, effId));
            StartCoroutine(StartExplosion(pos, Vector2Int.right, power, effId));
            StartCoroutine(StartExplosion(pos, Vector2Int.down, power, effId));
        }

        private IEnumerator StartExplosion(Vector2Int pos, Vector2Int dir, float power, string effId)
        {
            for (int i = 1; i < power; i++)
            {
                Vector2Int newPos = pos + dir * i;
                if (isEndOfMap(newPos))
                    break;

                BomberManBlock nextBlock = GetBlockInPos(newPos);
                if(nextBlock != null)
                {
                    if (nextBlock.canExplosion)
                    {
                        nextBlock.Explosion();                        
                    }
                    else 
                        break;

                    if (!nextBlock.canPenetrate)
                        break;
                }

                MakeExplosionEff(pos + dir * i, effId);
                yield return new WaitForSeconds(.05f);
            }

            yield return null;
        }

        private void MakeExplosionEff(Vector2Int pos, string effId)
        {
            GameObject pfExpEff = ResourceManager.LoadAsset<GameObject>($"Prefab/BomberMan/Effect/{effId}");
            GameObject goExpEff = Instantiate(pfExpEff, new Vector3(pos.x, .5f, pos.y), Quaternion.identity, null);
            Destroy(goExpEff, .1f);
        }

        public Vector2Int GetRandomSpawnPos()
        {
            Vector2Int spawnPos = GetRandomPos();
            BomberManBlock block = GetBlockInPos(spawnPos);
            while(block != null)
            {
                spawnPos = GetRandomPos();
                block = GetBlockInPos(spawnPos);
            }
            return spawnPos;
        }

        private Vector2Int GetRandomPos()
        {
            int x = Random.Range(-mapSize + 1, mapSize);
            int y = Random.Range(-mapSize + 1, mapSize);
            return new Vector2Int(x, y);
        }

        public int GetMapSize()
        {
            return mapSize;
        }
    }
}