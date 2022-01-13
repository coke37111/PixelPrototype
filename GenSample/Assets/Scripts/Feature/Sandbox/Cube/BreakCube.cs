using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class BreakCube : CubeBase
    {
        protected override CUBE_TYPE cubeType => CUBE_TYPE.BreakCube;

        public int cntToBreak = 1;
        public float timeToBreak = .5f;

        private int curCntToBreak = 0;
        private bool isBreaking = false;

        public void CheckBreak()
        {
            if (isBreaking)
                return;

            curCntToBreak++;

            if(curCntToBreak >= cntToBreak)
            {
                StartCoroutine(StartBreak());
            }
        }

        private IEnumerator StartBreak()
        {
            isBreaking = true;
            yield return new WaitForSeconds(timeToBreak);

            if(cubeRoot != null)
                cubeRoot.DestroyCube();
        }
    }
}