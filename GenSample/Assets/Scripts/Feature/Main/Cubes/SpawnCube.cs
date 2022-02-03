using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class SpawnCube : Cube
    {
        public void HideGuide()
        {
            transform.Find("Guide").gameObject.SetActive(false);
        }
    }
}