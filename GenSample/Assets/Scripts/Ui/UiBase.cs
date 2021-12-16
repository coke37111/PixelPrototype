using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectF
{
    public abstract class UiBase : MonoBehaviourEx
    {
        public bool _hideHudOnOpen = false;
        public bool _overPopup = false;
        public bool _useBlur = false;

        public bool CheckForOverlap(RectTransform a, RectTransform b)
        {
            var r1 = GetWorldRect(a);
            var r2 = GetWorldRect(b);

            return r1.Overlaps(r2);
        }

        public bool CheckOverlaps(RectTransform rectTrans1, RectTransform rectTrans2)
        {
            Rect rect1 = new Rect(rectTrans1.localPosition.x, rectTrans1.localPosition.y, rectTrans1.rect.width, rectTrans1.rect.height);
            Rect rect2 = new Rect(rectTrans2.localPosition.x, rectTrans2.localPosition.y, rectTrans2.rect.width, rectTrans2.rect.height);

            return rect1.Overlaps(rect2);
        }

        public virtual void Close()
        {
            OnClose();
            gameObject.SetActive(false);
        }

        public Rect GetWorldRect(RectTransform rt)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 topLeft = corners[0];

            Vector2 size = new Vector2(rt.rect.size.x, rt.rect.size.y);
            return new Rect(topLeft, size);
        }

        public bool Intersects(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, out Vector3 intersection)
        {
            intersection = Vector3.zero;

            Vector3 b = a2 - a1;
            Vector3 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector3 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            intersection = a1 + t * b;

            return true;
        }

        public abstract void OnOpen(object parameter);

        protected abstract void OnClose();

        protected void RemoveClickEvent(Button button)
        {
            button.onClick = null;
        }

        public bool IsOpend => gameObject.activeInHierarchy;
    }
}