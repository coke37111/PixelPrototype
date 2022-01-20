using UnityEngine;

public class UITweenScale : UITween {
	private RectTransform rectTransform = null;

	[SerializeField]
	private Vector3 from = new Vector3(1, 1, 1);

	[SerializeField]
	private Vector3 to = new Vector3(1, 1, 1);

	private void Awake() {
		rectTransform = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
	}

	protected override void SetValue(float time) {
		rectTransform.localScale = from + (to - from) * GetAnimationCurveValue(time);
	}
}
