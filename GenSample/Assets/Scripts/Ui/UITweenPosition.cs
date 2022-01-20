using UnityEngine;

public class UITweenPosition : UITween {
	private RectTransform rectTransform = null;

	[SerializeField]
	private Vector3 from = Vector3.zero;

	[SerializeField]
	private Vector3 to = Vector3.zero;

	private void Awake() {
		rectTransform = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
	}

	protected override void SetValue(float time) {
		rectTransform.anchoredPosition3D = from + (to - from) * GetAnimationCurveValue(time);
	}
}
