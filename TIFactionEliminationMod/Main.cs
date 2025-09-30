using HarmonyLib;
using UnityModManagerNet;

using System.Reflection;

using static UnityModManagerNet.UnityModManager;



namespace TIFactionEliminationMod
{
    /// <summary>
    /// Controls loading and managing the mod
    /// </summary>
    public class Main
    {
        public static bool enabled;
        public static ModEntry mod;
        public static Settings settings;

        /// <summary>
        /// Entry point of the application (as per ModInfo.json), which applies the Harmony patches
        /// </summary>
        /// <param name="modEntry"></param>
        /// <returns></returns>
        static bool Load(ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id);
            settings = ModSettings.Load<Settings>(modEntry);
            mod = modEntry;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
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

        static void OnGUI(ModEntry modEntry)
        {
            settings.Draw(modEntry);
        }

        static void OnSaveGUI(ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public class Settings : UnityModManager.ModSettings, IDrawable
        {
            [Draw("Maximum storable influence: (default: 5000.0, set 0 to disable)", Min = 0, Precision = 0)] public float influenceCap = 5000f;
            [Draw("Minimum influence per month to undo faction death: (default: 100.0, set 0 to disable)", Min = 0, Precision = 0)] public float monthlyInfluenceToClearMark = 100f;

            public override void Save(UnityModManager.ModEntry modEntry)
            {
                Save(this, modEntry);
            }

            public void OnChange() { }
        }
    }
}
