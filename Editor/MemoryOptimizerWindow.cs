using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static JeTeeS.MemoryOptimizer.MemoryOptimizerWindow.SqueezeScope;
using static JeTeeS.MemoryOptimizer.MemoryOptimizerWindow.SqueezeScope.SqueezeScopeType;
using static JeTeeS.TES.HelperFunctions.TESHelperFunctions;

namespace JeTeeS.MemoryOptimizer
{
    public class MemoryOptimizerWindow : EditorWindow
    {
        private const string menuPath = "Tools/TES/MemoryOptimizer";
        private string mainSavePath = "Assets/TES/MemoryOptimizer/";

        private readonly string[] paramTypes = { "int", "float", "bool" };
        public readonly string[] wdOptions = { "Auto-Detect", "Off", "On" };

        private VRCAvatarDescriptor avatarDescriptor;
        private AnimatorController avatarFXLayer;
        private VRCExpressionParameters expressionParameters;

        private List<MemoryOptimizerMain.MemoryOptimizerListData> boolsToOptimize = new List<MemoryOptimizerMain.MemoryOptimizerListData>();
        private int selectedBools;
        private List<MemoryOptimizerMain.MemoryOptimizerListData> intsNFloatsToOptimize = new List<MemoryOptimizerMain.MemoryOptimizerListData>();
        private int selectedIntsNFloats;
        private List<MemoryOptimizerMain.MemoryOptimizerListData> paramList;

        private Vector2 scollPosition;
        private int maxSyncSteps = 1;
        private int syncSteps = 1;
        private float stepDelay = 0.2f;
        private int newParamCost;
        private bool runOnce;
        private int longestParamName;
        private int wdOptionSelected = 0;
        private bool changeCheckEnabled = false;

        [MenuItem(menuPath)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow(typeof(MemoryOptimizerWindow), false, "Memory Optimizer", true);
        }

