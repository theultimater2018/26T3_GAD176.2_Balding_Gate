using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;            // for EditorUtility & AssetDatabase
#endif

namespace SmallScaleInc.CharacterCreatorFantasy
{
    public class GearPresetManager : MonoBehaviour
    {
        private const string RuntimePresetFileName = "SavedCharacters.json";

        [Header("Database (persists across Play Mode)")]
        public GearPresetDatabase presetDatabase;
        private List<GearPreset> activePresets = new List<GearPreset>();
        private readonly Dictionary<string, RuntimeAnimatorController> controllersByName = new Dictionary<string, RuntimeAnimatorController>();

        [System.Serializable]
        public class GearPreset
        {
            public RuntimeAnimatorController bodyAnimator;
            public Color                   bodyColor;

            public RuntimeAnimatorController headAnimator;
            public Color headColor;

            public RuntimeAnimatorController chestAnimator;
            public Color chestColor;

            public RuntimeAnimatorController legsAnimator;
            public Color legsColor;

            public RuntimeAnimatorController shoesAnimator;
            public Color shoesColor;

            public RuntimeAnimatorController handsAnimator;
            public Color handsColor;

            public RuntimeAnimatorController beltAnimator;
            public Color beltColor;

            public RuntimeAnimatorController slashAnimator;
            public Color slashColor;

            public RuntimeAnimatorController effectAnimator;
            public Color effectColor;

            public RuntimeAnimatorController effect2Animator;
            public Color effect2Color;

            // visibility flags
            public bool bodyVisible;
            public bool headVisible;
            public bool chestVisible;
            public bool legsVisible;
            public bool shoesVisible;
            public bool handsVisible;
            public bool beltVisible;
            public bool slashVisible;
            public bool effectVisible;
            public bool effect2Visible;

            public string weaponName;
            public string backpackName;
            public string shieldName;

            public Color backpackColor;
            public Color shieldColor;
            public Color mountColor;
            public Color   weaponColor; 

            public string shadowOpacityName;

            // Post-processing:
            public bool   sliceSpritesheet;
            public bool   staticIdle;
            public string outlineName;     // stores either "OutlineBlack", "OutlineGradient" or "None"
            public string resolutionName;  // stores either "resolution64", "resolution128" or "None"
            public string frameName;
            public int exportSettingsVersion;
            public bool outlineEnabled;
            public bool gradientOutlineEnabled;
            public bool glowOutlineEnabled;
            public int glowThickness;
            public bool resolution128;
            public bool resolution64;
            public bool maxFrames;
            public bool fourteenFrames;
            public bool twelveFrames;
            public bool tenFrames;
            public bool eightFrames;
            public bool sixFrames;
            public bool fourFrames;
            public bool exportTileset;
            public bool flatColor;
            public bool extractShadow;
            public bool extractEffects;
            public bool createAnimator;

            public string mountName;
            public string idleName;

            public string presetName; //Name of the preset character
            public string presetDate; //Date of creation of the preset character

        }

        [System.Serializable]
        private class RuntimePresetCollection
        {
            public List<RuntimeGearPreset> presets = new List<RuntimeGearPreset>();
        }

        [System.Serializable]
        private class RuntimeGearPreset
        {
            public string bodyAnimatorName;
            public Color bodyColor;
            public string headAnimatorName;
            public Color headColor;
            public string chestAnimatorName;
            public Color chestColor;
            public string legsAnimatorName;
            public Color legsColor;
            public string shoesAnimatorName;
            public Color shoesColor;
            public string handsAnimatorName;
            public Color handsColor;
            public string beltAnimatorName;
            public Color beltColor;
            public string slashAnimatorName;
            public Color slashColor;
            public string effectAnimatorName;
            public Color effectColor;
            public string effect2AnimatorName;
            public Color effect2Color;
            public bool bodyVisible;
            public bool headVisible;
            public bool chestVisible;
            public bool legsVisible;
            public bool shoesVisible;
            public bool handsVisible;
            public bool beltVisible;
            public bool slashVisible;
            public bool effectVisible;
            public bool effect2Visible;
            public string weaponName;
            public string backpackName;
            public string shieldName;
            public Color backpackColor;
            public Color shieldColor;
            public Color mountColor;
            public Color weaponColor;
            public string shadowOpacityName;
            public bool sliceSpritesheet;
            public bool staticIdle;
            public string outlineName;
            public string resolutionName;
            public string frameName;
            public int exportSettingsVersion;
            public bool outlineEnabled;
            public bool gradientOutlineEnabled;
            public bool glowOutlineEnabled;
            public int glowThickness;
            public bool resolution128;
            public bool resolution64;
            public bool maxFrames;
            public bool fourteenFrames;
            public bool twelveFrames;
            public bool tenFrames;
            public bool eightFrames;
            public bool sixFrames;
            public bool fourFrames;
            public bool exportTileset;
            public bool flatColor;
            public bool extractShadow;
            public bool extractEffects;
            public bool createAnimator;
            public string mountName;
            public string idleName;
            public string presetName;
            public string presetDate;
        }

        [Header("UI Toggles for Presets")]
        public Button presetButtonPrefab;  
        public Transform presetsToggleContainer;
        private readonly List<Button> presetButtons = new List<Button>();
        public TMP_InputField presetNameInput;

