using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using RoR2.Artifacts;
using System;
using UnityEngine;

namespace EvolutionConfig
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.EvolutionConfig", "Evolution Config", "1.0.3")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2API.Utils.R2APISubmoduleDependency(nameof(CommandHelper))]
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
                c.Emit(OpCodes.Ldloc_2);    //PickupDef
                c.EmitDelegate<Func<int, PickupDef, int>>((itemCount, pickup) =>
                {
                    switch(pickup.itemTier)
                    {
                        case ItemTier.Tier1:
                        case ItemTier.VoidTier1:
                            return whiteCount;
                        case ItemTier.Tier2:
                        case ItemTier.VoidTier2:
                            return greenCount;
                        case ItemTier.Tier3:
                        case ItemTier.VoidTier3:
                            return redCount;
                        default:
                            return itemCount;
                    }
                });
            };
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        [ConCommand(commandName = "evolution_additem", flags = ConVarFlags.ExecuteOnServer, helpText = "Add an item to Evolution. [string ItemName, int count]")]
        public static void EvolutionConfig_GrantItems(ConCommandArgs args)
        {
            string itemString = args.TryGetArgString(0);
            if (args.Count >= 2)
            {
                int? itemCount = args.TryGetArgInt(1);
                if (itemCount.HasValue)
                {
                    GrantMonsterTeamItem(itemString, itemCount.Value);
                    return;
                }
            }
            GrantMonsterTeamItem(itemString);
        }

        public static void GrantMonsterTeamItem(string itemName, int count = 1)
        {
            ItemIndex item = ItemCatalog.FindItemIndex(itemName);
            if (item != ItemIndex.None)
            {
                MonsterTeamGainsItemsArtifactManager.monsterTeamInventory.GiveItem(item, count);
            }
            else
            {
                Debug.LogError("Item not found. If you have DebugToolkit installed, type list_item to view a list of all internal item names. You must enter the item name exactly as it appears in the list.");
            }
        }
        public static void GrantMonsterTeamItem(ItemIndex itemIndex, int count)
        {
            MonsterTeamGainsItemsArtifactManager.monsterTeamInventory.GiveItem(itemIndex, count);
        }
    }
}