        private void OnGUI()
        {
            GUILayout.Space(5);
            using (new SqueezeScope((0, 0, Vertical, EditorStyles.helpBox)))
            {
                using (new SqueezeScope((0, 0, Vertical, EditorStyles.helpBox)))
                {
                    using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                    {
                        void OnAvatarChange()
                        {
                            if (avatarDescriptor)
                            {
                                avatarFXLayer = FindFXLayer(avatarDescriptor);
                                expressionParameters = FindExpressionParams(avatarDescriptor);
                            }
                            else
                            {
                                avatarFXLayer = null; 
                                expressionParameters = null;
                            }

                            OnChangeUpdate();
                        }
                        using (new ChangeCheckScope(OnAvatarChange))
                        {
                            avatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", avatarDescriptor, typeof(VRCAvatarDescriptor), true);
                            if (avatarDescriptor == null) { if (GUILayout.Button("Auto-detect")) { FillAvatarFields(null, avatarFXLayer, expressionParameters); } }
                        }
                    }

                    using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                    {
                        avatarFXLayer = (AnimatorController)EditorGUILayout.ObjectField("FX layer", avatarFXLayer, typeof(AnimatorController), true);
                        if (avatarFXLayer == null) { if (GUILayout.Button("Auto-detect")) { FillAvatarFields(avatarDescriptor, null, expressionParameters); } }
                    }

                    using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                    {
                        expressionParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField("Parameters", expressionParameters, typeof(VRCExpressionParameters), true);
                        if (expressionParameters == null) { if (GUILayout.Button("Auto-detect")) { FillAvatarFields(avatarDescriptor, avatarFXLayer, null); } }
                    }

                    if (!runOnce)
                    {
                        FillAvatarFields(null, null, null);
                        runOnce = true;
                    }

                    GUILayout.Space(5);

                    
                    using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                    {
                        EditorGUILayout.LabelField("Write defaults: ", EditorStyles.boldLabel);
                        wdOptionSelected = EditorGUILayout.Popup(wdOptionSelected, wdOptions, new GUIStyle(EditorStyles.popup) { fixedHeight = 18, stretchWidth = false });
                    }

                    using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                    {
                        EditorGUILayout.LabelField("Change check: ", EditorStyles.boldLabel);
                        if (syncSteps < 3)
                        {
                            changeCheckEnabled = false;
                            GUI.enabled = false;
                            GUI.backgroundColor = Color.red;
                            GUILayout.Button("Off", GUILayout.Width(203));
                            GUI.backgroundColor = Color.white;
                            GUI.enabled = true;
                        }

                        else if (changeCheckEnabled)
                        {
                            GUI.backgroundColor = Color.green;
                            if (GUILayout.Button("On", GUILayout.Width(203))) { changeCheckEnabled = !changeCheckEnabled; }
                            GUI.backgroundColor = Color.white;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.red;
                            if (GUILayout.Button("Off", GUILayout.Width(203))) { changeCheckEnabled = !changeCheckEnabled; }
                            GUI.backgroundColor = Color.white;
                        }
                    }
                    
                    
                    
                    
                    GUILayout.Space(5);
                }

                GUILayout.Space(5);

                if (avatarDescriptor != null && avatarFXLayer != null && expressionParameters != null)
                {
                    longestParamName = 0;
                    foreach (var x in expressionParameters.parameters) { longestParamName = Math.Max(longestParamName, x.name.Count()); }

                    using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                    {
                        EditorGUILayout.LabelField("Avatar parameters: ", EditorStyles.boldLabel);

                        if (GUILayout.Button("Select All")) 
                        {
                            foreach (MemoryOptimizerMain.MemoryOptimizerListData param in paramList) param.selected = true;
                            OnChangeUpdate();
                        }

                        if (GUILayout.Button("Clear selected parameters")) { ResetParamSelection(); }
                    }
                    
                    scollPosition = GUILayout.BeginScrollView(scollPosition, false, true);

                    for (int i = 0; i < expressionParameters.parameters.Length; i++)
                    {
                        using (new SqueezeScope((0, 0, Horizontal)))
                        {
                            //make sure the param list is always the same size as avatar's expression parameters
                            if (paramList == null || paramList.Count != expressionParameters.parameters.Length) { ResetParamSelection(); }
                            using (new SqueezeScope((0, 0, Horizontal)))
                            {
                                GUI.enabled = false;

                                EditorGUILayout.TextArea(expressionParameters.parameters[i].name, GUILayout.MinWidth(longestParamName * 8));
                                EditorGUILayout.Popup((int)expressionParameters.parameters[i].valueType, paramTypes, GUILayout.Width(50));
                                //EditorGUILayout.Toggle(avatarDescriptor.expressionParameters.parameters[i].networkSynced, GUILayout.MaxWidth(15));
                                GUI.enabled = true;

                                //System already installed
                                if (MemoryOptimizerMain.FindInstallation(avatarFXLayer))
                                {
                                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1);
                                    GUI.enabled = false;
                                    GUILayout.Button("System already installed!", GUILayout.Width(203));
                                }
                                //Param isn't network synced
                                else if (!expressionParameters.parameters[i].networkSynced)
                                {
                                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1);
                                    GUI.enabled = false;
                                    GUILayout.Button("Param not synced", GUILayout.Width(203));
                                }
                                //Param isn't in FX layer
                                else if (!(avatarFXLayer.parameters.Where(x => x.name == expressionParameters.parameters[i].name).Count() > 0))
                                {
                                    paramList[i].selected = false;
                                    GUI.backgroundColor = Color.yellow;
                                    if (GUILayout.Button("Add to FX", GUILayout.Width(100))) { avatarFXLayer.AddUniqueParam(paramList[i].param.name, paramList[i].param.valueType.ValueTypeToParamType()); }
                                    GUI.enabled = false;
                                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1);
                                    GUILayout.Button("Param not in FX", GUILayout.Width(100));
                                }
                                //Param isn't selected
                                else if (!paramList[i].selected)
                                {
                                    GUI.backgroundColor = Color.red;
                                    using (new ChangeCheckScope(OnChangeUpdate))
                                    {
                                        if (GUILayout.Button("Optimize", GUILayout.Width(203))) { paramList[i].selected = !paramList[i].selected; }
                                    }
                                }
                                //Param won't be optimized
                                else if (!paramList[i].willBeOptimized)
                                {
                                    GUI.backgroundColor = Color.yellow;
                                    using (new ChangeCheckScope(OnChangeUpdate))
                                    {
                                        if (GUILayout.Button("Optimize", GUILayout.Width(203))) { paramList[i].selected = !paramList[i].selected; }
                                    }
                                }
                                //Param will be optimized
                                else
                                {
                                    GUI.backgroundColor = Color.green;
                                    using (new ChangeCheckScope(OnChangeUpdate))
                                    {
                                        if (GUILayout.Button("Optimize", GUILayout.Width(203))) { paramList[i].selected = !paramList[i].selected; }
                                    }
                                }
                                GUI.enabled = true;
                            }

                        }

                        GUI.backgroundColor = Color.white;

                    }

                    GUILayout.EndScrollView();

