using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class SpawnCube : Cube
    {
        public bool isHideGuide = false;
        public void HideGuide()
        {
            if(isHideGuide)
                transform.Find("Guide").gameObject.SetActive(false);
        }
    }
}