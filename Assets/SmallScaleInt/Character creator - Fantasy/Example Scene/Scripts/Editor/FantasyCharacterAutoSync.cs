using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SmallScaleInc.CharacterCreatorFantasy
{
    public static class FantasyCharacterAutoSync
    {
        private const int DirectionRows = 8;
        private const float ClipFrameRate = 12f;

        private static readonly string[] BlendGroups =
        {
            "Idle", "Idle2", "Idle3", "Idle4",
            "Run", "Walk", "StrafeLeft", "StrafeRight", "RunBackwards",
            "CrouchIdle", "CrouchRun", "RideIdle", "RideRun"
        };

        private static readonly string[] OneShotGroups =
        {
            "Attack1", "Attack2", "Attack3", "Attack4", "Attack5",
            "AttackRun", "AttackRun2", "Special1", "Special2", "Taunt",
            "Die", "TakeDamage"
        };

        private static readonly string[] TriggerParameters =
        {
            "Attack1", "Attack2", "Attack3", "Attack4", "Attack5",
            "AttackRun", "AttackRun2",
            "Special1", "Special2", "Taunt",
            "Die", "TakeDamage"
        };

        private static readonly string[] BoolParameters =
        {
            "IsRun", "IsWalk", "IsStrafeLeft", "IsStrafeRight", "IsRunBackwards",
            "UseIdle2", "UseIdle3", "UseIdle4",
            "IsCrouching", "IsMounted",
            "Speed1x", "Speed2x"
        };

        private static readonly Dictionary<string, int> DirectionMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["E"] = 0,
            ["W"] = 1,
            ["S"] = 2,
            ["N"] = 3,
            ["NE"] = 4,
            ["NW"] = 5,
            ["SE"] = 6,
            ["SW"] = 7
        };

        private sealed class SyncSummary
        {
            public int CreatedClips;
            public int UpdatedClips;
            public int CreatedControllers;
            public int RebuiltControllers;
            public int RegisteredControllers;
            public int CreatedSceneObjects;
            public int CreatedToggles;
            public int UpdatedComponents;
            public readonly List<string> Warnings = new();
        }

        private sealed class GearFolderInfo
        {
            public string Name;
            public string FolderAssetPath;
            public List<string> SheetAssetPaths;
        }

        [MenuItem("Tools/Character Creator Fantasy/Sync Gear And Animations")]
        private static void SyncGearAndAnimations()
        {
            string spritesheetRoot = FindFirstValidFolder(
                "Assets/SmallScaleInt/Character creator - Fantasy/spritesheets",
                "Assets/SmallScaleInt/Character creator - Fantasy/Spritesheets");
            string animatorsRoot = FindFirstValidFolder("Assets/SmallScaleInt/Character creator - Fantasy/Animators");

            if (string.IsNullOrEmpty(spritesheetRoot) || string.IsNullOrEmpty(animatorsRoot))
            {
                EditorUtility.DisplayDialog(
                    "Character Creator Fantasy",
                    "Could not find the fantasy spritesheet and animator folders under Assets/SmallScaleInt.",
                    "OK");
                return;
            }

            SyncSummary summary = new();
            try
            {
                foreach (GearFolderInfo gearFolder in DiscoverGearFolders(spritesheetRoot))
                {
                    SyncGearFolder(gearFolder, animatorsRoot, summary);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Dictionary<string, RuntimeAnimatorController> controllersByName = LoadControllers(animatorsRoot);
                SyncOpenScenes(controllersByName, summary);
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog(
                    "Character Creator Fantasy",
                    "The sync failed. Check the Console for the full exception.",
                    "OK");
                return;
            }

            LogSummary(summary);
        }

        private static void SyncGearFolder(GearFolderInfo gearFolder, string animatorsRoot, SyncSummary summary)
        {
            int defaultColumns = 0;
            bool gearChanged = false;
            string controllerAssetPath = $"{animatorsRoot}/{gearFolder.Name}Controller.controller";
            bool controllerExists = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerAssetPath) != null;

            foreach (string sheetAssetPath in gearFolder.SheetAssetPaths)
            {
                if (!EnsureSpritesheetImport(sheetAssetPath, out int columns, out bool importChanged, summary))
                {
                    continue;
                }

                defaultColumns = Mathf.Max(defaultColumns, columns);

                string baseName = Path.GetFileNameWithoutExtension(sheetAssetPath);
                string animationName = baseName.Split('_')[0];

                List<Sprite> sprites = AssetDatabase
                    .LoadAllAssetRepresentationsAtPath(sheetAssetPath)
                    .OfType<Sprite>()
                    .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
                    .ToList();

                if (sprites.Count == 0)
                {
                    summary.Warnings.Add($"No sprites found after slicing {sheetAssetPath}.");
                    continue;
                }

                bool clipsChanged = EnsureDirectionalClips(
                    gearFolder.Name,
                    animationName,
                    baseName,
                    columns,
                    sprites,
                    importChanged,
                    animatorsRoot,
                    summary);

                gearChanged |= importChanged || clipsChanged;
            }

            if (defaultColumns > 0 && (gearChanged || !controllerExists))
            {
                RebuildAnimatorController(gearFolder.Name, animatorsRoot, defaultColumns, summary);
            }
        }

        private static IEnumerable<GearFolderInfo> DiscoverGearFolders(string spritesheetRoot)
        {
            string rootOnDisk = ToAbsolutePath(spritesheetRoot);
            if (!Directory.Exists(rootOnDisk))
            {
                yield break;
            }

            foreach (string directory in Directory.GetDirectories(rootOnDisk).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                string folderName = Path.GetFileName(directory);
                List<string> sheetAssetPaths = Directory
                    .GetFiles(directory, "*.png", SearchOption.TopDirectoryOnly)
                    .Select(ToAssetPath)
                    .Where(path => !string.IsNullOrEmpty(path))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (sheetAssetPaths.Count == 0)
                {
                    continue;
                }

                yield return new GearFolderInfo
                {
                    Name = folderName,
                    FolderAssetPath = ToAssetPath(directory),
                    SheetAssetPaths = sheetAssetPaths
                };
            }
        }

        private static bool EnsureSpritesheetImport(string sheetAssetPath, out int columns, out bool importChanged, SyncSummary summary)
        {
            columns = 0;
            importChanged = false;
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetAssetPath);
            TextureImporter importer = AssetImporter.GetAtPath(sheetAssetPath) as TextureImporter;
            if (texture == null || importer == null)
            {
                summary.Warnings.Add($"Skipped import sync for {sheetAssetPath} because the texture or importer could not be loaded.");
                return false;
            }

            int existingSpriteCount = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetAssetPath).OfType<Sprite>().Count();
            if (existingSpriteCount > 0 && existingSpriteCount % DirectionRows == 0)
            {
                columns = existingSpriteCount / DirectionRows;
            }

            int frameHeight = texture.height / DirectionRows;
            if (frameHeight <= 0 || texture.height % DirectionRows != 0)
            {
                summary.Warnings.Add($"Skipped {sheetAssetPath}: the texture height is not divisible by {DirectionRows} rows.");
                return false;
            }

            int inferredColumns = texture.width / frameHeight;
            if (inferredColumns <= 0 || texture.width % frameHeight != 0)
            {
                summary.Warnings.Add($"Skipped {sheetAssetPath}: expected square frames but width {texture.width} does not match row height {frameHeight}.");
                return false;
            }

            columns = inferredColumns;
            int expectedSpriteCount = DirectionRows * inferredColumns;
            bool shouldReslice = importer.spriteImportMode != SpriteImportMode.Multiple || existingSpriteCount != expectedSpriteCount;

            if (shouldReslice)
            {
                importChanged = true;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
#pragma warning disable 618
                importer.spritesheet = BuildSpriteMetaData(Path.GetFileNameWithoutExtension(sheetAssetPath), texture.width, texture.height, frameHeight, inferredColumns);
#pragma warning restore 618
                importer.SaveAndReimport();
            }

            return true;
        }

        private static SpriteMetaData[] BuildSpriteMetaData(string baseName, int textureWidth, int textureHeight, int frameSize, int columns)
        {
            List<SpriteMetaData> sprites = new(DirectionRows * columns);
            for (int row = 0; row < DirectionRows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Rect rect = new(
                        col * frameSize,
                        textureHeight - ((row + 1) * frameSize),
                        frameSize,
                        frameSize);

                    sprites.Add(new SpriteMetaData
                    {
                        name = $"{baseName}_{row}_{col}",
                        rect = rect,
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                }
            }

            return sprites.ToArray();
        }

        private static bool EnsureDirectionalClips(
            string gearName,
            string animationName,
            string baseName,
            int columns,
            IReadOnlyCollection<Sprite> sprites,
            bool forceRefresh,
            string animatorsRoot,
            SyncSummary summary)
        {
            string animationFolder = $"{animatorsRoot}/Animation Clips/{gearName}/{animationName}";
            EnsureFolderExists(animationFolder);

            Dictionary<string, Sprite> spritesByName = sprites.ToDictionary(sprite => sprite.name, sprite => sprite, StringComparer.Ordinal);
            int[] remap = { 7, 6, 5, 4, 3, 2, 1, 0 };
            bool changed = false;

            for (int row = 0; row < DirectionRows; row++)
            {
                string direction = GetDirectionName(remap[row]);
                string clipAssetPath = $"{animationFolder}/{animationName}_{direction}.anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);

                bool created = false;
                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, clipAssetPath);
                    summary.CreatedClips++;
                    created = true;
                    changed = true;
                }
                else if (!forceRefresh)
                {
                    continue;
                }
                else
                {
                    summary.UpdatedClips++;
                    changed = true;
                }

                clip.frameRate = ClipFrameRate;

                EditorCurveBinding binding = new()
                {
                    type = typeof(SpriteRenderer),
                    path = string.Empty,
                    propertyName = "m_Sprite"
                };

                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[columns];
                for (int col = 0; col < columns; col++)
                {
                    string spriteName = $"{baseName}_{row}_{col}";
                    spritesByName.TryGetValue(spriteName, out Sprite sprite);

                    keyframes[col] = new ObjectReferenceKeyframe
                    {
                        time = col / ClipFrameRate,
                        value = sprite
                    };
                }

                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                if (!created)
                {
                    EditorUtility.SetDirty(clip);
                }
            }

            return changed;
        }

        private static void RebuildAnimatorController(string gearName, string animatorsRoot, int defaultColumns, SyncSummary summary)
        {
            string controllerAssetPath = $"{animatorsRoot}/{gearName}Controller.controller";
            string clipsRoot = $"{animatorsRoot}/Animation Clips/{gearName}";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerAssetPath);

            if (controller == null)
            {
                EnsureFolderExists(animatorsRoot);
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerAssetPath);
                summary.CreatedControllers++;
            }
            else
            {
                summary.RebuiltControllers++;
            }

            foreach (AnimatorControllerParameter parameter in controller.parameters.ToArray())
            {
                controller.RemoveParameter(parameter);
            }

            controller.AddParameter("Direction", AnimatorControllerParameterType.Float);
            controller.AddParameter("DirIndex", AnimatorControllerParameterType.Int);

            foreach (string trigger in TriggerParameters)
            {
                controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);
            }

            foreach (string parameterName in BoolParameters)
            {
                controller.AddParameter(parameterName, AnimatorControllerParameterType.Bool);
            }

            while (controller.layers.Length > 1)
            {
                controller.RemoveLayer(controller.layers.Length - 1);
            }

            EnsureBaseLayer(controller);
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            ClearStateMachine(rootStateMachine);
            DestroyBlendTrees(controllerAssetPath);

            Dictionary<string, List<AnimationClip>> animationGroups = LoadAnimationGroups(clipsRoot);
            Dictionary<string, AnimatorState> states = BuildBlendStates(controller, rootStateMachine, animationGroups, defaultColumns);
            BuildLocomotionTransitions(states);
            BuildOneShotStates(rootStateMachine, animationGroups, states.TryGetValue("IdleBT", out AnimatorState idleState) ? idleState : null);

            if (states.TryGetValue("IdleBT", out AnimatorState idle))
            {
                rootStateMachine.defaultState = idle;
            }
            else if (states.Count > 0)
            {
                rootStateMachine.defaultState = states.Values.First();
            }

            EditorUtility.SetDirty(controller);
        }

        private static void EnsureBaseLayer(AnimatorController controller)
        {
            if (controller.layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
            }

            AnimatorControllerLayer[] layers = controller.layers;
            if (layers.Length == 0)
            {
                AnimatorControllerLayer fallbackLayer = new()
                {
                    name = "Base Layer",
                    defaultWeight = 1f,
                    stateMachine = new AnimatorStateMachine()
                };
                controller.layers = new[] { fallbackLayer };
                layers = controller.layers;
            }

            AnimatorControllerLayer baseLayer = layers[0];
            bool changed = false;

            if (string.IsNullOrWhiteSpace(baseLayer.name))
            {
                baseLayer.name = "Base Layer";
                changed = true;
            }

            if (Mathf.Approximately(baseLayer.defaultWeight, 0f))
            {
                baseLayer.defaultWeight = 1f;
                changed = true;
            }

            if (baseLayer.stateMachine == null)
            {
                baseLayer.stateMachine = new AnimatorStateMachine();
                changed = true;
            }

            if (changed)
            {
                layers[0] = baseLayer;
                controller.layers = layers;
            }
        }

        private static void ClearStateMachine(AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState child in stateMachine.states.ToArray())
            {
                stateMachine.RemoveState(child.state);
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines.ToArray())
            {
                stateMachine.RemoveStateMachine(child.stateMachine);
            }

            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions.ToArray())
            {
                stateMachine.RemoveAnyStateTransition(transition);
            }
        }

        private static void DestroyBlendTrees(string controllerAssetPath)
        {
            foreach (BlendTree tree in AssetDatabase.LoadAllAssetsAtPath(controllerAssetPath).OfType<BlendTree>().ToArray())
            {
                Object.DestroyImmediate(tree, true);
            }
        }

        private static Dictionary<string, List<AnimationClip>> LoadAnimationGroups(string clipsRoot)
        {
            Dictionary<string, List<AnimationClip>> groups = new(StringComparer.OrdinalIgnoreCase);
            if (!AssetDatabase.IsValidFolder(clipsRoot))
            {
                return groups;
            }

            foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { clipsRoot }))
            {
                string clipPath = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    continue;
                }

                string groupName = new DirectoryInfo(Path.GetDirectoryName(clipPath) ?? string.Empty).Name;
                if (!groups.TryGetValue(groupName, out List<AnimationClip> clips))
                {
                    clips = new List<AnimationClip>();
                    groups[groupName] = clips;
                }

                clips.Add(clip);
            }

            foreach ((string key, List<AnimationClip> clips) in groups.ToList())
            {
                groups[key] = clips.OrderBy(clip => clip.name, StringComparer.Ordinal).ToList();
            }

            return groups;
        }

        private static Dictionary<string, AnimatorState> BuildBlendStates(
            AnimatorController controller,
            AnimatorStateMachine rootStateMachine,
            Dictionary<string, List<AnimationClip>> animationGroups,
            int defaultColumns)
        {
            Dictionary<string, AnimatorState> states = new(StringComparer.Ordinal);

            foreach (string group in BlendGroups)
            {
                BlendTree tree = new()
                {
                    name = $"{group}BT_Tree",
                    blendType = BlendTreeType.Simple1D,
                    useAutomaticThresholds = false,
                    blendParameter = "Direction"
                };

                controller.AddMotion(tree, 0);
                AssetDatabase.AddObjectToAsset(tree, controller);

                if (animationGroups.TryGetValue(group, out List<AnimationClip> clips))
                {
                    foreach (AnimationClip clip in clips)
                    {
                        string direction = clip.name.Split('_').Last();
                        if (DirectionMap.TryGetValue(direction, out int directionValue))
                        {
                            tree.AddChild(clip, directionValue);
                        }
                    }
                }

                AnimatorState blendState = rootStateMachine.AddState($"{group}BT");
                blendState.motion = tree;
                blendState.speed = defaultColumns > 0 ? defaultColumns / 30f : 1f;

                states[blendState.name] = blendState;
                EditorUtility.SetDirty(tree);
            }

            AnimatorState idle = states.TryGetValue("IdleBT", out AnimatorState idleState) ? idleState : null;
            AnimatorState idle2 = states.TryGetValue("Idle2BT", out AnimatorState idle2State) ? idle2State : null;
            AnimatorState idle3 = states.TryGetValue("Idle3BT", out AnimatorState idle3State) ? idle3State : null;
            AnimatorState idle4 = states.TryGetValue("Idle4BT", out AnimatorState idle4State) ? idle4State : null;
            AnimatorState crouchIdle = states.TryGetValue("CrouchIdleBT", out AnimatorState crouchState) ? crouchState : null;
            AnimatorState rideIdle = states.TryGetValue("RideIdleBT", out AnimatorState rideState) ? rideState : null;

            AddBoolTransition(idle, idle2, "UseIdle2", true);
            AddBoolTransition(idle2, idle, "UseIdle2", false);

            AddBoolTransition(idle, idle3, "UseIdle3", true);
            AddBoolTransition(idle3, idle, "UseIdle3", false);

            AddBoolTransition(idle, idle4, "UseIdle4", true);
            AddBoolTransition(idle4, idle, "UseIdle4", false);

            AddBoolTransition(idle, crouchIdle, "IsCrouching", true);
            AddBoolTransition(idle2, crouchIdle, "IsCrouching", true);
            AddBoolTransition(idle3, crouchIdle, "IsCrouching", true);
            AddBoolTransition(idle4, crouchIdle, "IsCrouching", true);

            if (crouchIdle != null && idle != null)
            {
                AnimatorStateTransition transition = crouchIdle.AddTransition(idle);
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsCrouching");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "UseIdle2");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "UseIdle3");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "UseIdle4");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMounted");
                transition.hasExitTime = false;
                transition.duration = 0f;
            }

            AddBoolTransition(idle, rideIdle, "IsMounted", true);
            AddBoolTransition(idle2, rideIdle, "IsMounted", true);
            AddBoolTransition(idle3, rideIdle, "IsMounted", true);
            AddBoolTransition(idle4, rideIdle, "IsMounted", true);

            if (rideIdle != null && idle != null)
            {
                AnimatorStateTransition transition = rideIdle.AddTransition(idle);
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMounted");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "UseIdle2");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "UseIdle3");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "UseIdle4");
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsCrouching");
                transition.hasExitTime = false;
                transition.duration = 0f;
            }

            return states;
        }

        private static void BuildLocomotionTransitions(Dictionary<string, AnimatorState> states)
        {
            string[] idleStates = { "IdleBT", "Idle2BT", "Idle3BT", "Idle4BT" };
            string[] moveStates = { "RunBT", "WalkBT", "StrafeLeftBT", "StrafeRightBT", "RunBackwardsBT" };
            string[] moveParameters = { "IsRun", "IsWalk", "IsStrafeLeft", "IsStrafeRight", "IsRunBackwards" };

            foreach (string idleStateName in idleStates)
            {
                foreach ((string moveStateName, string parameterName) in moveStates.Zip(moveParameters, (stateName, parameter) => (stateName, parameter)))
                {
                    if (!states.TryGetValue(idleStateName, out AnimatorState idle) ||
                        !states.TryGetValue(moveStateName, out AnimatorState move))
                    {
                        continue;
                    }

                    AddBoolTransition(idle, move, parameterName, true);
                    AddBoolTransition(move, idle, parameterName, false);
                }
            }

            if (states.TryGetValue("CrouchIdleBT", out AnimatorState crouchIdle) &&
                states.TryGetValue("CrouchRunBT", out AnimatorState crouchRun))
            {
                foreach (string moveParameter in moveParameters)
                {
                    AddBoolTransition(crouchIdle, crouchRun, moveParameter, true);
                }

                AnimatorStateTransition backTransition = crouchRun.AddTransition(crouchIdle);
                foreach (string moveParameter in moveParameters)
                {
                    backTransition.AddCondition(AnimatorConditionMode.IfNot, 0, moveParameter);
                }
                backTransition.hasExitTime = false;
                backTransition.duration = 0f;
            }

            if (states.TryGetValue("RideIdleBT", out AnimatorState rideIdle) &&
                states.TryGetValue("RideRunBT", out AnimatorState rideRun))
            {
                foreach (string moveParameter in moveParameters)
                {
                    AddBoolTransition(rideIdle, rideRun, moveParameter, true);
                }

                AnimatorStateTransition backTransition = rideRun.AddTransition(rideIdle);
                foreach (string moveParameter in moveParameters)
                {
                    backTransition.AddCondition(AnimatorConditionMode.IfNot, 0, moveParameter);
                }
                backTransition.hasExitTime = false;
                backTransition.duration = 0f;
            }
        }

        private static void BuildOneShotStates(
            AnimatorStateMachine rootStateMachine,
            Dictionary<string, List<AnimationClip>> animationGroups,
            AnimatorState idleState)
        {
            foreach (string group in OneShotGroups)
            {
                if (!animationGroups.TryGetValue(group, out List<AnimationClip> clips) || clips.Count == 0)
                {
                    continue;
                }

                AnimatorStateMachine subMachine = rootStateMachine.AddStateMachine(group);
                foreach (AnimationClip clip in clips)
                {
                    string direction = clip.name.Split('_').Last();
                    if (!DirectionMap.TryGetValue(direction, out int directionValue))
                    {
                        continue;
                    }

                    AnimatorState state = subMachine.AddState($"{group}_{direction}");
                    state.motion = clip;

                    AnimatorStateTransition anyTransition = rootStateMachine.AddAnyStateTransition(state);
                    anyTransition.AddCondition(AnimatorConditionMode.If, 0, group);
                    anyTransition.AddCondition(AnimatorConditionMode.Equals, directionValue, "DirIndex");
                    anyTransition.hasExitTime = false;
                    anyTransition.duration = 0f;

                    if (idleState != null)
                    {
                        AnimatorStateTransition exitTransition = state.AddExitTransition(idleState);
                        exitTransition.hasExitTime = true;
                        exitTransition.exitTime = 1f;
                        exitTransition.duration = 0f;
                    }
                }
            }
        }

        private static void AddBoolTransition(AnimatorState from, AnimatorState to, string parameter, bool ifTrue)
        {
            if (from == null || to == null)
            {
                return;
            }

            AnimatorStateTransition transition = from.AddTransition(to);
            transition.AddCondition(ifTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, parameter);
            transition.hasExitTime = false;
            transition.duration = 0f;
        }

        private static Dictionary<string, RuntimeAnimatorController> LoadControllers(string animatorsRoot)
        {
            Dictionary<string, RuntimeAnimatorController> controllersByName = new(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets("t:RuntimeAnimatorController", new[] { animatorsRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                if (controller != null)
                {
                    string baseName = controller.name.Replace("Controller", string.Empty);
                    controllersByName[baseName] = controller;
                }
            }

            return controllersByName;
        }

        private static void SyncOpenScenes(Dictionary<string, RuntimeAnimatorController> controllersByName, SyncSummary summary)
        {
            List<GearPresetManager> presetManagers = GetSceneComponents<GearPresetManager>().ToList();
            foreach (GearPresetManager manager in presetManagers)
            {
                SyncGearPresetManager(manager, controllersByName, summary);
            }

            foreach (SpritesheetGenerator generator in GetSceneComponents<SpritesheetGenerator>())
            {
                SyncSpritesheetGenerator(generator, controllersByName, summary);
            }

            foreach (AnimatorGearSwitcher switcher in GetSceneComponents<AnimatorGearSwitcher>())
            {
                SyncAnimatorGearSwitcher(switcher, controllersByName, summary);
            }
        }

        private static void SyncGearPresetManager(
            GearPresetManager manager,
            IReadOnlyDictionary<string, RuntimeAnimatorController> controllersByName,
            SyncSummary summary)
        {
            bool changed = false;

            changed |= SyncGameObjectArrayWithToggles(
                "weapon",
                manager.weaponObjects,
                manager.weaponToggles,
                controllersByName,
                out GameObject[] weaponObjects,
                out Toggle[] weaponToggles,
                summary);
            if (changed)
            {
                manager.weaponObjects = weaponObjects;
                manager.weaponToggles = weaponToggles;
            }

            bool backpackChanged = SyncGameObjectArrayWithToggles(
                "backpack",
                manager.backpackObjects,
                manager.backpackToggles,
                controllersByName,
                out GameObject[] backpackObjects,
                out Toggle[] backpackToggles,
                summary);
            if (backpackChanged)
            {
                changed = true;
                manager.backpackObjects = backpackObjects;
                manager.backpackToggles = backpackToggles;
            }

            bool shieldChanged = SyncGameObjectArrayWithToggles(
                "shield",
                manager.shieldObjects,
                manager.shieldToggles,
                controllersByName,
                out GameObject[] shieldObjects,
                out Toggle[] shieldToggles,
                summary);
            if (shieldChanged)
            {
                changed = true;
                manager.shieldObjects = shieldObjects;
                manager.shieldToggles = shieldToggles;
            }

            bool mountChanged = SyncGameObjectArrayWithToggles(
                "mount",
                manager.mountObjects,
                manager.mountToggles,
                controllersByName,
                out GameObject[] mountObjects,
                out Toggle[] mountToggles,
                summary);
            if (mountChanged)
            {
                changed = true;
                manager.mountObjects = mountObjects;
                manager.mountToggles = mountToggles;
            }

            if (changed)
            {
                MarkSceneObjectDirty(manager, summary);
            }
        }

        private static void SyncSpritesheetGenerator(
            SpritesheetGenerator generator,
            IReadOnlyDictionary<string, RuntimeAnimatorController> controllersByName,
            SyncSummary summary)
        {
            bool changed = false;

            List<SpritesheetGenerator.Weapon> weapons = generator.weapons?.ToList() ?? new List<SpritesheetGenerator.Weapon>();
            changed |= SyncSceneEntries(
                weapons.Select(entry => entry.weaponGO).ToArray(),
                controllersByName,
                out GameObject[] syncedWeaponObjects,
                summary);
            if (changed)
            {
                generator.weapons = syncedWeaponObjects
                    .Select(CreateWeaponEntry)
                    .Where(entry => entry != null)
                    .ToArray();
            }

            List<SpritesheetGenerator.Backpack> backpacks = generator.backpacks?.ToList() ?? new List<SpritesheetGenerator.Backpack>();
            bool backpackChanged = SyncSceneEntries(
                backpacks.Select(entry => entry.backpackGO).ToArray(),
                controllersByName,
                out GameObject[] syncedBackpacks,
                summary);
            if (backpackChanged)
            {
                changed = true;
                generator.backpacks = syncedBackpacks
                    .Select(CreateBackpackEntry)
                    .Where(entry => entry != null)
                    .ToArray();
            }

            List<SpritesheetGenerator.Shield> shields = generator.shields?.ToList() ?? new List<SpritesheetGenerator.Shield>();
            bool shieldChanged = SyncSceneEntries(
                shields.Select(entry => entry.shieldGO).ToArray(),
                controllersByName,
                out GameObject[] syncedShields,
                summary);
            if (shieldChanged)
            {
                changed = true;
                generator.shields = syncedShields
                    .Select(CreateShieldEntry)
                    .Where(entry => entry != null)
                    .ToArray();
            }

            List<SpritesheetGenerator.Mount> mounts = generator.mounts?.ToList() ?? new List<SpritesheetGenerator.Mount>();
            bool mountChanged = SyncSceneEntries(
                mounts.Select(entry => entry.mountGO).ToArray(),
                controllersByName,
                out GameObject[] syncedMounts,
                summary);
            if (mountChanged)
            {
                changed = true;
                generator.mounts = syncedMounts
                    .Select(CreateMountEntry)
                    .Where(entry => entry != null)
                    .ToArray();
            }

            if (changed)
            {
                MarkSceneObjectDirty(generator, summary);
            }
        }

        private static void SyncAnimatorGearSwitcher(
            AnimatorGearSwitcher switcher,
            IReadOnlyDictionary<string, RuntimeAnimatorController> controllersByName,
            SyncSummary summary)
        {
            bool changed = false;
            HashSet<string> families = InferSwitcherFamilies(switcher);

            List<RuntimeAnimatorController> animatorControllers = switcher.animatorControllers != null
                ? switcher.animatorControllers.Where(controller => controller != null).ToList()
                : new List<RuntimeAnimatorController>();

            foreach (RuntimeAnimatorController controller in controllersByName.Values.OrderBy(ctrl => ctrl.name, new NaturalNameComparer()))
            {
                string controllerBaseName = controller.name.Replace("Controller", string.Empty);
                if (!families.Contains(GetFamilyPrefix(controllerBaseName)) ||
                    animatorControllers.Contains(controller))
                {
                    continue;
                }

                animatorControllers.Add(controller);
                changed = true;
                summary.RegisteredControllers++;
            }

            if (changed)
            {
                switcher.animatorControllers = animatorControllers
                    .OrderBy(controller => controller.name, new NaturalNameComparer())
                    .ToList();
            }

            GearPresetManager manager = GetSceneComponents<GearPresetManager>()
                .FirstOrDefault(candidate => candidate.gameObject.scene == switcher.gameObject.scene);

            if (switcher.isWeapon && manager != null)
            {
                List<Toggle> toggles = manager.weaponToggles?.Where(toggle => toggle != null).ToList() ?? new List<Toggle>();
                List<SpriteRenderer> renderers = manager.weaponObjects?
                    .Where(go => go != null)
                    .Select(go => go.GetComponent<SpriteRenderer>())
                    .Where(renderer => renderer != null)
                    .ToList() ?? new List<SpriteRenderer>();

                if (!AreSameContents(switcher.weaponToggles, toggles))
                {
                    switcher.weaponToggles = toggles.ToArray();
                    changed = true;
                }

                if (!AreSameContents(switcher.weaponSpriteRenderers, renderers))
                {
                    switcher.weaponSpriteRenderers = renderers;
                    changed = true;
                }
            }

            if (switcher.isBag && manager != null && !AreSameContents(switcher.bagToggles, manager.backpackToggles))
            {
                switcher.bagToggles = manager.backpackToggles?.Where(toggle => toggle != null).ToArray() ?? Array.Empty<Toggle>();
                changed = true;
            }

            if (switcher.isShield && manager != null && !AreSameContents(switcher.shieldToggles, manager.shieldToggles))
            {
                switcher.shieldToggles = manager.shieldToggles?.Where(toggle => toggle != null).ToArray() ?? Array.Empty<Toggle>();
                changed = true;
            }

            if (changed)
            {
                MarkSceneObjectDirty(switcher, summary);
            }
        }

        private static bool SyncGameObjectArrayWithToggles(
            string label,
            GameObject[] existingObjects,
            Toggle[] existingToggles,
            IReadOnlyDictionary<string, RuntimeAnimatorController> controllersByName,
            out GameObject[] syncedObjects,
            out Toggle[] syncedToggles,
            SyncSummary summary)
        {
            bool changed = false;
            List<GameObject> objects = existingObjects?.Where(go => go != null).ToList() ?? new List<GameObject>();
            List<Toggle> toggles = existingToggles?.Where(toggle => toggle != null).ToList() ?? new List<Toggle>();

            HashSet<string> families = new(
                objects.Select(go => GetFamilyPrefix(go.name))
                    .Concat(toggles.Select(toggle => GetFamilyPrefix(toggle.name)))
                    .Where(prefix => !string.IsNullOrEmpty(prefix) && !prefix.Equals("None", StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase);

            Dictionary<string, GameObject> templatesByFamily = objects
                .GroupBy(go => GetFamilyPrefix(go.name), StringComparer.OrdinalIgnoreCase)
                .Where(group => !string.IsNullOrEmpty(group.Key))
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            Dictionary<string, Toggle> toggleTemplatesByFamily = toggles
                .Where(toggle => !toggle.name.Equals("None", StringComparison.OrdinalIgnoreCase))
                .GroupBy(toggle => GetFamilyPrefix(toggle.name), StringComparer.OrdinalIgnoreCase)
                .Where(group => !string.IsNullOrEmpty(group.Key))
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            foreach ((string controllerName, RuntimeAnimatorController controller) in controllersByName
                .OrderBy(pair => pair.Key, new NaturalNameComparer()))
            {
                string family = GetFamilyPrefix(controllerName);
                if (!families.Contains(family) || objects.Any(go => go.name.Equals(controllerName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                GameObject propObject = FindOrCreateSceneObject(controllerName, controller, templatesByFamily, summary);
                if (propObject == null)
                {
                    summary.Warnings.Add($"Could not create a {label} scene object for {controllerName} because no template object was found.");
                    continue;
                }

                objects.Add(propObject);
                changed = true;

                Toggle toggle = FindOrCreateToggle(controllerName, propObject, toggleTemplatesByFamily, templatesByFamily, toggles, summary);
                if (toggle != null && toggles.All(existing => existing != toggle))
                {
                    toggles.Add(toggle);
                    changed = true;
                }
            }

            syncedObjects = objects.ToArray();
            syncedToggles = toggles.ToArray();
            return changed;
        }

        private static bool SyncSceneEntries(
            GameObject[] existingObjects,
            IReadOnlyDictionary<string, RuntimeAnimatorController> controllersByName,
            out GameObject[] syncedObjects,
            SyncSummary summary)
        {
            List<GameObject> objects = existingObjects?.Where(go => go != null).ToList() ?? new List<GameObject>();
            HashSet<string> families = new(
                objects.Select(go => GetFamilyPrefix(go.name))
                    .Where(prefix => !string.IsNullOrEmpty(prefix)),
                StringComparer.OrdinalIgnoreCase);

            Dictionary<string, GameObject> templatesByFamily = objects
                .GroupBy(go => GetFamilyPrefix(go.name), StringComparer.OrdinalIgnoreCase)
                .Where(group => !string.IsNullOrEmpty(group.Key))
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            bool changed = false;
            foreach ((string controllerName, RuntimeAnimatorController controller) in controllersByName
                .OrderBy(pair => pair.Key, new NaturalNameComparer()))
            {
                string family = GetFamilyPrefix(controllerName);
                if (!families.Contains(family) || objects.Any(go => go.name.Equals(controllerName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                GameObject propObject = FindOrCreateSceneObject(controllerName, controller, templatesByFamily, summary);
                if (propObject == null)
                {
                    continue;
                }

                objects.Add(propObject);
                changed = true;
            }

            syncedObjects = objects.ToArray();
            return changed;
        }

        private static GameObject FindOrCreateSceneObject(
            string objectName,
            RuntimeAnimatorController controller,
            IReadOnlyDictionary<string, GameObject> templatesByFamily,
            SyncSummary summary)
        {
            string family = GetFamilyPrefix(objectName);
            if (!templatesByFamily.TryGetValue(family, out GameObject template) || template == null)
            {
                template = templatesByFamily.Values.FirstOrDefault(candidate => candidate != null);
            }

            if (template == null || template.transform.parent == null)
            {
                return null;
            }

            Transform existing = template.transform.parent.Find(objectName);
            if (existing != null)
            {
                ApplyController(existing.gameObject, controller);
                return existing.gameObject;
            }

            GameObject clone = Object.Instantiate(template, template.transform.parent);
            clone.name = objectName;
            clone.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);
            ApplyController(clone, controller);

            summary.CreatedSceneObjects++;
            EditorSceneManager.MarkSceneDirty(clone.scene);
            return clone;
        }

        private static Toggle FindOrCreateToggle(
            string toggleName,
            GameObject targetObject,
            IReadOnlyDictionary<string, Toggle> toggleTemplatesByFamily,
            IReadOnlyDictionary<string, GameObject> objectTemplatesByFamily,
            IReadOnlyCollection<Toggle> existingToggles,
            SyncSummary summary)
        {
            Toggle existing = existingToggles.FirstOrDefault(toggle => toggle != null && toggle.name.Equals(toggleName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return existing;
            }

            string family = GetFamilyPrefix(toggleName);
            if (!toggleTemplatesByFamily.TryGetValue(family, out Toggle template) || template == null)
            {
                template = toggleTemplatesByFamily.Values.FirstOrDefault(toggle => toggle != null);
            }

            GameObject templateObject = null;
            if (!objectTemplatesByFamily.TryGetValue(family, out templateObject) || templateObject == null)
            {
                templateObject = objectTemplatesByFamily.Values.FirstOrDefault(candidate => candidate != null);
            }

            if (template == null || templateObject == null || template.transform.parent == null)
            {
                return null;
            }

            Transform existingChild = template.transform.parent.Find(toggleName);
            if (existingChild != null && existingChild.TryGetComponent(out Toggle existingToggle))
            {
                return existingToggle;
            }

            Toggle clone = Object.Instantiate(template, template.transform.parent);
            clone.name = toggleName;
            clone.isOn = false;
            clone.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);

            ReplaceToggleTargets(clone, templateObject, targetObject);
            UpdateToggleLabel(clone, toggleName);

            summary.CreatedToggles++;
            EditorSceneManager.MarkSceneDirty(clone.gameObject.scene);
            return clone;
        }

        private static void ReplaceToggleTargets(Toggle clonedToggle, GameObject templateRoot, GameObject newRoot)
        {
            SerializedObject serializedToggle = new(clonedToggle);
            SerializedProperty calls = serializedToggle.FindProperty("onValueChanged.m_PersistentCalls.m_Calls");
            if (calls == null)
            {
                return;
            }

            for (int index = 0; index < calls.arraySize; index++)
            {
                SerializedProperty call = calls.GetArrayElementAtIndex(index);
                SerializedProperty targetProperty = call.FindPropertyRelative("m_Target");
                if (targetProperty?.objectReferenceValue == null)
                {
                    continue;
                }

                Object templateTarget = targetProperty.objectReferenceValue;
                Object mappedTarget = MapTargetToClone(templateRoot, newRoot, templateTarget);
                if (mappedTarget != null)
                {
                    targetProperty.objectReferenceValue = mappedTarget;
                }

                SerializedProperty boolArgument = call.FindPropertyRelative("m_Arguments.m_BoolArgument");
                SerializedProperty methodName = call.FindPropertyRelative("m_MethodName");
                if (methodName != null &&
                    methodName.stringValue == "OnWeaponToggleChanged" &&
                    boolArgument != null)
                {
                    boolArgument.boolValue = GetFamilyPrefix(newRoot.name).Equals("Ranged", StringComparison.OrdinalIgnoreCase);
                }
            }

            serializedToggle.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(clonedToggle);
        }

        private static Object MapTargetToClone(GameObject templateRoot, GameObject newRoot, Object templateTarget)
        {
            if (templateTarget is Component templateComponent)
            {
                if (!templateComponent.transform.IsChildOf(templateRoot.transform) && templateComponent.gameObject != templateRoot)
                {
                    return null;
                }

                string relativePath = AnimationUtility.CalculateTransformPath(templateComponent.transform, templateRoot.transform);
                Transform newTransform = string.IsNullOrEmpty(relativePath) ? newRoot.transform : newRoot.transform.Find(relativePath);
                if (newTransform == null)
                {
                    return null;
                }

                Component[] templateComponents = templateComponent.transform.GetComponents(templateComponent.GetType());
                int componentIndex = Array.IndexOf(templateComponents, templateComponent);
                Component[] newComponents = newTransform.GetComponents(templateComponent.GetType());
                return componentIndex >= 0 && componentIndex < newComponents.Length ? newComponents[componentIndex] : newComponents.FirstOrDefault();
            }

            if (templateTarget is GameObject templateGameObject)
            {
                if (!templateGameObject.transform.IsChildOf(templateRoot.transform) && templateGameObject != templateRoot)
                {
                    return null;
                }

                string relativePath = AnimationUtility.CalculateTransformPath(templateGameObject.transform, templateRoot.transform);
                Transform newTransform = string.IsNullOrEmpty(relativePath) ? newRoot.transform : newRoot.transform.Find(relativePath);
                return newTransform != null ? newTransform.gameObject : null;
            }

            return null;
        }

        private static void UpdateToggleLabel(Toggle toggle, string text)
        {
            TMP_Text tmpText = toggle.GetComponentsInChildren<TMP_Text>(true)
                .FirstOrDefault(candidate => candidate.gameObject != toggle.gameObject);
            if (tmpText != null)
            {
                tmpText.text = text;
            }

            Text legacyText = toggle.GetComponentsInChildren<Text>(true)
                .FirstOrDefault(candidate => candidate.gameObject != toggle.gameObject);
            if (legacyText != null)
            {
                legacyText.text = text;
            }
        }

        private static void ApplyController(GameObject target, RuntimeAnimatorController controller)
        {
            Animator animator = target.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(animator);
            }
        }

        private static SpritesheetGenerator.Weapon CreateWeaponEntry(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            return new SpritesheetGenerator.Weapon
            {
                weaponGO = go,
                animator = go.GetComponent<Animator>(),
                spriteRenderer = go.GetComponent<SpriteRenderer>()
            };
        }

        private static SpritesheetGenerator.Backpack CreateBackpackEntry(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            return new SpritesheetGenerator.Backpack
            {
                backpackGO = go,
                animator = go.GetComponent<Animator>(),
                spriteRenderer = go.GetComponent<SpriteRenderer>()
            };
        }

        private static SpritesheetGenerator.Shield CreateShieldEntry(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            return new SpritesheetGenerator.Shield
            {
                shieldGO = go,
                animator = go.GetComponent<Animator>(),
                spriteRenderer = go.GetComponent<SpriteRenderer>()
            };
        }

        private static SpritesheetGenerator.Mount CreateMountEntry(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            return new SpritesheetGenerator.Mount
            {
                mountGO = go,
                animator = go.GetComponent<Animator>(),
                spriteRenderer = go.GetComponent<SpriteRenderer>()
            };
        }

        private static HashSet<string> InferSwitcherFamilies(AnimatorGearSwitcher switcher)
        {
            HashSet<string> families = new(StringComparer.OrdinalIgnoreCase);

            if (switcher.isHead) families.Add("Head");
            if (switcher.isChest) families.Add("Chest");
            if (switcher.isLegs) families.Add("Legs");
            if (switcher.isShoes) families.Add("Shoes");
            if (switcher.isHands) families.Add("Hands");
            if (switcher.isBelt) families.Add("Belt");
            if (switcher.isBag) families.Add("Bag");
            if (switcher.isShield) families.Add("Shield");

            if (switcher.animatorControllers != null)
            {
                foreach (RuntimeAnimatorController controller in switcher.animatorControllers)
                {
                    if (controller != null)
                    {
                        families.Add(GetFamilyPrefix(controller.name.Replace("Controller", string.Empty)));
                    }
                }
            }

            Animator animator = switcher.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                families.Add(GetFamilyPrefix(animator.runtimeAnimatorController.name.Replace("Controller", string.Empty)));
            }

            if (families.Count == 0)
            {
                families.Add(GetFamilyPrefix(switcher.gameObject.name));
            }

            families.RemoveWhere(string.IsNullOrEmpty);
            return families;
        }

        private static IEnumerable<T> GetSceneComponents<T>() where T : Component
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (T component in root.GetComponentsInChildren<T>(true))
                    {
                        yield return component;
                    }
                }
            }
        }

        private static void MarkSceneObjectDirty(Object target, SyncSummary summary)
        {
            EditorUtility.SetDirty(target);
            if (target is Component component)
            {
                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
            }
            else if (target is GameObject go)
            {
                EditorSceneManager.MarkSceneDirty(go.scene);
            }

            summary.UpdatedComponents++;
        }

        private static bool AreSameContents<T>(IReadOnlyCollection<T> left, IReadOnlyCollection<T> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            return left.SequenceEqual(right);
        }

        private static string GetDirectionName(int row)
        {
            return row switch
            {
                0 => "E",
                1 => "SE",
                2 => "S",
                3 => "SW",
                4 => "W",
                5 => "NW",
                6 => "N",
                7 => "NE",
                _ => "Unknown"
            };
        }

        private static string GetFamilyPrefix(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            int digitIndex = 0;
            while (digitIndex < name.Length && !char.IsDigit(name[digitIndex]))
            {
                digitIndex++;
            }

            return digitIndex == 0 ? name : name.Substring(0, digitIndex);
        }

        private static void EnsureFolderExists(string assetFolderPath)
        {
            Directory.CreateDirectory(ToAbsolutePath(assetFolderPath));
        }

        private static string FindFirstValidFolder(params string[] assetPaths)
        {
            return assetPaths.FirstOrDefault(AssetDatabase.IsValidFolder);
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string relativePath = assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                ? assetPath.Substring("Assets/".Length)
                : assetPath;
            return Path.Combine(Application.dataPath, relativePath).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string ToAssetPath(string absolutePath)
        {
            string normalizedAbsolute = Path.GetFullPath(absolutePath).Replace('\\', '/');
            string normalizedAssets = Application.dataPath.Replace('\\', '/');
            if (!normalizedAbsolute.StartsWith(normalizedAssets, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return $"Assets{normalizedAbsolute.Substring(normalizedAssets.Length)}";
        }

        private static void LogSummary(SyncSummary summary)
        {
            List<string> lines = new()
            {
                "Character creator fantasy sync finished.",
                $"Created clips: {summary.CreatedClips}",
                $"Updated clips: {summary.UpdatedClips}",
                $"Created controllers: {summary.CreatedControllers}",
                $"Rebuilt controllers: {summary.RebuiltControllers}",
                $"Registered controllers in scene components: {summary.RegisteredControllers}",
                $"Created scene gear objects: {summary.CreatedSceneObjects}",
                $"Created toggles: {summary.CreatedToggles}",
                $"Updated scene components: {summary.UpdatedComponents}"
            };

            if (summary.Warnings.Count > 0)
            {
                lines.Add("Warnings:");
                lines.AddRange(summary.Warnings.Select(warning => $"- {warning}"));
            }

            Debug.Log(string.Join(Environment.NewLine, lines));
        }

        private sealed class NaturalNameComparer : IComparer<string>, IComparer<KeyValuePair<string, RuntimeAnimatorController>>
        {
            public int Compare(KeyValuePair<string, RuntimeAnimatorController> x, KeyValuePair<string, RuntimeAnimatorController> y)
            {
                return Compare(x.Key, y.Key);
            }

            public int Compare(string left, string right)
            {
                left ??= string.Empty;
                right ??= string.Empty;

                string leftPrefix = GetFamilyPrefix(left);
                string rightPrefix = GetFamilyPrefix(right);
                int prefixCompare = string.Compare(leftPrefix, rightPrefix, StringComparison.OrdinalIgnoreCase);
                if (prefixCompare != 0)
                {
                    return prefixCompare;
                }

                int leftNumber = ExtractTrailingNumber(left);
                int rightNumber = ExtractTrailingNumber(right);
                int numberCompare = leftNumber.CompareTo(rightNumber);
                return numberCompare != 0
                    ? numberCompare
                    : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
            }

            private static int ExtractTrailingNumber(string value)
            {
                int index = value.Length - 1;
                while (index >= 0 && char.IsDigit(value[index]))
                {
                    index--;
                }

                return int.TryParse(value.Substring(index + 1), out int result) ? result : 0;
            }
        }
    }
}