                    using (new SqueezeScope((0, 0, EditorH)))
                    {
                        LabelWithHelpBox("Selected bools:  " + selectedBools);
                        LabelWithHelpBox("Selected ints and floats:  " + selectedIntsNFloats);
                    }

                    using (new SqueezeScope((0, 0, EditorH)))
                    {
                        LabelWithHelpBox("Original param cost: " + (selectedBools + (selectedIntsNFloats * 8)));
                        LabelWithHelpBox("New param cost: " + newParamCost);
                        LabelWithHelpBox("Amount you will save:  " + (selectedBools + (selectedIntsNFloats * 8) - newParamCost));
                        LabelWithHelpBox("Total sync time:  " + syncSteps * stepDelay + "s");
                    }
                    using (new SqueezeScope((0, 0, EditorH, EditorStyles.helpBox)))
                    {
                        if (maxSyncSteps < 2)
                        {
                            GUI.backgroundColor = Color.red;
                            GUILayout.Label("Too few parameters selected!", EditorStyles.boldLabel);
                            GUI.enabled = false;
                            EditorGUILayout.IntSlider(syncSteps, 0, 0);
                        }
                        else
                        {
                            GUILayout.Label("Syncing Steps", GUILayout.MaxWidth(100));
                            using (new ChangeCheckScope(OnChangeUpdate))
                            {
                                syncSteps = EditorGUILayout.IntSlider(syncSteps, 2, maxSyncSteps);
                            }
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    /*
                    using (new SqueezeScope((0, 0, EditorH, EditorStyles.helpBox)))
                    {
                        GUILayout.Label("Step Delay", GUILayout.MaxWidth(100));
                        stepDelay = EditorGUILayout.Slider(stepDelay, 0.1f, 0.5f);
                    }
                    */
                    GUI.enabled = true;
                }

                if (syncSteps > maxSyncSteps) { syncSteps = maxSyncSteps; }

                if (MemoryOptimizerMain.FindInstallation(avatarFXLayer))
                {
                    if (GUILayout.Button("Uninstall")) MemoryOptimizerMain.UninstallMemOpt(avatarDescriptor, avatarFXLayer, expressionParameters);
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("Uninstall");
                    GUI.enabled = true;
                }

                if (syncSteps < 2)
                {
                    GUI.enabled = false;
                    GUI.backgroundColor = Color.red;
                    GUILayout.Button("Select More Parameters!");
                }
                else if (!avatarDescriptor)
                {
                    GUI.enabled = false;
                    GUI.backgroundColor = Color.red;
                    GUILayout.Button("No avatar selected!");
                }
                else if (!avatarFXLayer)
                {
                    GUI.enabled = false;
                    GUI.backgroundColor = Color.red;
                    GUILayout.Button("No FX layer selected!");
                }
                else if (MemoryOptimizerMain.FindInstallation(avatarFXLayer))
                {
                    GUI.enabled = false;
                    GUI.backgroundColor = Color.red;
                    GUILayout.Button("System already installed! Please uninstall the current system");
                }
                else
                {
                    if (GUILayout.Button("Install")) MemoryOptimizerMain.InstallMemOpt(avatarDescriptor, avatarFXLayer, expressionParameters, boolsToOptimize, intsNFloatsToOptimize, syncSteps, stepDelay, changeCheckEnabled, wdOptionSelected, mainSavePath);
                }
                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
        }

        public void OnChangeUpdate()
        {
            if (paramList != null) foreach (MemoryOptimizerMain.MemoryOptimizerListData param in paramList) { param.willBeOptimized = false; }

            boolsToOptimize = paramList.FindAll(x => x.selected && x.param.valueType == VRCExpressionParameters.ValueType.Bool);
            selectedBools = boolsToOptimize.Count();
            boolsToOptimize = boolsToOptimize.Take(boolsToOptimize.Count() - (boolsToOptimize.Count() % syncSteps)).ToList();

            intsNFloatsToOptimize = paramList.FindAll(x => x.selected && (x.param.valueType == VRCExpressionParameters.ValueType.Int || x.param.valueType == VRCExpressionParameters.ValueType.Float));
            selectedIntsNFloats = intsNFloatsToOptimize.Count();
            intsNFloatsToOptimize = intsNFloatsToOptimize.Take(intsNFloatsToOptimize.Count() - (intsNFloatsToOptimize.Count() % syncSteps)).ToList();

            maxSyncSteps = new[] { selectedBools, selectedIntsNFloats }.Max();
            if (maxSyncSteps < 1) { maxSyncSteps = 1; }
            if (maxSyncSteps > 1 && syncSteps < 2) { syncSteps = 2; }

            if (syncSteps < 2) { newParamCost = selectedBools + (selectedIntsNFloats * 8); }
            else
            {
                foreach (MemoryOptimizerMain.MemoryOptimizerListData x in boolsToOptimize) { x.willBeOptimized = true; }
                foreach (MemoryOptimizerMain.MemoryOptimizerListData x in intsNFloatsToOptimize) { x.willBeOptimized = true; }

                int syncBitCost = (syncSteps - 1).DecimalToBinary().ToString().Count();

                newParamCost = (boolsToOptimize.Count / syncSteps) + (intsNFloatsToOptimize.Count / syncSteps * 8) + syncBitCost + (selectedBools - boolsToOptimize.Count) + ((selectedIntsNFloats - intsNFloatsToOptimize.Count) * 8);
            }
        }

