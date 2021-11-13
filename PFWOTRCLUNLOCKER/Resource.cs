using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;

namespace PFWOTRCLUNLOCKER
{
    public static class Resource
    {
        public static T GetBlueprint<T>(BlueprintGuid id) where T : SimpleBlueprint
        {
            T t = ResourcesLibrary.TryGetBlueprint(id) as T;
            if (t == null)
            {
                Main.Logger.Error(string.Format("COULD NOT LOAD: {0} - {1}", id, typeof(T)));
            }
            return t;
        }
        public static string[] MysticTheurgFactGuid = {

            "61bf15564948bff4ab847bfe5d5d0f1f", //ANGEL FIRE APOSTEL
            "d4ec3d89122b3764f88edc65c4618923", //CLERIC
            "14bdf99c1d33ec1479c77ba7e3e27133", //CRUSADER
            "3afb51e0dc2bd4b42a09054300bd2cc0",//DRUID
            "d44442bc85185d343adc93926dfd54ce",//FEY SPEAKER
            "7c2a18c92c4c475c8cb9f8f95124a550",//HUNTER
            "64bbf0ac60b78f24eb631a9c46e50e21",//INQUISITOR
            "3eaf8a57a407e3d4aa477f802f0a5bb4", //ORACLE
            "d96bb38e6122d2746818d2bcea2b8193", //PALADIN
            "94c6bcbe081e3c34998ce0e5d372169c",//RANGER
            "5454910ae7ca9234cba35df073ed5116",//SHAMEN

        };
    }
}
