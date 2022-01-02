using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectF
{
    public class DesignerPlayManager : MonoBehaviourEx
    {
        private static DesignerPlayManager _instance;
        
        private List<LandPlayer> _landPlayers = new List<LandPlayer>();
        public LandEntity[] _lands;

        public LandEntity _myLand;

        public static DesignerPlayManager Instance => _instance;

        public void OnClick(LandEntity landEntity)
        {
            if (_myLand == null)
            {
                _myLand = landEntity;
                _myLand.SetMy();
            }
        }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        private LandEntity GetLand(int index)
        {
            if (index < 0 || _lands.Length <= index)
                return null;

            return _lands[index];
        }

        private void Start()
        {
            var lands = _lands.ToList();

            for (int i = 0; i < lands.Count; ++i)
            {
                _lands[i].SetIndex(i);
            }

            foreach (var land in _lands)
            {
                land.SetNeighbor(Neighbor.Up, GetLand(land.Index - 11));
                land.SetNeighbor(Neighbor.Down, GetLand(land.Index + 11));
                land.SetNeighbor(Neighbor.Left, GetLand(land.Index - 1));
                land.SetNeighbor(Neighbor.Right, GetLand(land.Index + 1));
            }
                         
            var count = 0;

            while (count < 6)
            {
                var land = lands[Random.Range(0, lands.Count)];

                if (land && land.NoNeighbor)
                {
                    lands.Remove(land);
                    land.SetTeam(Team.DungeonKeeper);
                    ++count;
                }
            }

            count = 0;

            while (count < 6)
            {
                var land = lands[Random.Range(0, lands.Count)];

                if (land && land.NoNeighbor)
                {
                    lands.Remove(land);
                    land.SetTeam(Team.Heroes);
                    ++count;
                }
            }

            foreach (var l in lands)
                l.SetTeam(Team.LandMaster);

            for (int i = 0; i < 1000; ++i)
                _landPlayers.Add(new LandPlayer());
        }

        private void Update()
        {
        }
    }
}
