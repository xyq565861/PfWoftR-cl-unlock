using HarmonyLib;
using Kingmaker.UnitLogic;
using System;
using UnityEngine;
using UnityModManagerNet;

namespace PFWOTRCLUNLOCKER
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool displayGlamour = false;


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }


    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
            Harmony harmony = new Harmony(modEntry.Info.Id);
            Main.settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            ;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
            harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

            return true;

        }
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!Main.enabled)
            {
                return;
            }
           (new GUILayoutOption[1])[0] = GUILayout.ExpandWidth(false);
        }
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }
    [HarmonyPatch(typeof(Spellbook), "BaseLevel", MethodType.Getter)]
    public static class Spellbook_BaseLevel_Getter
    {
        public static bool Prefix(Spellbook __instance, ref int ___m_BaseLevelInternal, ref int __result)
        {
            __result = Math.Max(0, ___m_BaseLevelInternal + __instance.Blueprint.CasterLevelModifier);
            return false;
        }

    }

}
