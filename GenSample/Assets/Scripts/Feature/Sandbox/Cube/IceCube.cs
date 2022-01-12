namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class IceCube : CubeBase
    {
        protected override CUBE_TYPE cubeType => CUBE_TYPE.IceCube;
        public float dampRatio = .01f;

    }
}