        public Toggle showExamplesToggle;

        [Header("Confirmation Panel")]
        public GameObject confirmationPanel;
        public TextMeshProUGUI confirmationMessageText;
        public Button confirmationAcceptButton;
        public Button confirmationCancelButton;
        public bool autoCreateConfirmationPanel = true;
        private System.Action pendingConfirmationAction;



         [Header("Show/Hide Toggles (Body Parts)")]
        public Toggle bodyToggle;
        public Toggle headToggle;
        public Toggle chestToggle;
        public Toggle legsToggle;
        public Toggle shoesToggle;
        public Toggle handsToggle;
        public Toggle beltToggle;
        public Toggle slashDisplayToggle;
        public Toggle effectDisplayToggle;
        public Toggle effect2DisplayToggle;

        [Header("Save Preset Button")]
        public Button savePresetButton;

        // --- Gear Slot References ---
        [Header("Animators")]
        public Animator bodyAnimator, headAnimator, chestAnimator, legsAnimator, shoesAnimator;
        public Animator handsAnimator, beltAnimator, slashAnimator, effectAnimator, effect2Animator;

        [Header("Renderers")]
        public SpriteRenderer bodyRenderer, headRenderer, chestRenderer, legsRenderer, shoesRenderer;
        public SpriteRenderer handsRenderer, beltRenderer, slashRenderer, effectRenderer, effect2Renderer;

        [Header("Weapon/Backpack/Shield Toggles")]
        public Toggle[] weaponToggles;
        public Toggle[] backpackToggles;
        public Toggle[] shieldToggles;

        [Header("Gear Objects (with SpriteRenderer)")]
        public GameObject[] weaponObjects;    
        public GameObject[] backpackObjects;
        public GameObject[] shieldObjects;
        public GameObject[] mountObjects;
        
        [Header("Shadow Opacity Toggles")]
        public Toggle[] shadowToggles;

        [Header("Frames Toggles (always one on)")]
        public Toggle[] frameToggles;  // assign your 7 frame-count toggles here

        [Header("Post-Processing Toggles")]
        public Toggle   sliceSpritesheetToggle;
        public Toggle   staticIdleToggle;
        public Toggle[] outlineToggles;     // must contain your two outline toggles, named accordingly
        public Toggle[] resolutionToggles;  // must contain your two resolution toggles, named accordingly
        public SpritesheetGenerator spritesheetGenerator;

        [Header("Mount Toggles")]
        public Toggle[] mountToggles;   // assign your mount-selection toggles here

        [Header("Idle Toggles")]
        public Toggle[] idleToggles;  

        [Header("Player Controller")]
        public TopDownCharacterController playerController;


        void Awake()
        {
            InitializePresets();
            RefreshPresetButtons();
            SetupConfirmationPanel();

            if (showExamplesToggle != null)
               showExamplesToggle.onValueChanged.AddListener(_ => RefreshPresetButtons());

        }

        void Start()
        {
            savePresetButton?.onClick.AddListener(SaveCurrentAsPreset);
        }

