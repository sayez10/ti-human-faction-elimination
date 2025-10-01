// SPDX-FileCopyrightText: Copyright © 2025 explodoboy, sayez10
//
// SPDX-License-Identifier: MIT

using System;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;



namespace TIFactionEliminationMod
{
    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.AddToCurrentResource))]
    internal static class MarkFactionAsDead
    {
        // Vanilla game has 10 built-in (including 'none') factions, plus an extra 8 for modders
        private static readonly bool[] _markedAsDead = new bool[18];

        [HarmonyPostfix]
        private static void AddToCurrentResourcePostfix(TIFactionState __instance)
        {
            // This function jumps in AFTER a resource has been added to a faction's coffers.
            // It makes sure a faction that has been cleared of both influence and councilors can never come back.
            // It also enforces a cap to influence storage so killing off a faction doesn't take literally forever.

            // If mod has been disabled, abort patch
            if (!Main.enabled) { return; }

            // Game might not be fully initialized yet when the patched vanilla method is called the first time
            var factionIdeologyTemplate = __instance.ideology;
            if (factionIdeologyTemplate == null) { return; }

            // If the faction has been marked, then their influence is locked to 0 at all times
            int thisFaction = (int)factionIdeologyTemplate.ideology;
            float monthlyInfluenceIncome = __instance.GetMonthlyIncome(FactionResource.Influence);

            // We check this first to simplify conditions below
            if (_markedAsDead[thisFaction])
            {
                __instance.resources[FactionResource.Influence] = 0;

                // No need to waste time on re-checking _markedAsDead below, or setting the cap
                return;
            }

            // If the faction has no councilors, zero influence, and an active defecit, it's marked as dead
            if (__instance.resources[FactionResource.Influence] <= 0 && __instance.numActiveCouncilors == 0 && monthlyInfluenceIncome < 0)
            {
                _markedAsDead[thisFaction] = true;
                __instance.resources[FactionResource.Influence] = 0;

                // No need to proceed to enforcing the cap, influence is definitely under the cap
                return; 
            }

            // If the faction isn't marked as dead, then the influence cap is enforced as usual
            // Also, if influenceCap is zero, the check is skipped completely
            float currentInfluence = __instance.GetCurrentResourceAmount(FactionResource.Influence);
            if (Main.settings.influenceCap != 0 && currentInfluence > Main.settings.influenceCap)
            {
                __instance.resources[FactionResource.Influence] = Main.settings.influenceCap;
            }
        }

        // When called, every mark is cleared. This is to prevent marks carrying over between completely different saves.
        internal static void Reset()
        {
            for (int i = 0; i < _markedAsDead.Length; i++)
            {
                _markedAsDead[i] = false;
            }
        }
    }

    // This latches onto (what I think is) the last function in charge of loading a save. All I know for sure, is that it runs only once during a game load.
    // Basically, when a save is loaded for the first time in a session, it's populated with 'false' values.
    [HarmonyPatch(typeof(GameControl), nameof(GameControl.CompleteInit))]
    internal static class ResetFactionsStatus
    {
        [HarmonyPostfix]
        private static void CompleteInitPostfix()
        {
            // If mod has been disabled, abort patch
            if (!Main.enabled) { return; }

            MarkFactionAsDead.Reset();
        }
    }
}
