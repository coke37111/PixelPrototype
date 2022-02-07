using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class GuardCube : Cube
    {
        private MeshRenderer render;

        #region UNITY

        private void Start()
        {         
        }

        #endregion
        public void ShowCube()
        {
            if (render == null)
            {
                render = GetComponent<MeshRenderer>();
            }

            render.enabled = true;
        }

        public void HideCube()
        {
            if (render == null)
            {
                render = GetComponent<MeshRenderer>();
            }

            render.enabled = false;
        }
    }
}