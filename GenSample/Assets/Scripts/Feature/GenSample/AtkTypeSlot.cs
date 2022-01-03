using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Feature.GenSample
{
    public class AtkTypeSlot : MonoBehaviour
    {
        public Image[] imgAtkTypes;
        
        public void Build(int atkType)
        {
            for(int i = 0; i < imgAtkTypes.Length; i++)
            {
                if (i == atkType)
                    imgAtkTypes[i].gameObject.SetActive(true);
                else
                    imgAtkTypes[i].gameObject.SetActive(false);
            }
        }        
    }
}