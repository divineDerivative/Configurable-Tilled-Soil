using DivineFramework;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TilledSoil
{
    public class TilledSoilSettings : ModSettings
    {
        public int fertility = 120;
        public string canTillOn = "GrowSoil";
        public string canTurnIntoDirt = "SmoothableStone";
        public bool requireCost = true;
        public int soilCost = 1;
        public int workAmount = 500;

        internal static ThingDef DirtBag;
        internal static TerrainDef TilledSoil;
        internal static bool SoilRelocationActive;

        public float Fertility => fertility / 100f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref fertility, "Fertility", 120);
            Scribe_Values.Look(ref canTillOn, "CanTillOn", "GrowSoil");
            Scribe_Values.Look(ref canTurnIntoDirt, "CanTurnIntoDirt", "SmoothableStone");
            Scribe_Values.Look(ref requireCost, "RequireCost", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref soilCost, "SoilCost", 1);
            Scribe_Values.Look(ref workAmount, "WorkAmount", 500);
            base.ExposeData();
        }

        public void UpdateSettings()
        {
            TilledSoil.fertility = Fertility;
            TilledSoil.terrainAffordanceNeeded = DefDatabase<TerrainAffordanceDef>.GetNamed(canTillOn);

            if (requireCost)
            {
                TilledSoil.costList =
                [
                    new ThingDefCountClass(DirtBag, soilCost)
                ];
            }
            else
            {
                TilledSoil.costList = [];
            }
            if (!SoilRelocationActive)
            {
                TerrainDefOf.Soil.terrainAffordanceNeeded = DefDatabase<TerrainAffordanceDef>.GetNamed(canTurnIntoDirt);
            }
            if (Current.ProgramState == ProgramState.Entry)
            {
                cachedSoilRequirement = requireCost;
                cachedSoilCost = soilCost;
            }
        }

        float columnWidth = (1f / 3f) - 0.1f;
        internal void SetUpHandler(SettingsHandler<TilledSoilSettings> handler)
        {
            //Fertility
            SetUpFertilityRow(handler);

            //Affordance for tilling
            SetUpTillAffordanceRow(handler);

            //Affordance for dirt
            SetUpDirtAffordanceRow(handler);

            //Tilled soil cost required
            SetUpSoilRequiredRow(handler);

            //Tilled soil cost amount
            SetUpSoilCostRow(handler);

            //Tilled soil work amount
            SetUpWorkAmountRow(handler);

            //Reset button
            SetUpResetButton(handler);

            handler.Initialize();
        }

        void SetUpFertilityRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIContainter row = handler.RegisterNewRow("FertilityRow");
            row.AddLabel(() => "TilledSoil.FertilityLabel".Translate(fertility.ToString()), relative: columnWidth, name: "FertilityLabel");
            UIIntEntry<TilledSoilSettings> entry = row.AddIntEntry(this, AccessTools.FieldRefAccess<TilledSoilSettings, int>(AccessTools.Field(typeof(TilledSoilSettings), nameof(fertility))), 120, 0, 1000, relative: columnWidth, name: "FertilityAmountEntry");
            handler.RegisterResetable(entry);
            row.AddLabel("TilledSoil.FertilityExplanation".Translate, name: "FertilityExplanation");
        }

        void SetUpTillAffordanceRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIContainter row = handler.RegisterNewRow("TillAffordanceRow");
            row.AddLabel("TilledSoil.AffordanceTill".Translate, relative: columnWidth, name: "TillAffordanceLabel");
            UIContainter innerContainer = row.AddContainer(columnWidth);
            innerContainer.AddSpace();
            UIButtonTextResetable<TilledSoilSettings, string> button = innerContainer.AddButtonTextResetable(this, AccessTools.FieldRefAccess<TilledSoilSettings, string>(AccessTools.Field(typeof(TilledSoilSettings), nameof(canTillOn))), "GrowSoil", () => DefDatabase<TerrainAffordanceDef>.GetNamed(canTillOn).label, TillAffordanceOnClick, name: "TillAffordanceButton", relative: 0.6f);
            handler.RegisterResetable(button);
            row.AddLabel(TillAffordanceExplanationKey, name: "TillAffordanceExpalation");
        }

        internal static List<TerrainAffordanceDef> tillList;
        void TillAffordanceOnClick()
        {
            List<FloatMenuOption> options = [];
            foreach (TerrainAffordanceDef terrain in tillList)
            {
                options.Add(new FloatMenuOption(terrain.label, delegate
                {
                    canTillOn = terrain.defName;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        TaggedString TillAffordanceExplanationKey()
        {
            if (canTillOn == "Light")
            {
                return "TilledSoil.AffordanceTillLight".Translate();
            }
            else if (canTillOn == "GrowSoil")
            {
                return "TilledSoil.AffordanceTillGrowable".Translate();
            }
            else
            {
                Log.Error("Unexpected value of canTillOn: " + canTillOn);
                return "Error";
            }
        }

        void SetUpDirtAffordanceRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIContainter row = handler.RegisterNewRow("DirtAffordanceRow");
            row.AddLabel("TilledSoil.AffordanceDirt".Translate, relative: columnWidth, name: "DirtAffordanceLabel");
            UIContainter innerContainer = row.AddContainer(columnWidth);
            innerContainer.AddSpace();
            UIButtonTextResetable<TilledSoilSettings, string> button = innerContainer.AddButtonTextResetable(this, AccessTools.FieldRefAccess<TilledSoilSettings, string>(AccessTools.Field(typeof(TilledSoilSettings), nameof(canTurnIntoDirt))), "SmoothableStone", () => DefDatabase<TerrainAffordanceDef>.GetNamed(canTurnIntoDirt).label, DirtAffordanceOnClick, name: "DirtAffordanceButton", relative: 0.6f);
            handler.RegisterResetable(button);
            row.AddLabel(DirtAffordanceExplanationKey, name: "DirtAffordanceExplanation");
        }

        internal static List<TerrainAffordanceDef> dirtList;
        void DirtAffordanceOnClick()
        {
            List<FloatMenuOption> options = [];
            foreach (TerrainAffordanceDef terrain in dirtList)
            {
                options.Add(new FloatMenuOption(terrain.label, delegate
                {
                    canTurnIntoDirt = terrain.defName;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        TaggedString DirtAffordanceExplanationKey()
        {
            if (canTurnIntoDirt == "Light")
            {
                return "TilledSoil.AffordanceDirtLight".Translate();

            }
            else if (canTurnIntoDirt == "SmoothableStone")
            {
                return "TilledSoil.AffordanceDirtSmoothable".Translate();
            }
            else
            {
                Log.Error("Unexpected value of canTurnIntoDirt: " + canTurnIntoDirt);
                return "Error";
            }
        }

        internal static bool cachedSoilRequirement;
        void SetUpSoilRequiredRow(SettingsHandler<TilledSoilSettings> handler)
        {
            cachedSoilRequirement = requireCost;
            UIContainter row = handler.RegisterNewRow("SoilRequiredRow");
            row.AddLabel("TilledSoil.RequireCost".Translate, relative: columnWidth, name: "SoilRequiredLabel");
            UICheckbox<TilledSoilSettings> checkbox = row.AddCheckbox(this, AccessTools.FieldRefAccess<TilledSoilSettings, bool>(AccessTools.Field(typeof(TilledSoilSettings), nameof(requireCost))), true, relative: columnWidth, name: "SoilRequiredCheckbox");
            handler.RegisterResetable(checkbox);
            row.AddLabel(RequireCostExplanationKey, name: "SoilRequiredExplanation");
        }

        TaggedString RequireCostExplanationKey()
        {
            TaggedString result = "TilledSoil.RequireCostExplanation".Translate();
            if (Current.ProgramState == ProgramState.Playing && cachedSoilRequirement != requireCost)
            {
                result += "TilledSoil.ReloadRequired".Translate();
            }
            return result;
        }

        void SetUpSoilCostRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIContainter row = handler.RegisterNewRow("SoilCostRow");
            row.AddLabel("TilledSoil.DirtCost".Translate, relative: columnWidth, name: "SoilCostLabel");
            UIIntEntry<TilledSoilSettings> entry = row.AddIntEntry(this, AccessTools.FieldRefAccess<TilledSoilSettings, int>(AccessTools.Field(typeof(TilledSoilSettings), nameof(soilCost))), 1, 1, 100, relative: columnWidth, name: "SoilCostEntry");
            handler.RegisterResetable(entry);
            row.AddLabel(DirtCostExplanationKey, name: "SoilCostExplanation");
            row.HideWhen(() => !requireCost);
        }

        internal static int cachedSoilCost;
        TaggedString DirtCostExplanationKey()
        {
            TaggedString result = "TilledSoil.DirtCostExplanation".Translate();
            if (Current.ProgramState == ProgramState.Playing && cachedSoilCost != soilCost)
            {
                result += "TilledSoil.ReloadRequired".Translate();
            }
            return result;
        }

        void SetUpWorkAmountRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIContainter row = handler.RegisterNewRow("WorkAmountRow");
            row.AddLabel("TilledSoil.WorkAmount".Translate, relative: columnWidth, name: "WorkAmountLabel");
            UIIntEntry<TilledSoilSettings> entry = row.AddIntEntry(this, AccessTools.FieldRefAccess<TilledSoilSettings, int>(AccessTools.Field(typeof(TilledSoilSettings), nameof(workAmount))), 500, 1, 10000, relative: columnWidth, name: "WorkAmountEntry");
            handler.RegisterResetable(entry);
            row.AddLabel(WorkAmountExplanationKey, name: "WorkAmountExplanation");
        }

        internal static int cachedWorkAmount;
        TaggedString WorkAmountExplanationKey()
        {
            TaggedString result = "TilledSoil.WorkAmountExplanation".Translate();
            if (workAmount != cachedWorkAmount)
            {
                result += "TilledSoil.RestartRequired".Translate();
            }
            return result;
        }

        void SetUpResetButton(SettingsHandler<TilledSoilSettings> handler)
        {
            UIContainter row = handler.RegisterNewRow("ResetButtonRow");
            row.AddSpace(relative: columnWidth);
            row.AddResetButton(handler, relative: columnWidth, name: "ResetButton");
            row.AddSpace(relative: columnWidth);
        }
    }
}