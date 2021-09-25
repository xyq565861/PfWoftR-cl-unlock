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
        public bool unLockCasterLevel = true;
        public bool unLockClassLevel = true;
        public bool unLockCharacterLevel = true;
        public bool changeProtagonistXpTable = true;
        public bool changeStoryCompanionXpTable = true;
        public bool changeCustomCompanionXpTable = true;
        public int normalXpTableDifferenceAfter20 = 100000;

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
            GUILayoutOption[] options = new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(1000f)
            };
            Main.settings.unLockCasterLevel = GUILayout.Toggle(Main.settings.unLockCasterLevel, "To unlock the upper limit of 20 CL from one class and synchronize class level to caster level.", options);
            Main.settings.unLockClassLevel = GUILayout.Toggle(Main.settings.unLockClassLevel, "To unlock the upper limit of every class level to 40.", options);
            Main.settings.unLockCharacterLevel = GUILayout.Toggle(Main.settings.unLockCharacterLevel, "To unlock the upper limit of character level to 40.", options);
            Main.settings.changeProtagonistXpTable = GUILayout.Toggle(Main.settings.changeProtagonistXpTable, "Switch  your Protagonist level up curve to the legendary character xp table.", options);
            Main.settings.changeStoryCompanionXpTable = GUILayout.Toggle(Main.settings.changeStoryCompanionXpTable, "Switch  your StoryCompanion level up curve to the legendary character xp table.", options);
            Main.settings.changeCustomCompanionXpTable = GUILayout.Toggle(Main.settings.changeCustomCompanionXpTable, "Switch  your CustomCompanion level up curve to the legendary character xp table.", options);
            GUILayout.Label("Increment of level experience difference after level 20 without conversion to legend XP table", options);
            var maxBackupsToKeep = GUILayout.TextField(settings.normalXpTableDifferenceAfter20.ToString(), 6);
            if (GUI.changed && !int.TryParse(maxBackupsToKeep, out settings.normalXpTableDifferenceAfter20))
            {
                settings.normalXpTableDifferenceAfter20 = 100000;
            }
            //(new GUILayoutOption[1])[0] = GUILayout.ExpandWidth(false);
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
        [HarmonyPriority(Priority.High)]
        public static bool Prefix(Spellbook __instance, ref int ___m_BaseLevelInternal, ref int __result)
        {

            if (Main.settings.unLockCasterLevel)
            {
                __result = Math.Max(0, ___m_BaseLevelInternal + __instance.Blueprint.CasterLevelModifier);
                return false;
            }
            return true;
        }

    }


    [HarmonyPatch(typeof(UnitProgressionData))]
    
    public static class UnitProgressionData_Patch
    {
        [HarmonyPatch("ExperienceTable", MethodType.Getter)]
        [HarmonyAfter(new string[] { "ToyBox" })]
        [HarmonyPriority(Priority.Last)]


        public static void Postfix(UnitProgressionData __instance, ref BlueprintStatProgression __result)
        {
            if (Main.settings.changeProtagonistXpTable)
            {
                if (__instance.Owner.IsMainCharacter || __instance.Owner.Unit.IsCloneOfMainCharacter)
                {
                    __result = Game.Instance.BlueprintRoot.Progression.LegendXPTable.Or(null) ?? Game.Instance.BlueprintRoot.Progression.XPTable;
                }
            }
            if (Main.settings.changeStoryCompanionXpTable)
            {
                if (__instance.Owner.Unit.IsStoryCompanion())
                {
                    __result = Game.Instance.BlueprintRoot.Progression.LegendXPTable.Or(null) ?? Game.Instance.BlueprintRoot.Progression.XPTable;
                }
            }
            if (Main.settings.changeCustomCompanionXpTable)
            {
                if (__instance.Owner.Unit.IsCustomCompanion())
                {
                    __result = Game.Instance.BlueprintRoot.Progression.LegendXPTable.Or(null) ?? Game.Instance.BlueprintRoot.Progression.XPTable;
                }
            }
            //Main.Logger.Log(__instance.Owner.CharacterName);
            //Main.Logger.Log("IsMainCharacter:" + __instance.Owner.IsMainCharacter.ToString());
            //Main.Logger.Log("IsPet:" + __instance.Owner.Unit.IsPet.ToString());
            //Main.Logger.Log("IsStoryCompanion:" + __instance.Owner.Unit.IsStoryCompanion().ToString());
            //Main.Logger.Log("IsCloneOfMainCharacter:" + __instance.Owner.Unit.IsCloneOfMainCharacter.ToString());
            //Main.Logger.Log("IsCustomCompanion:" + __instance.Owner.Unit.IsCustomCompanion().ToString());
            //Main.Logger.Log("CharacterLevel:" + __instance.CharacterLevel);
            //Main.Logger.Log("Experience:" + __instance.Experience);
            //Main.Logger.Log("normal:" + Game.Instance.BlueprintRoot.Progression.XPTable.GetBonus(__instance.CharacterLevel + 1));
            //Main.Logger.Log("Legend:" + Game.Instance.BlueprintRoot.Progression.LegendXPTable.GetBonus(__instance.CharacterLevel + 1));
            //Main.Logger.Log("__result:" + __result.GetBonus(__instance.CharacterLevel + 1));



        }

        [HarmonyPatch("MaxCharacterLevel", MethodType.Getter)]
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(ref int __result)
        {
            if (Main.settings.unLockCharacterLevel)
            {
                __result = 40;
            }
        }
    }

    [HarmonyPatch(typeof(ApplyClassProgression), "ApplyProgressionLevel")]
    public static class ApplyClassProgression_Patch
    {
        [HarmonyPriority(Priority.Low)]
        private static bool Prefix(ref int level)
        {
            if (Main.settings.unLockCharacterLevel || Main.settings.unLockClassLevel)
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
            }
            return true;

        }
    }

    [HarmonyPatch(typeof(ProgressionRoot), "XPTable", MethodType.Getter)]
    public static class ProgressionRoot_XPTable_Patch
    {
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(ref BlueprintStatProgressionReference ___m_XPTable, ref BlueprintStatProgression __result, ProgressionRoot __instance)
        {

            if (Main.settings.unLockCharacterLevel)
            {
                int diff= Main.settings.normalXpTableDifferenceAfter20;
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
                    for (int i = 0; i < 21; i++)
                    {
                        array[i] = blueprintStatProgression.Bonuses[i];
                    }
                    int num = array[20] - array[19];
                    for (int i = 21; i < 41; i++)
                    {
                        array[i] = array[i-1] + num;
                        num = num + diff;
                    }

                    m_StatProgression.Bonuses = array;
                }
                if (m_StatProgression.Bonuses.Length == 0)
                {
                    m_StatProgression.Bonuses = blueprintStatProgression.Bonuses;
                }
                __result = m_StatProgression;
            }
        }
    }
    [HarmonyPatch(typeof(BlueprintCharacterClass))]
    public static class BlueprintCharacterClass_Patch
    {
        [HarmonyPatch("MeetsPrerequisites")]
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(ref UnitDescriptor unit, BlueprintCharacterClass __instance, ref bool __result)
        {

            if (Main.settings.unLockClassLevel)
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
    }

    [HarmonyPatch(typeof(ProgressionData), "GetLevelEntry")]
    public static class ProgressionData_Patch
    {
        [HarmonyPriority(Priority.High)]
        public static bool Prefix(ProgressionData __instance, int level, ref LevelEntry __result)
        {
            if (Main.settings.unLockCharacterLevel || Main.settings.unLockClassLevel)
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
            return true;
        }


    }
}