        public void SaveCurrentAsPreset()
        {
            // 1) Grab the names & visibilities first
            string shieldName    = GetActiveToggleName(shieldToggles);
            string weaponName    = GetActiveToggleName(weaponToggles);
            string backpackName  = GetActiveToggleName(backpackToggles);
            string mountName     = GetActiveToggleName(mountToggles);
            string idleName      = GetActiveToggleName(idleToggles);
            string shadowName    = GetActiveToggleName(shadowToggles);
            string frameName     = GetActiveToggleName(frameToggles);
            string outlineName   = GetActiveToggleName(outlineToggles);
            string resolutionName= GetActiveToggleName(resolutionToggles);
            string name = string.IsNullOrWhiteSpace(presetNameInput.text) ? "Character" : presetNameInput.text.Trim();
            string date = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

            bool sliceSS         = sliceSpritesheetToggle.isOn;
            bool staticIdle      = staticIdleToggle.isOn;
            bool bodyVis         = bodyToggle.isOn;
            bool headVis         = headToggle.isOn;
            bool chestVis        = chestToggle.isOn;
            bool legsVis         = legsToggle.isOn;
            bool shoesVis        = shoesToggle.isOn;
            bool handsVis        = handsToggle.isOn;
            bool beltVis         = beltToggle.isOn;
            bool slashVis        = slashDisplayToggle.isOn;
            bool effectVis       = effectDisplayToggle.isOn;
            bool effect2Vis       = effect2DisplayToggle.isOn;

            // 2) Build the preset
            var p = new GearPreset
            {
                // Animators & base‐colors
                bodyAnimator    = bodyAnimator.runtimeAnimatorController,
                bodyColor       = bodyRenderer.color,

                headAnimator    = headAnimator.runtimeAnimatorController,
                headColor       = headRenderer.color,

                chestAnimator   = chestAnimator.runtimeAnimatorController,
                chestColor      = chestRenderer.color,

                legsAnimator    = legsAnimator.runtimeAnimatorController,
                legsColor       = legsRenderer.color,

                shoesAnimator   = shoesAnimator.runtimeAnimatorController,
                shoesColor      = shoesRenderer.color,

                handsAnimator   = handsAnimator.runtimeAnimatorController,
                handsColor      = handsRenderer.color,

                beltAnimator    = beltAnimator.runtimeAnimatorController,
                beltColor       = beltRenderer.color,

                slashAnimator   = slashAnimator.runtimeAnimatorController,
                effectAnimator  = effectAnimator.runtimeAnimatorController,
                effect2Animator  = effect2Animator.runtimeAnimatorController,
                // Visibility
                bodyVisible     = bodyVis,
                headVisible     = headVis,
                chestVisible    = chestVis,
                legsVisible     = legsVis,
                shoesVisible    = shoesVis,
                handsVisible    = handsVis,
                beltVisible     = beltVis,
                slashVisible    = slashVis,
                effectVisible   = effectVis,
                effect2Visible   = effect2Vis,

                // Toggle‐based names
                shieldName        = shieldName,
                weaponName        = weaponName,
                backpackName      = backpackName,
                mountName         = mountName,
                idleName          = idleName,
                shadowOpacityName = shadowName,
                frameName         = frameName,

                // Post‐processing
                sliceSpritesheet  = sliceSS,
                staticIdle        = staticIdle,
                outlineName       = outlineName,
                resolutionName    = resolutionName,

                // Character name and date
                presetName = name,
                presetDate = date
            };
            CaptureExportSettings(p);

            // 3) Now read all of the actual SpriteRenderer colors…

            // Shield color
            var shGO = FindObjectByName(shieldObjects, shieldName);
            p.shieldColor = shGO != null
                ? shGO.GetComponent<SpriteRenderer>().color
                : Color.white;

            // Weapon color
            var wGO = FindObjectByName(weaponObjects, weaponName);
            p.weaponColor = wGO != null
                ? wGO.GetComponent<SpriteRenderer>().color
                : Color.white;

            // Slash color
            p.slashColor = slashRenderer.color;

            // Effect color
            p.effectColor = effectRenderer.color;

            // Effect color
            p.effect2Color = effect2Renderer.color;

            // Backpack color
            var bpGO = FindObjectByName(backpackObjects, backpackName);
            p.backpackColor = bpGO != null
                ? bpGO.GetComponent<SpriteRenderer>().color
                : Color.white;


            // Mount color
            var mtGO = FindObjectByName(mountObjects, mountName);
            p.mountColor = mtGO != null
                ? mtGO.GetComponent<SpriteRenderer>().color
                : Color.white;


            // 4) Persist & UI
            RequestSavePreset(p);
        }



        public void ApplyPreset(int i)
        {
            if (i < 0 || i >= activePresets.Count) return;
            var p = activePresets[i];

            if (presetNameInput != null)
                presetNameInput.text = p.presetName;

            // --- BODY (includes skin) ---
            bodyToggle.isOn = p.bodyVisible;
            if (p.bodyVisible)
            {
                bodyAnimator.runtimeAnimatorController = p.bodyAnimator;
                bodyRenderer.color                   = p.bodyColor;
            }

            // --- HEAD ---
            headToggle.isOn = p.headVisible;
            if (p.headVisible)
            {
                headAnimator.runtimeAnimatorController = p.headAnimator;
                headRenderer.color                    = p.headColor;
            }

            // --- CHEST ---
            chestToggle.isOn = p.chestVisible;
            if (p.chestVisible)
            {
                chestAnimator.runtimeAnimatorController = p.chestAnimator;
                chestRenderer.color                    = p.chestColor;
            }

            // --- LEGS ---
            legsToggle.isOn = p.legsVisible;
            if (p.legsVisible)
            {
                legsAnimator.runtimeAnimatorController = p.legsAnimator;
                legsRenderer.color                    = p.legsColor;
            }

            // --- SHOES ---
            shoesToggle.isOn = p.shoesVisible;
            if (p.shoesVisible)
            {
                shoesAnimator.runtimeAnimatorController = p.shoesAnimator;
                shoesRenderer.color                    = p.shoesColor;
            }

            // --- HANDS ---
            handsToggle.isOn = p.handsVisible;
            if (p.handsVisible)
            {
                handsAnimator.runtimeAnimatorController = p.handsAnimator;
                handsRenderer.color                    = p.handsColor;
            }

            // --- BELT ---
            beltToggle.isOn = p.beltVisible;
            if (p.beltVisible)
            {
                beltAnimator.runtimeAnimatorController = p.beltAnimator;
                beltRenderer.color                    = p.beltColor;
            }

            // --- SLASH EFFECT ---
            slashDisplayToggle.isOn = p.slashVisible;
            if (p.slashVisible)
            {
                slashAnimator.runtimeAnimatorController = p.slashAnimator;
                slashRenderer.color                    = p.slashColor;
            }

            // --- MAGIC EFFECT ---
            effectDisplayToggle.isOn = p.effectVisible;
            if (p.effectVisible)
            {
                effectAnimator.runtimeAnimatorController = p.effectAnimator;
                effectRenderer.color                    = p.effectColor;
            }

            // --- MAGIC EFFECT 2---
            effect2DisplayToggle.isOn = p.effect2Visible;
            if (p.effect2Visible)
            {
                effect2Animator.runtimeAnimatorController = p.effect2Animator;
                effect2Renderer.color                    = p.effect2Color;
            }

            // shadow opacity
            SetToggleByName(shadowToggles, p.shadowOpacityName);

            // --- POST-PROCESSING ---
            sliceSpritesheetToggle.isOn = p.sliceSpritesheet;
            staticIdleToggle.isOn       = p.staticIdle;

            // Outline: could be "OutlineBlack", "OutlineGradient" or "None"
            if (p.outlineName == "None")
            {
                // turn *all* outline toggles off
                foreach (var tog in outlineToggles)
                    tog.isOn = false;
            }
            else
            {
                // turn only the matching one on
                SetToggleByName(outlineToggles, p.outlineName);
            }

            SetToggleByName(resolutionToggles, p.resolutionName);
            SetToggleByName(frameToggles, p.frameName);
            ApplyExportSettings(p);

            // --- MOUNT & IDLE (exactly like shadow/frames) ---
                        // Mount
            SetToggleByName(mountToggles, p.mountName);
            var mt = FindObjectByName(mountObjects, p.mountName);
            if (mt) mt.GetComponent<SpriteRenderer>().color = p.mountColor;

            SetToggleByName(idleToggles,  p.idleName);
            ApplyIdleToPlayer(p.idleName);

            // --- WEAPON / BACKPACK / SHIELD ---
            // Shield
            SetToggleByName(shieldToggles, p.shieldName);
            var sh = FindObjectByName(shieldObjects, p.shieldName);
            if (sh) sh.GetComponent<SpriteRenderer>().color = p.shieldColor;

            SetToggleByName(weaponToggles, p.weaponName);
            var w = FindObjectByName(weaponObjects, p.weaponName);
            if (w) w.GetComponent<SpriteRenderer>().color = p.weaponColor;
                        // Backpack
            SetToggleByName(backpackToggles, p.backpackName);
            var bp = FindObjectByName(backpackObjects, p.backpackName);
            if (bp) bp.GetComponent<SpriteRenderer>().color = p.backpackColor;

        }


