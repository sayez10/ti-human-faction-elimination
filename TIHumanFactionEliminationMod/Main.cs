// SPDX-FileCopyrightText: Copyright © 2025 explodoboy, sayez10
//
// SPDX-License-Identifier: MIT

using HarmonyLib;
using UnityModManagerNet;

using System.Reflection;

using static UnityModManagerNet.UnityModManager;



namespace TIHumanFactionEliminationMod
{
    /// <summary>
    /// Controls loading and managing the mod
    /// </summary>
    public class Main
    {
        public static bool enabled;
        public static ModEntry mod;

        /// <summary>
        /// Entry point of the application (as per ModInfo.json), which applies the Harmony patches
        /// </summary>
        /// <param name="modEntry"></param>
        /// <returns></returns>
        static bool Load(ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id);
            mod = modEntry;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            return true;
        }

        /// <summary>
        /// Toggles the enabled state when the mod is toggled by the UMM interface or the TI interface
        /// </summary>
        /// <param name="modEntry"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static bool OnToggle(ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
    }
}
