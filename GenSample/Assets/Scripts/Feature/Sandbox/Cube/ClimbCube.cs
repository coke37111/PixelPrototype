using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class ClimbCube : CubeBase
    {
        protected override CUBE_TYPE cubeType => CUBE_TYPE.Climb;
    }
}