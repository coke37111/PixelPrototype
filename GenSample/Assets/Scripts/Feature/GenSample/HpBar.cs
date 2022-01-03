using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Feature.GenSample
{
    public class HpBar : MonoBehaviour
    {
        public Image hpGauge;

        public void SetGauge(float ratio)
        {
            if (ratio <= 0f)
                ratio = 0f;

            Vector3 gaugeScale = hpGauge.transform.localScale;
            gaugeScale.x = ratio;

            hpGauge.transform.localScale = gaugeScale;
        }
    }
}