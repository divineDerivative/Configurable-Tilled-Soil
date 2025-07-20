using static TilledSoil.TilledSoilLogger;
using DivineFramework.UI;
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
        public bool tillingDestroysPlants;

        internal static ThingDef DirtBag;
        internal static TerrainDef TilledSoil;
        internal static bool SoilRelocationActive;
        internal static bool VFEActive;
        internal static TerrainDef PackedDirt;

        //Mod specific settings
        public bool packedDirtRequire;
        public int packedDirtCost;

        public float Fertility => fertility / 100f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref fertility, "Fertility", 120);
            Scribe_Values.Look(ref canTillOn, "CanTillOn", "GrowSoil");
            Scribe_Values.Look(ref canTurnIntoDirt, "CanTurnIntoDirt", "SmoothableStone");
            Scribe_Values.Look(ref requireCost, "RequireCost", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref soilCost, "SoilCost", 1);
            Scribe_Values.Look(ref workAmount, "WorkAmount", 500);
            Scribe_Values.Look(ref tillingDestroysPlants, "TillingDestroysPlants", true);

            Scribe_Values.Look(ref packedDirtRequire, "PackedDirt", true);
            Scribe_Values.Look(ref packedDirtCost, "PackedDirtCost", 1);
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
#if v1_6
                DefOfTS.Dirt.terrainAffordanceNeeded = DefDatabase<TerrainAffordanceDef>.GetNamed(canTurnIntoDirt);
#else
                TerrainDefOf.Soil.terrainAffordanceNeeded = DefDatabase<TerrainAffordanceDef>.GetNamed(canTurnIntoDirt);
