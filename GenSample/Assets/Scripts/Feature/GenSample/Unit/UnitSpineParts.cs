using Assets.Scripts.Settings;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample.Unit
{
    public class UnitSpineParts : MonoBehaviour
    {

        #region UNITY

        private void Update()
        {
            if (RoomSettings.roomType == RoomSettings.ROOM_TYPE.Bomberman)
                return;

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