        /// <summary>
        /// Spawns one preset-button in your UI list and wires it to ApplyPreset(index).
        /// </summary>
        private void CreateButtonForPreset(int index)
        {
            var btn = Instantiate(presetButtonPrefab, presetsToggleContainer);
            var preset = activePresets[index];

            // Update name and date
            var nameText = btn.transform.Find("PresetName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.SetText(preset.presetName);

            var dateText = btn.transform.Find("PresetDate")?.GetComponent<TextMeshProUGUI>();
            if (dateText != null)
                dateText.SetText(preset.presetDate);

            // ✅ Find DeleteButton recursively and hook up delete logic
            Button[] allButtons = btn.GetComponentsInChildren<Button>(true);
            foreach (var b in allButtons)
            {
                if (b.name == "DeleteButton")
                {
                    int idx = index; // Capture the index in the closure
                    b.onClick.AddListener(() => DeletePreset(idx));
                    break;
                }
            }

            // Hook up apply logic
            int applyIndex = index;
            btn.onClick.AddListener(() => RequestApplyPreset(applyIndex));

            presetButtons.Add(btn);
        }


        private void DeletePreset(int index)
        {
            if (index < 0 || index >= activePresets.Count) return;

            activePresets.RemoveAt(index);

            // Destroy the UI button and refresh all
            foreach (var btn in presetButtons)
                Destroy(btn.gameObject);
            presetButtons.Clear();

            RefreshPresetButtons();


        #if UNITY_EDITOR
            EditorUtility.SetDirty(presetDatabase);
            AssetDatabase.SaveAssets();
        #else
            SaveRuntimePresets();
        #endif
        }

        private void RequestApplyPreset(int index)
        {
            if (index < 0 || index >= activePresets.Count) return;

            string presetName = string.IsNullOrEmpty(activePresets[index].presetName)
                ? "this character"
                : activePresets[index].presetName;

            ShowConfirmation(
                $"Are you sure you want to load '{presetName}'?",
                () => ApplyPreset(index));
        }

        private void RequestSavePreset(GearPreset preset)
        {
            int existingIndex = FindPresetIndexByName(preset.presetName);
            if (existingIndex >= 0)
            {
                ShowConfirmation(
                    $"Are you sure you want to save '{preset.presetName}'?\nA character with this name already exists and will be overwritten.",
                    () => CommitSavePreset(preset, existingIndex));
                return;
            }

            CommitSavePreset(preset, -1);
        }

        private void CommitSavePreset(GearPreset preset, int overwriteIndex)
        {
            if (overwriteIndex >= 0 && overwriteIndex < activePresets.Count)
                activePresets[overwriteIndex] = preset;
            else
                activePresets.Add(preset);

            RefreshPresetButtons();
            PersistPresets();
        }

        private int FindPresetIndexByName(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName)) return -1;

