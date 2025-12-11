#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FoliageRenormalizer
{
    [CustomEditor(typeof(FoliageRenormalizerUtility))]
    public class FoliageRenormalizerUtilityEditor : Editor
    {
        public static string ProxySavePath;
        public static string FoliageSavePath;
        public static GUIStyle TitleStyle;
        public static GUIStyle SubTitleStyle;
        public static GUIStyle WarningGUIStyleBold;
        public static GUIStyle WarningGUIStyle;
        public static GUIStyle SoftWarningGUIStyle;
        public static GUIStyle LeftRadioToggleStyle;

        public static void CreateFontStyles()
        {
            TitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            TitleStyle.wordWrap = false;

            SubTitleStyle = new GUIStyle(EditorStyles.label);
            SubTitleStyle.wordWrap = true;

            WarningGUIStyleBold = new GUIStyle(EditorStyles.boldLabel);
            WarningGUIStyleBold.normal.textColor = Color.red;
            WarningGUIStyleBold.wordWrap = true;

            WarningGUIStyle = new GUIStyle(EditorStyles.label);
            WarningGUIStyle.normal.textColor = Color.red;
            WarningGUIStyle.wordWrap = true;

            SoftWarningGUIStyle = new GUIStyle(EditorStyles.label);
            SoftWarningGUIStyle.normal.textColor = Color.yellow;
            SoftWarningGUIStyle.wordWrap = true;

            LeftRadioToggleStyle = new GUIStyle(EditorStyles.radioButton);
            LeftRadioToggleStyle.padding.left += 6;
        }

        public static void EnsureSavePathsLoaded()
        {
            if (string.IsNullOrEmpty(ProxySavePath))
                LoadProxySavePath();
            if (string.IsNullOrEmpty(FoliageSavePath))
                LoadFoliageSavePath();
        }

        private static void LoadSavePaths()
        {
            LoadProxySavePath();
            LoadFoliageSavePath();
        }

        private static string LoadProxySavePath()
        {
            ProxySavePath = EditorPrefs.GetString("FoliageRenormalizerProxySavePath", "Assets/Foliage Renormalizer/Generated Meshes/Proxies");
            return ProxySavePath;
        }

        private static string LoadFoliageSavePath()
        {
            FoliageSavePath = EditorPrefs.GetString("FoliageRenormalizerFoliageSavePath", "Assets/Foliage Renormalizer/Generated Meshes/Foliage");
            return FoliageSavePath;
        }

        private void OnEnable()
        {
            LoadSavePaths();
            CreateFontStyles();
            var util = (FoliageRenormalizerUtility)target;
            CleanupDependents(util);
        }

        public override void OnInspectorGUI()
        {
            //base.DrawDefaultInspector();
            //GUILayout.Space(15);
            GUILayout.Space(5);

            var util = (FoliageRenormalizerUtility)target;
            bool anyChanged = false;

            if (TitleStyle == null)
                CreateFontStyles();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Foliage Renormlizer", TitleStyle);
            GUILayout.Label("There is no reason to remove this component. Nothing stored on this component will make it into a player build of your game, and removing it will just make it harder to touch up in the future.", SubTitleStyle);
            GUILayout.Label("If you decide you do not like the results, please click the 'Restore Original Mesh' button before removing.", SubTitleStyle);

            GUILayout.Space(8);
            EditorGUILayout.EndVertical();
            GUILayout.Space(8);

            #region Save Paths
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Mesh Save Paths", EditorStyles.boldLabel);
            EnsureSavePathsLoaded();

            EditorGUI.BeginChangeCheck();
            string newProxyPath = EditorGUILayout.DelayedTextField("Proxy Mesh Save Path", ProxySavePath);
            if (EditorGUI.EndChangeCheck())
            {
                ProxySavePath = newProxyPath;
                EditorPrefs.SetString("FoliageRenormalizerProxySavePath", ProxySavePath);
            }

            EditorGUI.BeginChangeCheck();
            string newFoliagePath = EditorGUILayout.DelayedTextField("Foliage Mesh Save Path", FoliageSavePath);
            if (EditorGUI.EndChangeCheck())
            {
                FoliageSavePath = newFoliagePath;
                EditorPrefs.SetString("FoliageRenormalizerFoliageSavePath", FoliageSavePath);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(8);

            #endregion

            if (util.TargetRenderer == null && util.GetComponent<Renderer>() != null)
            {
                util.TargetRenderer = util.GetComponent<Renderer>();
                EditorUtility.SetDirty(util);
            }
            Renderer[] ChildRenderers = util.GetComponentsInChildren<Renderer>();
            if (ChildRenderers.Length == 0)
            {
                EditorGUILayout.LabelField($"This GameObject has no renderer or child renderers!", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                return;
            }

            if (util.TargetRenderer == null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Target Renderer is not assigned! This will be the source of the proxy mesh generation, and should be part of LOD 0.", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                EditorGUILayout.LabelField($"Select Target Mesh:");
                for (int i = 0; i < ChildRenderers.Length; i++)
                {
                    if (GUILayout.Button($"{ChildRenderers[i].gameObject.name} (Mesh Renderer)"))
                    {
                        Undo.RecordObject(util, "Set Target Mesh Filter");
                        util.TargetRenderer = ChildRenderers[i];
                        EditorUtility.SetDirty(util);
                    }
                }
                EditorGUILayout.EndVertical();
                return;
            }

            if (!util.TargetRenderer.TryGetComponent<MeshFilter>(out MeshFilter targetFilter))
            {
                EditorGUILayout.LabelField($"Target Renderer does not have a MeshFilter component!", FoliageRenormalizerUtilityEditor.WarningGUIStyle);
                return;
            }

            #region General Options
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Shared Settings", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Target MeshFilter", targetFilter, typeof(MeshFilter), true);
            }

            if (targetFilter)
            {
                var mesh = targetFilter.sharedMesh;
                using (new EditorGUI.DisabledScope(mesh == null))
                {
                    EditorGUI.BeginChangeCheck();
                    util.ShowSubmeshSelectionMask = EditorGUILayout.Foldout(util.ShowSubmeshSelectionMask, "Material/Submesh Selector", true);
                    if (EditorGUI.EndChangeCheck())
                        anyChanged = true;

                    if (util.ShowSubmeshSelectionMask)
                    {
                        List<Material> materials = GetValidRendererMaterials(util.TargetRenderer);
                        DrawMaterialsSubmeshSelection(materials, util);
                    }

                    //string[] names = BuildSubmeshNames(targetFilter);
                    //int newMask = EditorGUILayout.MaskField("Submeshes", util.submeshMask, names);
                    //if (newMask != util.submeshMask)
                    //{
                    //    Undo.RecordObject(util, "Change Submesh Mask");
                    //    util.submeshMask = newMask;
                    //    anyChanged = true;
                    //}

                    //bool[] SubMeshMask = SubMeshSelectedMask(mesh, util.submeshMask);
                    //DrawMaterialDoubleSidedWarnings(targetFilter, SubMeshMask);
                }
            }

            //bool newInclude = EditorGUILayout.Toggle("Include Child Meshes", util.general.includeChildren);
            //if (newInclude != util.general.includeChildren)
            //{
            //    Undo.RecordObject(util, "Toggle Include Children");
            //    util.general.includeChildren = newInclude;
            //    anyChanged = true;
            //}

            bool newGrass = EditorGUILayout.Toggle("Grass Mode", util.general.grassMode);
            if (newGrass != util.general.grassMode)
            {
                Undo.RecordObject(util, "Toggle Grass Mode");
                util.general.grassMode = newGrass;
                anyChanged = true;
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            #endregion

            #region Grass Mode Options

            if (util.general.grassMode)
            {
                DrawGrassPanel(util, targetFilter, ref anyChanged);
                if (anyChanged) EditorUtility.SetDirty(util);
                return;
            }

            #endregion

            #region Proxy Generaiton Options
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Proxy Mesh Generation Settings", EditorStyles.boldLabel);
            
            int newRes = EditorGUILayout.IntSlider(new GUIContent("SDF Resolution", "Higher values = More detail retained in generated proxy mesh."), util.proxy.voxelResolution, 8, 256);
            if (newRes != util.proxy.voxelResolution)
            {
                Undo.RecordObject(util, "Change SDF Resolution");
                util.proxy.voxelResolution = newRes;
                anyChanged = true;
            }

            float newTight = EditorGUILayout.Slider(new GUIContent("Tightness", "0 = convex hull (blobby mesh). \n1 = highly detailed 'tight' mesh. \nIf you are close to 1, but notice 'air pockets' in your proxy mesh, reduce this value."), util.proxy.tightness, 0f, 1f);
            if (!Mathf.Approximately(newTight, util.proxy.tightness))
            {
                Undo.RecordObject(util, "Change Tightness");
                util.proxy.tightness = newTight;
                anyChanged = true;
            }

            int newBlurIters = EditorGUILayout.IntSlider(new GUIContent("SDF Blur Iterations", "Higher value = smoother output mesh & normals, but with slightly less detail retained."), util.proxy.blurIterations, 0, 5);
            if (newBlurIters != util.proxy.blurIterations)
            {
                Undo.RecordObject(util, "Change Blur Iterations");
                util.proxy.blurIterations = newBlurIters;
                anyChanged = true;
            }

            if (ShowProxyGenAdvancedOptionsToggle(util))
                anyChanged = true;

            if (util.ShowProxyGeneratorAdvancedOptions)
            {
                //GUILayout.Space(8);
                GUILayout.Label("Post Processing", EditorStyles.boldLabel);

                bool newWeld = EditorGUILayout.Toggle("Weld Vertices", util.proxy.optWeld);
                if (newWeld != util.proxy.optWeld)
                {
                    Undo.RecordObject(util, "Toggle Weld Vertices");
                    util.proxy.optWeld = newWeld;
                    anyChanged = true;
                }

                if (util.proxy.optWeld)
                {
                    float newEps = EditorGUILayout.FloatField("Weld Epsilon", util.proxy.optWeldEps);
                    if (!Mathf.Approximately(newEps, util.proxy.optWeldEps))
                    {
                        Undo.RecordObject(util, "Change Weld Epsilon");
                        util.proxy.optWeldEps = newEps;
                        anyChanged = true;
                    }
                }
            }

            GUILayout.Space(8);
            using (new EditorGUI.DisabledScope(targetFilter == null))
            {
                if (GUILayout.Button("Generate Proxy Mesh"))
                    GenerateProxy(util, targetFilter);
            }

            using (new EditorGUI.DisabledScope(util.GeneratedProxyMesh == null))
            {
                string buttonText = "Show Proxy Mesh";
                if (util.ProxyMeshFilter != null)
                    buttonText = "Stop Showing Proxy Mesh";
                if (GUILayout.Button(buttonText))
                {
                    if (util.ProxyMeshFilter == null)
                        SpawnProxyMesh(util);
                    else
                        DestroyProxyMesh(util);

                    EditorUtility.SetDirty(util.gameObject);
                }
            }
            EditorGUILayout.EndVertical();

            #endregion

            #region Transfer Options

            GUILayout.Space(8);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawTransfer(util, targetFilter, ref anyChanged);
            EditorGUILayout.EndVertical();

            #endregion

            if (anyChanged)
                EditorUtility.SetDirty(util);

            GUILayout.Space(8);
            DrawDependentProxyStealersBox(util, ChildRenderers);
        }

        void CleanupDependents(FoliageRenormalizerUtility util)
        {
            if (util.Dependants == null)
            {
                util.Dependants = new FoliageRenormalizerProxyStealer[0];
                EditorUtility.SetDirty(util);
            }
            List<FoliageRenormalizerProxyStealer> stealers = new List<FoliageRenormalizerProxyStealer>();
            bool changed = false;
            for (int i = 0; i < util.Dependants.Length; i++)
            {
                if (util.Dependants[i] == null || util.Dependants[i].Source == null || util.Dependants[i].Source != util)
                {
                    changed = true;
                    continue;
                }
                stealers.Add(util.Dependants[i]);
            }

            if (changed)
            {
                util.Dependants = stealers.ToArray();
                EditorUtility.SetDirty(util);
            }
        }

        private bool ShowProxyGenAdvancedOptionsToggle(FoliageRenormalizerUtility util)
        {
            EditorGUI.BeginChangeCheck();
            util.ShowProxyGeneratorAdvancedOptions = EditorGUILayout.Foldout(util.ShowProxyGeneratorAdvancedOptions, "Advanced Options", true);
            return EditorGUI.EndChangeCheck();
        }

        public const string GenerateNewMeshText = "Generate New Mesh";
        public const string RestoreOriginalMeshText = "Restore Original Mesh";
        public const string RestoreRemeshedMeshText = "Switch to previously generated Foliage Mesh";
        private void DrawGrassPanel(FoliageRenormalizerUtility util, MeshFilter targetFilter, ref bool anyChanged)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Grass Normals", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Foliage (receives)", targetFilter, typeof(MeshFilter), true);
            EditorGUI.EndDisabledGroup();

            bool newApply = EditorGUILayout.Toggle("Apply Current Rotation", util.grass.applyRotation);
            if (newApply != util.grass.applyRotation)
            {
                Undo.RecordObject(util, "Toggle Grass Apply Rotation");
                util.grass.applyRotation = newApply;
                anyChanged = true;
            }

            GUILayout.Space(6);

            bool newBakeAO = EditorGUILayout.Toggle("Enable AO Baker", util.baker.BakeGroundAOGrassMode);
            if (newBakeAO != util.baker.BakeGroundAOGrassMode)
            {
                Undo.RecordObject(util, "Toggle Grass AO Baker");
                util.baker.BakeGroundAOGrassMode = newBakeAO;
                anyChanged = true;
            }
            var newChannel = (FoliageRenormalizerUtility.ColorChannel)EditorGUILayout.EnumPopup("Vertex Color Channel", util.baker.AoBakeChannel);
            if (newChannel != util.baker.AoBakeChannel)
            {
                Undo.RecordObject(util, "Change AO Channel");
                util.baker.AoBakeChannel = newChannel;
                anyChanged = true;
            }
            float newStartHeight = EditorGUILayout.Slider(new GUIContent("Ground AO Start Height", "As a percentage of the source meshes Bounds"), util.baker.GroundAoStartHeight, 0f, 1f);
            if (!Mathf.Approximately(newStartHeight, util.baker.GroundAoStartHeight))
            {
                Undo.RecordObject(util, "Change Ground AO Start Height");
                util.baker.GroundAoStartHeight = newStartHeight;
                if (util.baker.GroundAoEndHeight < util.baker.GroundAoStartHeight)
                    util.baker.GroundAoEndHeight = util.baker.GroundAoStartHeight;
                anyChanged = true;
            }
            float newEndHeight = EditorGUILayout.Slider(new GUIContent("Ground AO End Height", "As a percentage of the source meshes Bounds"), util.baker.GroundAoEndHeight, 0f, 1f);
            if (!Mathf.Approximately(newEndHeight, util.baker.GroundAoEndHeight))
            {
                Undo.RecordObject(util, "Change Ground AO End Height");
                util.baker.GroundAoEndHeight = newEndHeight;
                if (util.baker.GroundAoEndHeight < util.baker.GroundAoStartHeight)
                    util.baker.GroundAoStartHeight = util.baker.GroundAoEndHeight;
                anyChanged = true;
            }
            float newGroundDarkenAmount = EditorGUILayout.Slider("Ground AO Darkening Amount", util.baker.GroundAoDarkeningStrength, 0f, 1.0f);
            if (!Mathf.Approximately(newGroundDarkenAmount, util.baker.GroundAoDarkeningStrength))
            {
                Undo.RecordObject(util, "Change Ground AO Darken Amount");
                util.baker.GroundAoDarkeningStrength = newGroundDarkenAmount;
                anyChanged = true;
            }

            DrawVertexColorDebug(util);

            using (new EditorGUI.DisabledScope(targetFilter == null || targetFilter.sharedMesh == null))
            {
                if (GUILayout.Button(GenerateNewMeshText))
                {
                    var s = FoliageNormalTransfer.GetSettingsFromUtil(util);
                    FoliageNormalTransfer.GenerateGrassNormals(util, targetFilter, s, SelectedSubmeshes(util, util), util.grass.applyRotation);
                    util.LastGenerationID++;
                    EditorUtility.SetDirty(util);
                }
            }

            using (new EditorGUI.DisabledScope(util.OriginalFoliageMesh == null || (util.RemeshedFoliageMesh == null && targetFilter.sharedMesh == util.OriginalFoliageMesh)))
            {
                string buttonText = RestoreOriginalMeshText;
                if (util.RemeshedFoliageMesh != null && util.OriginalFoliageMesh != null && targetFilter.sharedMesh == util.OriginalFoliageMesh)
                    buttonText = RestoreRemeshedMeshText;
                if (GUILayout.Button(buttonText))
                    SwapMeshFilterMesh(targetFilter, util);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawTransfer(FoliageRenormalizerUtility util, MeshFilter targetFilter, ref bool anyChanged)
        {
            GUILayout.Label("Foliage Remesher", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Foliage (receives)", targetFilter, typeof(MeshFilter), true);
            EditorGUILayout.ObjectField("Proxy (source)", util.GeneratedProxyMesh, typeof(Mesh), true);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(8);
            GUILayout.Label("Foliage Normals", EditorStyles.boldLabel);
            DrawNormalsDebug(util);

            bool newTransferNormals = EditorGUILayout.Toggle("Enable Normal Transfer", util.baker.EnableNormalTransfer);
            if (newTransferNormals != util.baker.EnableNormalTransfer)
            {
                Undo.RecordObject(util, "Toggle Normal Transfer");
                util.baker.EnableNormalTransfer = newTransferNormals;
                anyChanged = true;
            }
            EditorGUI.BeginDisabledGroup(!util.baker.EnableNormalTransfer);
            float newProxyNormalInfluence = EditorGUILayout.Slider(new GUIContent("Proxy Normal Influence", "Spherically interpolates each vertex normal towards the proxy meshes vertex normal by this value"), util.baker.ProxyNormalInfluence, 0f, 1.0f);
            if (!Mathf.Approximately(newProxyNormalInfluence, util.baker.ProxyNormalInfluence))
            {
                Undo.RecordObject(util, "Change Proxy Normal Influene");
                util.baker.ProxyNormalInfluence = newProxyNormalInfluence;
                anyChanged = true;
            }

            float newUpward = EditorGUILayout.Slider(new GUIContent("Normal Upward Bias", "Spherically interpolates each vertex normal upward by this value (after stealing the proxy normal)"), util.baker.upwardBias, 0f, 1.0f);
            if (!Mathf.Approximately(newUpward, util.baker.upwardBias))
            {
                Undo.RecordObject(util, "Change Upward Bias");
                util.baker.upwardBias = newUpward;
                anyChanged = true;
            }
            EditorGUI.EndDisabledGroup();

            //EditorGUI.BeginChangeCheck();
            //util.ShowFoliageTransferAdvancedOptions = EditorGUILayout.Foldout(util.ShowFoliageTransferAdvancedOptions, "Advanced Options", true);
            //if (EditorGUI.EndChangeCheck())
            //    anyChanged = true;

            //if (util.ShowFoliageTransferAdvancedOptions)
            //{

            //}

            GUILayout.Space(10);
            #region AO Settings
            GUILayout.Label("Vertex Color AO Baker", EditorStyles.boldLabel);

            //AO shared
            var newChannel = (FoliageRenormalizerUtility.ColorChannel)EditorGUILayout.EnumPopup("Vertex Color Channel", util.baker.AoBakeChannel);
            if (newChannel != util.baker.AoBakeChannel)
            {
                Undo.RecordObject(util, "Change AO Channel");
                util.baker.AoBakeChannel = newChannel;
                anyChanged = true;
            }

            DrawVertexColorDebug(util);

            GUILayout.Space(4);

            //SDF AO
            bool newBakeAO = EditorGUILayout.Toggle("Enable SDF AO Baker", util.baker.BakeAO);
            if (newBakeAO != util.baker.BakeAO)
            {
                Undo.RecordObject(util, "Toggle AO Baker");
                util.baker.BakeAO = newBakeAO;
                anyChanged = true;
            }
            EditorGUI.BeginDisabledGroup(!util.baker.BakeAO);
            float newMinAo = EditorGUILayout.Slider(new GUIContent("AO Minimum Distance", "As a percentage of the source meshes Bounds"), util.baker.AoMinimumdistance, 0f, .5f);
            if (!Mathf.Approximately(newMinAo, util.baker.AoMinimumdistance))
            {
                Undo.RecordObject(util, "Change AO Min Distance");
                util.baker.AoMinimumdistance = newMinAo;
                if (util.baker.AoMaximumdistance < util.baker.AoMinimumdistance)
                    util.baker.AoMaximumdistance = util.baker.AoMinimumdistance;
                anyChanged = true;
            }
            float newMaxAO = EditorGUILayout.Slider(new GUIContent("AO Maximum Distance", "As a percentage of the source meshes Bounds"), util.baker.AoMaximumdistance, 0f, .5f);
            if (!Mathf.Approximately(newMaxAO, util.baker.AoMaximumdistance))
            {
                Undo.RecordObject(util, "Change AO Max Distance");
                util.baker.AoMaximumdistance = newMaxAO;
                if (util.baker.AoMaximumdistance < util.baker.AoMinimumdistance)
                    util.baker.AoMinimumdistance = util.baker.AoMaximumdistance;
                anyChanged = true;
            }
            float newDarkenAmount = EditorGUILayout.Slider("AO Darkening Amount", util.baker.AoDarkeningStrength, 0f, 1.0f);
            if (!Mathf.Approximately(newDarkenAmount, util.baker.AoDarkeningStrength))
            {
                Undo.RecordObject(util, "Change AO Darken Amount");
                util.baker.AoDarkeningStrength = newDarkenAmount;
                anyChanged = true;
            }
            EditorGUI.EndDisabledGroup();

            //'Distance to Ground-Plane' AO
            EditorGUI.BeginChangeCheck();
            util.ShowGroundPlaneAOBakerOptions = EditorGUILayout.Foldout(util.ShowGroundPlaneAOBakerOptions, "Ground-Distance AO", true);
            if (EditorGUI.EndChangeCheck())
                anyChanged = true;

            if (util.ShowGroundPlaneAOBakerOptions)
            {
                bool newBakeGroundAO = EditorGUILayout.Toggle("  Enable Ground-Distance AO Baker", util.baker.BakeGroundAO);
                if (newBakeGroundAO != util.baker.BakeGroundAO)
                {
                    Undo.RecordObject(util, "Toggle Ground-Distance AO Baker");
                    util.baker.BakeGroundAO = newBakeGroundAO;
                    anyChanged = true;
                }
                EditorGUI.BeginDisabledGroup(!util.baker.BakeGroundAO);
                float newStartHeight = EditorGUILayout.Slider(new GUIContent("  Ground AO Start Height", "As a percentage of the source meshes Bounds"), util.baker.GroundAoStartHeight, 0f, 1f);
                if (!Mathf.Approximately(newStartHeight, util.baker.GroundAoStartHeight))
                {
                    Undo.RecordObject(util, "Change Ground AO Start Height");
                    util.baker.GroundAoStartHeight = newStartHeight;
                    if (util.baker.GroundAoEndHeight < util.baker.GroundAoStartHeight)
                        util.baker.GroundAoEndHeight = util.baker.GroundAoStartHeight;
                    anyChanged = true;
                }
                float newEndHeight = EditorGUILayout.Slider(new GUIContent("  Ground AO End Height", "As a percentage of the source meshes Bounds"), util.baker.GroundAoEndHeight, 0f, 1f);
                if (!Mathf.Approximately(newEndHeight, util.baker.AoMaximumdistance))
                {
                    Undo.RecordObject(util, "Change Ground AO End Height");
                    util.baker.GroundAoEndHeight = newEndHeight;
                    if (util.baker.GroundAoEndHeight < util.baker.GroundAoStartHeight)
                        util.baker.GroundAoStartHeight = util.baker.GroundAoEndHeight;
                    anyChanged = true;
                }
                float newGroundDarkenAmount = EditorGUILayout.Slider("  Ground AO Darkening Amount", util.baker.GroundAoDarkeningStrength, 0f, 1.0f);
                if (!Mathf.Approximately(newGroundDarkenAmount, util.baker.GroundAoDarkeningStrength))
                {
                    Undo.RecordObject(util, "Change Ground AO Darken Amount");
                    util.baker.GroundAoDarkeningStrength = newGroundDarkenAmount;
                    anyChanged = true;
                }
                EditorGUI.EndDisabledGroup();
            }
            

            #endregion

            GUILayout.Space(10);
            bool disabled = targetFilter == null || util.GeneratedProxyMesh == null;
            using (new EditorGUI.DisabledScope(disabled || targetFilter.sharedMesh == null))
            {
                string text = GenerateNewMeshText;
                if (!util.GeneratedProxyMesh)
                    text = "You must generate a Proxy Mesh before Remeshing.";
                if (GUILayout.Button(text))
                {
                    var s = FoliageNormalTransfer.GetSettingsFromUtil(util);

                    FoliageNormalTransfer.Transfer(util, targetFilter, util.GeneratedProxyMesh, s, SelectedSubmeshes(util, util));

                    util.LastGenerationID++;
                    EditorUtility.SetDirty(util);
                }
            }

            using (new EditorGUI.DisabledScope(util.OriginalFoliageMesh == null || (util.RemeshedFoliageMesh == null && targetFilter.sharedMesh == util.OriginalFoliageMesh)))
            {
                string buttonText = RestoreOriginalMeshText;
                if (util.RemeshedFoliageMesh != null && util.OriginalFoliageMesh != null && targetFilter.sharedMesh == util.OriginalFoliageMesh)
                    buttonText = RestoreRemeshedMeshText;
                if (GUILayout.Button(buttonText))
                    SwapMeshFilterMesh(targetFilter, util);
            }
        }

        public static void DrawNormalsDebug(FoliageRenormalizerStorage storage)
        {
            bool newShowNormals = EditorGUILayout.Toggle("Debug Render Normals?", storage.ShowMaterialNormals);
            if (newShowNormals != storage.ShowMaterialNormals)
            {
                Undo.RecordObject(storage, "Toggle Render Normals");

                if (storage.TargetRenderer != null)
                {
                    Renderer mr = storage.TargetRenderer;
                    if (storage.VertexColorPreviewChannel == 0 && !storage.ShowMaterialNormals)
                        storage.OriginalMaterials = mr.sharedMaterials;

                    if (storage.VertexColorPreviewChannel != 0)
                        storage.VertexColorPreviewChannel = 0;

                    if (newShowNormals)
                    {
                        Material mat = (Material)Resources.Load("Show Normals");
                        if (mat != null)
                        {
                            Material[] newMats = new Material[mr.sharedMaterials.Length];
                            for (int i = 0; i < newMats.Length; i++)
                                newMats[i] = mat;
                            mr.sharedMaterials = newMats;
                        }
                        else
                            Debug.LogError("Could not find Vertex Normal Debug Material.");
                    }
                    else if (storage.OriginalMaterials != null)
                        mr.sharedMaterials = storage.OriginalMaterials;

                    EditorUtility.SetDirty(mr);
                }
                storage.ShowMaterialNormals = newShowNormals;
                EditorUtility.SetDirty(storage);
            }
        }

        public static void DrawVertexColorDebug(FoliageRenormalizerStorage storage)
        {
            const float box = 20f;
            const float label = 12f;
            const float gap = 12f;

            int ChannelRadioRow(int value)
            {
                int newValue = value;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Debug Render Vertex Color", GUILayout.Width(170));

                    void Draw(string text, float width, int code)
                    {
                        if (GUILayout.Toggle(newValue == code, text, LeftRadioToggleStyle, GUILayout.Width(width + box)))
                            newValue = code;
                    }

                    Draw("None", label * 2.75f, 0); GUILayout.Space(gap);
                    Draw("R", label, 1); GUILayout.Space(gap);
                    Draw("G", label, 2); GUILayout.Space(gap);
                    Draw("B", label, 3); GUILayout.Space(gap);
                    Draw("A", label, 4);
                }

                return newValue;
            }

            EditorGUI.BeginChangeCheck();
            int previous = storage.VertexColorPreviewChannel;
            int newChan = ChannelRadioRow(previous);
            if (EditorGUI.EndChangeCheck())
            {
                if (storage.TargetRenderer != null)
                {
                    Renderer mr = storage.TargetRenderer;
                    if (previous == 0 && !storage.ShowMaterialNormals)
                        storage.OriginalMaterials = mr.sharedMaterials;

                    if (newChan == 0 && storage.OriginalMaterials != null)
                        mr.sharedMaterials = storage.OriginalMaterials;
                    else
                    {
                        if (storage.ShowMaterialNormals)
                            storage.ShowMaterialNormals = false;

                        Material mat = null;

                        switch (newChan)
                        {
                            case 1: mat = (Material)Resources.Load("Show Vertex Colors R"); break;
                            case 2: mat = (Material)Resources.Load("Show Vertex Colors G"); break;
                            case 3: mat = (Material)Resources.Load("Show Vertex Colors B"); break;
                            case 4: mat = (Material)Resources.Load("Show Vertex Colors A"); break;
                        }

                        if (mat != null)
                        {
                            Material[] newMats = new Material[mr.sharedMaterials.Length];
                            for (int i = 0; i < newMats.Length; i++)
                                newMats[i] = mat;
                            mr.sharedMaterials = newMats;
                        }
                        else
                            Debug.LogError("Could not find the Vertex Color Debug Material!");
                    }
                }
                else
                    Debug.LogError("Could Not Find Mesh Renderer on GameObject!");

                Undo.RecordObject(storage, "Change Vertex Color Debug");
                storage.VertexColorPreviewChannel = newChan; // 0=None, 1=R, 2=G, 3=B, 4=A
                EditorUtility.SetDirty(storage);
                //anyChanged = true;
            }
        }

        private bool[] SubMeshSelectedMask(Mesh mesh, int submeshMask)
        {
            bool[] SubMeshMask = new bool[mesh.subMeshCount];
            for (int sm = 0; sm < mesh.subMeshCount; sm++)
            {
                if ((submeshMask & (1 << sm)) == 0)
                    SubMeshMask[sm] = false;
                else
                    SubMeshMask[sm] = true;
            }
            return SubMeshMask;
        }

        public static List<Material> GetValidRendererMaterials(Renderer mr)
        {
            List<Material> materials = new List<Material>();
            for (int i = 0; i < mr.sharedMaterials.Length; i++)
            {
                if (mr.sharedMaterials[i] == null)
                    continue;
                Material mat = mr.sharedMaterials[i];
                if (materials.Contains(mat))
                    continue;
                materials.Add(mat);
            }
            return materials;
        }
        
        private static bool[] SelectedSubmeshes(FoliageRenormalizerStorage targetStorage, FoliageRenormalizerUtility sourceSettings)
        {
            bool[] SubMeshMask = new bool[targetStorage.TargetRenderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount];
            if (targetStorage.TargetRenderer.sharedMaterials == null) return SubMeshMask;
            for (int i = 0; i < targetStorage.TargetRenderer.sharedMaterials.Length; i++)
            {
                if (i >= targetStorage.TargetRenderer.sharedMaterials.Length || targetStorage.TargetRenderer.sharedMaterials[i] == null)
                    continue;

                if (sourceSettings.IgnoredMaterials.Contains(targetStorage.TargetRenderer.sharedMaterials[i]))
                    continue;

                SubMeshMask[i] = true;
            }

            return SubMeshMask;
        }

        public static void SwapMeshFilterMesh(MeshFilter targetFilter, FoliageRenormalizerStorage storage)
        {
            if (!targetFilter) return;
            if (!storage) return;
            
            if (storage.OriginalFoliageMesh)
            {
                if (storage.RemeshedFoliageMesh && targetFilter.sharedMesh == storage.OriginalFoliageMesh)
                {
                    targetFilter.sharedMesh = storage.RemeshedFoliageMesh;
                    targetFilter.transform.localRotation = storage.RemeshedRotation;
                }
                else
                {
                    targetFilter.sharedMesh = storage.OriginalFoliageMesh;
                    storage.RemeshedRotation = targetFilter.transform.localRotation;
                    targetFilter.transform.localRotation = storage.OriginalRotation;
                }
                
                EditorUtility.SetDirty(targetFilter.gameObject);
            }
            else
            {
                Debug.LogWarning("No OriginalMesh stored on FoliageRenormalizerUtility.");
            }
        }

        private void GenerateProxy(FoliageRenormalizerUtility util, MeshFilter targetFilter)
        {
            if (!targetFilter) return;
            var rootGO = targetFilter.gameObject;

            Mesh srcMesh = Instantiate(targetFilter.sharedMesh);
            bool[] submeshMask = SelectedSubmeshes(util, util);
            srcMesh = BuildTempMaskedMesh(targetFilter.sharedMesh, submeshMask);
            if (srcMesh == null) return;
            srcMesh.RecalculateBounds();
            Mesh mesh = SDFProxyBuilder.Build(srcMesh, util.proxy.voxelResolution, Mathf.Clamp01(util.proxy.tightness), util.proxy.blurIterations);
            SDFProxyBuilder.PostProcessProxy(mesh, util);

            if (!mesh)
            {
                Debug.LogWarning("Mesh generation returned null.");
                return;
            }

            FolderUtil.EnsureFolders(ProxySavePath);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{ProxySavePath}/{rootGO.name + "_Proxy"}.asset");
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);

            if (util.GeneratedProxyMesh != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(util.GeneratedProxyMesh));
            }

            util.GeneratedProxyMesh = mesh;

            if (util.ProxyMeshFilter != null)
            {
                
                if (util.ProxyMeshFilter.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
                {
                    util.ProxyMeshFilter.sharedMesh = mesh;
                    SetProxyMeshMaterial(mr);
                }
                else
                {
                    DestroyProxyMesh(util);
                    SpawnProxyMesh(util);
                }
                EditorUtility.SetDirty(util.ProxyMeshFilter.gameObject);
            }

            EditorUtility.SetDirty(util);

            Debug.Log($"[Ultimate Foliage Renormalizer] Proxy generated. Verts={mesh.vertexCount} Tris={mesh.triangles.Length / 3}");
        }

        void DestroyProxyMesh(FoliageRenormalizerUtility util)
        {
            DestroyImmediate(util.ProxyMeshFilter.gameObject);
        }

        void SpawnProxyMesh(FoliageRenormalizerUtility util)
        {
            var go = new GameObject(util.gameObject.name + "_Proxy");
            go.transform.SetParent(util.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var mf = go.AddComponent<MeshFilter>();
            util.ProxyMeshFilter = mf;
            mf.sharedMesh = util.GeneratedProxyMesh;
            var mr = go.AddComponent<MeshRenderer>();
            SetProxyMeshMaterial(mr);
        }

        void SetProxyMeshMaterial(MeshRenderer mr)
        {
            try
            {
                mr.sharedMaterial = (Material)Resources.Load("Show Normals");
            }
            catch { }
        }

        public static string[] BuildSubmeshNames(MeshFilter mf)
        {
            if (mf.sharedMesh == null) return BuildGenericSubmeshNames(8);
            Mesh mesh = mf.sharedMesh;
            mf.TryGetComponent<MeshRenderer>(out MeshRenderer rend);
            if (rend == null || rend.sharedMaterials == null) return BuildGenericSubmeshNames(8);

            int n = Mathf.Max(1, mesh.subMeshCount);
            var arr = new string[n];
            for (int i = 0; i < n; i++)
            {
                arr[i] = "Submesh " + i;
                if (i >= rend.sharedMaterials.Length || rend.sharedMaterials[i] == null)
                {
                    arr[i] += ": Missing Material";
                    continue;
                }
                    
                arr[i] += $": {rend.sharedMaterials[i].name}";
            }
                
            return arr;
        }

        public static string[] BuildGenericSubmeshNames(int count)
        {
            var arr = new string[Mathf.Max(1, count)];
            for (int i = 0; i < arr.Length; i++) arr[i] = "Submesh " + i;
            return arr;
        }

        private static Mesh BuildTempMaskedMesh(Mesh src, bool[] submeshMask)
        {
            if (!src) return null;

            if (submeshMask == null) return src;

            bool all = true;
            for (int i = 0; i < submeshMask.Length; i++)
            {
                if (!submeshMask[i])
                    all = false;
            }
            if (all) return src;

            var list = new List<CombineInstance>();
            for (int sm = 0; sm < Mathf.Max(1, src.subMeshCount); sm++)
            {
                if (!submeshMask[sm]) continue;

                var ci = new CombineInstance
                {
                    mesh = src,
                    subMeshIndex = sm,
                    transform = Matrix4x4.identity
                };
                list.Add(ci);
            }

            if (list.Count == 0) return null;

            var tmp = new Mesh { name = src.name + "_Masked" };

            tmp.indexFormat = src.indexFormat;

            tmp.CombineMeshes(list.ToArray(), true, false, false);
            tmp.RecalculateBounds();

            return tmp;
        }

        public void DrawDependentProxyStealersBox(FoliageRenormalizerUtility util, Renderer[] ChildRenderers)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Dependent Proxy Stealer Settings", EditorStyles.boldLabel);

            GUILayout.Space(5);
            bool shownWarning = false;
            for (int i = 0; i < ChildRenderers.Length; i++)
            {
                if (ChildRenderers[i] == null)
                    continue;
                if (ChildRenderers[i] != util.TargetRenderer && 
                    (!ChildRenderers[i].TryGetComponent<FoliageRenormalizerProxyStealer>(out FoliageRenormalizerProxyStealer stealer) || stealer.Source != util) )
                {
                    if (ChildRenderers[i].TryGetComponent<MeshFilter>(out MeshFilter mf) && mf.sharedMesh == util.GeneratedProxyMesh)
                        continue;

                    if (!shownWarning)
                    {
                        GUILayout.Label("The following Child Renderers are not assigned to this Foliage Renormalizer component:", SoftWarningGUIStyle);
                        shownWarning = true;
                    }

                    if (GUILayout.Button($"Assign '{ChildRenderers[i].gameObject.name}' as a Dependent of this proxy utility."))
                    {
                        if (stealer == null)
                            stealer = ChildRenderers[i].gameObject.AddComponent<FoliageRenormalizerProxyStealer>();
                        stealer.Source = util;
                        stealer.TargetRenderer = ChildRenderers[i];
                        TryAddProxyStealerToUtil(util, stealer);
                        EditorUtility.SetDirty(ChildRenderers[i].gameObject);
                    }
                }
            }

            if (util.Dependants == null || util.Dependants.Length == 0)
            {
                GUILayout.Label("No Proxy Stealers assigned.");
                EditorGUILayout.EndVertical();
                return;
            }

            GUILayout.Space(5);
            DrawDependentProxyStealers(util);

            EditorGUILayout.EndVertical();
        }

        public static void TryAddProxyStealerToUtil(FoliageRenormalizerUtility util, FoliageRenormalizerProxyStealer stealer)
        {
            if (util.Dependants == null)
                util.Dependants = new FoliageRenormalizerProxyStealer[0];

            if (util.TryAddDependent(stealer))
            {
                EditorUtility.SetDirty(stealer);
                EditorUtility.SetDirty(util);
            }
        }

        void DrawDependentProxyStealers(FoliageRenormalizerUtility util)
        {
            GUILayout.Label("Dependent Proxy Stealers", EditorStyles.boldLabel);
            GUILayout.Space(1);

            EditorGUI.BeginChangeCheck();
            util.DrawDependentSubmeshMask = EditorGUILayout.Foldout(util.DrawDependentSubmeshMask, "Override Proxy Stealer Material / Submesh Selector", true);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(util);

            if (util.DrawDependentSubmeshMask)
            {
                List<Material> sourceMaterials = GetValidRendererMaterials(util.TargetRenderer);
                List<Material> uniqueMaterials = new List<Material>();
                for (int i = 0; i < util.Dependants.Length; i++)
                {
                    FoliageRenormalizerProxyStealer stealer = util.Dependants[i];
                    if (stealer.TryGetComponent<Renderer>(out Renderer newRend))
                    {
                        stealer.TargetRenderer = newRend;
                        EditorUtility.SetDirty(stealer);
                    }
                    else
                        continue;
                    List<Material> stealerMaterial = GetValidRendererMaterials(stealer.TargetRenderer);
                    for (int m = 0; m < stealerMaterial.Count; m++)
                    {
                        if (!sourceMaterials.Contains(stealerMaterial[m]))
                        {
                            uniqueMaterials.Add(stealerMaterial[m]);
                        }
                    }
                }
                EditorGUI.indentLevel += 2;
                
                if (uniqueMaterials.Count > 0)
                    DrawMaterialsSubmeshSelection(uniqueMaterials, util);
                else
                    GUILayout.Label("No unique materials detected!");

                EditorGUI.indentLevel -= 2;
            }

            GUILayout.Space(2);
            //EditorGUILayout.LabelField(util.Dependants.Length.ToString());

            float ObjectFieldWidth = 200;
            for (int i = 0; i < util.Dependants.Length; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (util.Dependants[i].LastGenerationID != util.LastGenerationID)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(GUIContent.none, util.Dependants[i], typeof(FoliageRenormalizerProxyStealer), true, GUILayout.Width(ObjectFieldWidth));
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.LabelField($" Outdated!", WarningGUIStyle, GUILayout.Width(70));
                        if (GUILayout.Button($"Regenerate {util.Dependants[i].gameObject.name.ToString()} Mesh", GUILayout.ExpandWidth(true)))
                        {
                            GenerateProxyStealerMesh(util.Dependants[i]);
                        }
                    }
                    else
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(GUIContent.none, util.Dependants[i], typeof(FoliageRenormalizerProxyStealer), true, GUILayout.Width(ObjectFieldWidth));
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.LabelField($" Up to date!", GUILayout.Width(70));
                    }
                }
            }
        }

        public static void DrawMaterialsSubmeshSelection(List<Material> mats, FoliageRenormalizerUtility util)
        {
            List<Material> enabledMaterials = new List<Material>();

            for (int i = 0; i < mats.Count; i++)
            {
                Material mat = mats[i];
                bool disabled = util.IgnoredMaterials.Contains(mat);
                if (!disabled)
                    enabledMaterials.Add(mat);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("", GUILayout.Width(8));

                    EditorGUI.BeginChangeCheck();
                    bool newIncludeMat = GUILayout.Toggle(!disabled, mat.name, LeftRadioToggleStyle, GUILayout.Width(1000));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(util, "Change Submesh Selection");
                        if (newIncludeMat)
                            util.IgnoredMaterials.Remove(mat);
                        else
                            util.IgnoredMaterials.Add(mat);
                        EditorUtility.SetDirty(util);
                    }
                }
            }

            DrawMaterialDoubleSidedWarnings(enabledMaterials);
        }

        public static void GenerateProxyStealerMesh(FoliageRenormalizerProxyStealer stealer)
        {
            FoliageRenormalizerUtility util = stealer.Source;
            var s = FoliageNormalTransfer.GetSettingsFromUtil(util);

            if (!stealer.TryGetComponent<MeshFilter>(out MeshFilter targetFilter))
            {
                Debug.LogError("Foliage Renormalizer Proxy Stealer does not have a mesh filter.");
                return;
            }

            if (util.general.grassMode)
                FoliageNormalTransfer.GenerateGrassNormals(stealer, targetFilter, s, SelectedSubmeshes(stealer, util), util.grass.applyRotation);
            else
                FoliageNormalTransfer.Transfer(stealer, targetFilter, util.GeneratedProxyMesh, s, SelectedSubmeshes(stealer, util));

            stealer.LastGenerationID = util.LastGenerationID;
            EditorUtility.SetDirty(stealer);
        }

        public static void DrawMaterialDoubleSidedWarnings(List<Material> materials)
        {
            bool FirstWarningShown = false;
            void ShowWarning(string warning)
            {
                if (!FirstWarningShown)
                {
                    EditorGUILayout.LabelField("The Following materials have incorrect normals modes. Please change them after Renormalizing!", WarningGUIStyleBold);
                    FirstWarningShown = true;
                }
                EditorGUILayout.LabelField(" - " + warning, WarningGUIStyle);
            }

            for (int i = 0; i < materials.Count; i++)
            {
                Material mat = materials[i];
                //ShowWarning($"Material '{mat.name}': HDRP Double Sided detected with wrong flip mode. Set 'Normal Mode' to 'None'.");
                if (mat.HasProperty("_DoubleSidedConstants"))   //HDRP: Lit, Shadergraph, and ASE
                {
                    //flip      = -1,-1,-1,0
                    //mirror    = 1,1,-1,0
                    //none      = 1,1,1,0
                    bool doubleSidedOn = mat.IsKeywordEnabled("_DOUBLESIDED_ON");
                    Vector4 doubleSidedNormal = mat.GetVector("_DoubleSidedConstants");
                    if (doubleSidedOn && !Mathf.Approximately(doubleSidedNormal.z, 1))
                        ShowWarning($"Material '{mat.name}': HDRP Double Sided detected with wrong flip mode. Set 'Normal Mode' to 'None'.");
                }

                //if (mat.HasProperty("Backface_Normal_Mode"))   //Speedtree URP. not needed atm.
                //{
                //    float backfaceMode = mat.GetFloat("Backface_Normal_Mode");
                //    if (backfaceMode < 1.5f)
                //        ShowWarning($"Material '{mat.name}': URP Speedtree detected with wrong flip mode. Set 'Backface Normal Mode' to > 1.5. YOU MUST MODIFY THE URP SPEEDTREE SHADERGRAPH. PLEASE REFER TO THE QUICK START GUIDE.");
                //}

                if (mat.HasProperty("_RenderNormal"))   //TVE
                {
                    bool renderBackFaces = true;
                    if (mat.HasProperty("_CullMode"))
                    {
                        float cullMode = mat.GetFloat("_CullMode");
                        renderBackFaces = Mathf.Approximately(cullMode, 0);    //if both faces rendered
                        renderBackFaces |= Mathf.Approximately(cullMode, 1);        //if just backfaces rendered
                    }
                    else if (mat.HasProperty("_RenderCull"))
                    {
                        float cullMode = mat.GetFloat("_RenderCull");
                        renderBackFaces = Mathf.Approximately(cullMode, 0);    //if both faces rendered
                        renderBackFaces |= Mathf.Approximately(cullMode, 1);        //if just backfaces rendered
                    }
                    if (renderBackFaces && !Mathf.Approximately(mat.GetFloat("_RenderNormal"), 2f))
                        ShowWarning($"Material '{mat.name}': TVE Double Sided detected with wrong normals mode. Set 'Render Normals' to 'Same'.");
                }
            }
        }
    }

    public static class FolderUtil
    {
        public static string EnsureFolders(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new System.ArgumentException("path");
            path = path.Replace("\\", "/");

            if (path == "Assets") return "Assets";
            if (!path.StartsWith("Assets/")) throw new System.ArgumentException("Path must start with Assets/");

            string[] parts = path.Split('/');
            string current = "Assets";

            for (int i = 1; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
            return current;
        }

        public static string EnsureParentFolder(string assetPath)
        {
            var dir = System.IO.Path.GetDirectoryName(assetPath).Replace("\\", "/");
            return EnsureFolders(dir);
        }
    }
}
#endif
