using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Kingmaker.UnitLogic;

namespace PFWOTRCLUNLOCKER
{
    class ClUnlocker
    {
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
    }
}
