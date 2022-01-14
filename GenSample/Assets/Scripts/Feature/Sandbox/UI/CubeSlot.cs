using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Feature.Sandbox.UI
{
    public class CubeSlot : MonoBehaviour
    {
        public Image imageSelect;
        public RawImage imageTile;
        public Text textSlotNum;
        public Text textCubeName;

        private string cubeType;
        private UnityAction<string> selectListener;

        public void Build(int num, string tileName)
        {
            textSlotNum.text = (num + 1).ToString();

            Texture2D TextureTile = ResourceManager.LoadAsset<Texture2D>($"Image/Texture/Sandbox/Cube/{tileName}");
            imageTile.texture = TextureTile;

            DeselectSlot();
        }

        public void RegisterSelectListener(string cubeType, UnityAction<string> listener)
        {
            this.cubeType = cubeType;
            selectListener = listener;

            textCubeName.text = cubeType;
        }

        public void SelectSlot()
        {
            selectListener?.Invoke(cubeType);
            imageSelect.gameObject.SetActive(true);
        }

        public void DeselectSlot()
        {
            imageSelect.gameObject.SetActive(false);
        }        
    }
}