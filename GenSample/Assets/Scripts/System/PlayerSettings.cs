namespace Assets.Scripts.System
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
        }

        public const string PLAYER_LIVES = "GenPlayerLives";
        public const string PLAYER_LOADED_LEVEL = "GenPlayerLoadedLevel";
        public const string MOB_DIE = "GenMobDie";
        public const string FAIL_GAME = "GenGameFail";
    }
}