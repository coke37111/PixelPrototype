using Assets.Scripts.Util;
using Spine.Unity;
using UnityEngine;

namespace Assets.Scripts.Spine
{
    [RequireComponent(typeof(SkeletonMecanim))]
    public class SpineEventListener : MonoBehaviour
    {
        void testEvent_001()
        {
            Log.Print($"testEvent_001");
        }
    }
}