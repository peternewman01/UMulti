using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FoliageRenormalizer
{
    [CustomEditor(typeof(FoliageRenormalizerProxyStealer))]
    public class FoliageRenormalizerProxyStealerEditor : Editor
    {
        private void OnEnable()
        {
            FoliageRenormalizerUtilityEditor.CreateFontStyles();
        }

        public override void OnInspectorGUI()
        {
            //base.DrawDefaultInspector();
            //GUILayout.Space(15);
            GUILayout.Space(5);

            if (FoliageRenormalizerUtilityEditor.TitleStyle == null)
                FoliageRenormalizerUtilityEditor.CreateFontStyles();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Foliage Renormlizer", FoliageRenormalizerUtilityEditor.TitleStyle);
            GUILayout.Label("There is no reason to remove this component. Nothing stored on this component will make it into a player build of your game, and removing it will just make it harder to touch up in the future.", FoliageRenormalizerUtilityEditor.SubTitleStyle);
            GUILayout.Label("If you decide you do not like the results, please click the 'Restore Original Mesh' button before removing.", FoliageRenormalizerUtilityEditor.SubTitleStyle);

            GUILayout.Space(8);
            EditorGUILayout.EndVertical();
            GUILayout.Space(8);

            var helper = (FoliageRenormalizerProxyStealer)target;

            if (helper.TryGetComponent<FoliageRenormalizerUtility>(out FoliageRenormalizerUtility bad))
            {
                EditorGUILayout.LabelField($"This component should NOT be on the same gameobject as a Foliage Renormalizer Utility!", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                return;
            }

            if (helper.TargetRenderer == null && helper.GetComponent<Renderer>() != null)
            {
                helper.TargetRenderer = helper.GetComponent<Renderer>();
                EditorUtility.SetDirty(helper);
            }
            if (helper.TargetRenderer == null)
            {
                EditorGUILayout.LabelField($"No Mesh Renderer on this GameObject!", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                return;
            }
            helper.TargetRenderer.TryGetComponent<MeshFilter>(out MeshFilter targetFilter);
            FoliageRenormalizerUtilityEditor.EnsureSavePathsLoaded();

            if ((helper.Source == null && helper.LastGenerationID != 0) || (helper.Source != null && helper.LastGenerationID != 0 && helper.Source.LastGenerationID == 0))
            {
                helper.LastGenerationID = 0;
                EditorUtility.SetDirty(helper);
            }

            EditorGUILayout.ObjectField("Source: ", helper.Source, typeof(FoliageRenormalizerUtility), true);

            GUILayout.Space(4);

            if (helper.Source == null)
            {
                GUILayout.Label("You must assign a source Renormalizer Utility first!", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                if (GUILayout.Button("Try to Auto Find Source Renormalizer Utility"))
                {
                    FoliageRenormalizerUtility util = helper.transform.parent.GetComponentInChildren<FoliageRenormalizerUtility>();
                    if (util != null)
                    {
                        helper.Source = util;
                        EditorUtility.SetDirty(helper);
                    }
                    else
                        Debug.LogError("Foliage Renormalizer Utility not found!");
                }
            }
            //else
            //{
            //    EditorGUI.BeginDisabledGroup(true);
            //    EditorGUILayout.ObjectField("Source: ", helper.Source, typeof(FoliageRenormalizerUtility), true);
            //    EditorGUI.EndDisabledGroup();
            //}
            
            GUILayout.Space(8);

            if (helper.Source == null)
            {
                return;
            }

            if (helper.Source.TargetRenderer == null)
            {
                EditorGUILayout.LabelField($"Target Renderer is not assigned to Utility component!", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                return;
            }

            if (!helper.Source.TargetRenderer.TryGetComponent<MeshFilter>(out MeshFilter sourceMeshFilter))
            {
                EditorGUILayout.LabelField($"Target Renderer does not have a MeshFilter component!", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Normal Transfer Submesh Mask Overrides", EditorStyles.boldLabel);
            GUILayout.Space(2);

            FoliageRenormalizerUtilityEditor.TryAddProxyStealerToUtil(helper.Source, helper);

            if (targetFilter)
            {
                var sourceMesh = sourceMeshFilter.sharedMesh;

                var mesh = targetFilter.sharedMesh;

                //bool mismatch = sourceMesh.subMeshCount != mesh.subMeshCount;

                //using (new EditorGUI.DisabledScope(mismatch))
                //{
                //    if (mismatch)
                //    {
                //        GUILayout.Label("You must override submesh mask if the submesh count doesnt match!", EditorStyles.boldLabel);
                //        if (!helper.OverrideSubMeshMask)
                //        {
                //            helper.OverrideSubMeshMask = true;
                //            EditorUtility.SetDirty(helper);
                //        }
                //    }
                //    bool newOverride = EditorGUILayout.Toggle("Override Normal Transfer Submesh Mask?", helper.OverrideSubMeshMask);
                //    if (newOverride != helper.OverrideSubMeshMask)
                //    {
                //        Undo.RecordObject(helper, "Toggle Override Submesh");
                //        helper.OverrideSubMeshMask = newOverride;
                //        EditorUtility.SetDirty(helper);
                //    }
                //}

                EditorGUI.BeginChangeCheck();
                helper.Source.DrawDependentSubmeshMask = EditorGUILayout.Foldout(helper.Source.DrawDependentSubmeshMask, "Override Material / Submesh Selector", true);
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(helper.Source);

                if (helper.Source.DrawDependentSubmeshMask)
                {
                    List<Material> sourceMaterials = FoliageRenormalizerUtilityEditor.GetValidRendererMaterials(helper.Source.TargetRenderer);
                    List<Material> stealerMaterial = FoliageRenormalizerUtilityEditor.GetValidRendererMaterials(helper.TargetRenderer);
                    List<Material> uniqueMaterials = new List<Material>();
                    for (int m = 0; m < stealerMaterial.Count; m++)
                    {
                        if (!sourceMaterials.Contains(stealerMaterial[m]))
                        {
                            uniqueMaterials.Add(stealerMaterial[m]);
                        }
                    }

                    if (uniqueMaterials.Count > 0)
                        FoliageRenormalizerUtilityEditor.DrawMaterialsSubmeshSelection(uniqueMaterials, helper.Source);
                    else
                        GUILayout.Label("No unique materials detected!");

                    //using (new EditorGUI.DisabledScope(mesh == null))
                    //{
                    //    string[] names = mesh ? FoliageRenormalizerUtilityEditor.BuildSubmeshNames(targetFilter) : FoliageRenormalizerUtilityEditor.BuildGenericSubmeshNames(8);
                    //    int newMask = EditorGUILayout.MaskField("Normal Transfer Submesh Override", helper.submeshMask, names);
                    //    if (newMask != helper.submeshMask)
                    //    {
                    //        Undo.RecordObject(helper, "Change Submesh Override Mask");
                    //        helper.submeshMask = newMask;
                    //        EditorUtility.SetDirty(helper);
                    //    }
                    //}
                }
            }

            EditorGUILayout.EndVertical();
            

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Foliage Remesher", EditorStyles.boldLabel);
            GUILayout.Space(2);

            FoliageRenormalizerUtilityEditor.DrawNormalsDebug(helper);
            FoliageRenormalizerUtilityEditor.DrawVertexColorDebug(helper);
            GUILayout.Space(6);

            if (helper.Source == null)
                GUILayout.Label("You must assign a source Renormalizer Utility first!");
            else if (helper.Source.GeneratedProxyMesh == null && !helper.Source.general.grassMode)
                GUILayout.Label("The source Renormalizer Utility doesnt have a generated proxy mesh!");

            if (helper.Source != null && helper.Source.LastGenerationID != helper.LastGenerationID)
            {
                EditorGUILayout.LabelField($"This Mesh is out of date! Regenerate now.", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
            }
            using (new EditorGUI.DisabledScope(helper.Source == null || (helper.Source.GeneratedProxyMesh == null && !helper.Source.general.grassMode)))
            {
                if (GUILayout.Button(FoliageRenormalizerUtilityEditor.GenerateNewMeshText))
                {
                    FoliageRenormalizerUtilityEditor.GenerateProxyStealerMesh(helper);
                }
            }

            using (new EditorGUI.DisabledScope(helper.OriginalFoliageMesh == null || (helper.RemeshedFoliageMesh == null && targetFilter.sharedMesh == helper.OriginalFoliageMesh)))
            {
                string buttonText = FoliageRenormalizerUtilityEditor.RestoreOriginalMeshText;
                if (helper.RemeshedFoliageMesh != null && helper.OriginalFoliageMesh != null && targetFilter.sharedMesh == helper.OriginalFoliageMesh)
                    buttonText = FoliageRenormalizerUtilityEditor.RestoreRemeshedMeshText;
                if (GUILayout.Button(buttonText))
                    FoliageRenormalizerUtilityEditor.SwapMeshFilterMesh(targetFilter, helper);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
