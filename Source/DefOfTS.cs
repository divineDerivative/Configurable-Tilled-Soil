using RimWorld;
using Verse;

namespace TilledSoil
{
    [DefOf]
    public static class DefOfTS
    {
        public static DesignationDef GatherDirtBags;
        public static ThingDef DirtBag;
        public static JobDef GatherDirtJob;

        static DefOfTS()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfTS));
        }
    }
}
