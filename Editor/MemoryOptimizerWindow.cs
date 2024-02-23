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

        private const string editorPrefKey = "Mem_Opt_Pref_";
        private const string unlockSyncStepsEPKey = editorPrefKey + "unlockSyncSteps";
        private const string backUpModeEPKey = editorPrefKey + "BackUpMode";

        private bool unlockSyncSteps = false;
        private int backupMode = 0;

        private readonly string[] paramTypes = { "Int", "Float", "Bool" };
        public readonly string[] wdOptions = { "Auto-Detect", "Off", "On" };
        public readonly string[] backupModes = { "Auto", "Off", "On" };

        private int menuNumber = 0;
        private Vector2 scrollPosition;
        private bool runOnce;

        private VRCAvatarDescriptor avatarDescriptor;
        private AnimatorController avatarFXLayer;
        private VRCExpressionParameters expressionParameters;

        private List<MemoryOptimizerMain.MemoryOptimizerListData> boolsToOptimize = new List<MemoryOptimizerMain.MemoryOptimizerListData>();
        private int selectedBools;
        private List<MemoryOptimizerMain.MemoryOptimizerListData> intsNFloatsToOptimize = new List<MemoryOptimizerMain.MemoryOptimizerListData>();
        private int selectedIntsNFloats;
        private List<MemoryOptimizerMain.MemoryOptimizerListData> paramList;

        private int maxSyncSteps = 1;
        private int syncSteps = 1;
        private float stepDelay = 0.2f;
        private int newParamCost;
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
            using (new SqueezeScope((0, 0, Horizontal)))
            {
                if (GUILayout.Button("Install menu"))
                    menuNumber = 0;
                if (GUILayout.Button("Settings menu"))
                    menuNumber = 1;
            }
            GUILayout.Space(5);

            if (menuNumber == 0)
            {
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
                                if (avatarDescriptor == null)
                                    if (GUILayout.Button("Auto-detect"))
                                        FillAvatarFields(null, avatarFXLayer, expressionParameters);
                            }
                        }

                        using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                        {
                            avatarFXLayer = (AnimatorController)EditorGUILayout.ObjectField("FX Layer", avatarFXLayer, typeof(AnimatorController), true);
                            if (avatarFXLayer == null)
                                if (GUILayout.Button("Auto-Detect"))
                                    FillAvatarFields(avatarDescriptor, null, expressionParameters);
                        }

                        using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                        {
                            expressionParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField("Parameters", expressionParameters, typeof(VRCExpressionParameters), true);
                            if (expressionParameters == null)
                                if (GUILayout.Button("Auto-Detect"))
                                    FillAvatarFields(avatarDescriptor, avatarFXLayer, null);
                        }

                        if (!runOnce)
                        {
                            FillAvatarFields(null, null, null);
                            runOnce = true;
                        }

                        GUILayout.Space(5);

                        using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                        {
                            EditorGUILayout.LabelField("Write Defaults: ", EditorStyles.boldLabel);
                            wdOptionSelected = EditorGUILayout.Popup(wdOptionSelected, wdOptions, new GUIStyle(EditorStyles.popup) { fixedHeight = 18, stretchWidth = false });
                        }

                        using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                        {
                            EditorGUILayout.LabelField("Change Check: ", EditorStyles.boldLabel);
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
                                if (GUILayout.Button("On", GUILayout.Width(203)))
                                    changeCheckEnabled = !changeCheckEnabled;

                                GUI.backgroundColor = Color.white;
                            }
                            else
                            {
                                GUI.backgroundColor = Color.red;
                                if (GUILayout.Button("Off", GUILayout.Width(203)))
                                    changeCheckEnabled = !changeCheckEnabled;

                                GUI.backgroundColor = Color.white;
                            }
                        }
                        GUILayout.Space(5);
                    }
                    GUILayout.Space(5);
                    if (avatarDescriptor != null && avatarFXLayer != null && expressionParameters != null)
                    {
                        longestParamName = 0;
                        foreach (var x in expressionParameters.parameters)
                            longestParamName = Math.Max(longestParamName, x.name.Count());

                        using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                        {
                            EditorGUILayout.LabelField("Avatar Parameters: ", EditorStyles.boldLabel);

                            if (GUILayout.Button("Deselect Prefix"))
                            {
                                EditorInputDialog.Show("", "Please enter your prefix to deselect", "", name =>
                                {
                                    if (!string.IsNullOrEmpty(name))
                                        foreach (MemoryOptimizerMain.MemoryOptimizerListData param in paramList.FindAll(x => x.param.name.StartsWith(name, true, null))) param.selected = false;
                                });

                                OnChangeUpdate();
                            }

                            if (GUILayout.Button("Select All"))
                            {
                                foreach (MemoryOptimizerMain.MemoryOptimizerListData param in paramList) param.selected = true;
                                OnChangeUpdate();
                            }

                            if (GUILayout.Button("Clear Selected Parameters"))
                                ResetParamSelection();
                        }

                        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

                        for (int i = 0; i < expressionParameters.parameters.Length; i++)
                        {
                            using (new SqueezeScope((0, 0, Horizontal)))
                            {
                                //make sure the param list is always the same size as avatar's expression parameters
                                if (paramList == null || paramList.Count != expressionParameters.parameters.Length)
                                    ResetParamSelection();

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
                                        GUILayout.Button("System Already Installed!", GUILayout.Width(203));
                                    }
                                    //Param isn't network synced
                                    else if (!expressionParameters.parameters[i].networkSynced)
                                    {
                                        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1);
                                        GUI.enabled = false;
                                        GUILayout.Button("Param Not Synced", GUILayout.Width(203));
                                    }
                                    //Param isn't in FX layer
                                    else if (!(avatarFXLayer.parameters.Count(x => x.name == expressionParameters.parameters[i].name) > 0))
                                    {
                                        paramList[i].selected = false;
                                        GUI.backgroundColor = Color.yellow;
                                        if (GUILayout.Button("Add To FX", GUILayout.Width(100)))
                                            avatarFXLayer.AddUniqueParam(paramList[i].param.name);

                                        GUI.enabled = false;
                                        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1);
                                        GUILayout.Button("Param Not In FX", GUILayout.Width(100));
                                    }
                                    //Param isn't selected
                                    else if (!paramList[i].selected)
                                    {
                                        GUI.backgroundColor = Color.red;
                                        using (new ChangeCheckScope(OnChangeUpdate))
                                            if (GUILayout.Button("Optimize", GUILayout.Width(203)))
                                                paramList[i].selected = !paramList[i].selected;
                                    }
                                    //Param won't be optimized
                                    else if (!paramList[i].willBeOptimized)
                                    {
                                        GUI.backgroundColor = Color.yellow;
                                        using (new ChangeCheckScope(OnChangeUpdate))
                                            if (GUILayout.Button("Optimize", GUILayout.Width(203)))
                                                paramList[i].selected = !paramList[i].selected;
                                    }
                                    //Param will be optimized
                                    else
                                    {
                                        GUI.backgroundColor = Color.green;
                                        using (new ChangeCheckScope(OnChangeUpdate))
                                            if (GUILayout.Button("Optimize", GUILayout.Width(203)))
                                                paramList[i].selected = !paramList[i].selected;
                                    }
                                    GUI.enabled = true;
                                }
                            }
                            GUI.backgroundColor = Color.white;
                        }
                        GUILayout.EndScrollView();

                        using (new SqueezeScope((0, 0, EditorH)))
                        {
                            LabelWithHelpBox("Selected Bools:  " + selectedBools);
                            LabelWithHelpBox("Selected Ints and Floats:  " + selectedIntsNFloats);
                        }

                        using (new SqueezeScope((0, 0, EditorH)))
                        {
                            LabelWithHelpBox("Original Param Cost:  " + (selectedBools + (selectedIntsNFloats * 8)));
                            LabelWithHelpBox("New Param Cost:  " + newParamCost);
                            LabelWithHelpBox("Amount You Will Save:  " + (selectedBools + (selectedIntsNFloats * 8) - newParamCost));
                            LabelWithHelpBox("Total Sync Time:  " + syncSteps * stepDelay + "s");
                        }

                        using (new SqueezeScope((0, 0, EditorH, EditorStyles.helpBox)))
                        {
                            if (MemoryOptimizerMain.FindInstallation(avatarFXLayer))
                            {
                                GUI.backgroundColor = Color.black;
                                GUILayout.Label("System Already Installed!", EditorStyles.boldLabel);
                                GUI.enabled = false;
                                EditorGUILayout.IntSlider(syncSteps, 0, 0);
                            }
                            else if (maxSyncSteps < 2)
                            {
                                GUI.backgroundColor = Color.red;
                                GUILayout.Label("Too Few Parameters Selected!", EditorStyles.boldLabel);
                                GUI.enabled = false;
                                EditorGUILayout.IntSlider(syncSteps, 0, 0);
                            }
                            else
                            {
                                GUILayout.Label("Syncing Steps", GUILayout.MaxWidth(100));
                                using (new ChangeCheckScope(OnChangeUpdate))
                                    syncSteps = EditorGUILayout.IntSlider(syncSteps, 2, unlockSyncSteps ? Math.Max(maxSyncSteps, 4) : maxSyncSteps);
                            }
                            GUI.backgroundColor = Color.white;
                        }
                        GUI.enabled = true;
                    }
                    if (syncSteps > maxSyncSteps)
                        syncSteps = maxSyncSteps;

                    if (MemoryOptimizerMain.FindInstallation(avatarFXLayer))
                        if (GUILayout.Button("Uninstall"))
                            MemoryOptimizerMain.UninstallMemOpt(avatarDescriptor, avatarFXLayer, expressionParameters);
                    else
                    {
                        GUI.enabled = false;
                        GUILayout.Button("Uninstall");
                        GUI.enabled = true;
                    }

                    if (MemoryOptimizerMain.FindInstallation(avatarFXLayer))
                    {
                        GUI.enabled = false;
                        GUI.backgroundColor = Color.black;
                        GUILayout.Button("System Already Installed! Please Uninstall Before Reinstalling.");
                    }
                    else if (syncSteps < 2)
                    {
                        GUI.enabled = false;
                        GUI.backgroundColor = Color.red;
                        GUILayout.Button("Select More Parameters!");
                    }
                    else if (!avatarDescriptor)
                    {
                        GUI.enabled = false;
                        GUI.backgroundColor = Color.red;
                        GUILayout.Button("No Avatar Selected!");
                    }
                    else if (!avatarFXLayer)
                    {
                        GUI.enabled = false;
                        GUI.backgroundColor = Color.red;
                        GUILayout.Button("No FX Layer Selected!");
                    }
                    else
                        if (GUILayout.Button("Install"))
                            MemoryOptimizerMain.InstallMemOpt(avatarDescriptor, avatarFXLayer, expressionParameters, boolsToOptimize, intsNFloatsToOptimize, syncSteps, stepDelay, changeCheckEnabled, wdOptionSelected, mainSavePath);
                }
            }
            else if (menuNumber == 1)
            {
                unlockSyncSteps = EditorPrefs.GetBool(unlockSyncStepsEPKey);
                backupMode = EditorPrefs.GetInt(backUpModeEPKey);

                using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                {
                    EditorGUILayout.LabelField("Backup Mode: ", EditorStyles.boldLabel);
                    backupMode = EditorGUILayout.Popup(backupMode, backupModes, new GUIStyle(EditorStyles.popup) { fixedHeight = 18, stretchWidth = false });
                }

                if (unlockSyncSteps)
                    GUI.backgroundColor = Color.green;
                else 
                    GUI.backgroundColor = Color.red;
                using (new SqueezeScope((0, 0, Horizontal, EditorStyles.helpBox)))
                    if (GUILayout.Button("Unlock sync steps"))
                        EditorPrefs.SetBool(unlockSyncStepsEPKey, !unlockSyncSteps);

                GUI.backgroundColor = Color.white;
            }
            else 
                menuNumber = 0;

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        public void OnChangeUpdate()
        {
            if (paramList == null) ResetParamSelection();

            foreach (MemoryOptimizerMain.MemoryOptimizerListData param in paramList)
            {
                param.willBeOptimized = false;
                if (!param.param.networkSynced || !(avatarFXLayer.parameters.Count(x => x.name == param.param.name) > 0))
                    param.selected = false;
            }

            boolsToOptimize = paramList.FindAll(x => x.selected && x.param.valueType == VRCExpressionParameters.ValueType.Bool);
            selectedBools = boolsToOptimize.Count();
            intsNFloatsToOptimize = paramList.FindAll(x => x.selected && (x.param.valueType == VRCExpressionParameters.ValueType.Int || x.param.valueType == VRCExpressionParameters.ValueType.Float));
            selectedIntsNFloats = intsNFloatsToOptimize.Count();

            newParamCost = selectedBools + (selectedIntsNFloats * 8);

            maxSyncSteps = Math.Max(Math.Max(selectedBools, selectedIntsNFloats), 1);
            if (maxSyncSteps == 1)
                return;
            if (syncSteps < 2)
                syncSteps = 2;

            boolsToOptimize = boolsToOptimize.Take(selectedBools - (selectedBools % syncSteps)).ToList();
            intsNFloatsToOptimize = intsNFloatsToOptimize.Take(selectedIntsNFloats - (selectedIntsNFloats % syncSteps)).ToList();
            
            foreach (MemoryOptimizerMain.MemoryOptimizerListData param in boolsToOptimize)
                param.willBeOptimized = true;
            foreach (MemoryOptimizerMain.MemoryOptimizerListData param in intsNFloatsToOptimize)
                param.willBeOptimized = true;

            int syncBitCost = (syncSteps - 1).DecimalToBinary().ToString().Count();

            newParamCost = (boolsToOptimize.Count / syncSteps) + (intsNFloatsToOptimize.Count / syncSteps * 8) + syncBitCost + (selectedBools - boolsToOptimize.Count) + ((selectedIntsNFloats - intsNFloatsToOptimize.Count) * 8);
            
        }

        public void ResetParamSelection()
        {
            paramList = new List<MemoryOptimizerMain.MemoryOptimizerListData>();

            if (expressionParameters.parameters.Length > 0)
            {
                foreach (VRCExpressionParameters.Parameter param in expressionParameters.parameters)
                    paramList.Add(new MemoryOptimizerMain.MemoryOptimizerListData(param, false, false));
            }

            maxSyncSteps = 1;
            syncSteps = 1;
            OnChangeUpdate();
        }

        public void FillAvatarFields(VRCAvatarDescriptor descriptor, AnimatorController controller, VRCExpressionParameters parameters)
        {
            if (descriptor == null)
                avatarDescriptor = FindObjectOfType<VRCAvatarDescriptor>();
            else
            {
                if (controller == null)
                    avatarFXLayer = FindFXLayer(avatarDescriptor);
                if (parameters == null)
                    expressionParameters = FindExpressionParams(avatarDescriptor);
            }

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
                    callBack();
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

            public SqueezeScope(SqueezeSettings input) : this(new[] { input })
            {
            }

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
                    case Horizontal:
                        GUILayout.BeginHorizontal(squeezeSettings.style);
                        break;
                    case Vertical:
                        GUILayout.BeginVertical(squeezeSettings.style);
                        break;
                    case EditorH:
                        EditorGUILayout.BeginHorizontal(squeezeSettings.style);
                        break;
                    case EditorV:
                        EditorGUILayout.BeginVertical(squeezeSettings.style);
                        break;
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
                        case Horizontal:
                            GUILayout.EndHorizontal();
                            break;
                        case Vertical:
                            GUILayout.EndVertical();
                            break;
                        case EditorH:
                            EditorGUILayout.EndHorizontal();
                            break;
                        case EditorV:
                            EditorGUILayout.EndVertical();
                            break;
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

            public static implicit operator SqueezeSettings((int, int, SqueezeScopeType) val)
            {
                return new SqueezeSettings { width1 = val.Item1, width2 = val.Item2, type = val.Item3, style = GUIStyle.none };
            }

            public static implicit operator SqueezeSettings((int, int, SqueezeScopeType, GUIStyle) val)
            {
                return new SqueezeSettings { width1 = val.Item1, width2 = val.Item2, type = val.Item3, style = val.Item4 };
            }
        }

        //https://forum.unity.com/threads/is-there-a-way-to-input-text-using-a-unity-editor-utility.473743/#post-7191802
        //https://forum.unity.com/threads/is-there-a-way-to-input-text-using-a-unity-editor-utility.473743/#post-7229248
        //Thanks to JelleJurre for help
        public class EditorInputDialog : EditorWindow
        {
            string description, inputText;
            string okButton, cancelButton;
            bool initializedPosition = false;
            Action onOKButton;

            bool shouldClose = false;
            Vector2 maxScreenPos;

            #region OnGUI()
            void OnGUI()
            {
                // Check if Esc/Return have been pressed
                var e = Event.current;
                if (e.type == EventType.KeyDown)
                {
                    switch (e.keyCode)
                    {
                        // Escape pressed
                        case KeyCode.Escape:
                            shouldClose = true;
                            e.Use();
                            break;

                        // Enter pressed
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            onOKButton?.Invoke();
                            shouldClose = true;
                            e.Use();
                            break;
                    }
                }

                if (shouldClose)
                {  // Close this dialog
                    Close();
                    //return;
                }

                // Draw our control
                var rect = EditorGUILayout.BeginVertical();

                EditorGUILayout.Space(12);
                EditorGUILayout.LabelField(description);

                EditorGUILayout.Space(8);
                GUI.SetNextControlName("inText");
                inputText = EditorGUILayout.TextField("", inputText);
                GUI.FocusControl("inText");   // Focus text field
                EditorGUILayout.Space(12);

                // Draw OK / Cancel buttons
                var r = EditorGUILayout.GetControlRect();
                r.width /= 2;
                if (GUI.Button(r, okButton))
                {
                    onOKButton?.Invoke();
                    shouldClose = true;
                }

                r.x += r.width;
                if (GUI.Button(r, cancelButton))
                {
                    inputText = null;   // Cancel - delete inputText
                    shouldClose = true;
                }

                EditorGUILayout.Space(8);
                EditorGUILayout.EndVertical();

                // Force change size of the window
                if (rect.width != 0 && minSize != rect.size)
                {
                    minSize = maxSize = rect.size;
                }

                // Set dialog position next to mouse position
                if (!initializedPosition && e.type == EventType.Layout)
                {
                    initializedPosition = true;

                    // Move window to a new position. Make sure we're inside visible window
                    var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    mousePos.x += 32;
                    if (mousePos.x + position.width > maxScreenPos.x) mousePos.x -= position.width + 64; // Display on left side of mouse
                    if (mousePos.y + position.height > maxScreenPos.y) mousePos.y = maxScreenPos.y - position.height;

                    position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

                    // Focus current window
                    Focus();
                }
            }
            #endregion OnGUI()

            #region Show()
            /// <summary>
            /// Returns text player entered, or null if player cancelled the dialog.
            /// </summary>
            /// <param name="title"></param>
            /// <param name="description"></param>
            /// <param name="inputText"></param>
            /// <param name="okButton"></param>
            /// <param name="cancelButton"></param>
            /// <returns></returns>
            //public static string Show(string title, string description, string inputText, string okButton = "OK", string cancelButton = "Cancel")
            public static void Show(string title, string description, string inputText, Action<string> callBack, string okButton = "OK", string cancelButton = "Cancel")
            {
                // Make sure our popup is always inside parent window, and never offscreen
                // So get caller's window size
                var maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));

                if (EditorWindow.HasOpenInstances<EditorInputDialog>())
                    return;

                var window = CreateInstance<EditorInputDialog>();
                window.maxScreenPos = maxPos;
                window.titleContent = new GUIContent(title);
                window.description = description;
                window.inputText = inputText;
                window.okButton = okButton;
                window.cancelButton = cancelButton;
                window.onOKButton += () => callBack(window.inputText);
                window.ShowPopup();
            }
            #endregion Show()
        }
    }
}
