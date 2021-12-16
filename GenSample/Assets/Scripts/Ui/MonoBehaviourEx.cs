using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectF
{
    public class MonoBehaviourEx : MonoBehaviour
    {
        public bool Exist(string targetName)
        {
            return transform.FindChildrenRecursively(targetName).Any();
        }

        public GameObject FindChild(string targetName)
        {
            var list = transform.FindChildrenRecursively(targetName);
            if (list.Any() == false)
                throw new System.Exception($"하이어라키에 오브젝트 없음 : {targetName}");

            return transform.FindChildrenRecursively(targetName).First().gameObject;
        }

        public IEnumerable<GameObject> FindChildAll(string targetName)
        {
            var list = transform.FindChildrenRecursively(targetName);
            if (list.Any() == false)
                throw new System.Exception($"하이어라키에 오브젝트 없음 : {targetName}");

            return transform.FindChildrenRecursively(targetName).Select(t => t.gameObject);
        }

        public float GetAngle(Vector3 from, Vector3 to)
        {
            Vector3 v = (from - to).normalized;

            return Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg - 180.0f;
        }

        public float GetAngleForUi(Vector3 from, Vector3 to)
        {
            Vector3 v = (from - to).normalized;

            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90;
        }

        public string GetText(string targetName)
        {
            return FindChild(targetName).GetComponent<TMPro.TextMeshProUGUI>().text;
        }

        public Sprite GetUiSprite(string path)
        {
            return Resources.Load<Sprite>($"ui/{path}");
        }

        public void SetActive(string targetName, bool active)
        {
            FindChild(targetName).SetActive(active);
        }

        public void SetColor(string targetName, Color color)
        {
            FindChild(targetName).GetComponent<Image>().color = color;
        }

        public void SetLayerRecursively(GameObject obj, string layer)
        {
            obj.layer = LayerMask.NameToLayer(layer);
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;

                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public void SetText(string targetName, string text)
        {
            FindChild(targetName).GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        public void SetTextColor(string targetName, Color color)
        {
            FindChild(targetName).GetComponent<TMPro.TextMeshProUGUI>().color = color;
        }

        protected void AddClickEvent(Button button, UnityAction callback)
        {
            var clickEvent = new Button.ButtonClickedEvent();
            clickEvent.AddListener(callback);

            button.onClick = clickEvent;
        }

        protected void AddInputEvent(TMPro.TMP_InputField inputField, Action<string> onChange)
        {
            inputField.onValueChanged.AddListener((v) => { onChange?.Invoke(v); });
        }
    }
}