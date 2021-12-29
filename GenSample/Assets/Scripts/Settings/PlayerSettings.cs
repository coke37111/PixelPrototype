namespace Assets.Scripts.Settings
{
    public static class PlayerSettings
    {
        public enum EventCodeType
        {
            Move,
            Knockback,
            MobAttackBy,
            MobDie,
            MakeAtkEff,
            MobRegenHp,
            PlayerDie,
            Clear,
            Fail,
        }

        public const string PLAYER_LOADED_LEVEL = "GenPlayerLoadedLevel"; // 모든 player 씬 로드 체크 후 player 생성
        public const string PLAYER_DIE = "GEN_PLAYER_DIE"; // player 낙하로 인한 종료 체크 new
        public const string FAIL_GAME = "GenGameFail"; // 시간 초과로 인한 종료 체크

        public const int DEFAULT_PLAYER_LIVES = 1;

        private static bool isConn = false;

        public static void ConnectNetwork()
        {
            isConn = true;
        }

        public static void DisconnectNetwork()
        {
            isConn = false;
        }

        public static bool IsConnectNetwork()
        {
            return isConn;
        }
    }
}