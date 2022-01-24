using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BomberManObjectContainer : MonoBehaviour
    {
        BombermanMapController mapCtrl;

        public void SetMapController(BombermanMapController mapCtrl)
        {
            this.mapCtrl = mapCtrl;
        }

        public void RegisterBlocks()
        {
            if (mapCtrl == null)
            {
                Log.Error($"Set MapController First!");
                return;
            }

            for(int i = 0; i < transform.childCount; i++)
            {
                BomberManBlock block = transform.GetChild(i).GetComponent<BomberManBlock>();
                if(block == null)
                {
                    Log.Error($"Cannot Find Block Component in {transform.GetChild(i).name}");
                    continue;
                }

                mapCtrl.RegisterBlock(block);
            }
        }
    }
}