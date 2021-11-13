using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
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
        public bool changeProtagonistXpTable = false;
        public bool changeStoryCompanionXpTable = false;
        public bool changeCustomCompanionXpTable = false;
        public int normalXpTableXpNeed20To21 = 1050000;
        public int normalXpTableDifferenceIncreaseAfter20 = 100000;

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
            GUILayout.Label("Base experience difference lv20 to lv21 without conversion to legend XP table", options);
            var maxNXpBaseToKeep = GUILayout.TextField(settings.normalXpTableXpNeed20To21.ToString(), 7);
            if (GUI.changed && !int.TryParse(maxNXpBaseToKeep, out settings.normalXpTableXpNeed20To21))
            {
                settings.normalXpTableXpNeed20To21 = 1050000;
            }
            GUILayout.Label("Increment of level experience difference after level 20 without conversion to legend XP table", options);
            var maxnNXpIncrementToKeep = GUILayout.TextField(settings.normalXpTableDifferenceIncreaseAfter20.ToString(), 6);
            if (GUI.changed && !int.TryParse(maxnNXpIncrementToKeep, out settings.normalXpTableDifferenceIncreaseAfter20))
            {
                settings.normalXpTableDifferenceIncreaseAfter20 = 100000;
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




}