            for (int i = 0; i < activePresets.Count; i++)
            {
                if (string.Equals(activePresets[i].presetName, presetName, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private void PersistPresets()
        {
            #if UNITY_EDITOR
            if (presetDatabase != null)
                EditorUtility.SetDirty(presetDatabase);
            AssetDatabase.SaveAssets();
            #else
            SaveRuntimePresets();
            #endif
        }

        private void SetupConfirmationPanel()
        {
            if (confirmationPanel == null && autoCreateConfirmationPanel)
                CreateFallbackConfirmationPanel();

            if (confirmationPanel != null)
                confirmationPanel.SetActive(false);

            if (confirmationAcceptButton != null)
            {
                confirmationAcceptButton.onClick.RemoveListener(AcceptConfirmation);
                confirmationAcceptButton.onClick.AddListener(AcceptConfirmation);
            }

            if (confirmationCancelButton != null)
            {
                confirmationCancelButton.onClick.RemoveListener(CancelConfirmation);
                confirmationCancelButton.onClick.AddListener(CancelConfirmation);
            }
        }

        private void CreateFallbackConfirmationPanel()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindAnyObjectByType<Canvas>();

            if (canvas == null)
                return;

            GameObject panelObject = new GameObject("PresetConfirmationPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);
            confirmationPanel = panelObject;

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520f, 210f);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.88f);

            GameObject messageObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
            messageObject.transform.SetParent(panelObject.transform, false);
            confirmationMessageText = messageObject.GetComponent<TextMeshProUGUI>();
            confirmationMessageText.alignment = TextAlignmentOptions.Center;
            confirmationMessageText.fontSize = 24f;
            confirmationMessageText.color = Color.white;

            RectTransform messageRect = messageObject.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.08f, 0.42f);
            messageRect.anchorMax = new Vector2(0.92f, 0.88f);
            messageRect.offsetMin = Vector2.zero;
            messageRect.offsetMax = Vector2.zero;

            confirmationAcceptButton = CreateConfirmationButton(panelObject.transform, "AcceptButton", "Accept", new Vector2(-95f, -62f));
            confirmationCancelButton = CreateConfirmationButton(panelObject.transform, "CancelButton", "Cancel", new Vector2(95f, -62f));
        }

        private Button CreateConfirmationButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(150f, 46f);
            buttonRect.anchoredPosition = anchoredPosition;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.18f, 0.18f, 1f);

            GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObject.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 22f;
            labelText.color = Color.white;

