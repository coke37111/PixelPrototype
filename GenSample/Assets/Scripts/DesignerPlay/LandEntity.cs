using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectF
{
    public enum Team
    {
        Heroes,
        LandMaster,
        DungeonKeeper,
    }

    public enum Neighbor
    {
        Up,
        Down,
        Left,
        Right,
    }

    public class LandEntity : MonoBehaviourEx
    {
        private TMPro.TextMeshProUGUI _text;
        private Team _team = Team.LandMaster;
        private int _index;
        private Dictionary<Neighbor, LandEntity> _neighbors = new Dictionary<Neighbor, LandEntity>();

        public int Index => _index;
        public bool IsLandMaster => _team == Team.LandMaster;

        public void SetIndex(int index)
        {
            _index = index;
        }

        public void SetNeighbor(Neighbor neighbor, LandEntity landEntity)
        {
            if (landEntity == null)
                return;

            _neighbors[neighbor] = landEntity;
        }

        public void SetTeam(Team team)
        {
            _team = team;

            if (_team == Team.Heroes)
            {
                GetComponent<Image>().color = new Color(0.5803922f, 0.5803922f, 1f);
                
            }
            else if (_team == Team.DungeonKeeper)
            {
                GetComponent<Image>().color = new Color(1f, 0.5792453f, 0.5792453f);
            }

        }

        void Start()
        {
            _text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            AddClickEvent(GetComponent<Button>(), OnClick);
        }

        private void OnClick()
        {
            DesignerPlayManager.Instance.OnClick(this);
            //print(gameObject.name);
        }

        public void SetMy()
        {
            _text.text = "³»¶¥";
        }

        void Update()
        {

        }

        public bool NoNeighbor => _neighbors.All(t => t.Value != null && t.Value.IsLandMaster);
    }
}