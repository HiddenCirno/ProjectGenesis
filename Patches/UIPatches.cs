using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ERecipeType_1 = ERecipeType;

namespace ProjectGenesis.Patches
{
    class UIPatches
    {
#region UITankWindow

        // Specify color of each fluid here, one per line.
        private static Dictionary<int, Color32> FLUID_COLOR = new Dictionary<int, Color32>
                                                              {
                                                                  { 新物品.硝酸, new Color32(61, 137, 224, 255) },
                                                                  { 新物品.双酚A, new Color32(176, 106, 85, 255) },
                                                                  { 新物品.三氯化铁, new Color32(116, 152, 99, 255) },
                                                                  { 新物品.氢燃料, new Color32(187, 217, 219, 255) },
                                                                  { 新物品.氢氯酸, new Color32(99, 179, 148, 255) },
                                                                  { 新物品.煤焦油, new Color32(167, 91, 0, 255) },
                                                                  { 新物品.氯苯, new Color32(226, 72, 86, 255) },
                                                                  { 新物品.邻苯二甲酸, new Color32(214, 39, 98, 255) },
                                                                  { 新物品.间苯二甲酸二苯酯, new Color32(51, 255, 173, 255) },
                                                                  { 新物品.环氧氯丙烷, new Color32(188, 149, 92, 255) },
                                                                  { 新物品.甘油, new Color32(218, 207, 147, 255) },
                                                                  { 新物品.反物质燃料, new Color32(33, 44, 65, 255) },
                                                                  { 新物品.二硝基氯苯, new Color32(147, 230, 43, 255) },
                                                                  { 新物品.二氯联苯胺, new Color32(109, 183, 101, 255) },
                                                                  { 新物品.二甲苯, new Color32(218, 127, 78, 255) },
                                                                  { 新物品.二氨基联苯胺, new Color32(158, 212, 68, 255) },
                                                                  { 新物品.氘核燃料, new Color32(117, 184, 41, 255) },
                                                                  { 新物品.丙酮, new Color32(115, 177, 74, 255) },
                                                                  { 新物品.苯酚, new Color32(119, 176, 123, 255) },
                                                                  { 新物品.氨, new Color32(216, 216, 216, 255) },
                                                                  { 6999, new Color32(236, 220, 219, 255) },
                                                                  { 新物品.有机液体, new Color32(66, 8, 89, 255) },
                                                                  { 新物品.乙烯, new Color32(185, 185, 185, 255) },
                                                                  { 新物品.盐水, new Color32(90, 126, 179, 255) },
                                                                  { 新物品.氧气, new Color32(170, 198, 255, 255) },
                                                                  { 6211, new Color32(10, 60, 16, 255) },
                                                                  { 6202, new Color32(223, 222, 31, 255) },
                                                                  { 6213, new Color32(29, 29, 135, 255) },
                                                                  { 6215, new Color32(255, 128, 52, 255) },
                                                                  { 6207, new Color32(116, 99, 22, 255) },
                                                                  { 6214, new Color32(142, 138, 60, 255) },
                                                                  { 6203, new Color32(202, 167, 27, 255) },
                                                                  { 6204, new Color32(224, 209, 23, 255) },
                                                                  { 6212, new Color32(222, 214, 0, 255) },
                                                                  { 6210, new Color32(138, 172, 164, 255) },
                                                                  { 6219, new Color32(193, 130, 58, 255) },
                                                                  { 6201, new Color32(241, 181, 37, 255) },
                                                                  { 6209, new Color32(230, 81, 21, 255) },
                                                                  { 6208, new Color32(220, 122, 29, 255) },
                                                                  { 6205, new Color32(131, 209, 255, 255) },
                                                              };

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITankWindow), "_OnUpdate")]
        public static void UITankWindow_OnUpdate(ref UITankWindow __instance)
        {
            TankComponent tankComponent = __instance.storage.tankPool[__instance.tankId];
            if (tankComponent.id != __instance.tankId)
            {
                return;
            }

            if (FLUID_COLOR.ContainsKey(tankComponent.fluidId))
            {
                __instance.exchangeAndColoring(FLUID_COLOR[tankComponent.fluidId]);
            }
        }

#endregion

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeProto), "madeFromString", MethodType.Getter)]
        public static void RecipeProto_madeFromString(ref RecipeProto __instance, ref string __result)
        {
            ERecipeType type = (ERecipeType)__instance.Type;
            if (type == ERecipeType.电路蚀刻)
            {
                __result = "电路蚀刻机".Translate();
            }
            else if (type == ERecipeType.高精度加工)
            {
                __result = "高精度装配线".Translate();
            }
            else if (type == ERecipeType.矿物处理)
            {
                __result = "矿物处理厂".Translate();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
        public static void AssemblerComponent_InternalUpdate(ref AssemblerComponent __instance, float power,
                                                             int[] productRegister, int[] consumeRegister)
        {
            if (GameMain.history.TechUnlocked(1814) && __instance.recipeType == (ERecipeType_1)ERecipeType.Chemical
                                                    && __instance.speed == 20000)
            {
                __instance.speed = 40000;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "GetPropValue")]
        public static void ItemProto_GetPropValue(ref ItemProto __instance, ref string __result, int index,
                                                  StringBuilder sb, int incLevel)
        {
            if (GameMain.history.TechUnlocked(1814) && __instance.Type == EItemType.Production
                                                    && __instance.prefabDesc.assemblerRecipeType
                                                    == (ERecipeType_1)ERecipeType.Chemical && index == 22)
                __result = "4x";
        }


#region UITechNode

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITechNode), "UpdateLayoutDynamic")]
        public static void UITechNode_UpdateLayoutDynamic(ref UITechNode __instance, bool forceUpdate = false,
                                                          bool forceReset = false)
        {
            float num4
                = Mathf.Max(__instance.unlockText.preferredWidth - 40f + __instance.unlockTextTrans.anchoredPosition.x,
                            Math.Min(__instance.techProto.unlockRecipeArray.Length, 3) * 46) + __instance.baseWidth;
            if (num4 < __instance.minWidth)
            {
                num4 = __instance.minWidth;
            }

            if (num4 > __instance.maxWidth) num4 = __instance.maxWidth;

            if (__instance.focusState < 1f)
            {
                __instance.panelRect.sizeDelta
                    = new Vector2(Mathf.Lerp(__instance.minWidth, num4, __instance.focusState),
                                  __instance.panelRect.sizeDelta.y);
            }
            else
            {
                __instance.panelRect.sizeDelta
                    = new Vector2(Mathf.Lerp(num4, __instance.maxWidth, __instance.focusState - 1f),
                                  __instance.panelRect.sizeDelta.y);
            }

            __instance.titleText.rectTransform.sizeDelta = new Vector2(__instance.panelRect.sizeDelta.x
                                                                       - ((GameMain.history
                                                                                   .TechState(__instance.techProto.ID)
                                                                                   .curLevel > 0)
                                                                           ? 65
                                                                           : 25), 24f);
        }

#endregion
    }
}
