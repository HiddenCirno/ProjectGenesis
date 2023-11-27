﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CommonAPI.Systems.UI;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable once CommentTypo
// ReSharper disable PossibleInvalidCastExceptionInForeachLoop

namespace ProjectGenesis.Patches.UI
{
    /// <summary>
    ///     special thanks for https://github.com/appuns/DSPMoreRecipes/blob/main/DSPMoreRecipes.cs
    /// </summary>
    internal static class GridIndexExpandPatches
    {
        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnInit")]
        [HarmonyPostfix]
        public static void UIReplicatorWindow_OnInit_Postfix(UIReplicatorWindow __instance)
        {
            __instance.windowRect.sizeDelta = new Vector2(900, 811);
            __instance.recipeGroup.sizeDelta = new Vector2(782, 322);
            __instance.queueGroup.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(782f, 46f);
        }

        [HarmonyPatch(typeof(UIGame), "_OnInit")]
        [HarmonyPostfix]
        public static void UIGame_OnInit_Postfix(UIGame __instance)
        {
            __instance.assemblerWindow.recipeGroup.sizeDelta = new Vector2(190, 100);
            __instance.recipePicker.pickerTrans.sizeDelta = new Vector2(830, 476);
            __instance.itemPicker.pickerTrans.sizeDelta = new Vector2(830, 476);
            __instance.signalPicker.pickerTrans.sizeDelta = new Vector2(830, 476);
        }

        [HarmonyPatch(typeof(UIRecipePicker), "_OnCreate")]
        [HarmonyPatch(typeof(UISignalPicker), "_OnCreate")]
        [HarmonyPatch(typeof(UIItemPicker), "_OnCreate")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void UIRecipePicker_OnOpen_Postfix(ManualBehaviour __instance)
            => __instance.transform.Find("content").GetComponent<RectTransform>().sizeDelta = new Vector2(782, 322);

        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshRecipeIcons")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "TestMouseRecipeIndex")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "SetSelectedRecipeIndex")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "SetSelectedRecipe")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnInit")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnCreate")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnUpdate")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RepositionQueueText")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshQueueIcons")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "TestMouseQueueIndex")]
        [HarmonyPatch(typeof(UIRecipePicker), "_OnUpdate")]
        [HarmonyPatch(typeof(UIRecipePicker), "RefreshIcons")]
        [HarmonyPatch(typeof(UIRecipePicker), "TestMouseIndex")]
        [HarmonyPatch(typeof(UIItemPicker), "_OnUpdate")]
        [HarmonyPatch(typeof(UIItemPicker), "RefreshIcons")]
        [HarmonyPatch(typeof(UIItemPicker), "TestMouseIndex")]
        [HarmonyPatch(typeof(UISignalPicker), "_OnUpdate")]
        [HarmonyPatch(typeof(UISignalPicker), "RefreshIcons")]
        [HarmonyPatch(typeof(UISignalPicker), "TestMouseIndex")]
        [HarmonyPatch(typeof(UIShowSignalTipExtension), "OnUpdate")]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var source = new List<CodeInstruction>(instructions);
            for (int index = 0; index < source.Count; ++index)
            {
                if (source[index].opcode == OpCodes.Ldc_I4_S && source[index].operand is sbyte operand)
                {
                    if (operand == 12) source[index].operand = (sbyte)17;
                    if (operand == 9) source[index].operand = (sbyte)7;
                }
            }

            return source.AsEnumerable();
        }

        [HarmonyPatch(typeof(UIReplicatorWindow), "SetMaterialProps")]
        [HarmonyPatch(typeof(UIRecipePicker), "SetMaterialProps")]
        [HarmonyPatch(typeof(UIItemPicker), "SetMaterialProps")]
        [HarmonyPatch(typeof(UISignalPicker), "SetMaterialProps")]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var source = new List<CodeInstruction>(instructions);
            for (int index = 0; index < source.Count; ++index)
            {
                if (source[index].opcode == OpCodes.Ldc_R4 && source[index].operand is float operand)
                {
                    if (operand is 12f) source[index].operand = 17f;
                    if (operand is 9f) source[index].operand = 7f;
                }
            }

            return source.AsEnumerable();
        }

        [HarmonyPatch(typeof(UIStationStorage), "OnSelectItemButtonClick")]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> UIStationStorage_OnSelectItemButtonClick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, -300f));
            matcher.SetOperandAndAdvance(-600f);
            return matcher.InstructionEnumeration();
        }
    }
}