        public void ResetParamSelection()
        {
            paramList = new List<MemoryOptimizerMain.MemoryOptimizerListData>();
            foreach (VRCExpressionParameters.Parameter param in expressionParameters.parameters) { paramList.Add(new MemoryOptimizerMain.MemoryOptimizerListData(param, false, false)); }
            maxSyncSteps = 1;
            syncSteps = 1;
            OnChangeUpdate();
        }

        
        public void FillAvatarFields(VRCAvatarDescriptor descriptor, AnimatorController controller, VRCExpressionParameters parameters)
        {
            if (descriptor == null) avatarDescriptor = FindObjectOfType<VRCAvatarDescriptor>(); 
            if (controller == null) avatarFXLayer = FindFXLayer(avatarDescriptor);
            if (parameters == null) expressionParameters = FindExpressionParams(avatarDescriptor);

            OnChangeUpdate();
        }

        public class ChangeCheckScope : IDisposable
        {
            public Action callBack;

            public ChangeCheckScope(Action callBack)
            {
                this.callBack = callBack;
                EditorGUI.BeginChangeCheck();
            }

            public void Dispose()
            {
                if (EditorGUI.EndChangeCheck())
                {
                    callBack();
                }
            }
        }

        public class SqueezeScope : IDisposable
        {
            private readonly SqueezeSettings[] settings;

            public enum SqueezeScopeType
            {
                Horizontal,
                Vertical,
                EditorH,
                EditorV
            }

            public SqueezeScope(SqueezeSettings input) : this(new[] { input }) { } 
          
            public SqueezeScope(params SqueezeSettings[] input)
            {
                settings = input;
                foreach (var squeezeSettings in input)
                {
                    BeginSqueeze(squeezeSettings);
                }
            }

            private void BeginSqueeze(SqueezeSettings squeezeSettings)
            {
                switch (squeezeSettings.type)
                {
                    case Horizontal: GUILayout.BeginHorizontal(squeezeSettings.style); break;
                    case Vertical: GUILayout.BeginVertical(squeezeSettings.style); break;
                    case EditorH: EditorGUILayout.BeginHorizontal(squeezeSettings.style); break;
                    case EditorV: EditorGUILayout.BeginVertical(squeezeSettings.style); break;
                }

                GUILayout.Space(squeezeSettings.width1);
            }

            public void Dispose()
            {
                foreach (var squeezeSettings in settings.Reverse())
                {
                    GUILayout.Space(squeezeSettings.width2);
                    switch (squeezeSettings.type)
                    {
                        case Horizontal: GUILayout.EndHorizontal(); break;
                        case Vertical: GUILayout.EndVertical(); break;
                        case EditorH: EditorGUILayout.EndHorizontal(); break;
                        case EditorV: EditorGUILayout.EndVertical(); break;
                    }
                }
            }
        }

        public struct SqueezeSettings
        {
            public int width1;
            public int width2;
            public SqueezeScopeType type;
            public GUIStyle style;

            public static implicit operator SqueezeSettings((int, int) val)
            {
                return new SqueezeSettings { width1 = val.Item1, width2 = val.Item2, type = Horizontal, style = GUIStyle.none };
            }
            public static implicit operator SqueezeSettings((int, int, SqueezeScope.SqueezeScopeType) val)
            {
                return new SqueezeSettings { width1 = val.Item1, width2 = val.Item2, type = val.Item3, style = GUIStyle.none };
            }

            public static implicit operator SqueezeSettings((int, int, SqueezeScope.SqueezeScopeType, GUIStyle) val)
            {
                return new SqueezeSettings { width1 = val.Item1, width2 = val.Item2, type = val.Item3, style = val.Item4 };
            }
        }
    }
}
