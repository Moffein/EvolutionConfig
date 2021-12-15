using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;

namespace EvolutionConfig
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.EvolutionConfig", "Evolution Config", "1.0.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class EvolutionConfig : BaseUnityPlugin
    {
        public static int whiteCount = 1;
        public static int greenCount = 1;
        public static int redCount = 1;


        private void ReadConfig()
        {
            whiteCount = Config.Bind("Item Counts", "Common", 1, "How many items of this tier to grant.").Value;
            greenCount = Config.Bind("Item Counts", "Uncommon", 1, "How many items of this tier to grant.").Value;
            redCount = Config.Bind("Item Counts", "Legendary", 1, "How many items of this tier to grant.").Value;
        }

        public void Awake()
        {
            ReadConfig();

            IL.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.GrantMonsterTeamItem += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchCallvirt<Inventory>("GiveItem")
                    );
                c.Emit(OpCodes.Ldloc_0);    //ItemTier
                c.EmitDelegate<Func<int, ItemTier, int>>((itemCount, tier) =>
                {
                    switch(tier)
                    {
                        case ItemTier.Tier1:
                            return whiteCount;
                        case ItemTier.Tier2:
                            return greenCount;
                        case ItemTier.Tier3:
                            return redCount;
                        default:
                            return itemCount;
                    }
                });
            };
        }
    }
}