#endif
            }
            if (VFEActive)
            {
                if (packedDirtRequire)
                {
                    PackedDirt.costList =
                    [
                        new ThingDefCountClass(DirtBag, packedDirtCost)
                    ];
                }
                else
                {
                    PackedDirt.costList = [];
                }
            }
            if (Current.ProgramState == ProgramState.Entry)
            {
                cachedSoilRequirement = requireCost;
                cachedSoilCost = soilCost;
                cachedPackedDirtCost = packedDirtCost;
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

            //Tilling destroys plants
            SetUpTillingDestroysRow(handler);

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

        void SetUpTillingDestroysRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIRow row = handler.RegisterNewRow("TillingDestroysRow");
            row.AddLabel("TilledSoil.TillingDestroys".Translate, relative: columnWidth);
            row.Add(NewElement.Checkbox(relative: columnWidth)
                .WithReference(this, nameof(tillingDestroysPlants), tillingDestroysPlants)
                .RegisterResettable(handler, true)
                .Alignment(UnityEngine.TextAlignment.Right), "TillingDestroysCheckbox");
            row.AddLabel("TilledSoil.TillingDestroysExplanation".Translate);
        }

        void SetUpFertilityRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIRow row = handler.RegisterNewRow("FertilityRow");
            row.AddLabel(() => "TilledSoil.FertilityLabel".Translate(fertility.ToString()), relative: columnWidth, name: "FertilityLabel");
            row.Add(NewElement.InputLine<int>(relative: columnWidth)
                .WithReference(this, nameof(fertility), fertility)
                .MinMax(0, 1000)
                .WithIncrementButtons()
                .RegisterResettable(handler, 120), name: "FertilityAmountEntry");
            row.AddLabel("TilledSoil.FertilityExplanation".Translate, name: "FertilityExplanation");
        }

        void SetUpTillAffordanceRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIRow row = handler.RegisterNewRow("TillAffordanceRow");
            row.AddLabel("TilledSoil.AffordanceTill".Translate, relative: columnWidth, name: "TillAffordanceLabel");
            UIRow innerContainer = row.AddRow(relative: columnWidth);
            innerContainer.AddSpace();
            innerContainer.Add(NewElement.Button(TillAffordanceOnClick, relative: 0.6f)
                .WithReference(this, nameof(canTillOn), canTillOn)
                .WithLabel(() => DefDatabase<TerrainAffordanceDef>.GetNamed(canTillOn).label)
                .RegisterResettable(handler, "GrowSoil"), "TillAffordanceButton");
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
                LogUtil.Error("Unexpected value of canTillOn: " + canTillOn);
                return "Error";
            }
        }

        void SetUpDirtAffordanceRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIRow row = handler.RegisterNewRow("DirtAffordanceRow");
            row.AddLabel("TilledSoil.AffordanceDirt".Translate, relative: columnWidth, name: "DirtAffordanceLabel");
            UIRow innerContainer = row.AddRow(relative: columnWidth);
            innerContainer.AddSpace();
            innerContainer.Add(NewElement.Button(DirtAffordanceOnClick, relative: 0.6f)
                .WithReference(this, nameof(canTurnIntoDirt), canTurnIntoDirt)
                .RegisterResettable(handler, "SmoothableStone")
                .WithLabel(() => DefDatabase<TerrainAffordanceDef>.GetNamed(canTurnIntoDirt).label), name: "DirtAffordanceButton");
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
                LogUtil.Error("Unexpected value of canTurnIntoDirt: " + canTurnIntoDirt);
                return "Error";
            }
        }

        internal static bool cachedSoilRequirement;
        void SetUpSoilRequiredRow(SettingsHandler<TilledSoilSettings> handler)
        {
            cachedSoilRequirement = requireCost;
            UIRow row = handler.RegisterNewRow("SoilRequiredRow");
            row.AddLabel("TilledSoil.RequireCost".Translate, relative: columnWidth);
            row.Add(NewElement.Checkbox(relative: columnWidth)
                .WithReference(this, nameof(requireCost), requireCost)
                .RegisterResettable(handler, true)
                .Alignment(UnityEngine.TextAlignment.Right), name: "SoilRequiredCheckbox");
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
            UIRow row = handler.RegisterNewRow("SoilCostRow");
            row.AddLabel("TilledSoil.DirtCost".Translate, relative: columnWidth, name: "SoilCostLabel");
            row.Add(NewElement.InputLine<int>(relative: columnWidth)
                .WithReference(this, nameof(soilCost), soilCost)
                .MinMax(1, 100)
                .WithIncrementButtons()
                .RegisterResettable(handler, 1), name: "SoilCostEntry");
            row.AddLabel(() => DirtCostExplanationKey(cachedSoilCost, soilCost), name: "SoilCostExplanation");
            row.HideWhen(() => !requireCost);
        }

        internal static int cachedSoilCost;
        internal static int cachedPackedDirtCost;
        TaggedString DirtCostExplanationKey(int cachedCost, int currentCost)
        {
            TaggedString result = "TilledSoil.DirtCostExplanation".Translate();
            if (Current.ProgramState == ProgramState.Playing && cachedCost != currentCost)
            {
                result += "TilledSoil.ReloadRequired".Translate();
            }
            return result;
        }

        void SetUpWorkAmountRow(SettingsHandler<TilledSoilSettings> handler)
        {
            UIRow row = handler.RegisterNewRow("WorkAmountRow");
            row.AddLabel("TilledSoil.WorkAmount".Translate, relative: columnWidth, name: "WorkAmountLabel");
            row.Add(NewElement.InputLine<int>(relative: columnWidth)
                .WithReference(this, nameof(workAmount), workAmount)
                .WithIncrementButtons()
                .MinMax(1, 10000)
                .RegisterResettable(handler, 500), name: "WorkAmountEntry");
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
            UIRow row = handler.RegisterNewRow("ResetButtonRow");
            row.AddSpace(relative: columnWidth);
            row.AddResetButton(handler, relative: columnWidth, name: "ResetButton");
            row.AddSpace();
        }

        internal void SetUpIntegrationHandler(SettingsHandler<TilledSoilSettings> handler)
        {
            UIRow titleRow = handler.RegisterNewRow();
            titleRow.AddSpace();
            Text.Font = GameFont.Medium;
            titleRow.AddHeader("TiledSoil.ModIntegrationHeader".Translate, absolute: Text.CalcSize("TiledSoil.ModIntegrationHeader".Translate()).x);
            Text.Font = GameFont.Small;
            titleRow.AddSpace();
            handler.AddLine();

            if (VFEActive)
            {
                UIRow dirtRow = handler.RegisterNewRow("PackedDirt");
                dirtRow.AddLabel("TilledSoil.PackedDirtRequire".Translate, relative: columnWidth);
                dirtRow.Add(NewElement.Checkbox(relative: columnWidth)
                    .WithReference(this, nameof(packedDirtRequire), packedDirtRequire)
                    .Alignment(UnityEngine.TextAlignment.Right)
                .RegisterResettable(handler, true));
                dirtRow.AddLabel("TilledSoil.PackedDirtRequireExplanation".Translate);
                UIRow costRow = handler.RegisterNewRow("CostAmount");
                costRow.AddLabel("TilledSoil.DirtCost".Translate, relative: columnWidth, name: "SoilCostLabel");
                costRow.Add(NewElement.InputLine<int>(relative: columnWidth)
                    .WithReference(this, nameof(packedDirtCost), packedDirtCost)
                    .MinMax(1, 100)
                    .WithIncrementButtons()
                    .RegisterResettable(handler, 1), name: "SoilCostEntry");
                costRow.AddLabel(() => DirtCostExplanationKey(cachedPackedDirtCost, packedDirtCost), name: "SoilCostExplanation");
                costRow.HideWhen(() => !packedDirtRequire);
            }

            UIRow resetRow = handler.RegisterNewRow("ResetButtonRow");
            resetRow.AddSpace(relative: columnWidth);
            resetRow.AddResetButton(handler, relative: columnWidth, name: "ResetButton");
            resetRow.AddSpace();
        }
    }
}