using RimWorld;
using Verse;

namespace TilledSoil
{
    [DefOf]
    public static class DefOfTS
    {
        public static DesignationDef GatherDirtBags;
        public static JobDef GatherDirtJob;
        public static TerrainAffordanceDef GrowSoil;
#if v1_6
        public static TerrainDef Dirt;
#endif

        static DefOfTS()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfTS));
        }
    }
}
