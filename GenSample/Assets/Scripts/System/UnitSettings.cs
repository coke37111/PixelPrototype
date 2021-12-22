using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.System
{
    public static class UnitSettings
    {
        public static readonly string[] unitParts = 
        {
            "cos",
            "hair1",
            "hair2",
            "skin",
            "hat",
            "wp_1",
            "wp_shild",
        };

        public static readonly string[] unitTypes = 
        {
            "human_m",
            "human_s",
            "human_l",
            "char_st_01",
            "char_st_01_human_m",
            "char_st_01_human_s",
            "char_st_01_human_l",
        };

        public static Dictionary<string, string> GetSelectUnitPartDict()
        {
            Dictionary<string, string> unitPartData = new Dictionary<string, string>();

            string unitType = "";
            for (int i = 0; i < unitTypes.Length; i++)
            {
                if (Random.Range(0f, 1f) >= .3f || i == unitTypes.Length - 1)
                {
                    unitType = unitTypes[i];
                    break;
                }
            }

            foreach (string unitPartName in unitParts)
            {
                string dirPath = $"Image/Unit/{unitType}/imgs/{unitPartName}";
                Sprite selectSprite = GetRandomSprite(dirPath);
                if (selectSprite == null)
                {
                    Log.Error($"Cannot SpawnPlayer! : Cannot find sprite {dirPath}!");
                    continue;
                }
                string spriteName = GetRandomSprite(dirPath).name;

                string resPath = Path.Combine(dirPath, spriteName);

                unitPartData.Add(unitPartName, resPath);
            }

            return unitPartData;
        }

        private static Sprite GetRandomSprite(string path)
        {
            Sprite resultSprite = null;

            Sprite[] loadedSprite = ResourceManager.LoadAssets<Sprite>(path);

            for (int i = 0; i < loadedSprite.Length; i++)
            {
                if (Random.Range(0f, 1f) > .5f || i == loadedSprite.Length - 1)
                {
                    resultSprite = loadedSprite[i];
                    break;
                }
            }

            return resultSprite;
        }
    }
}