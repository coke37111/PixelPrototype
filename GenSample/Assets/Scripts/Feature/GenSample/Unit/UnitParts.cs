using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitParts : MonoBehaviour
    {
        private Dictionary<string, SpriteRenderer> srDict = new Dictionary<string, SpriteRenderer>();


        #region UNITY

        private void Update()
        {
            RotateSprite();
        }

        #endregion

        public void RotateSprite()
        {
            Vector3 quaUnit = transform.rotation.eulerAngles;
            Vector3 quaCam = Camera.main.transform.rotation.eulerAngles;
            //quaUnit.x = quaCam.x;
            quaUnit.y = quaCam.y;
            Quaternion quaUnitNew = Quaternion.Euler(quaUnit);
            transform.rotation = quaUnitNew;
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