            return buttonObject.GetComponent<Button>();
        }

        private void ShowConfirmation(string message, System.Action onAccept)
        {
            if (confirmationPanel == null || confirmationAcceptButton == null || confirmationCancelButton == null)
            {
                onAccept?.Invoke();
                return;
            }

            pendingConfirmationAction = onAccept;
            if (confirmationMessageText != null)
                confirmationMessageText.text = message;

            confirmationPanel.SetActive(true);
        }

        private void AcceptConfirmation()
        {
            System.Action action = pendingConfirmationAction;
            pendingConfirmationAction = null;

            if (confirmationPanel != null)
                confirmationPanel.SetActive(false);

            action?.Invoke();
        }

        private void CancelConfirmation()
        {
            pendingConfirmationAction = null;

            if (confirmationPanel != null)
                confirmationPanel.SetActive(false);
        }

        private void CaptureExportSettings(GearPreset preset)
        {
            SpritesheetGenerator generator = ResolveSpritesheetGenerator();

            if (generator == null)
            {
                preset.exportSettingsVersion = 0;
                preset.sliceSpritesheet = GetToggleState(sliceSpritesheetToggle, preset.sliceSpritesheet);
                preset.staticIdle = GetToggleState(staticIdleToggle, preset.staticIdle);
                return;
            }

            preset.exportSettingsVersion = 1;
            preset.sliceSpritesheet = GetToggleState(generator.sliceSpritesheets, preset.sliceSpritesheet);
            preset.staticIdle = GetToggleState(generator.staticIdleAnimation, preset.staticIdle);
            preset.outlineEnabled = GetToggleState(generator.outlineToggle, false);
            preset.gradientOutlineEnabled = GetToggleState(generator.gradientOutlineToggle, false);
            preset.glowOutlineEnabled = GetToggleState(generator.glowOutlineToggle, false);
            preset.glowThickness = generator.glowThicknessSlider != null
                ? Mathf.RoundToInt(generator.glowThicknessSlider.value)
                : generator.glowThickness;
            preset.resolution128 = GetToggleState(generator.use128Toggle, false);
            preset.resolution64 = GetToggleState(generator.use64Toggle, false);
            preset.maxFrames = GetToggleState(generator.maxFramesToggle, false);
            preset.fourteenFrames = GetToggleState(generator.fourteenFramesToggle, false);
            preset.twelveFrames = GetToggleState(generator.twelveFramesToggle, false);
            preset.tenFrames = GetToggleState(generator.tenFramesToggle, false);
            preset.eightFrames = GetToggleState(generator.eightFramesToggle, false);
            preset.sixFrames = GetToggleState(generator.sixFramesToggle, false);
            preset.fourFrames = GetToggleState(generator.fourFramesToggle, false);
            preset.exportTileset = GetToggleState(generator.exportTilesetToggle, false);
            preset.flatColor = GetToggleState(generator.flatColorToggle, false);
            preset.extractShadow = GetToggleState(generator.extractShadowToggle, false);
            preset.extractEffects = GetToggleState(generator.extractEffectsToggle, false);
            preset.createAnimator = GetToggleState(generator.createAnimatorToggle, false);
        }

        private void ApplyExportSettings(GearPreset preset)
        {
            if (preset.exportSettingsVersion <= 0)
                return;

            SetToggleState(sliceSpritesheetToggle, preset.sliceSpritesheet);
            SetToggleState(staticIdleToggle, preset.staticIdle);

            SpritesheetGenerator generator = ResolveSpritesheetGenerator();
            if (generator == null)
                return;

            SetToggleState(generator.sliceSpritesheets, preset.sliceSpritesheet);
            SetToggleState(generator.staticIdleAnimation, preset.staticIdle);
            SetToggleState(generator.outlineToggle, preset.outlineEnabled);
            SetToggleState(generator.gradientOutlineToggle, preset.gradientOutlineEnabled);
            SetToggleState(generator.glowOutlineToggle, preset.glowOutlineEnabled);

            if (generator.glowThicknessSlider != null)
                generator.glowThicknessSlider.value = Mathf.Clamp(preset.glowThickness, 1, 20);
            else if (preset.glowThickness > 0)
                generator.glowThickness = preset.glowThickness;

            SetToggleState(generator.use128Toggle, preset.resolution128);
            SetToggleState(generator.use64Toggle, preset.resolution64);
            SetToggleState(generator.maxFramesToggle, preset.maxFrames);
            SetToggleState(generator.fourteenFramesToggle, preset.fourteenFrames);
            SetToggleState(generator.twelveFramesToggle, preset.twelveFrames);
            SetToggleState(generator.tenFramesToggle, preset.tenFrames);
            SetToggleState(generator.eightFramesToggle, preset.eightFrames);
            SetToggleState(generator.sixFramesToggle, preset.sixFrames);
            SetToggleState(generator.fourFramesToggle, preset.fourFrames);
            SetToggleState(generator.exportTilesetToggle, preset.exportTileset);
            SetToggleState(generator.flatColorToggle, preset.flatColor);
            SetToggleState(generator.extractShadowToggle, preset.extractShadow);
            SetToggleState(generator.extractEffectsToggle, preset.extractEffects);
            SetToggleState(generator.createAnimatorToggle, preset.createAnimator);
        }

        private SpritesheetGenerator ResolveSpritesheetGenerator()
        {
            if (spritesheetGenerator == null)
                spritesheetGenerator = FindAnyObjectByType<SpritesheetGenerator>();

            return spritesheetGenerator;
        }

        private bool GetToggleState(Toggle toggle, bool fallback)
        {
            return toggle != null ? toggle.isOn : fallback;
        }

        private void SetToggleState(Toggle toggle, bool value)
        {
            if (toggle != null)
                toggle.isOn = value;
        }



        /// <summary>
        /// Returns the GameObject.name of the first Toggle that’s on, or "None".
        /// </summary>
        private string GetActiveToggleName(Toggle[] arr)
        {
            if (arr == null)
                return "None";

            foreach (var tog in arr)
                if (tog != null && tog.isOn)
                    return tog.gameObject.name;
            return "None";
        }

        /// <summary>
        /// Finds the Toggle whose GameObject.name == name and turns it on (off all others).
        /// Falls back to "None" toggle, then to the first in the array.
        /// </summary>
        private void SetToggleByName(Toggle[] arr, string name)
        {
            if (arr == null)
                return;

            bool found = false;
            // 1) exact match
            foreach (var tog in arr)
            {
                if (tog == null)
                    continue;

                if (tog.gameObject.name == name)
                {
                    tog.isOn = true;
                    found = true;
                }
                else
                {
                    tog.isOn = false;
                }
            }
            if (found) return;

            // 2) "None"
            foreach (var tog in arr)
            {
                if (tog == null)
                    continue;

                if (tog.gameObject.name == "None")
                {
                    tog.isOn = true;
                    found = true;
                }
                else
                {
                    tog.isOn = false;
                }
            }
            if (found) return;

            // 3) fallback
            if (arr.Length > 0)
            {
                Toggle fallback = null;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] != null)
                    {
                        fallback = arr[i];
                        break;
                    }
                }

                if (fallback != null)
                {
                    fallback.isOn = true;
                    Debug.LogWarning($"Could not find toggle '{name}' or 'None'; defaulting to '{fallback.gameObject.name}'.");
                }
            }
        }

        private GameObject FindObjectByName(GameObject[] arr, string name)
        {
            foreach (var go in arr)
                if (go != null && go.name == name)
                    return go;
            return null;
        }

        private void ApplyIdleToPlayer(string idleName)
        {
            TopDownCharacterController controller = playerController != null
                ? playerController
                : FindAnyObjectByType<TopDownCharacterController>();

            if (controller == null) return;

            controller.ApplyIdleVariantImmediately(GetIdleVariantFromName(idleName));
        }

        private int GetIdleVariantFromName(string idleName)
        {
            if (string.IsNullOrEmpty(idleName)) return 1;

            if (idleName.Contains("4")) return 4;
            if (idleName.Contains("3")) return 3;
            if (idleName.Contains("2")) return 2;
            return 1;
        }

        private void InitializePresets()
        {
            BuildControllerLookup();

            #if UNITY_EDITOR
            activePresets = presetDatabase != null && presetDatabase.presets != null
                ? presetDatabase.presets
                : new List<GearPreset>();
            #else
            activePresets = LoadRuntimePresets();
            #endif
        }

        private string RuntimePresetPath => Path.Combine(Application.persistentDataPath, RuntimePresetFileName);

        private List<GearPreset> LoadRuntimePresets()
        {
            string path = RuntimePresetPath;
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    RuntimePresetCollection collection = JsonUtility.FromJson<RuntimePresetCollection>(json);
                    if (collection != null && collection.presets != null)
                    {
                        List<GearPreset> loaded = new List<GearPreset>();
                        foreach (RuntimeGearPreset runtimePreset in collection.presets)
                            loaded.Add(FromRuntimePreset(runtimePreset));
                        return loaded;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Could not load saved characters from '{path}': {ex.Message}");
                }
            }

            List<GearPreset> defaults = presetDatabase != null && presetDatabase.presets != null
                ? new List<GearPreset>(presetDatabase.presets)
                : new List<GearPreset>();

            SaveRuntimePresets(defaults);
            return defaults;
        }

        private void SaveRuntimePresets()
        {
            SaveRuntimePresets(activePresets);
        }

        private void SaveRuntimePresets(List<GearPreset> presets)
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);

                RuntimePresetCollection collection = new RuntimePresetCollection();
                foreach (GearPreset preset in presets)
                    collection.presets.Add(ToRuntimePreset(preset));

                string json = JsonUtility.ToJson(collection, true);
                File.WriteAllText(RuntimePresetPath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not save characters to '{RuntimePresetPath}': {ex.Message}");
            }
        }

        private RuntimeGearPreset ToRuntimePreset(GearPreset preset)
        {
            return new RuntimeGearPreset
            {
                bodyAnimatorName = GetControllerName(preset.bodyAnimator),
                bodyColor = preset.bodyColor,
                headAnimatorName = GetControllerName(preset.headAnimator),
                headColor = preset.headColor,
                chestAnimatorName = GetControllerName(preset.chestAnimator),
                chestColor = preset.chestColor,
                legsAnimatorName = GetControllerName(preset.legsAnimator),
                legsColor = preset.legsColor,
                shoesAnimatorName = GetControllerName(preset.shoesAnimator),
                shoesColor = preset.shoesColor,
                handsAnimatorName = GetControllerName(preset.handsAnimator),
                handsColor = preset.handsColor,
                beltAnimatorName = GetControllerName(preset.beltAnimator),
                beltColor = preset.beltColor,
                slashAnimatorName = GetControllerName(preset.slashAnimator),
                slashColor = preset.slashColor,
                effectAnimatorName = GetControllerName(preset.effectAnimator),
                effectColor = preset.effectColor,
                effect2AnimatorName = GetControllerName(preset.effect2Animator),
                effect2Color = preset.effect2Color,
                bodyVisible = preset.bodyVisible,
                headVisible = preset.headVisible,
                chestVisible = preset.chestVisible,
                legsVisible = preset.legsVisible,
                shoesVisible = preset.shoesVisible,
                handsVisible = preset.handsVisible,
                beltVisible = preset.beltVisible,
                slashVisible = preset.slashVisible,
                effectVisible = preset.effectVisible,
                effect2Visible = preset.effect2Visible,
                weaponName = preset.weaponName,
                backpackName = preset.backpackName,
                shieldName = preset.shieldName,
                backpackColor = preset.backpackColor,
                shieldColor = preset.shieldColor,
                mountColor = preset.mountColor,
                weaponColor = preset.weaponColor,
                shadowOpacityName = preset.shadowOpacityName,
                sliceSpritesheet = preset.sliceSpritesheet,
                staticIdle = preset.staticIdle,
                outlineName = preset.outlineName,
                resolutionName = preset.resolutionName,
                frameName = preset.frameName,
                exportSettingsVersion = preset.exportSettingsVersion,
                outlineEnabled = preset.outlineEnabled,
                gradientOutlineEnabled = preset.gradientOutlineEnabled,
                glowOutlineEnabled = preset.glowOutlineEnabled,
                glowThickness = preset.glowThickness,
                resolution128 = preset.resolution128,
                resolution64 = preset.resolution64,
                maxFrames = preset.maxFrames,
                fourteenFrames = preset.fourteenFrames,
                twelveFrames = preset.twelveFrames,
                tenFrames = preset.tenFrames,
                eightFrames = preset.eightFrames,
                sixFrames = preset.sixFrames,
                fourFrames = preset.fourFrames,
                exportTileset = preset.exportTileset,
                flatColor = preset.flatColor,
                extractShadow = preset.extractShadow,
                extractEffects = preset.extractEffects,
                createAnimator = preset.createAnimator,
                mountName = preset.mountName,
                idleName = preset.idleName,
                presetName = preset.presetName,
                presetDate = preset.presetDate
            };
        }

        private GearPreset FromRuntimePreset(RuntimeGearPreset preset)
        {
            return new GearPreset
            {
                bodyAnimator = ResolveController(preset.bodyAnimatorName),
                bodyColor = preset.bodyColor,
                headAnimator = ResolveController(preset.headAnimatorName),
                headColor = preset.headColor,
                chestAnimator = ResolveController(preset.chestAnimatorName),
                chestColor = preset.chestColor,
                legsAnimator = ResolveController(preset.legsAnimatorName),
                legsColor = preset.legsColor,
                shoesAnimator = ResolveController(preset.shoesAnimatorName),
                shoesColor = preset.shoesColor,
                handsAnimator = ResolveController(preset.handsAnimatorName),
                handsColor = preset.handsColor,
                beltAnimator = ResolveController(preset.beltAnimatorName),
                beltColor = preset.beltColor,
                slashAnimator = ResolveController(preset.slashAnimatorName),
                slashColor = preset.slashColor,
                effectAnimator = ResolveController(preset.effectAnimatorName),
                effectColor = preset.effectColor,
                effect2Animator = ResolveController(preset.effect2AnimatorName),
                effect2Color = preset.effect2Color,
                bodyVisible = preset.bodyVisible,
                headVisible = preset.headVisible,
                chestVisible = preset.chestVisible,
                legsVisible = preset.legsVisible,
                shoesVisible = preset.shoesVisible,
                handsVisible = preset.handsVisible,
                beltVisible = preset.beltVisible,
                slashVisible = preset.slashVisible,
                effectVisible = preset.effectVisible,
                effect2Visible = preset.effect2Visible,
                weaponName = preset.weaponName,
                backpackName = preset.backpackName,
                shieldName = preset.shieldName,
                backpackColor = preset.backpackColor,
                shieldColor = preset.shieldColor,
                mountColor = preset.mountColor,
                weaponColor = preset.weaponColor,
                shadowOpacityName = preset.shadowOpacityName,
                sliceSpritesheet = preset.sliceSpritesheet,
                staticIdle = preset.staticIdle,
                outlineName = preset.outlineName,
                resolutionName = preset.resolutionName,
                frameName = preset.frameName,
                exportSettingsVersion = preset.exportSettingsVersion,
                outlineEnabled = preset.outlineEnabled,
                gradientOutlineEnabled = preset.gradientOutlineEnabled,
                glowOutlineEnabled = preset.glowOutlineEnabled,
                glowThickness = preset.glowThickness,
                resolution128 = preset.resolution128,
                resolution64 = preset.resolution64,
                maxFrames = preset.maxFrames,
                fourteenFrames = preset.fourteenFrames,
                twelveFrames = preset.twelveFrames,
                tenFrames = preset.tenFrames,
                eightFrames = preset.eightFrames,
                sixFrames = preset.sixFrames,
                fourFrames = preset.fourFrames,
                exportTileset = preset.exportTileset,
                flatColor = preset.flatColor,
                extractShadow = preset.extractShadow,
                extractEffects = preset.extractEffects,
                createAnimator = preset.createAnimator,
                mountName = preset.mountName,
                idleName = preset.idleName,
                presetName = preset.presetName,
                presetDate = preset.presetDate
            };
        }

        private string GetControllerName(RuntimeAnimatorController controller)
        {
            return controller != null ? controller.name : string.Empty;
        }

        private RuntimeAnimatorController ResolveController(string controllerName)
        {
            if (string.IsNullOrEmpty(controllerName)) return null;

            if (controllersByName.TryGetValue(controllerName, out RuntimeAnimatorController controller))
                return controller;

            Debug.LogWarning($"Could not find animator controller '{controllerName}' while loading saved character.");
            return null;
        }

        private void BuildControllerLookup()
        {
            controllersByName.Clear();

            AddControllerToLookup(bodyAnimator != null ? bodyAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(headAnimator != null ? headAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(chestAnimator != null ? chestAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(legsAnimator != null ? legsAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(shoesAnimator != null ? shoesAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(handsAnimator != null ? handsAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(beltAnimator != null ? beltAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(slashAnimator != null ? slashAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(effectAnimator != null ? effectAnimator.runtimeAnimatorController : null);
            AddControllerToLookup(effect2Animator != null ? effect2Animator.runtimeAnimatorController : null);

            AnimatorGearSwitcher[] switchers = FindObjectsByType<AnimatorGearSwitcher>(FindObjectsInactive.Include);
            foreach (AnimatorGearSwitcher switcher in switchers)
            {
                if (switcher == null || switcher.animatorControllers == null) continue;

                foreach (RuntimeAnimatorController controller in switcher.animatorControllers)
                    AddControllerToLookup(controller);
            }

            if (presetDatabase == null || presetDatabase.presets == null) return;

            foreach (GearPreset preset in presetDatabase.presets)
            {
                AddControllerToLookup(preset.bodyAnimator);
                AddControllerToLookup(preset.headAnimator);
                AddControllerToLookup(preset.chestAnimator);
                AddControllerToLookup(preset.legsAnimator);
                AddControllerToLookup(preset.shoesAnimator);
                AddControllerToLookup(preset.handsAnimator);
                AddControllerToLookup(preset.beltAnimator);
                AddControllerToLookup(preset.slashAnimator);
                AddControllerToLookup(preset.effectAnimator);
                AddControllerToLookup(preset.effect2Animator);
            }
        }

        private void AddControllerToLookup(RuntimeAnimatorController controller)
        {
            if (controller == null || string.IsNullOrEmpty(controller.name)) return;
            if (!controllersByName.ContainsKey(controller.name))
                controllersByName.Add(controller.name, controller);
        }

        private void RefreshPresetButtons()
        {
            // Clear all existing buttons
            foreach (var btn in presetButtons)
                Destroy(btn.gameObject);
            presetButtons.Clear();

            // Re-create only those that should be shown
            for (int i = 0; i < activePresets.Count; i++)
            {
                var preset = activePresets[i];

                // Skip if it's an Example and we're hiding them
                if (showExamplesToggle != null && !showExamplesToggle.isOn && !string.IsNullOrEmpty(preset.presetDate) && preset.presetDate.StartsWith("Example"))
                    continue;

                CreateButtonForPreset(i);
            }
        }


    }
}
