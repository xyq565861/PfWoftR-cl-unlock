using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utils;

namespace PFWOTRCLUNLOCKER
{
    class ClassCapUnlocker
    {
        [HarmonyPatch(typeof(UnitProgressionData))]

        public static class UnitProgressionData_Patch
        {

            [HarmonyPatch("CanAddArchetype")]
            [HarmonyPriority(Priority.High)]
            public static bool Prefix(UnitProgressionData __instance, BlueprintCharacterClass characterClass, BlueprintArchetype archetype, ref bool __result)
            {
               
                if (Main.settings.unLockClassLevel)
                {
                    if (characterClass.PrestigeClass)
                    {

                        ClassData classData = __instance.Classes.FirstOrDefault((ClassData cd) => cd.CharacterClass == characterClass);
                        if (classData == null)
                        {
                            classData = new ClassData(characterClass);
                            __instance.Classes.Add(classData);
                        }
                        int level = classData.Level>10 ? 9 : classData.Level;
                        __result=level <= archetype.MinFeatureLevel && __instance.SureProgressionData(characterClass.Progression).CanAddArchetype(archetype);
                        return false;
                    }

                }

                return true;
            }





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



            }

            [HarmonyPatch("AddClassLevel")]
            [HarmonyPriority(Priority.High)]
            public static bool Prefix(UnitProgressionData __instance, BlueprintCharacterClass characterClass, ref System.Collections.Generic.List<BlueprintCharacterClass> ___m_ClassesOrder, ref int? ___m_CharacterLevel)
            {
                if (!Main.settings.unLockCharacterLevel)
                {
                    return true;

                }
                if (characterClass.IsHigherMythic)
                {
                    return true;
                }

                if (__instance.ObligatoryMythicClassesQueue != null && characterClass.IsMythic)
                {
                    return true;
                }
                bool flage = true;
                int[] bonuses = BlueprintRoot.Instance.Progression.XPTable.Bonuses;
                if (Main.settings.changeProtagonistXpTable)
                {
                    if (__instance.Owner.IsMainCharacter || __instance.Owner.Unit.IsCloneOfMainCharacter)
                    {
                        bonuses =  BlueprintRoot.Instance.Progression.LegendXPTable.Bonuses;
                        flage=false;
                    }
                }
                if (Main.settings.changeStoryCompanionXpTable)
                {
                    if (__instance.Owner.Unit.IsStoryCompanion())
                    {
                        bonuses =BlueprintRoot.Instance.Progression.LegendXPTable.Bonuses;
                        flage=false;
                    }
                }
                if (Main.settings.changeCustomCompanionXpTable)
                {
                    if (__instance.Owner.Unit.IsCustomCompanion())
                    {
                        bonuses = BlueprintRoot.Instance.Progression.LegendXPTable.Bonuses;
                        flage=false;
                    }
                }
                if (flage)
                {
                    return true;
                }
                var sureClassData = typeof(UnitProgressionData).GetMethod("SureClassData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);



                if (characterClass.IsMythic)
                {
                    return true;
                }
                else
                {
                    try
                    {
                        var classDataV = sureClassData.Invoke(__instance, new object[] { characterClass });

                        ClassData classData = (ClassData)classDataV;
                        int num = classData.Level;
                        classData.Level = num + 1;
                        ___m_ClassesOrder.Add(characterClass);
                        num = __instance.CharacterLevel;
                        ___m_CharacterLevel = num + 1;
                        string[] my_guids = Resource.MysticTheurgFactGuid;
                        foreach (string my_guid in my_guids)
                        {
                            Guid guid = Guid.Parse(my_guid);
                            BlueprintFact blueprintFact = Resource.GetBlueprint<BlueprintFact>(new BlueprintGuid(guid));
                            if (blueprintFact!=null)
                            {
                                if (__instance.Features.GetFact(blueprintFact)?.Rank>=10)
                                {
                                    __instance.Features.GetFact(blueprintFact)?.SetRank(9);
                                }


                            }

                        }

                    }
                    catch (Exception e)
                    {
                        Main.Logger.Log(e.ToString());
                        Main.Logger.Log(e.Message);
                    }

                }

                __instance.Classes.Sort();
                __instance.Features.AddFeature(characterClass.Progression, null).SetSource(characterClass, 1);
                __instance.Owner.OnGainClassLevel(characterClass);
                __instance.UpdateAdditionalVisualSettings();
                var syncWithView = typeof(UnitProgressionData).GetMethod("SyncWithView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                syncWithView.Invoke(__instance, new object[] { });
                return false;

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
            private static bool Prefix(ref int level, BlueprintProgression progression)
            {

                if (Main.settings.unLockCharacterLevel || Main.settings.unLockClassLevel)
                {


                    int i = level;


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
                    if (i >= 40)
                    {
                        i = 10;
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
                    int base20To21 = Main.settings.normalXpTableXpNeed20To21;
                    int diff = Main.settings.normalXpTableDifferenceIncreaseAfter20;
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
                        //int num = array[20] - array[19];
                        int num = base20To21;
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
                    if (i >= 40)
                    {
                        i = 20;
                    }
                    level = i;
                    __result = __instance.LevelEntries.FirstOrDefault((LevelEntry le) => le.Level == level) ?? new LevelEntry(); ;
                    return false;
                }
                return true;
            }


        }
        [HarmonyPatch(typeof(LevelUpHelper), "UpdateProgression")]
        public static class LevelUpHelper_Patch
        {
            [HarmonyPriority(Priority.High)]
            public static bool Prefix(LevelUpState state, UnitDescriptor unit, BlueprintProgression progression)
            {
                try
                {
                    if (Main.settings.unLockCharacterLevel || Main.settings.unLockClassLevel)
                    {
                        if (progression.FirstClass!=null)
                        {
                            if (progression.FirstClass.PrestigeClass)
                            {

                                ProgressionData progressionData = unit.Progression.SureProgressionData(progression);
                                int num = progressionData.Level;
                                int num2 = progressionData.Blueprint.CalcLevel(unit);
                                progressionData.Level = num2;
                                if (num >= num2)
                                {
                                    return false;
                                }
                                if (progression.ExclusiveProgression != null && state.SelectedClass != progression.ExclusiveProgression)
                                {
                                    return false;
                                }
                                if (!progression.GiveFeaturesForPreviousLevels)
                                {
                                    num = num2 - 1;
                                }
                                for (int i = num + 1; i <= num2; i++)
                                {
                                    int w = i;
                                    if (progression.FirstClass.PrestigeClass&&w>10)
                                    {

                                        if (w % 2 == 0)
                                        {
                                            w = 8;
                                        }
                                        else
                                        {
                                            w = 9;

                                        }
                                        if (w==40||w==20)
                                        {
                                            w=10;
                                        }
                                    }
                                    LevelEntry levelEntry = progressionData.GetLevelEntry(w);
                                    LevelUpHelper.AddFeaturesFromProgression(state, unit, levelEntry.Features, progression, w);
                                }
                                return false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Logger.Log(e.ToString());
                    Main.Logger.Log(e.Message);
                    Main.Logger.Log(e.StackTrace);
                }

                return true;
            }

        }

        [HarmonyPatch(typeof(RestoreClassFeature), "Upgrade")]
        public static class RestoreClassFeature_Patch
        {
            [HarmonyPriority(Priority.High)]
            public static bool Prefix(RestoreClassFeature __instance, UnitEntityData unit)
            {
                try
                {
                    if (Main.settings.unLockCharacterLevel || Main.settings.unLockClassLevel)
                    {
                        List<ValueTuple<BlueprintProgression, int>> list = TempList.Get<ValueTuple<BlueprintProgression, int>>();
                        Dictionary<BlueprintCharacterClass, int> dictionary = new Dictionary<BlueprintCharacterClass, int>();
                        foreach (BlueprintCharacterClass blueprintCharacterClass in unit.Progression.ClassesOrder)
                        {
                            int num = dictionary.Get(blueprintCharacterClass, 0);
                            num = (dictionary[blueprintCharacterClass] = num + 1);
                            foreach (ProgressionData progressionData in unit.Progression.GetClassProgressions(blueprintCharacterClass))
                            {
                                int w = num;
                                if (progressionData.Blueprint.FirstClass!=null)
                                {
                                    if (progressionData.Blueprint.FirstClass.PrestigeClass&&w>10)
                                    {

                                        if (w % 2 == 0)
                                        {
                                            w = 8;
                                        }
                                        else
                                        {
                                            w = 9;

                                        }
                                        if (w==40||w==20)
                                        {
                                            w=10;
                                        }
                                    }
                                }
                                int num2 = progressionData.GetLevelEntry(w).Features.Count((BlueprintFeatureBase i) => i == __instance.Feature);
                                while (num2-- > 0)
                                {
                                    list.Add(new ValueTuple<BlueprintProgression, int>(progressionData.Blueprint, num));
                                }
                            }
                        }
                        Feature feature = unit.Progression.Features.GetFact(__instance.Feature);
                        if (((feature != null) ? feature.RankToSource : null) != null && list.Count <= feature.RankToSource.Count)
                        {
                            return false;
                        }
                        if (feature != null)
                        {
                            List<Feature.SourceAndLevel> rankToSource = feature.RankToSource;
                            if (rankToSource != null)
                            {
                                rankToSource.Clear();
                            }
                        }
                        foreach (ValueTuple<BlueprintProgression, int> valueTuple in list)
                        {
                            BlueprintProgression item = valueTuple.Item1;
                            int item2 = valueTuple.Item2;
                            feature = unit.Progression.Features.AddFeature(__instance.Feature, null);
                            feature.SetSource(item, item2);
                        }
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Main.Logger.Log(e.ToString());
                    Main.Logger.Log(e.Message);
                }

                return true;
            }

        }

    }
}