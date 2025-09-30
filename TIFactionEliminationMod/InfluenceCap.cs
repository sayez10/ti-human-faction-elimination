using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using UnityModManagerNet;

namespace TIFactionEliminationMod
{
    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.AddToCurrentResource))]
    internal static class InfluenceCap
    {
        // Vanilla game has 10 built-in (including 'none') factions, plus an extra 8 for modders
        private static readonly bool[] _markedForDeath = new bool[18];

        [HarmonyPostfix]
        private static void LimitInfluence(TIFactionState __instance)
        {
            // This function jumps in AFTER a resource has been added to a faction's coffers.
            // It makes sure a faction that has been cleared of both influence and councilors can never come back, without working for it.
            // It also enforces a cap to influence storage so killing off a faction doesn't take literally forever.

            // If mod has been disabled, abort patch.
            if (!Main.enabled) { return; }

            // Game might not be fully initialized yet when the patched vanilla method is called the first time
            var factionIdeologyTemplate = __instance.ideology;
            if (factionIdeologyTemplate == null) { return; }

            // If the faction has been marked, then their influence is locked to 0 at all times, no matter what.
            int thisFaction = (int)factionIdeologyTemplate.ideology;
            var monthlyIncome = __instance.GetMonthlyIncome(FactionResource.Influence);
            if (_markedForDeath[thisFaction])
            {
                // If a faction manages to generate 100 influence a month, their mark is cleared, and the faction can start storing influence again.
                // This represents sufficient traction, whether on earth or in space, causing the recreation of a previously-defunct faction.
                // Also, if monthlyInfluenceToClearMark is zero, the check is skipped; factions remain dead.
                if (Main.settings.monthlyInfluenceToClearMark != 0 && monthlyIncome > Main.settings.monthlyInfluenceToClearMark) { _markedForDeath[thisFaction] = false; return; }

                __instance.resources[FactionResource.Influence] = 0;
                return; // No need to waste time on re-checking _markedForDeath below, or setting the cap.
            }

            // If the faction has no councilors, and have zero influence with an active defecit, they are marked for death.
            // First checks if the faction is already marked. If so, checking again is totally unnecessary.
            if (!_markedForDeath[thisFaction] && __instance.resources[FactionResource.Influence] <= 0 && __instance.numActiveCouncilors == 0 && monthlyIncome < 0)
            {
                _markedForDeath[thisFaction] = true;
                __instance.resources[FactionResource.Influence] = 0;

                return; // No need to proceed to enforcing the cap. Influence is definitely under the cap, now.
            }

            // If the faction isn't marked for death, then the influence cap is enforced as usual.
            // Also, if influenceCap is zero, the check is skipped completely; there is no cap.
            float influence = __instance.GetCurrentResourceAmount(FactionResource.Influence);
            if (Main.settings.influenceCap != 0 && influence > Main.settings.influenceCap)
            {
                __instance.resources[FactionResource.Influence] = Main.settings.influenceCap;
            }
        }

        // When called, every mark is cleared. This is to prevent marks carrying over between completely different saves.
        internal static void Reset()
        {
            for (int i = 0; i < _markedForDeath.Length; i++)
            {
                _markedForDeath[i] = false;
            }
        }
    }

    // This latches onto (what I think is) the last function in charge of loading a save. All I know for sure, is that it runs only once during a game load.
    // Basically, when a save is loaded for the first time in a session, it's populated with 'false' values.
    [HarmonyPatch(typeof(GameControl), nameof(GameControl.CompleteInit))]
    internal static class RefreshInfluenceCap
    {
        [HarmonyPostfix]
        private static void TriggerRefresh()
        {
            // If mod has been disabled, abort reset
            if (!Main.enabled) { return; }

            InfluenceCap.Reset();
        }
    }
}
