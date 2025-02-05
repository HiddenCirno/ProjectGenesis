﻿using System.Collections.Generic;
using CommonAPI.Systems;
using UnityEngine;
using xiaoye97;

// ReSharper disable CommentTypo
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

#pragma warning disable CS0618

namespace ProjectGenesis.Utils
{
    internal static class CopyModelUtils
    {
        internal static void AddCopiedModelProto()
        {
            var tankModel = CopyModelProto(121, 451, Color.HSVToRGB(0.5571f, 0.3188f, 0.8980f));
            LDBTool.PreAddProto(tankModel);

            var oreFactoryModel = CopyModelProto(194, 452, Color.HSVToRGB(0.2035f, 0.8326f, 0.9373f));
            LDBTool.PreAddProto(oreFactoryModel);

            var testCraftingTableModel = CopyModelProto(49, 453, Color.HSVToRGB(0.0710f, 0.7412f, 0.8941f));
            LDBTool.PreAddProto(testCraftingTableModel);

            var testCraftingTableModel2 = CopyModelProto(49, 454, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            LDBTool.PreAddProto(testCraftingTableModel2);

            var testCraftingTableModel3 = CopyModelProto(49, 455, Color.HSVToRGB(0.1404f, 0.8294f, 0.9882f));
            LDBTool.PreAddProto(testCraftingTableModel3);

            var testCraftingTableModel4 = CopyModelProto(49, 456, Color.HSVToRGB(0.9814f, 0.6620f, 0.8471f));
            LDBTool.PreAddProto(testCraftingTableModel4);

            var testCraftingTableModel5 = CopyModelProto(49, 460, new Color(0.3216F, 0.8157F, 0.09020F));
            LDBTool.PreAddProto(testCraftingTableModel5);

            var testCraftingTableModel6 = CopyModelProto(49, 461, new Color(0.3059F, 0.2196F, 0.4941F));
            LDBTool.PreAddProto(testCraftingTableModel6);

            var antiMatterModel = CopyModelProto(118, 457, Color.HSVToRGB(0.5985f, 0.7333f, 0.2353f));
            LDBTool.PreAddProto(antiMatterModel);

            var assembleModel = CopyModelProto(67, 458, Color.HSVToRGB(0.9688f, 0.9068f, 0.9255f));
            LDBTool.PreAddProto(assembleModel);

            var circleModel = CopyModelProto(69, 459, Color.grey);
            LDBTool.PreAddProto(circleModel);

            var megapumper = CopyModelProto(119, 462, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            LDBTool.PreAddProto(megapumper);

            AddAtmosphericCollectStation();
        }

        private static void AddAtmosphericCollectStation()
        {
            var color = new Color32(60, 179, 113, 255);
            var oriModel = LDB.models.Select(50); //ILS
            Debug.Log(oriModel.name);
            var desc = oriModel.prefabDesc;
            var newMats = new List<Material>();

            foreach (Material[] lodMats in desc.lodMaterials)
            {
                if (lodMats == null) continue;

                foreach (var mat in lodMats)
                {
                    if (mat == null) continue;
                    var newMaterial = new Material(mat);
                    newMaterial.SetColor("_Color", color);
                    newMats.Add(newMaterial);
                }
            }

            oriModel = LDB.models.Select(73); // ray receiver
            var collectEffectMat = new Material(oriModel.prefabDesc.lodMaterials[0][3]);

            //_TintColor
            //      Values on Ray Receiver = Color32(230, 255, 253, 255)
            collectEffectMat.SetColor("_TintColor", new Color32(131, 127, 197, 255));

            //_AuroraColor
            //      Values on Ray Receiver = White

            //_PolarColor
            //      Values on Ray Receiver = Color32(234, 177, 255, 203)
            collectEffectMat.SetColor("_PolarColor", new Color32(234, 255, 253, 170));

            //_Multiplier: Entire Effect Transparency
            //      Values on Ray Receiver = 40

            //_AuroraMultiplier: Aurora Transparency
            //      Values on Ray Receiver = 0.2

            //_AlphaMultiplier Also Entire Effect Transparency?
            //      Values on Ray Receiver = 1

            //_AuroraMask: ??
            //      Values on Ray Receiver = (0.03, 0.01, -0.1, 1)

            //_PScle: particle strength ("sparkle")
            //      Values on Ray Receiver = 20

            //_WrpScale: wrap scale ??
            //      Values on Ray Receiver = 0.1

            //_UVSpeed: ?? area of the aurora texture sampled?
            //      Values on Ray Receiver = (-0.07, 0, 0.1, 1)

            //_InvFade: Soft Particles Factor Range(0.01, 3) ??
            //      Values on Ray Receiver = 0.414

            //_SideFade: Also Entire Effect Transparency? Range(0,2)
            //      Values on Ray Receiver = 0.429

            //_NoiseScale ("Disturbance control x: Disturbance speed yz Disturbance strength in lateral and longitudinal directions w: Rotational disturbance", Vector) = (1,1,1,1) //15 1.8 1.6 0.4
            //      Values on Ray Receiver = (15, 1.8, 1.6, 0.4)
            //collectEffectMat.SetVector("_NoiseScale", new Vector4(75f, 1f, 20f, 0.1f));

            //_Aurora: Aurora control x: height of aurora (percentage of atmosphere) y: width z: thickness of aurora (percentage of aurora to ground) w: disturbance
            //      Values on Ray Receiver = (75, 2.45, 20, 0.1)
            collectEffectMat.SetVector("_Aurora", new Vector4(75f, 1f, 20f, 0.1f));

            //_Beam: Light column control x: width y: height (percentage from starting point to atmosphere) z: starting height w: intensity
            //      Values on Ray Receiver = (10, 78, 10, 0.1)
            collectEffectMat.SetVector("_Beam", new Vector4(12f, 78f, 24f, 1f));

            //_Particle: Particle Controls x: scale y: height z: velocity w: stagger value
            //      Values on Ray Receiver = (1, 12.7, 1.2, 0.5)
            collectEffectMat.SetVector("_Particle", new Vector4(2f, 30f, 5f, 0.8f));

            //_Circle: Aperture Control x: Zoom y: Height z: Speed w: Ellipse Deformation
            //      Values on Ray Receiver = (1.49, 10, 1, 0.01)
            collectEffectMat.SetVector("_Circle", new Vector4(2.5f, 34f, 1f, 0.04f));

            newMats.Add(collectEffectMat);
            ProtoRegistry.RegisterModel(463, "Assets/genesis-models/entities/prefabs/atmospheric-collect-station", newMats.ToArray());
        }

        private static ModelProto CopyModelProto(int oriId, int id, Color color)
        {
            var oriModel = LDB.models.Select(oriId);
            var model = oriModel.Copy();
            model.Name = id.ToString();
            model.ID = id;
            var desc = oriModel.prefabDesc;
            model.prefabDesc = new PrefabDesc(id, desc.prefab, desc.colliderPrefab);
            for (var i = 0; i < model.prefabDesc.lodMaterials.Length; i++)
            {
                if (model.prefabDesc.lodMaterials[i] == null) continue;
                for (var j = 0; j < model.prefabDesc.lodMaterials[i].Length; j++)
                {
                    if (model.prefabDesc.lodMaterials[i][j] == null) continue;
                    model.prefabDesc.lodMaterials[i][j] = new Material(desc.lodMaterials[i][j]);
                }
            }

            try
            {
                model.prefabDesc.lodMaterials[0][0].color = color;
                model.prefabDesc.lodMaterials[1][0].color = color;
                model.prefabDesc.lodMaterials[2][0].color = color;
            }
            catch
            {
                // ignored
            }

            model.prefabDesc.modelIndex = id;
            model.prefabDesc.hasBuildCollider = true;
            model.prefabDesc.colliders = desc.colliders;
            model.prefabDesc.buildCollider = desc.buildCollider;
            model.prefabDesc.buildColliders = desc.buildColliders;
            model.prefabDesc.colliderPrefab = desc.colliderPrefab;

            model.sid = "";
            model.SID = "";
            return model;
        }
    }
}
