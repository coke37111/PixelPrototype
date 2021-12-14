using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class Log
    {
        public static void Print(params object[] param)
        {
            Debug.Log(string.Join(", ", param));
        }

        public static void Error(params object[] param)
        {
            Print(param);
        }
    }
}