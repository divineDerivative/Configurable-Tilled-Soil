using DivineFramework;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
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
                TilledSoilMod.cachedSoilRequirement = requireCost;
                TilledSoilMod.cachedSoilCost = soilCost;
            }
        }
    }

    public class TilledSoilMod : Mod
    {
        public static TilledSoilSettings settings;

        public TilledSoilMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<TilledSoilSettings>();
            Harmony harmony = new("divineDerivative.tilledsoil");
            harmony.PatchAll();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            settings.UpdateSettings();
        }

        public override string SettingsCategory() => "TilledSoil.ModTitle".Translate();

        SettingsHandler<TilledSoilSettings> settingsHandler = new();

        public override void DoSettingsWindowContents(Rect canvas)
        {
            float columnWidth = (1f / 3f) - 0.1f;
            Listing_Standard list = new();
            list.Begin(canvas);
            if (!settingsHandler.Initialized)
            {
                settingsHandler.width = canvas.width;
                //Fertility
                SetUpFertilityRow();

                //Affordance for tilling
                SetUpTillAffordanceRow();

                //Affordance for dirt
                SetUpDirtAffordanceRow();

                //Tilled soil cost required
                SetUpSoilRequiredRow();

                //Tilled soil cost amount
                SetUpSoilCostRow();

                //Tilled soil work amount
                SetUpWorkAmountRow();

                //Reset button
                SetUpResetButton();

                settingsHandler.Initialize();
            }

            settingsHandler.Draw(list);

            list.End();

            void SetUpFertilityRow()
            {
                UIContainter row = settingsHandler.RegisterNewRow("FertilityRow");
                row.AddLabel(() => "TilledSoil.FertilityLabel".Translate(settings.fertility.ToString()), relative: columnWidth, name: "FertilityLabel");
                UIIntEntry<TilledSoilSettings> entry = row.AddIntEntry(settings, AccessTools.FieldRefAccess<TilledSoilSettings, int>(AccessTools.Field(typeof(TilledSoilSettings), nameof(settings.fertility))), 120, 0, 1000, relative: columnWidth, name: "FertilityAmountEntry");
                settingsHandler.RegisterResetable(entry);
                row.AddLabel("TilledSoil.FertilityExplanation".Translate, name: "FertilityExplanation");
            }

            void SetUpTillAffordanceRow()
            {
                UIContainter row = settingsHandler.RegisterNewRow("TillAffordanceRow");
                row.AddLabel("TilledSoil.AffordanceTill".Translate, relative: columnWidth, name: "TillAffordanceLabel");
                UIContainter innerContainer = row.AddContainer(columnWidth);
                innerContainer.AddSpace();
                UIButtonTextResetable<TilledSoilSettings, string> button = innerContainer.AddButtonTextResetable(settings, AccessTools.FieldRefAccess<TilledSoilSettings, string>(AccessTools.Field(typeof(TilledSoilSettings), nameof(settings.canTillOn))), "GrowSoil", () => DefDatabase<TerrainAffordanceDef>.GetNamed(settings.canTillOn).label, TillAffordanceOnClick, name: "TillAffordanceButton", relative: 0.6f);
                settingsHandler.RegisterResetable(button);
                row.AddLabel(TillAffordanceExplanationKey, name: "TillAffordanceExpalation");
            }

            void SetUpDirtAffordanceRow()
            {
                UIContainter row = settingsHandler.RegisterNewRow("DirtAffordanceRow");
                row.AddLabel("TilledSoil.AffordanceDirt".Translate, relative: columnWidth, name: "DirtAffordanceLabel");
                UIContainter innerContainer = row.AddContainer(columnWidth);
                innerContainer.AddSpace();
                UIButtonTextResetable<TilledSoilSettings, string> button = innerContainer.AddButtonTextResetable(settings, AccessTools.FieldRefAccess<TilledSoilSettings, string>(AccessTools.Field(typeof(TilledSoilSettings), nameof(settings.canTurnIntoDirt))), "SmoothableStone", () => DefDatabase<TerrainAffordanceDef>.GetNamed(settings.canTurnIntoDirt).label, DirtAffordanceOnClick, name: "DirtAffordanceButton", relative: 0.6f);
                settingsHandler.RegisterResetable(button);
                row.AddLabel(DirtAffordanceExplanationKey, name: "DirtAffordanceExplanation");
            }

            void SetUpSoilRequiredRow()
            {
                cachedSoilRequirement = settings.requireCost;
                UIContainter row = settingsHandler.RegisterNewRow("SoilRequiredRow");
                row.AddLabel("TilledSoil.RequireCost".Translate, relative: columnWidth, name: "SoilRequiredLabel");
                UICheckbox<TilledSoilSettings> checkbox = row.AddCheckbox(settings, AccessTools.FieldRefAccess<TilledSoilSettings, bool>(AccessTools.Field(typeof(TilledSoilSettings), nameof(settings.requireCost))), true, relative: columnWidth, name: "SoilRequiredCheckbox");
                settingsHandler.RegisterResetable(checkbox);
                row.AddLabel(RequireCostExplanationKey, name: "SoilRequiredExplanation");
            }

            void SetUpSoilCostRow()
            {
                UIContainter row = settingsHandler.RegisterNewRow("SoilCostRow");
                row.AddLabel("TilledSoil.DirtCost".Translate, relative: columnWidth, name: "SoilCostLabel");
                UIIntEntry<TilledSoilSettings> entry = row.AddIntEntry(settings, AccessTools.FieldRefAccess<TilledSoilSettings, int>(AccessTools.Field(typeof(TilledSoilSettings), nameof(settings.soilCost))), 1, 1, 100, relative: columnWidth, name: "SoilCostEntry");
                settingsHandler.RegisterResetable(entry);
                row.AddLabel(DirtCostExplanationKey, name: "SoilCostExplanation");
                row.HideWhen(() => !settings.requireCost);
            }

            void SetUpWorkAmountRow()
            {
                UIContainter row = settingsHandler.RegisterNewRow("WorkAmountRow");
                row.AddLabel("TilledSoil.WorkAmount".Translate, relative: columnWidth, name: "WorkAmountLabel");
                UIIntEntry<TilledSoilSettings> entry = row.AddIntEntry(settings, AccessTools.FieldRefAccess<TilledSoilSettings, int>(AccessTools.Field(typeof(TilledSoilSettings), nameof(settings.workAmount))), 500, 1, 10000, relative: columnWidth, name: "WorkAmountEntry");
                settingsHandler.RegisterResetable(entry);
                row.AddLabel(WorkAmountExplanationKey, name: "WorkAmountExplanation");
            }

            void SetUpResetButton()
            {
                UIContainter row = settingsHandler.RegisterNewRow("ResetButtonRow");
                row.AddSpace(relative: columnWidth);
                row.AddResetButton(settingsHandler, relative: columnWidth, name: "ResetButton");
                row.AddSpace(relative: columnWidth);
            }
        }

        static TaggedString TillAffordanceExplanationKey()
        {
            if (settings.canTillOn == "Light")
            {
                return "TilledSoil.AffordanceTillLight".Translate();
            }
            else if (settings.canTillOn == "GrowSoil")
            {
                return "TilledSoil.AffordanceTillGrowable".Translate();
            }
            else
            {
                Log.Error("Unexpected value of canTillOn: " + settings.canTillOn);
                return "Error";
            }
        }

        internal static List<TerrainAffordanceDef> tillList;
        static void TillAffordanceOnClick()
        {
            List<FloatMenuOption> options = [];
            foreach (TerrainAffordanceDef terrain in tillList)
            {
                options.Add(new FloatMenuOption(terrain.label, delegate
                {
                    settings.canTillOn = terrain.defName;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        static TaggedString DirtAffordanceExplanationKey()
        {
            if (settings.canTurnIntoDirt == "Light")
            {
                return "TilledSoil.AffordanceDirtLight".Translate();

            }
            else if (settings.canTurnIntoDirt == "SmoothableStone")
            {
                return "TilledSoil.AffordanceDirtSmoothable".Translate();
            }
            else
            {
                Log.Error("Unexpected value of canTurnIntoDirt: " + settings.canTurnIntoDirt);
                return "Error";
            }
        }

        internal static List<TerrainAffordanceDef> dirtList;
        static void DirtAffordanceOnClick()
        {
            List<FloatMenuOption> options = [];
            foreach (TerrainAffordanceDef terrain in dirtList)
            {
                options.Add(new FloatMenuOption(terrain.label, delegate
                {
                    settings.canTurnIntoDirt = terrain.defName;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        internal static bool cachedSoilRequirement;
        private static TaggedString RequireCostExplanationKey()
        {
            TaggedString result = "TilledSoil.RequireCostExplanation".Translate();
            if (Current.ProgramState == ProgramState.Playing && cachedSoilRequirement != settings.requireCost)
            {
                result += "TilledSoil.ReloadRequired".Translate();
            }
            return result;
        }

        internal static int cachedSoilCost;
        static TaggedString DirtCostExplanationKey()
        {
            TaggedString result = "TilledSoil.DirtCostExplanation".Translate();
            if (Current.ProgramState == ProgramState.Playing && cachedSoilCost != settings.soilCost)
            {
                result += "TilledSoil.ReloadRequired".Translate();
            }
            return result;
        }

        internal static int cachedWorkAmount;

        static TaggedString WorkAmountExplanationKey()
        {
            TaggedString result = "TilledSoil.WorkAmountExplanation".Translate();
            if (settings.workAmount != cachedWorkAmount)
            {
                result += "TilledSoil.RestartRequired".Translate();
            }
            return result;
        }
    }

    [StaticConstructorOnStartup]
    public class OnStartup
    {
        static OnStartup()
        {
            TilledSoilMod.settings.ExposeData();
            if (ModsConfig.IsActive("mlie.soilrelocationframework") || ModsConfig.IsActive("udderlyevelyn.soilrelocation"))
            {
                TilledSoilSettings.SoilRelocationActive = true;
                TilledSoilSettings.DirtBag = DefDatabase<ThingDef>.GetNamed("SR_Soil");
            }
            else
            {
                TilledSoilSettings.DirtBag = DefDatabase<ThingDef>.GetNamed("DirtBag");
            }
            TilledSoilMod.tillList = [TerrainAffordanceDefOf.Light, DefOfTS.GrowSoil,];
            TilledSoilMod.dirtList = [TerrainAffordanceDefOf.Light, TerrainAffordanceDefOf.SmoothableStone,];
            TilledSoilMod.settings.UpdateSettings();

            VersionCheck.NeededVersion(new("0.1"), "TilledSoil.ModTitle".Translate());
        }
    }
}