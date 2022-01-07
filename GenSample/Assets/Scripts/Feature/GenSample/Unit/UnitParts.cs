using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitParts : MonoBehaviour
    {
        private Dictionary<string, SpriteRenderer> srDict = new Dictionary<string, SpriteRenderer>();
        
        public void RotateSprite()
        {
            Quaternion quaUnit = transform.rotation;
            float camRotX = Camera.main.transform.rotation.x;
            float camRotY = Camera.main.transform.rotation.y;
            quaUnit.x = camRotX;
            quaUnit.y = camRotY;
            transform.rotation = quaUnit;
        }

        public void SetSprite(string partsName, string spritePath)
        {
            Sprite spritePart = ResourceManager.LoadAsset<Sprite>(spritePath);
            if (srDict.ContainsKey(partsName))
            {
                srDict[partsName].sprite = spritePart;
            }
            else
            {
                Transform trPart = transform.Find(partsName);
                if(trPart == null)
                {
                    Log.Error($"{partsName} part 가 존재하지 않습니다");
                    return;
                }

                SpriteRenderer srPart = trPart.GetComponent<SpriteRenderer>();
                if(srPart == null)
                {
                    Log.Error($"{partsName} part에 SpriteRenderer가 존재하지 않습니다.");
                    return;
                }
                srPart.sprite = spritePart;

                srDict.Add(partsName, srPart);
            }
        }
        
        public void FlipX(bool isLeft)
        {
            foreach(SpriteRenderer sr in srDict.Values)
            {
                sr.flipX = !isLeft;
            }
        }
    }
}