﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class InitialTechPatches
    {
        private static readonly List<int> InitialTechs = new List<int> { 1, 1901, 1902, 1415 }, BonusTechs = new List<int> { 1801 };

        [HarmonyPatch(typeof(GameData), "SetForNewGame")]
        [HarmonyPostfix]
        public static void SetForNewGame(GameData __instance)
        {
            foreach (var tech in InitialTechs) __instance.history.UnlockTech(tech);
            foreach (var tech in BonusTechs) __instance.history.UnlockTech(tech);
        }

        [HarmonyPatch(typeof(GameData), "Import")]
        [HarmonyPostfix]
        public static void Import(GameData __instance)
        {
            // ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

            foreach (var tech in InitialTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }

            foreach (var tech in BonusTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }

            foreach (var (key, value) in __instance.history.techStates)
            {
                if (value.unlocked)
                {
                    var techProto = LDB.techs.Select(key);
                    if (techProto != null)
                        foreach (var t in techProto.UnlockRecipes)
                            __instance.history.UnlockRecipe(t);
                }
            }
        }

        [HarmonyPatch(typeof(UITechNode), "DoBuyoutTech")]
        [HarmonyPatch(typeof(UITechNode), "DoStartTech")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerEnter")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerExit")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerDown")]
        [HarmonyPatch(typeof(UITechNode), "OnOtherIconClick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UITechNode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UITechNode), nameof(UITechNode.techProto))),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))), new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<int, bool>>(id => InitialTechs.Contains(id)));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse_S);
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UITechNode), "DoStartTech")]
        [HarmonyPatch(typeof(UITechNode), "DoBuyoutTech")]
        [HarmonyPatch(typeof(UITechNode), "OnUnlockDirectButton")]
        [HarmonyPostfix]
        public static void UITechNode_OnQueueUpdate_Postfix()
        {
            var tree = UIRoot.instance.uiGame.techTree;
            UITechTree_OnQueueUpdate_Postfix(tree);
        }

        [HarmonyPatch(typeof(GameHistoryData), "RemoveTechInQueue")]
        [HarmonyPostfix]
        public static void GameHistoryData_RemoveTechInQueue_Postfix()
        {
            var tree = UIRoot.instance.uiGame.techTree;
            UITechTree_OnQueueUpdate_Postfix(tree);
        }

        [HarmonyPatch(typeof(UITechTree), "Do1KeyUnlock")]
        [HarmonyPostfix]
        public static void UITechTree_OnQueueUpdate_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.HideTechModeValue) return;

            var history = GameMain.history;
            foreach (var (tech, node) in __instance.nodes)
            {
                if (node != null && tech < 2000)
                {
                    var techSought = TechSought(history, tech);
                    if (techSought || PreTechSought(history, node.techProto))
                    {
                        node.gameObject.SetActive(true);
                        if (node.techProto.postTechArray.Length > 0) node.connGroup.gameObject.SetActive(techSought);
                    }
                    else
                    {
                        node.gameObject.SetActive(false);
                        node.connGroup.gameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(UITechTree), "OnPageChanged")]
        [HarmonyPostfix]
        public static void UITechTree_OnPageChanged_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.HideTechModeValue) return;

            if (__instance.page != 0) return;
            var history = GameMain.history;
            
            // ReSharper disable once EnforceForeachStatementBraces
            foreach (var (tech, node) in __instance.nodes)
                if (node != null && tech < 2000)
                {
                    var techSought = TechSought(history, tech);
                    node.gameObject.SetActive(techSought || PreTechSought(history, node.techProto));
                    if (node.techProto.postTechArray.Length > 0) node.connGroup.gameObject.SetActive(techSought);
                }
        }

        private static bool TechSought(GameHistoryData history, int tech) => history.TechInQueue(tech) || history.TechUnlocked(tech);

        private static bool PreTechSought(GameHistoryData history, TechProto proto) => proto.PreTechs.Any(i => TechSought(history, i));
    }
}
