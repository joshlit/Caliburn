using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public static class AtlasRAHelpers
    {
        // Augmented Str, Dex, etc. 6 points per level.
        public static int GetStatEnhancerAmountForLevel(int level)
        {
            if (level < 1) return 0;

            switch (level)
            {
                case 1: return 6;
                case 2: return 12;
                case 3: return 18;
                case 4: return 24;
                case 5: return 30;
                default: return 30;
            }
        }

        // Mastery of Arms, Pain, etc. 3% per level.
        public static int GetPropertyEnhancerAmountForLevel(int level)
        {
            if (level < 1) return 0;

            switch (level)
            {
                case 1: return 3;
                case 2: return 6;
                case 3: return 9;
                case 4: return 12;
                case 5: return 15;
                default: return 15;
            }
        }

        // Shared by almost all passive OF Realm Abilities.
        public static int GetCommonPropertyEnhancerCostForUpgrade(int level)
        {
            switch (level)
            {
                case 0: return 1;
                case 1: return 3;
                case 2: return 6;
                case 3: return 10;
                case 4: return 14;
                default: return 1000;
            }
        }

        public static bool HasAugDexLevel(GamePlayer player, int level)
        {
            AtlasOF_RADexterityEnhancer augDex = player.GetAbility<AtlasOF_RADexterityEnhancer>();
            if (augDex == null)
                return false;

            return player.CalculateSkillLevel(augDex) >= level;
        }

        public static bool HasAugStrLevel(GamePlayer player, int level)
        {
            AtlasOF_RAStrengthEnhancer augStr = player.GetAbility<AtlasOF_RAStrengthEnhancer>();
            if (augStr == null)
                return false;

            return player.CalculateSkillLevel(augStr) >= level;
        }

        public static bool HasAugConLevel(GamePlayer player, int level)
        {
            AtlasOF_RAConstitutionEnhancer augCon = player.GetAbility<AtlasOF_RAConstitutionEnhancer>();
            if (augCon == null)
                return false;

            return player.CalculateSkillLevel(augCon) >= level;
        }

        public static bool HasAugAcuityLevel(GamePlayer player, int level)
        {
            AtlasOF_RAAcuityEnhancer augAcuity = player.GetAbility<AtlasOF_RAAcuityEnhancer>();
            if (augAcuity == null)
                return false;

            return player.CalculateSkillLevel(augAcuity) >= level;
        }
    }
}
