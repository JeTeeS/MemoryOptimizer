using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace JeTeeS.TES.HelperFunctions
{
    public static class TESHelperFunctions
    {
        public static AnimatorController FindFXLayer(VRCAvatarDescriptor descriptor)
        {
            return (AnimatorController)descriptor.baseAnimationLayers.Where(x => x.type == VRCAvatarDescriptor.AnimLayerType.FX && x.animatorController != null).FirstOrDefault().animatorController;
        }

        public static VRCExpressionParameters FindExpressionParams(VRCAvatarDescriptor descriptor)
        {
            if (descriptor == null) return null;
            return descriptor.expressionParameters;
        }

        public static AnimatorControllerParameterType ValueTypeToParamType(this VRCExpressionParameters.ValueType valueType)
        {
            switch (valueType)
            {
                case VRCExpressionParameters.ValueType.Float: return AnimatorControllerParameterType.Float;
                case VRCExpressionParameters.ValueType.Int: return AnimatorControllerParameterType.Int;
                case VRCExpressionParameters.ValueType.Bool: return AnimatorControllerParameterType.Bool;

                default: return AnimatorControllerParameterType.Float;
            }
        }
        public static VRCExpressionParameters.ValueType ParamTypeToValueType(this AnimatorControllerParameterType paramType)
        {
            switch (paramType)
            {
                case AnimatorControllerParameterType.Float: return VRCExpressionParameters.ValueType.Float;
                case AnimatorControllerParameterType.Int: return VRCExpressionParameters.ValueType.Int;
                case AnimatorControllerParameterType.Bool: return VRCExpressionParameters.ValueType.Bool;

                default: return VRCExpressionParameters.ValueType.Float;
            }
        }

        public static string GetControllerParentFolder(AnimatorController controller)
        {
            List<string> subPaths = controller.GetAssetPath().Split(@"\/".ToCharArray()).ToList();
            subPaths.RemoveAt(subPaths.Count - 1);
            string returnString = "";
            foreach (string subPath in subPaths) returnString += subPath + "/";
            return returnString;
        }
        public static Vector3 AngleRadiusToPos(float angle, float radius, Vector3 offset)
        {
            Vector3 result = new Vector3((float)Math.Sin(angle) * radius, (float)Math.Cos(angle) * radius, 0);
            result += offset;

            return result;
        }

        public static void LabelWithHelpBox(string text)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(text);
            GUILayout.EndVertical();
        }

        public static int FindLayerIndex(this AnimatorController controller, AnimatorControllerLayer layer)
        {
            for (int i = 0; i < controller.layers.Count(); i++)
            {
                if (controller.layers[i].stateMachine == layer.stateMachine)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void RemoveLayer(this AnimatorController controller, AnimatorControllerLayer layer)
        {
            int i = controller.FindLayerIndex(layer);
            if (i == -1)
            {
                Debug.LogError("Layer " + layer.name + "was not found in " + controller.name);
                return;
            }
            controller.RemoveLayer(i);
        }

        public static List<ChildAnimatorState> FindAllStatesInLayer(this AnimatorControllerLayer layer)
        {
            List<ChildAnimatorState> returnList = new List<ChildAnimatorState>();

            Queue<AnimatorStateMachine> stateMachines = new Queue<AnimatorStateMachine>();
            stateMachines.Enqueue(layer.stateMachine);
            while (stateMachines.Count > 0)
            {
                AnimatorStateMachine currentStateMachine = stateMachines.Dequeue();
                foreach (ChildAnimatorState state in currentStateMachine.states) returnList.Add(state);
                foreach (ChildAnimatorStateMachine stateMachine in currentStateMachine.stateMachines) stateMachines.Enqueue(stateMachine.stateMachine);
            }

            return returnList;
        }

        public static int FindWDInLayer(this AnimatorControllerLayer layer)
        {
            Queue<ChildAnimatorState> stateQueue = new Queue<ChildAnimatorState>();
            bool firstWD;
            foreach (var state in layer.FindAllStatesInLayer())
            {
                stateQueue.Enqueue(state);
            }
            ChildAnimatorState currentState = stateQueue.Dequeue();
            firstWD = currentState.state.writeDefaultValues;
            while (stateQueue.Count > 1)
            {
                currentState = stateQueue.Dequeue();

                if (currentState.state.writeDefaultValues != firstWD && !currentState.state.name.Contains("WD On")) return -1;
            }

            return Convert.ToInt32(firstWD);
        }

        public static int FindWDInController(this AnimatorController controller)
        {
            Queue<AnimatorControllerLayer> layerQueue = new Queue<AnimatorControllerLayer>();
            AnimatorControllerLayer currentLayer = null;
            int firstWD = -2;
            foreach (var layer in controller.layers)
            {
                layerQueue.Enqueue(layer);
            }

            currentLayer = layerQueue.Dequeue();
            while (currentLayer.IsBlendTreeLayer())
            {
                currentLayer = layerQueue.Dequeue();
            }

            firstWD = currentLayer.FindWDInLayer();
            if (firstWD == -1) return -1;

            while (layerQueue.Count > 1)
            {
                currentLayer = layerQueue.Dequeue();
                if (currentLayer.FindWDInLayer() != firstWD) return -1;
            }

            return Convert.ToInt32(firstWD);
        }

        public static bool IsBlendTreeLayer(this AnimatorControllerLayer layer)
        {
            if (layer == null || layer.stateMachine == null) return false;
            foreach (var state in layer.stateMachine.states)
            {
                if (!state.state.name.Contains("WD On")) return false;
            }
            return true;
        }

        public static AnimatorControllerParameter AddUniqueParam(this AnimatorController controller, string paramName, AnimatorControllerParameterType paramType = AnimatorControllerParameterType.Float, float defualtValue = 0)
        {
            foreach (AnimatorControllerParameter param in controller.parameters)
            {
                if (param.name == paramName)
                {
                    if (param.type != paramType) Debug.LogError("Paramter " + param.name + " is of type: " + param.type.ToString() + " not " + paramType.ToString() + "!");
                    return param;
                }
            }
            AnimatorControllerParameter controllerParam = new AnimatorControllerParameter();
            if (paramType == AnimatorControllerParameterType.Float) controllerParam = new AnimatorControllerParameter() { name = paramName, type = paramType, defaultFloat = defualtValue };
            else if (paramType == AnimatorControllerParameterType.Int) controllerParam = new AnimatorControllerParameter() { name = paramName, type = paramType, defaultInt = ((int)defualtValue) };
            else if (paramType == AnimatorControllerParameterType.Bool)
            {
                if (defualtValue > 0) controllerParam = new AnimatorControllerParameter() { name = paramName, type = paramType, defaultBool = true };
                else controllerParam = new AnimatorControllerParameter() { name = paramName, type = paramType, defaultBool = false };
            }
            else Debug.LogError("Paramter " + paramName + " is not a float, int or bool??");

            controller.AddParameter(controllerParam);
            return controller.parameters.Last(x => x.name == paramName && x.type == paramType);
        }

        public static void AddUniqueSyncedParam(this VRCExpressionParameters vrcExpressionParameters, string name, VRCExpressionParameters.ValueType valueType, bool isNetworkSynced = true, bool isSaved = true, float defaultValue = 0)
        {
            foreach (VRCExpressionParameters.Parameter param in vrcExpressionParameters.parameters)
            {
                if (param.name == name)
                {
                    if (param.valueType != valueType) Debug.LogError("Paramter " + param.name + " is not of type: " + param.valueType.ToString() + "!");
                    return;
                }
            }
            VRCExpressionParameters.Parameter[] newList = new VRCExpressionParameters.Parameter[vrcExpressionParameters.parameters.Length + 1];
            for (int i = 0; i < vrcExpressionParameters.parameters.Length; i++) newList[i] = vrcExpressionParameters.parameters[i];
            newList[newList.Length - 1] = new VRCExpressionParameters.Parameter() { name = name, valueType = valueType, networkSynced = isNetworkSynced, saved = isSaved, defaultValue = defaultValue };
            vrcExpressionParameters.parameters = newList;
        }

        public static void AddUniqueSyncedParamToController(string name, AnimatorController controller, VRCExpressionParameters vrcExpressionParameters, AnimatorControllerParameterType paramType = AnimatorControllerParameterType.Float, VRCExpressionParameters.ValueType valueType = VRCExpressionParameters.ValueType.Float)
        {
            controller.AddUniqueParam(name, paramType);
            vrcExpressionParameters.AddUniqueSyncedParam(name, valueType);
        }

        public static AnimatorControllerLayer AddLayer(this AnimatorController controller, string name, float defaultWeight = 1)
        {
            AnimatorControllerLayer layer = new AnimatorControllerLayer()
            {
                name = name,
                defaultWeight = defaultWeight,
                stateMachine = new AnimatorStateMachine() { hideFlags = HideFlags.HideInHierarchy },
            };
            controller.AddLayer(layer);
            return layer;
        }

        public static void AddHiddenIdentifier(this AnimatorStateMachine animatorStateMachine, string identifierString)
        {
            AnimatorStateTransition identifier = animatorStateMachine.AddAnyStateTransition((AnimatorStateMachine)null);
            identifier.canTransitionToSelf = false;
            identifier.mute = true;
            identifier.isExit = true;
            identifier.name = identifierString;
        }

        public static List<AnimatorControllerLayer> FindHiddenIdentifier(this AnimatorController animatorController, string identifierString)
        {
            if (animatorController == null) return null;
            List<AnimatorControllerLayer> returnList = new List<AnimatorControllerLayer>();

            foreach (AnimatorControllerLayer layer in animatorController.layers)
            {
                foreach (AnimatorStateTransition anyStateTransition in layer.stateMachine.anyStateTransitions)
                {
                    if (anyStateTransition.isExit && anyStateTransition.mute && anyStateTransition.name == identifierString)
                    {
                        returnList.Add(layer);
                    }
                }
            }
            return returnList;
        }
        public static void ReadyPath(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
                AssetDatabase.ImportAsset(path);
            }
            /*
            string[] subPaths = path.Split(@"\/".ToCharArray());
            foreach (string subPath in subPaths) Debug.Log(subPath);
            string tempPath = "";

            if (subPaths[subPaths.Length - 1].Contains('.')) subPaths = subPaths.Take(subPaths.Length - 1).ToArray();
            for (int i = 0; i < subPaths.Length; i++)
            {
                if (i == 0) tempPath += subPaths[i];
                else
                {
                    if (!AssetDatabase.IsValidFolder(tempPath + "/" + subPaths[i]))
                    {
                        Debug.Log("Creating Folder " + subPaths[i] + " at " + tempPath);
                        AssetDatabase.CreateFolder(tempPath, subPaths[i]);
                    }
                    tempPath += "/" + subPaths[i];
                }
            }
            */
        }

        public static AnimationClip MakeAAP(string paramName, string saveAssetsTo, float value = 0, float animLenght = 1, string assetName = "") => MakeAAP(new string[1] { paramName }, saveAssetsTo, value, animLenght, assetName);
        public static AnimationClip MakeAAP(string[] paramNames, string saveAssetsTo, float value = 0, float animLenght = 1, string assetName = "")
        {
            AnimationClip animClip;
            if (paramNames.Length == 0) Debug.LogError("param list is empty!");
            if (assetName == "") assetName = paramNames[0] + "_AAP " + value;
            string saveName = assetName.Replace('/', '_');
            animClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(saveAssetsTo + saveName + ".anim", typeof(AnimationClip));
            if (animClip != null)
            {
                return animClip;
            }
            ReadyPath(saveAssetsTo);

            animLenght /= 60f;
            animClip = new AnimationClip
            {
                name = assetName,
                wrapMode = WrapMode.Clamp,
            };
            foreach (string paramName in paramNames)
            {
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0, value);
                curve.AddKey(animLenght, value);
                animClip.SetCurve("", typeof(Animator), paramName, curve);
            }

            AssetDatabase.CreateAsset(animClip, saveAssetsTo + saveName + ".anim");
            return animClip;
        }

        public static string GetAssetPath(this UnityEngine.Object item) => AssetDatabase.GetAssetPath(item);
        public static void SaveToAsset(this UnityEngine.Object item, UnityEngine.Object saveTo) => AssetDatabase.AddObjectToAsset(item, AssetDatabase.GetAssetPath(saveTo));
        public static void SaveUnsavedAssetsToController(this AnimatorController controller)
        {
            Queue<ChildAnimatorStateMachine> childStateMachines = new Queue<ChildAnimatorStateMachine>();
            List<ChildAnimatorState> states = new List<ChildAnimatorState>();
            List<AnimatorStateTransition> transitions = new List<AnimatorStateTransition>();

            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                if (GetAssetPath(layer.stateMachine) == "") layer.stateMachine.SaveToAsset(controller);
                foreach (var state in layer.stateMachine.states) states.Add(state);

                foreach (ChildAnimatorStateMachine tempChildStateMachine in layer.stateMachine.stateMachines)
                {
                    childStateMachines.Enqueue(tempChildStateMachine);
                    foreach (ChildAnimatorState state in tempChildStateMachine.stateMachine.states) states.Add(state);
                }
                while (childStateMachines.Count > 0)
                {
                    ChildAnimatorStateMachine childStateMachine = childStateMachines.Dequeue();
                    foreach (ChildAnimatorStateMachine tempChildStateMachine in childStateMachine.stateMachine.stateMachines)
                    {
                        childStateMachines.Enqueue(tempChildStateMachine);
                        foreach (ChildAnimatorState state in tempChildStateMachine.stateMachine.states) states.Add(state);
                    }
                    if (GetAssetPath(childStateMachine.stateMachine) == "") childStateMachine.stateMachine.SaveToAsset(controller);
                }
                foreach (AnimatorStateTransition anyState in layer.stateMachine.anyStateTransitions)
                {
                    transitions.Add(anyState);
                }
            }
            foreach (ChildAnimatorState state in states)
            {
                foreach (AnimatorStateTransition transition in state.state.transitions) transitions.Add(transition);
                if (GetAssetPath(state.state) == "") state.state.SaveToAsset(controller);
                if (state.state.motion is BlendTree) SaveUnsavedBlendtreesToController((BlendTree)state.state.motion, controller);
            }
            foreach (AnimatorStateTransition transition in transitions)
            {
                if (GetAssetPath(transition) == "") transition.SaveToAsset(controller);
            }
        }

        public static void SaveUnsavedBlendtreesToController(BlendTree blendTree, UnityEngine.Object saveTo)
        {
            Queue<BlendTree> blendTrees = new Queue<BlendTree>();
            blendTrees.Enqueue(blendTree);
            while (blendTrees.Count > 0)
            {
                BlendTree subBlendTree = blendTrees.Dequeue();
                if (GetAssetPath(subBlendTree) == "") subBlendTree.SaveToAsset(saveTo);
                foreach (ChildMotion child in subBlendTree.children)
                {
                    if (child.motion is BlendTree) blendTrees.Enqueue((BlendTree)child.motion);
                }
            }
        }

        public static AnimatorControllerParameter AddSmoothedVer(this AnimatorControllerParameter param, float minValue, float maxValue, AnimatorController controller, string smoothedParamName, string saveTo, string smoothingAmountParamName = "SmoothingAmount", string mainBlendTreeIdentifier = "MainBlendTree", string mainBlendTreeLayerName = "MainBlendTree", string smoothingParentTreeName = "SmoothingParentTree", string constantOneName = "ConstantOne")
        {
            BlendTree smoothingParentTree = GetOrGenerateChildTree(controller, smoothingParentTreeName, mainBlendTreeIdentifier, mainBlendTreeLayerName, constantOneName);
            AnimationClip smoothingAnimMin = MakeAAP(smoothedParamName, saveTo, minValue, 1, smoothedParamName + minValue);
            AnimationClip smoothingAnimMax = MakeAAP(smoothedParamName, saveTo, maxValue, 1, smoothedParamName + maxValue);
            controller.AddUniqueParam(smoothingAmountParamName, AnimatorControllerParameterType.Float, 0.1f);
            AnimatorControllerParameter constantOneParam = controller.AddUniqueParam(constantOneName, AnimatorControllerParameterType.Float, 1);
            AnimatorControllerParameter smoothedParam = controller.AddUniqueParam(smoothedParamName);

            BlendTree smoothedValue = new BlendTree() { blendType = BlendTreeType.Simple1D, blendParameter = smoothedParamName, name = smoothedParamName, useAutomaticThresholds = false, hideFlags = HideFlags.HideInHierarchy };

            ChildMotion[] tempChildren = new ChildMotion[2];
            tempChildren[0].motion = smoothingAnimMin;
            tempChildren[0].timeScale = 1;
            tempChildren[0].threshold = minValue;

            tempChildren[1].motion = smoothingAnimMax;
            tempChildren[1].timeScale = 1;
            tempChildren[1].threshold = maxValue;

            smoothedValue.children = tempChildren;

            BlendTree originalValue = new BlendTree() { blendType = BlendTreeType.Simple1D, blendParameter = param.name, name = param.name + "_Original", useAutomaticThresholds = false, hideFlags = HideFlags.HideInHierarchy };
            tempChildren = new ChildMotion[2];
            tempChildren[0].motion = smoothingAnimMin;
            tempChildren[0].timeScale = 1;
            tempChildren[0].threshold = minValue;
            tempChildren[1].motion = smoothingAnimMax;
            tempChildren[1].timeScale = 1;
            tempChildren[1].threshold = maxValue;
            originalValue.children = tempChildren;

            BlendTree smoother = new BlendTree() { blendType = BlendTreeType.Simple1D, blendParameter = smoothingAmountParamName, name = param.name + " Smoothing Tree", hideFlags = HideFlags.HideInHierarchy };
            smoother.AddChild(smoothedValue);
            smoother.AddChild(originalValue);
            smoother.useAutomaticThresholds = false;
            smoother.children[0].threshold = minValue;
            smoother.children[1].threshold = maxValue;

            smoothingParentTree.AddChild(smoother);
            tempChildren = smoothingParentTree.children;
            tempChildren[tempChildren.Length - 1].directBlendParameter = constantOneParam.name;
            smoothingParentTree.children = tempChildren;
            return smoothedParam;
        }

        public static AnimatorControllerParameter AddParamDifferential(AnimatorControllerParameter param1, AnimatorControllerParameter param2, AnimatorController controller, string saveTo, float minValue = -1, float maxValue = 1, string differentialParamName = "", string mainBlendTreeIdentifier = "MainBlendTree", string mainBlendTreelayerName = "MainBlendTree", string differentialParentTreeName = "DifferentialParentTree", string constantOneName = "ConstantOne")
        {
            BlendTree differentialParentTree = GetOrGenerateChildTree(controller, differentialParentTreeName, mainBlendTreeIdentifier, mainBlendTreelayerName, constantOneName);
            AnimatorControllerParameter constantOneParam = controller.AddUniqueParam(constantOneName, AnimatorControllerParameterType.Float, 1);
            if (differentialParamName == "")
            {
                differentialParamName = param1.name + "_Minus_" + param2.name;
                for (int i = 0; i < differentialParamName.Length; i++)
                {
                    if (differentialParamName[i] == '/' || differentialParamName[i] == '\\')
                    {
                        differentialParamName.Remove(i);
                    }
                }
            }

            AnimatorControllerParameter differentialParam = controller.AddUniqueParam(differentialParamName);

            if (minValue >= 0 && maxValue >= 0)
            {
                AnimationClip animationClipNegative = MakeAAP(differentialParamName, saveTo, -1);
                AnimationClip animationClipPositive = MakeAAP(differentialParamName, saveTo, 1);

                differentialParentTree.AddChild(animationClipPositive);
                differentialParentTree.AddChild(animationClipNegative);
                ChildMotion[] tempChildren = differentialParentTree.children;
                tempChildren[tempChildren.Length - 2].directBlendParameter = param1.name;
                tempChildren[tempChildren.Length - 1].directBlendParameter = param2.name;

                differentialParentTree.children = tempChildren;
            }
            else
            {
                AnimationClip animationClipMin = MakeAAP(differentialParamName, saveTo, minValue);
                AnimationClip animationClipMax = MakeAAP(differentialParamName, saveTo, maxValue);
                controller.AddUniqueParam(differentialParamName);

                BlendTree param1Tree = new BlendTree() { blendType = BlendTreeType.Simple1D, blendParameter = param1.name, name = param1.name + "Tree", useAutomaticThresholds = false, hideFlags = HideFlags.HideInHierarchy };
                BlendTree param2Tree = new BlendTree() { blendType = BlendTreeType.Simple1D, blendParameter = param2.name, name = param2.name + "Tree", useAutomaticThresholds = false, hideFlags = HideFlags.HideInHierarchy };

                ChildMotion[] tempChildren = new ChildMotion[2];
                tempChildren[0].motion = animationClipMin;
                tempChildren[0].threshold = -1;
                tempChildren[0].timeScale = 1;
                tempChildren[1].motion = animationClipMax;
                tempChildren[1].threshold = 1;
                tempChildren[1].timeScale = 1;
                param1Tree.children = tempChildren;

                tempChildren = new ChildMotion[2];
                tempChildren[0].motion = animationClipMax;
                tempChildren[0].threshold = -1;
                tempChildren[0].timeScale = 1;
                tempChildren[1].motion = animationClipMin;
                tempChildren[1].threshold = 1;
                tempChildren[1].timeScale = 1;
                param2Tree.children = tempChildren;

                differentialParentTree.AddChild(param1Tree);
                differentialParentTree.AddChild(param2Tree);

                tempChildren = differentialParentTree.children;
                tempChildren[tempChildren.Length - 2].directBlendParameter = constantOneParam.name;
                tempChildren[tempChildren.Length - 1].directBlendParameter = constantOneParam.name;
                differentialParentTree.children = tempChildren;
            }
            return differentialParam;
        }

        public static BlendTree GetOrgenerateMainBlendTree(AnimatorController fxLayer, string mainBlendTreeIdentifier, string layerName, string constantOneName)
            => GetMainBlendTree(fxLayer, mainBlendTreeIdentifier) ?? GenerateMainBlendTree(fxLayer, mainBlendTreeIdentifier, layerName, constantOneName);

        private static BlendTree GetMainBlendTree(AnimatorController fxLayer, string mainBlendTreeIdentifier)
        {
            List<AnimatorControllerLayer> mainBlendTrees = FindHiddenIdentifier(fxLayer, mainBlendTreeIdentifier);

            if (mainBlendTrees.Count > 0 && mainBlendTrees[0].stateMachine.states.Length > 0 && mainBlendTrees[0].stateMachine.states[0].state.motion is BlendTree)
            {
                return (BlendTree)mainBlendTrees[0].stateMachine.states[0].state.motion;
            }
            return null;
        }

        private static BlendTree GenerateMainBlendTree(AnimatorController fxLayer, string mainBlendTreeIdentifier, string layerName, string constantOneName)
        {
            fxLayer.AddUniqueParam(constantOneName, AnimatorControllerParameterType.Float, 1);

            AnimatorControllerLayer mainBlendTreeLayer = AddLayer(fxLayer, layerName);

            mainBlendTreeLayer.stateMachine.name = layerName;
            mainBlendTreeLayer.stateMachine.anyStatePosition = new Vector3(20, 20, 0);
            mainBlendTreeLayer.stateMachine.entryPosition = new Vector3(20, 50, 0);
            AnimatorState state = mainBlendTreeLayer.stateMachine.AddState("MainBlendTree (WD On)", new Vector3(0, 100, 0));
            state.hideFlags = HideFlags.HideInHierarchy;
            BlendTree mainBlendTree = new BlendTree()
            {
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Direct,
                blendParameter = constantOneName,
                name = "MainBlendTree",
            };
            state.motion = mainBlendTree;
            state.writeDefaultValues = true;
            mainBlendTreeLayer.stateMachine.AddHiddenIdentifier(mainBlendTreeIdentifier);
            return (BlendTree)state.motion;
        }

        public static BlendTree GetOrGenerateChildTree(AnimatorController fxLayer, string name, string mainBlendTreeIdentifier, string mainBlendTreeLayerName, string constantOneName)
            => GetChildTree(fxLayer, name, mainBlendTreeIdentifier, mainBlendTreeLayerName, constantOneName) ?? GenerateChildTree(fxLayer, name, mainBlendTreeIdentifier, mainBlendTreeLayerName, constantOneName);

        private static BlendTree GenerateChildTree(AnimatorController controller, string name, string mainBlendTreeIdentifier, string mainBlendTreeLayerName, string constantOneName)
        {
            BlendTree mainBlendTree = GetOrgenerateMainBlendTree(controller, mainBlendTreeIdentifier, mainBlendTreeLayerName, constantOneName);

            BlendTree smoothedParentTree = new BlendTree()
            {
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Direct,
                blendParameter = constantOneName,
                name = name,
            };
            mainBlendTree.AddChild(smoothedParentTree);

            var tempChildren = mainBlendTree.children;
            tempChildren[tempChildren.Length - 1].directBlendParameter = constantOneName;
            mainBlendTree.children = tempChildren;

            return (BlendTree)mainBlendTree.children.Last().motion;
        }

        private static BlendTree GetChildTree(AnimatorController controller, string name, string mainBlendTreeIdentifier, string mainBlendTreeLayerName, string constantOneName)
        {
            BlendTree mainBlendTree = GetOrgenerateMainBlendTree(controller, mainBlendTreeIdentifier, mainBlendTreeLayerName, constantOneName);

            foreach (ChildMotion child in mainBlendTree.children)
            {
                if (child.motion.name == name)
                {
                    return (BlendTree)child.motion;
                }
            }
            return null;
        }

        public static int DecimalToBinary(this int i)
        {
            if (i <= 0) { return 0; }
            string result = "";
            for (int j = i; j > 0; j /= 2)
            {
                result = (j % 2).ToString() + result;
            }
            return Int32.Parse(result);
        }

    }

}