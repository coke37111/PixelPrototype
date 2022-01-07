using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample.Unit
{
    public class UnitSpineParts : MonoBehaviour
    {

        #region UNITY

        private void Update()
        {
            RotateSprite();
        }

        #endregion

        public void RotateSprite()
        {
            Vector3 quaUnit = transform.rotation.eulerAngles;
            Vector3 quaCam = Camera.main.transform.rotation.eulerAngles;
            quaUnit.x = quaCam.x;
            quaUnit.y = quaCam.y;
            Quaternion quaUnitNew = Quaternion.Euler(quaUnit);
            transform.rotation = quaUnitNew;
        }
    }
}