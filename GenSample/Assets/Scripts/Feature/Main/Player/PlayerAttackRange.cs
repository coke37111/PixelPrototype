using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Player
{
    public class PlayerAttackRange : MonoBehaviour
    {
        private List<Collider> targetList = new List<Collider>();

        #region UNITY

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                if (!targetList.Contains(other))
                    targetList.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.tag == "Player")
            {
                if (targetList.Contains(other))
                    targetList.Remove(other);
            }
        }

        #endregion

        public List<Collider> GetTargetList()
        {
            return targetList;
        }
    }
}