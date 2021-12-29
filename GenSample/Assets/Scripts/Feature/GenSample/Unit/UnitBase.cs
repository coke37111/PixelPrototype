using Assets.Scripts.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class UnitBase : MonoBehaviour
    {
        protected Rigidbody rb;

        private UnitParts unitParts;

        private bool oldLeftDir;
        protected bool isLeftDir
        {
            get => oldLeftDir;
            set
            {
                if (oldLeftDir != value)
                {
                    oldLeftDir = value;
                    OnChangeDir(value);
                }
            }
        }

        protected bool controlable;

        #region UNITY

        protected virtual void Update()
        {
            if (!controlable)
                return;

            Move();
            Jump();
            Attack();
        }

        protected virtual void OnCollisionEnter(Collision coll)
        {
        }

        protected virtual void OnCollisionExit(Collision coll)
        {
        }

        #endregion

        #region abstract method

        protected abstract void Move();
        protected abstract void Jump();
        protected abstract void Attack();

        #endregion

        public virtual void Init()
        {
            rb = GetComponent<Rigidbody>();
            unitParts = transform.GetComponentInChildren<UnitParts>();
            if (unitParts == null)
                Log.Error($"unitParts component가 존재하지 않습니다!");

            transform.SetParent(FindObjectOfType<UnitContainer>().transform);
        }

        protected virtual void OnChangeDir(bool isLeft)
        {
            if (unitParts == null)
                Log.Error($"unitParts component가 존재하지 않습니다!");
            else
                unitParts.FlipX(isLeft);
        }

        public void SetSprite(Dictionary<string, string> unitPartsList)
        {
            if (unitParts == null)
            {
                Log.Error($"unitParts component가 존재하지 않습니다!");
                return;
            }

            foreach (string unitPartName in unitPartsList.Keys)
            {
                string resPath = unitPartsList[unitPartName];
                unitParts.SetSprite(unitPartName, resPath);
            }

            unitParts.RotateSprite();
        }

        public virtual void ResetSpawnPos(Vector3 pos) { }

        public virtual void Knockback(float centerX, float centerZ) { }
    }
}