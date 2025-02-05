using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;
using NebulaAPI;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Patches.Logic.MegaAssembler;
using ProjectGenesis.Patches.Logic.PlanetFocus;
using ProjectGenesis.Patches.UI.UIPlanetFocus;
using ProjectGenesis.Utils;
using xiaoye97;
using ERecipeType_1 = ERecipeType;
using static ProjectGenesis.Utils.PrefabFixUtils;
using static ProjectGenesis.Utils.JsonDataUtils;
using static ProjectGenesis.Utils.CopyModelUtils;
using static ProjectGenesis.Utils.PlanetThemeUtils;

#pragma warning disable CS0618

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    [BepInDependency(IncompatibleCheckPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(TabSystem), nameof(AssemblerRecipeSystem))]
    public class ProjectGenesis : BaseUnityPlugin, IModCanSave, IMultiplayerMod
    {
        public const string MODGUID = "org.LoShin.GenesisBook";
        public const string MODNAME = "GenesisBook";
        public const string VERSION = "2.6.0";

        public string Version => VERSION;

        internal static ManualLogSource logger;
        internal static ConfigFile configFile;
        internal static UIPlanetFocusWindow PlanetFocusWindow;

        //无限堆叠开关(私货)
        private readonly bool StackSizeButton = false;

        internal static int[] TableID;
        private Harmony Harmony;

        internal static bool AtmosphericEmissionValue;

        internal static string ModPath;

        private static ConfigEntry<bool> EnableAtmosphericEmissionEntry;

        public void Awake()
        {
            logger = Logger;
            logger.Log(LogLevel.Info, "GenesisBook Awake");

            configFile = Config;

            if (IncompatibleCheckPlugin.DSPBattleInstalled)
            {
                logger.Log(LogLevel.Error, "They Come From Void is installed, which is incompatible with GenesisBook. Load Cancelled.");
                return;
            }

            EnableAtmosphericEmissionEntry = Config.Bind("config", "EnableAtmosphericEmission", true,
                                                         "Enable Atmospheric Emission tech effect in game, which may casue resource waste in low resource rate game.\n是否启用大气排污科技效果；注：大气排污科技在低资源倍率下可能导致资源浪费");
            AtmosphericEmissionValue = EnableAtmosphericEmissionEntry.Value;
            Config.Save();

            var executingAssembly = Assembly.GetExecutingAssembly();

            ModPath = Path.GetDirectoryName(executingAssembly.Location);

            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", ModPath);
            resources.LoadAssetBundle("texpack");
            ProtoRegistry.AddResource(resources);

            var resources_models = new ResourceData("org.LoShin.GenesisBook", "genesis-models", ModPath);
            resources_models.LoadAssetBundle("genesis-models");
            ProtoRegistry.AddResource(resources_models);

            TableID = new int[]
                      {
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab1",
                                                new TabData("精炼页面".TranslateFromJson(), "Icons/Tech/1101")),
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2",
                                                new TabData("化工页面".TranslateFromJson(), "Assets/texpack/化工科技")),
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab3",
                                                new TabData("信息页面".TranslateFromJson(), "Assets/texpack/主机科技")),
                      };

            NebulaModAPI.RegisterPackets(executingAssembly);

            NebulaModAPI.OnPlanetLoadRequest += planetId =>
            {
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new GenesisBookPlanetLoadRequest(planetId));
            };

            NebulaModAPI.OnPlanetLoadFinished += GenesisBookPlanetDataProcessor.ProcessBytesLater;

            Harmony = new Harmony(MODGUID);

            foreach (var type in executingAssembly.GetTypes()) Harmony.PatchAll(type);

            LDBTool.PreAddDataAction += PreAddDataAction;
            LDBTool.PostAddDataAction += PostAddDataAction;
        }

        private void PreAddDataAction()
        {
            LDB.veins.Select(14).name = "钨矿";
            LDB.veins.Select(14).Name = "钨矿".TranslateFromJson();

            LDB.items.OnAfterDeserialize();

            AdjustPlanetThemeDataVanilla();
            AddCopiedModelProto();
            ImportJson(TableID);
        }

        private void PostAddDataAction()
        {
            LDB.strings.OnAfterDeserialize();
            LDB.items.OnAfterDeserialize();
            LDB.recipes.OnAfterDeserialize();
            LDB.techs.OnAfterDeserialize();
            LDB.models.OnAfterDeserialize();
            LDB.milestones.OnAfterDeserialize();
            LDB.journalPatterns.OnAfterDeserialize();
            LDB.themes.OnAfterDeserialize();
            LDB.veins.OnAfterDeserialize();

            GameMain.gpuiManager.Init();

            foreach (var milestone in LDB.milestones.dataArray) milestone.Preload();
            foreach (var journalPattern in LDB.journalPatterns.dataArray) journalPattern.Preload();

            //飞行舱拆除
            var @base = LDB.veges.Select(9999);
            @base.MiningItem = new[] { 1801, 1101, 1104 };
            @base.MiningCount = new[] { 6, 50, 50 };
            @base.MiningChance = new float[] { 1, 1, 1 };
            @base.Preload();

            ref var locstrs = ref AccessTools.StaticFieldRefAccess<StringProtoSet>(AccessTools.Field(typeof(Localization), "_strings"))();
            locstrs = LDB.strings;

            foreach (var proto in LDB.veins.dataArray)
            {
                proto.Preload();
                proto.name = proto.Name.TranslateFromJson();
            }

            foreach (var proto in LDB.techs.dataArray) proto.Preload();

            for (var i = 0; i < LDB.items.dataArray.Length; ++i)
            {
                LDB.items.dataArray[i].recipes = null;
                LDB.items.dataArray[i].rawMats = null;
                LDB.items.dataArray[i].Preload(i);
            }

            for (var i = 0; i < LDB.recipes.dataArray.Length; ++i) LDB.recipes.dataArray[i].Preload(i);

            foreach (var proto in LDB.techs.dataArray)
            {
                proto.PreTechsImplicit = proto.PreTechsImplicit.Except(proto.PreTechs).ToArray();
                proto.UnlockRecipes = proto.UnlockRecipes.Distinct().ToArray();
                proto.Preload2();
            }

            ModelPostFix(LDB.models);
            ItemPostFix(LDB.items);
            SetBuildBar();

            ItemProto.InitFluids();
            ItemProto.InitItemIds();
            ItemProto.InitFuelNeeds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();
            ItemProto.stationCollectorId = 2105;

            foreach (var proto in LDB.items.dataArray)
            {
                StorageComponent.itemIsFuel[proto.ID] = proto.HeatValue > 0L;
                if (StackSizeButton) proto.StackSize = 10000000;
                StorageComponent.itemStackCount[proto.ID] = proto.StackSize;
            }

            // JsonHelper.ExportAsJson(@"D:\Git\ProjectGenesis\dependencies");
        }

        internal static void SetAtmosphericEmission(bool value)
        {
            AtmosphericEmissionValue = value;
            EnableAtmosphericEmissionEntry.Value = value;
            logger.LogInfo("AtmosphericEmissionSettingChanged");
            configFile.Save();
        }

        public void Export(BinaryWriter w)
        {
            MegaAssemblerPatches.Export(w);
            PlanetFocusPatches.Export(w);
        }

        public void Import(BinaryReader r)
        {
            MegaAssemblerPatches.Import(r);
            PlanetFocusPatches.Import(r);
        }

        public void IntoOtherSave()
        {
            MegaAssemblerPatches.IntoOtherSave();
            PlanetFocusPatches.IntoOtherSave();
        }

        public bool CheckVersion(string hostVersion, string clientVersion) => hostVersion.Equals(clientVersion);
    }
}
