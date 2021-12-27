using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Assets.Scripts.Settings;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings.SO;

namespace Assets.Scripts.Feature.GenSample
{
    public class GenCountdownTimer : MonoBehaviourPunCallbacks
    {

        /// <summary>
        ///     OnCountdownTimerHasExpired delegate.
        /// </summary>
        public delegate void CountdownTimerHasExpired();

        private bool isTimerRunning;

        private int startTime;

        [Header("Reference to a Text component for visualizing the countdown")]
        public TMPro.TextMeshProUGUI Text;

        private GameSettingSO gameSetting;

        /// <summary>
        ///     Called when the timer has expired.
        /// </summary>
        public static event CountdownTimerHasExpired OnCountdownTimerHasExpired;


        #region UNITY

        public void Start()
        {
            if (this.Text == null) Debug.LogError("Reference to 'Text' is not set. Please set a valid reference.", this);
        }

        public override void OnEnable()
        {
            Debug.Log("OnEnable CountdownTimer");
            base.OnEnable();

            // the starttime may already be in the props. look it up.
            Initialize();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Debug.Log("OnDisable CountdownTimer");
        }


        public void Update()
        {
            if (!this.isTimerRunning) return;

            float countdown = TimeRemaining();
            this.Text.text = string.Format("Ready {0}", countdown.ToString("n0"));

            if (countdown > 0.0f) return;

            OnTimerEnds();
        }

        #endregion

        private void OnTimerRuns()
        {
            this.isTimerRunning = true;
            this.enabled = true;
        }

        private void OnTimerEnds()
        {
            this.isTimerRunning = false;
            this.enabled = false;

            Debug.Log("Emptying info text.", this.Text);
            this.Text.text = string.Empty;

            if (OnCountdownTimerHasExpired != null) OnCountdownTimerHasExpired();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Debug.Log("CountdownTimer.OnRoomPropertiesUpdate " + propertiesThatChanged.ToStringFull());
            Initialize();
        }

        private void Initialize()
        {
            if (!PhotonNetwork.IsConnected)
                return;

            int propStartTime;
            if (TryGetStartTime(out propStartTime))
            {
                this.startTime = propStartTime;
                Debug.Log("Initialize sets StartTime " + this.startTime + " server time now: " + PhotonNetwork.ServerTimestamp + " remain: " + TimeRemaining());


                this.isTimerRunning = TimeRemaining() > 0;

                if (this.isTimerRunning)
                    OnTimerRuns();
                else
                    OnTimerEnds();
            }
        }


        private float TimeRemaining()
        {
            if(gameSetting == null)
            {
                gameSetting = ResourceManager.LoadAsset<GameSettingSO>(GameSettingSO.path);
            }

            int timer = PhotonNetwork.ServerTimestamp - this.startTime;
            return gameSetting.startDelay - timer / 1000f;
        }


        public static bool TryGetStartTime(out int startTimestamp)
        {
            startTimestamp = PhotonNetwork.ServerTimestamp;

            object startTimeFromProps;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomSettings.CountdownStartTime, out startTimeFromProps))
            {
                startTimestamp = (int)startTimeFromProps;
                return true;
            }

            return false;
        }


        public static void SetStartTime()
        {
            int startTime = 0;
            bool wasSet = TryGetStartTime(out startTime);

            Hashtable props = new Hashtable
            {
                {RoomSettings.CountdownStartTime, (int)PhotonNetwork.ServerTimestamp}
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);


            Debug.Log("Set Custom Props for Time: " + props.ToStringFull() + " wasSet: " + wasSet);
        }
    }
}