using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using System;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace PFWOTRCLUNLOCKER
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool unLockCasterLevel = false;
        public bool unLockClassLevel = false;

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
    public static class Spellbook_BaseLevel_Getter_Patch
    {
        public static bool Prefix(Spellbook __instance, ref int ___m_BaseLevelInternal, ref int __result)
        {
            __result = Math.Max(0, ___m_BaseLevelInternal + __instance.Blueprint.CasterLevelModifier);
            return false;
        }

    }


    [HarmonyPatch(typeof(UnitProgressionData))]
    public static class UnitProgressionData_Patch
    {
        [HarmonyPatch("ExperienceTable", MethodType.Getter)]
        public static void Postfix(ref BlueprintStatProgression __result)
        {
            __result = Game.Instance.BlueprintRoot.Progression.XPTable; ;
        }

        [HarmonyPatch("MaxCharacterLevel", MethodType.Getter)]
        public static void Postfix(ref int __result)
        {

            __result = 40;

        }
    }

    [HarmonyPatch(typeof(ApplyClassProgression), "ApplyProgressionLevel")]
    public static class ApplyClassProgression_Patch
    {
        private static bool Prefix(ref int level)
        {
            int i = level;
            if (i >= 40)
            {
                i = 20;
            }
            if (level > 20)
            {
                if (i % 2 == 0)
                {
                    i = 18;
                }
                else
                {
                    i = 19;

                }

            }
            level = i;
            return true;

        }
    }

    [HarmonyPatch(typeof(ProgressionRoot), "XPTable", MethodType.Getter)]
    public static class ProgressionRoot_XPTable_Patch
    {
        public static void Postfix(ref BlueprintStatProgressionReference ___m_XPTable, ref BlueprintStatProgression __result, ProgressionRoot __instance)
        {


            BlueprintStatProgression m_StatProgression = new BlueprintStatProgression();

            BlueprintStatProgressionReference xptable = ___m_XPTable;
            if (xptable == null)
            {
                __result = null;
            }
            BlueprintStatProgression blueprintStatProgression = xptable.Get();
            if (41 > blueprintStatProgression.Bonuses.Length)
            {
                int[] array = new int[41];
                for (int i = 1; i < 20; i++)
                {
                    int num = i * 2;
                    array[num - 1] = blueprintStatProgression.Bonuses[i];
                    array[num] = (blueprintStatProgression.Bonuses[i] + blueprintStatProgression.Bonuses[i + 1]) / 2;
                }
                array[39] = (array[38] + blueprintStatProgression.Bonuses[20]) / 2;
                array[40] = blueprintStatProgression.Bonuses[20];
                m_StatProgression.Bonuses = array;
            }
            if (m_StatProgression.Bonuses.Length == 0)
            {
                m_StatProgression.Bonuses = blueprintStatProgression.Bonuses;
            }
            __result = m_StatProgression;

        }
    }
    [HarmonyPatch(typeof(BlueprintCharacterClass))]
    public static class BlueprintCharacterClass_Patch
    {
        [HarmonyPatch("MeetsPrerequisites")]
        public static void Postfix(ref UnitDescriptor unit, BlueprintCharacterClass __instance, ref bool __result)
        {


            if (!__result)
            {
                int classLevel = unit.Progression.GetClassLevel(__instance);

                if (classLevel >= 20 && classLevel < 40)
                {
                    __result = true;
                }
                if (__instance.PrestigeClass && classLevel < 40 && classLevel >= 10)
                {
                    __result = true;
                }
            }


        }
    }

    [HarmonyPatch(typeof(ProgressionData), "GetLevelEntry")]
    public static class ProgressionData_Patch
    {
        public static bool Prefix(ProgressionData __instance, int level, ref LevelEntry __result)
        {
            int i = level;
            if (i >= 40)
            {
                i = 20;
            }
            if (i > 20)
            {
                if (i % 2 == 0)
                {
                    i = 18;
                }
                else
                {
                    i = 19;

                }
            }
            level = i;
            __result = __instance.LevelEntries.FirstOrDefault((LevelEntry le) => le.Level == level) ?? new LevelEntry(); ;
            return false;
        }


    }
}
