
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using TMPro;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D.Sprites;
#endif

namespace SmallScaleInc.CharacterCreatorFantasy
{
    public class SpritesheetGenerator : MonoBehaviour
    {
        private string parentFolder = "SmallScaleInt/Character creator - Fantasy/Spritesheets";

        private Dictionary<TextMeshProUGUI, string> lastTMPValues = new Dictionary<TextMeshProUGUI, string>();
        private Dictionary<TextMeshProUGUI, Coroutine> runningAnimations = new Dictionary<TextMeshProUGUI, Coroutine>();
        private Dictionary<TextMeshProUGUI, Vector3> originalScales = new Dictionary<TextMeshProUGUI, Vector3>();


        // --- Gear Slot References (Shoes, Chest, belt, hands, Legs, Head) ---
        [Header("Gear Animators")]
        public Animator shoesAnimator;
        public Animator chestAnimator;
        public Animator beltAnimator;
        public Animator handsAnimator;
        public Animator legsAnimator;
        public Animator headAnimator;
        
        [Header("Gear Sprite Renderers")]
        public SpriteRenderer shoesRenderer;
        public SpriteRenderer chestRenderer;
        public SpriteRenderer beltRenderer;
        public SpriteRenderer handsRenderer;
        public SpriteRenderer legsRenderer;
        public SpriteRenderer headRenderer;

        [Header("Gear UI TMP (Names)")]
        public TextMeshProUGUI shoesGearNameText;
        public TextMeshProUGUI chestGearNameText;
        public TextMeshProUGUI beltGearNameText;
        public TextMeshProUGUI handsGearNameText;
        public TextMeshProUGUI legsGearNameText;
        public TextMeshProUGUI headGearNameText;

        [Header("Right panel Gear Names")] //This get auto updated according to the above names.
        public TextMeshProUGUI shoesGearNameText2;
        public TextMeshProUGUI chestGearNameText2;
        public TextMeshProUGUI beltGearNameText2;
        public TextMeshProUGUI handsGearNameText2;
        public TextMeshProUGUI legsGearNameText2;
        public TextMeshProUGUI headGearNameText2;
        // public TextMeshProUGUI skinNameText2;
        public TextMeshProUGUI slashNameText2;
        public TextMeshProUGUI effectNameText2;
        public TextMeshProUGUI effect2NameText2;
        
        [Header("Gear UI TMP (Colors)")]
        public TextMeshProUGUI shoesColorText;
        public TextMeshProUGUI chestColorText;
        public TextMeshProUGUI beltColorText;
        public TextMeshProUGUI handsColorText;
        public TextMeshProUGUI legsColorText;
        public TextMeshProUGUI headColorText;

        // --- Weapon Slot Section ---
        [System.Serializable]
        public class Weapon
        {
            public GameObject weaponGO;            // The weapon GameObject.
            public Animator animator;              // Weapon animator.
            public SpriteRenderer spriteRenderer;  // Weapon sprite renderer.
        }
        
        [Header("Weapon Setup")]
        public Weapon[] weapons;  // Array of all available weapons.
        
        [Header("Weapon UI TMP")]
        public TextMeshProUGUI weaponNameText;
        public TextMeshProUGUI weaponColorText;
        
        // --- Backpack Slot Section ---
        [System.Serializable]
        public class Backpack
        {
            public GameObject backpackGO;            // The backpack GameObject.
            public Animator animator;                // Backpack animator.
            public SpriteRenderer spriteRenderer;    // Backpack sprite renderer.
        }
        
        [Header("Backpack Setup")]
        public Backpack[] backpacks;  // Array of all available backpacks.
        
        [Header("Backpack UI TMP")]
        public TextMeshProUGUI backpackNameText;
        public TextMeshProUGUI backpackColorText;
        
        // --- Shield Slot Section ---
        [System.Serializable]
        public class Shield
        {
            public GameObject shieldGO;            // The shield GameObject.
            public Animator animator;              // Shield animator.
            public SpriteRenderer spriteRenderer;  // Shield sprite renderer.
        }
        
        [Header("Shield Setup")]
        public Shield[] shields;  // Array of all available shields.
        
        [Header("Shield UI TMP")]
        public TextMeshProUGUI shieldNameText;
        public TextMeshProUGUI shieldColorText;

        // --- Mount Slot Section ---
        [System.Serializable]
        public class Mount
        {
            public GameObject mountGO;            // The mount GameObject.
            public Animator animator;             // Mount animator.
            public SpriteRenderer spriteRenderer; // Mount sprite renderer.
        }

        [Header("Mount Setup")]
        public Mount[] mounts;  // Array of all available mounts.

        [Header("Mount UI TMP")]
        public TextMeshProUGUI mountNameText;
        public TextMeshProUGUI mountColorText;

        [Header("Skin Color Setup")]
        public SpriteRenderer skinColorRenderer;   // The skin's SpriteRenderer.
        public TextMeshProUGUI skinColorText;        // TMP to display the skin color hex.
        public TextMeshProUGUI skinNameText;
        public Animator skinAnimator;
        public Toggle skinToggle; //If false, no skin will be visable or included in the generated spritesheets. 

        [Header("Shadow Setup")]
        public Animator shadowAnimator;
        public SpriteRenderer shadowRenderer;
        public TextMeshProUGUI shadowGearNameText;
        public TextMeshProUGUI shadowColorText;

        [Header("Slash Setup")]
        public Animator slashAnimator;
        public SpriteRenderer slashRenderer;
        public TextMeshProUGUI slashNameText;
        public TextMeshProUGUI slashColorText;

        // New toggle to enable/disable slash display. if true the slash 
        // will be included in the generated spritesheets and extra spritesheets will be generated.
        public Toggle slashToggle;

        [Header("Effect Setup")]
        public Animator effectAnimator;
        public SpriteRenderer effectRenderer;
        public TextMeshProUGUI effectNameText;
        public TextMeshProUGUI effectColorText;

        public Animator effect2Animator;
        public SpriteRenderer effect2Renderer;
        public TextMeshProUGUI effect2NameText;
        public TextMeshProUGUI effect2ColorText;

        public Toggle effectToggle;
        public Toggle effect2Toggle;

        [Header("Load Screen UI")]
        public GameObject loadScreenPanel;
        public Slider loadProgressSlider;
        public TextMeshProUGUI currentlyGeneratingTMP;

        public Toggle sliceSpritesheets; // If true, slice the generated spritesheets

        public Toggle staticIdleAnimation; // If on, idle animations will be made static.
        public Toggle maxFramesToggle;   // 15 frames (default)
        public Toggle fourteenFramesToggle;   // 14 frames
        public Toggle twelveFramesToggle;   // 12 frames
        public Toggle tenFramesToggle;   // 10 frames
        public Toggle eightFramesToggle; // 8 frames
        public Toggle sixFramesToggle; // 6 frames
        public Toggle fourFramesToggle;  // 4 frames

        [Header("Outline")]
        public Toggle outlineToggle;  // If on, draw a 1px black outline around the character
        public Toggle gradientOutlineToggle;

        [Header("Glow Settings")]
        public Toggle glowOutlineToggle;
        public Slider glowThicknessSlider;
        public TextMeshProUGUI glowThicknessLabel;
        public int glowThickness = 4; // default


        public TextMeshProUGUI outlineColorText; //Outline color

        [Header("Outline Preview")]
        public bool enableOutlinePreview = true;
        public SpriteRenderer outlinePreviewRenderer;
        

        [Header("Outline Gradient brightness = 0 = pure inverted colour; 1 = pure white")]
        public float gradientBrightness = 0.2f;

        public Toggle use128Toggle;
        public Toggle use64Toggle;
        public Toggle exportTilesetToggle;
        public Toggle flatColorToggle;

        [Header("Extracted Export Layers")]
        public Toggle extractShadowToggle;
        public Toggle extractEffectsToggle;

        [Header("Live Preview Resolution")]
        public Transform livePreviewRoot;
        public bool enableHalfResolutionLivePreview = true;
        public bool enableFlatColorLivePreview = true;
        public bool enableFrameCountLivePreview = true;


        [Header("Character name input")]
        public TMP_InputField characterNameInput;  // this is what the character will be named.

        [Header("Animation Toggles")]
        public Toggle attack1Toggle;
        public Toggle attack2Toggle;
        public Toggle attack3Toggle;
        public Toggle attack4Toggle;
        public Toggle attack5Toggle;
        public Toggle attack6Toggle;
        public Toggle kickToggle;
        public Toggle rollingToggle;
        public Toggle slideToggle;

        public Toggle attackRunToggle;
        public Toggle attackRun2Toggle;
        public Toggle rideIdleAttack1Toggle;
        public Toggle rideRunAttack1Toggle;
        public Toggle special1Toggle;

        public Toggle crouchIdleToggle;
        public Toggle crouchRunToggle;
        public Toggle dieToggle;
        public Toggle idleToggle;
        public Toggle idle2Toggle;
        public Toggle idle3Toggle;
        public Toggle idle4Toggle;

        public Toggle runToggle;
        public Toggle runBackwardsToggle;
        public Toggle strafeLeftToggle;
        public Toggle strafeRightToggle;
        public Toggle walkToggle;
        public Toggle rideIdleToggle;
        public Toggle rideRunToggle;

        public Toggle takeDamageToggle;
        public Toggle tauntToggle;

        [Header("Animator Generation")]
        public Toggle createAnimatorToggle;
        public AnimatorClipBuilder animatorClipBuilder; // assign in inspector

        [Header("Player Build Visibility")]
        public List<GameObject> hideInPlayerBuild = new List<GameObject>();

        private Texture2D outlinePreviewTexture;
        private Sprite outlinePreviewSprite;
        private bool ownsOutlinePreviewRenderer;
        private bool suppressOutlinePreview;
        private int outlinePreviewWidth;
        private int outlinePreviewHeight;
        private readonly List<PreviewLayer> outlinePreviewLayers = new List<PreviewLayer>();
        private readonly HashSet<Texture2D> unreadableOutlinePreviewTextures = new HashSet<Texture2D>();
        private readonly Dictionary<Sprite, Sprite> halfResolutionSprites = new Dictionary<Sprite, Sprite>();
        private readonly Dictionary<Sprite, Sprite> fullResolutionSprites = new Dictionary<Sprite, Sprite>();
        private readonly Dictionary<Sprite, Texture2D> halfResolutionTextures = new Dictionary<Sprite, Texture2D>();
        private readonly Dictionary<Sprite, Sprite> flatPreviewSprites = new Dictionary<Sprite, Sprite>();
        private readonly Dictionary<Sprite, Sprite> flatHalfResolutionSprites = new Dictionary<Sprite, Sprite>();
        private readonly Dictionary<Sprite, Texture2D> flatPreviewTextures = new Dictionary<Sprite, Texture2D>();
        private readonly Dictionary<SpriteRenderer, FrameReductionPreviewState> frameReductionPreviewStates = new Dictionary<SpriteRenderer, FrameReductionPreviewState>();

        private struct PreviewLayer
        {
            public SpriteRenderer renderer;
            public Color tint;
        }

        private class FrameReductionPreviewState
        {
            public Sprite heldSourceSprite;
            public Texture2D texture;
            public int row = -1;
            public int column = -1;
        }




        void Awake()
        {
            ApplyPlayerBuildVisibility();
            CacheInitialTMPValues();
            UpdateGearUI();
            if (glowThicknessSlider != null)
            {
                glowThicknessSlider.minValue = 1;
                glowThicknessSlider.maxValue = 20;
                glowThicknessSlider.value = glowThickness;

                UpdateGlowThicknessUI(glowThicknessSlider.value);
                glowThicknessSlider.onValueChanged.AddListener(UpdateGlowThicknessUI);
            }
        }

        private void ApplyPlayerBuildVisibility()
        {
#if !UNITY_EDITOR
            HideBuildOnlyObject(sliceSpritesheets != null ? sliceSpritesheets.gameObject : null);
            HideBuildOnlyObject(createAnimatorToggle != null ? createAnimatorToggle.gameObject : null);

            if (sliceSpritesheets != null)
                sliceSpritesheets.isOn = false;

            if (createAnimatorToggle != null)
                createAnimatorToggle.isOn = false;

            if (animatorClipBuilder != null)
            {
                animatorClipBuilder.includePlayerPrefab = false;
                animatorClipBuilder.shouldPreviewCharacter = false;
                if (animatorClipBuilder.previewCanvas != null)
                    animatorClipBuilder.previewCanvas.SetActive(false);
                if (animatorClipBuilder.showShadowPreviewToggle != null)
                    animatorClipBuilder.showShadowPreviewToggle.gameObject.SetActive(false);
                if (animatorClipBuilder.showEffectsPreviewToggle != null)
                    animatorClipBuilder.showEffectsPreviewToggle.gameObject.SetActive(false);
            }

            for (int i = 0; i < hideInPlayerBuild.Count; i++)
                HideBuildOnlyObject(hideInPlayerBuild[i]);
#endif
        }

        private void HideBuildOnlyObject(GameObject target)
        {
            if (target != null)
                target.SetActive(false);
        }

        void Start()
        {
            SyncDisplayToggleListeners(slashToggle);
            SyncDisplayToggleListeners(effectToggle);
            SyncDisplayToggleListeners(effect2Toggle);
        }

        void Update()
        {
            UpdateGearUI();
        }

        void LateUpdate()
        {
            UpdateLivePreviewResolutionSprites();
            UpdateOutlinePreview();
        }

        void OnDestroy()
        {
            if (outlinePreviewSprite != null)
                Destroy(outlinePreviewSprite);

            if (outlinePreviewTexture != null)
                Destroy(outlinePreviewTexture);

            DestroyHalfResolutionPreviewAssets();
            DestroyFlatPreviewAssets();

            if (ownsOutlinePreviewRenderer && outlinePreviewRenderer != null)
                Destroy(outlinePreviewRenderer.gameObject);
        }

        private void UpdateLivePreviewResolutionSprites()
        {
            Transform target = ResolveLivePreviewRoot();
            if (target == null)
                return;

            bool halfResolution = enableHalfResolutionLivePreview && use64Toggle != null && use64Toggle.isOn;
            bool flatColor = enableFlatColorLivePreview && flatColorToggle != null && flatColorToggle.isOn;
            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                ApplyLivePreviewSpriteEffects(renderers[i], halfResolution, flatColor);
        }

        private Transform ResolveLivePreviewRoot()
        {
            if (livePreviewRoot != null)
                return livePreviewRoot;

            if (skinColorRenderer != null)
                livePreviewRoot = skinColorRenderer.transform;

            return livePreviewRoot;
        }

        private void ApplyLivePreviewSpriteEffects(SpriteRenderer renderer, bool halfResolution, bool flatColor)
        {
            if (renderer == null || renderer == outlinePreviewRenderer || renderer.sprite == null)
                return;

            Sprite sourceSprite = GetFullResolutionSprite(renderer.sprite);
            sourceSprite = ApplyLivePreviewFrameReduction(renderer, sourceSprite);
            Sprite targetSprite = sourceSprite;
            bool canApplyFlatColor = flatColor && renderer != shadowRenderer;

            if (canApplyFlatColor)
                targetSprite = GetFlatPreviewSprite(sourceSprite, halfResolution);
            else if (halfResolution)
                targetSprite = GetHalfResolutionSprite(sourceSprite);

            if (targetSprite != null && renderer.sprite != targetSprite)
                renderer.sprite = targetSprite;
        }

        private Sprite ApplyLivePreviewFrameReduction(SpriteRenderer renderer, Sprite sourceSprite)
        {
            if (!enableFrameCountLivePreview || sourceSprite == null)
            {
                frameReductionPreviewStates.Remove(renderer);
                return sourceSprite;
            }

            int targetColumns = GetTargetColumns();
            if (targetColumns >= 15)
            {
                frameReductionPreviewStates.Remove(renderer);
                return sourceSprite;
            }

            if (!TryGetSpriteSheetCell(sourceSprite, out int row, out int column))
                return sourceSprite;

            if (!frameReductionPreviewStates.TryGetValue(renderer, out FrameReductionPreviewState state))
            {
                state = new FrameReductionPreviewState();
                frameReductionPreviewStates[renderer] = state;
            }

            bool changedRowOrTexture = state.texture != sourceSprite.texture || state.row != row;
            bool includeFrame = IsFrameColumnIncluded(column, targetColumns);

            if (includeFrame || state.heldSourceSprite == null || changedRowOrTexture)
                state.heldSourceSprite = sourceSprite;

            state.texture = sourceSprite.texture;
            state.row = row;
            state.column = column;

            return state.heldSourceSprite != null ? state.heldSourceSprite : sourceSprite;
        }

        private bool TryGetSpriteSheetCell(Sprite sprite, out int row, out int column)
        {
            row = 0;
            column = 0;

            Rect rect = sprite.rect;
            if (rect.width <= 0f || rect.height <= 0f)
                return false;

            column = Mathf.RoundToInt(rect.x / rect.width);
            row = Mathf.RoundToInt(rect.y / rect.height);
            return column >= 0 && row >= 0;
        }

        private bool IsFrameColumnIncluded(int column, int targetColumns)
        {
            switch (targetColumns)
            {
                case 14:
                    return column >= 0 && column <= 13;
                case 12:
                    return column == 0 || column == 1 || column == 2 || column == 3 ||
                           column == 4 || column == 6 || column == 7 || column == 8 ||
                           column == 9 || column == 10 || column == 12 || column == 13;
                case 10:
                    return column == 0 || column == 1 || column == 3 || column == 4 ||
                           column == 6 || column == 7 || column == 9 || column == 10 ||
                           column == 12 || column == 13;
                case 8:
                    return column == 0 || column == 2 || column == 4 || column == 6 ||
                           column == 8 || column == 10 || column == 12 || column == 14;
                case 6:
                    return column == 0 || column == 3 || column == 6 ||
                           column == 9 || column == 12 || column == 14;
                case 4:
                    return column == 0 || column == 4 || column == 8 || column == 12;
                default:
                    return true;
            }
        }

        private Sprite GetFullResolutionSprite(Sprite sprite)
        {
            return fullResolutionSprites.TryGetValue(sprite, out Sprite fullResolutionSprite)
                ? fullResolutionSprite
                : sprite;
        }

        private Sprite GetHalfResolutionSprite(Sprite sourceSprite)
        {
            if (sourceSprite == null)
                return null;

            if (halfResolutionSprites.TryGetValue(sourceSprite, out Sprite cachedSprite))
                return cachedSprite;

            int sourceWidth;
            int sourceHeight;
            Color[] sourcePixels = GetSpriteFramePixels(sourceSprite, out sourceWidth, out sourceHeight);
            if (sourcePixels == null)
                return null;

            int targetWidth = Mathf.Max(1, sourceWidth / 2);
            int targetHeight = Mathf.Max(1, sourceHeight / 2);
            Color[] targetPixels = new Color[targetWidth * targetHeight];

            for (int y = 0; y < targetHeight; y++)
            {
                int sourceY = Mathf.Min(sourceHeight - 1, y * 2);
                for (int x = 0; x < targetWidth; x++)
                {
                    int sourceX = Mathf.Min(sourceWidth - 1, x * 2);
                    targetPixels[y * targetWidth + x] = sourcePixels[sourceY * sourceWidth + sourceX];
                }
            }

            Texture2D texture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(targetPixels);
            texture.Apply();

            Vector2 halfPivot = sourceSprite.pivot * 0.5f;
            Sprite halfResolutionSprite = Sprite.Create(
                texture,
                new Rect(0, 0, targetWidth, targetHeight),
                new Vector2(halfPivot.x / targetWidth, halfPivot.y / targetHeight),
                sourceSprite.pixelsPerUnit * 0.5f);

            halfResolutionSprite.name = sourceSprite.name + "_LivePreview64";
            halfResolutionSprites[sourceSprite] = halfResolutionSprite;
            fullResolutionSprites[halfResolutionSprite] = sourceSprite;
            halfResolutionTextures[halfResolutionSprite] = texture;
            return halfResolutionSprite;
        }

        private Sprite GetFlatPreviewSprite(Sprite sourceSprite, bool halfResolution)
        {
            if (sourceSprite == null)
                return null;

            Dictionary<Sprite, Sprite> cache = halfResolution ? flatHalfResolutionSprites : flatPreviewSprites;
            if (cache.TryGetValue(sourceSprite, out Sprite cachedSprite))
                return cachedSprite;

            int sourceWidth;
            int sourceHeight;
            Color[] sourcePixels = GetSpriteFramePixels(sourceSprite, out sourceWidth, out sourceHeight);
            if (sourcePixels == null)
                return null;

            int targetWidth = halfResolution ? Mathf.Max(1, sourceWidth / 2) : sourceWidth;
            int targetHeight = halfResolution ? Mathf.Max(1, sourceHeight / 2) : sourceHeight;
            Color[] targetPixels = new Color[targetWidth * targetHeight];

            for (int y = 0; y < targetHeight; y++)
            {
                int sourceY = halfResolution ? Mathf.Min(sourceHeight - 1, y * 2) : y;
                for (int x = 0; x < targetWidth; x++)
                {
                    int sourceX = halfResolution ? Mathf.Min(sourceWidth - 1, x * 2) : x;
                    Color sourcePixel = sourcePixels[sourceY * sourceWidth + sourceX];
                    targetPixels[y * targetWidth + x] = sourcePixel.a > 0.01f
                        ? new Color(1f, 1f, 1f, sourcePixel.a)
                        : new Color(0f, 0f, 0f, 0f);
                }
            }

            Texture2D texture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(targetPixels);
            texture.Apply();

            Vector2 pivot = halfResolution ? sourceSprite.pivot * 0.5f : sourceSprite.pivot;
            float pixelsPerUnit = halfResolution ? sourceSprite.pixelsPerUnit * 0.5f : sourceSprite.pixelsPerUnit;
            Sprite flatSprite = Sprite.Create(
                texture,
                new Rect(0, 0, targetWidth, targetHeight),
                new Vector2(pivot.x / targetWidth, pivot.y / targetHeight),
                pixelsPerUnit);

            flatSprite.name = sourceSprite.name + (halfResolution ? "_LivePreviewFlat64" : "_LivePreviewFlat");
            cache[sourceSprite] = flatSprite;
            fullResolutionSprites[flatSprite] = sourceSprite;
            flatPreviewTextures[flatSprite] = texture;
            return flatSprite;
        }

        private Color[] GetSpriteFramePixels(Sprite sprite, out int width, out int height)
        {
            Rect spriteRect = sprite.rect;
            int x = Mathf.RoundToInt(spriteRect.x);
            int y = Mathf.RoundToInt(spriteRect.y);
            width = Mathf.RoundToInt(spriteRect.width);
            height = Mathf.RoundToInt(spriteRect.height);

            if (width <= 0 || height <= 0)
                return null;

            if (unreadableOutlinePreviewTextures.Contains(sprite.texture))
                return ReadSpritePixelsFromRenderTexture(sprite, x, y, width, height);

            try
            {
                return sprite.texture.GetPixels(x, y, width, height);
            }
            catch (UnityException)
            {
                unreadableOutlinePreviewTextures.Add(sprite.texture);
                return ReadSpritePixelsFromRenderTexture(sprite, x, y, width, height);
            }
        }

        private void DestroyHalfResolutionPreviewAssets()
        {
            foreach (Sprite sprite in halfResolutionSprites.Values)
            {
                if (sprite != null)
                    Destroy(sprite);
            }

            foreach (Texture2D texture in halfResolutionTextures.Values)
            {
                if (texture != null)
                    Destroy(texture);
            }

            halfResolutionSprites.Clear();
            fullResolutionSprites.Clear();
            halfResolutionTextures.Clear();
        }

        private void DestroyFlatPreviewAssets()
        {
            foreach (Sprite sprite in flatPreviewSprites.Values)
            {
                if (sprite != null)
                    Destroy(sprite);
            }

            foreach (Sprite sprite in flatHalfResolutionSprites.Values)
            {
                if (sprite != null)
                    Destroy(sprite);
            }

            foreach (Texture2D texture in flatPreviewTextures.Values)
            {
                if (texture != null)
                    Destroy(texture);
            }

            flatPreviewSprites.Clear();
            flatHalfResolutionSprites.Clear();
            flatPreviewTextures.Clear();
        }

        private void SyncDisplayToggleListeners(Toggle toggle)
        {
            if (toggle != null)
                toggle.onValueChanged.Invoke(toggle.isOn);
        }

        private void SyncRendererToToggle(Toggle toggle, Renderer renderer)
        {
            if (toggle != null && renderer != null)
                renderer.enabled = toggle.isOn;
        }

        private void UpdateOutlinePreview()
        {
            if (suppressOutlinePreview || !enableOutlinePreview || !IsAnyOutlineEnabled())
            {
                SetOutlinePreviewVisible(false);
                return;
            }

            SpriteRenderer reference = GetOutlinePreviewReferenceRenderer();
            if (reference == null || reference.sprite == null)
            {
                SetOutlinePreviewVisible(false);
                return;
            }

            int width = Mathf.RoundToInt(reference.sprite.rect.width);
            int height = Mathf.RoundToInt(reference.sprite.rect.height);
            if (width <= 0 || height <= 0)
            {
                SetOutlinePreviewVisible(false);
                return;
            }

            BuildOutlinePreviewLayers();
            if (outlinePreviewLayers.Count == 0)
            {
                SetOutlinePreviewVisible(false);
                return;
            }

            EnsureOutlinePreviewRenderer(reference);
            EnsureOutlinePreviewTexture(reference, width, height);

            Color[] bodyPixels = new Color[width * height];
            Color[] transparentPixels = new Color[width * height];
            Vector2 referencePivot = reference.sprite.pivot;

            for (int i = 0; i < outlinePreviewLayers.Count; i++)
                CompositePreviewLayer(outlinePreviewLayers[i], bodyPixels, width, height, referencePivot);

            Color[] outlinedPixels = BuildOutlinePreviewPixels(transparentPixels, bodyPixels, width, height);
            outlinePreviewTexture.SetPixels(outlinedPixels);
            outlinePreviewTexture.Apply();

            UpdateOutlinePreviewSorting();
            SetOutlinePreviewVisible(true);
        }

        private bool IsAnyOutlineEnabled()
        {
            return (outlineToggle != null && outlineToggle.isOn) ||
                   (gradientOutlineToggle != null && gradientOutlineToggle.isOn) ||
                   (glowOutlineToggle != null && glowOutlineToggle.isOn);
        }

        private void SetOutlinePreviewVisible(bool visible)
        {
            if (outlinePreviewRenderer != null)
                outlinePreviewRenderer.enabled = visible;
        }

        public void SetOutlinePreviewSuppressed(bool suppressed)
        {
            suppressOutlinePreview = suppressed;
            if (suppressed)
                SetOutlinePreviewVisible(false);
        }

        private SpriteRenderer GetOutlinePreviewReferenceRenderer()
        {
            if (IsRendererUsableForOutlinePreview(skinColorRenderer))
                return skinColorRenderer;

            BuildOutlinePreviewLayers();
            return outlinePreviewLayers.Count > 0 ? outlinePreviewLayers[0].renderer : null;
        }

        private void EnsureOutlinePreviewRenderer(SpriteRenderer reference)
        {
            if (outlinePreviewRenderer == null)
            {
                GameObject go = new GameObject("Outline Preview");
                outlinePreviewRenderer = go.AddComponent<SpriteRenderer>();
                ownsOutlinePreviewRenderer = true;
            }

            Transform previewTransform = outlinePreviewRenderer.transform;
            Transform referenceTransform = reference.transform;

            if (previewTransform.parent != referenceTransform.parent)
                previewTransform.SetParent(referenceTransform.parent, false);

            previewTransform.localPosition = referenceTransform.localPosition;
            previewTransform.localRotation = referenceTransform.localRotation;
            previewTransform.localScale = referenceTransform.localScale;

            outlinePreviewRenderer.sortingLayerID = reference.sortingLayerID;
            outlinePreviewRenderer.color = Color.white;
        }

        private void EnsureOutlinePreviewTexture(SpriteRenderer reference, int width, int height)
        {
            bool needsTexture = outlinePreviewTexture == null ||
                                outlinePreviewWidth != width ||
                                outlinePreviewHeight != height;

            if (!needsTexture)
                return;

            if (outlinePreviewSprite != null)
                Destroy(outlinePreviewSprite);
            if (outlinePreviewTexture != null)
                Destroy(outlinePreviewTexture);

            outlinePreviewWidth = width;
            outlinePreviewHeight = height;
            outlinePreviewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            outlinePreviewTexture.filterMode = FilterMode.Point;
            outlinePreviewTexture.wrapMode = TextureWrapMode.Clamp;

            Vector2 normalizedPivot = new Vector2(
                reference.sprite.pivot.x / width,
                reference.sprite.pivot.y / height);

            outlinePreviewSprite = Sprite.Create(
                outlinePreviewTexture,
                new Rect(0, 0, width, height),
                normalizedPivot,
                reference.sprite.pixelsPerUnit);

            outlinePreviewRenderer.sprite = outlinePreviewSprite;
        }

        private void BuildOutlinePreviewLayers()
        {
            outlinePreviewLayers.Clear();
            AddCurrentBodyGearRenderers(outlinePreviewLayers);
            SortPreviewLayers(outlinePreviewLayers);
        }

        private void AddCurrentBodyGearRenderers(List<PreviewLayer> layers)
        {
            if (skinToggle == null || skinToggle.isOn)
                AddPreviewLayer(layers, skinColorRenderer);

            AddPreviewLayer(layers, shoesRenderer);
            AddPreviewLayer(layers, legsRenderer);

            Mount activeMount = GetActiveMount();
            if (activeMount != null)
                AddPreviewLayer(layers, activeMount.spriteRenderer);

            AddPreviewLayer(layers, chestRenderer);
            AddPreviewLayer(layers, beltRenderer);
            AddPreviewLayer(layers, handsRenderer);

            Shield activeShield = GetActiveShield();
            if (activeShield != null)
                AddPreviewLayer(layers, activeShield.spriteRenderer);

            Weapon activeWeapon = GetActiveWeapon();
            if (activeWeapon != null && !IsMagicWeaponSelected())
                AddPreviewLayer(layers, activeWeapon.spriteRenderer);

            Backpack activeBackpack = GetActiveBackpack();
            if (activeBackpack != null)
                AddPreviewLayer(layers, activeBackpack.spriteRenderer);

            AddPreviewLayer(layers, headRenderer);
        }

        private void AddPreviewLayer(List<PreviewLayer> layers, SpriteRenderer renderer)
        {
            if (!IsRendererUsableForOutlinePreview(renderer))
                return;

            layers.Add(new PreviewLayer
            {
                renderer = renderer,
                tint = renderer.color
            });
        }

        private bool IsRendererUsableForOutlinePreview(SpriteRenderer renderer)
        {
            return renderer != null &&
                   renderer.enabled &&
                   renderer.gameObject.activeInHierarchy &&
                   renderer.sprite != null;
        }

        private bool IsMagicWeaponSelected()
        {
            return weaponNameText != null &&
                   !string.IsNullOrEmpty(weaponNameText.text) &&
                   weaponNameText.text.StartsWith("Magic");
        }

        private void SortPreviewLayers(List<PreviewLayer> layers)
        {
            for (int i = 1; i < layers.Count; i++)
            {
                PreviewLayer layer = layers[i];
                int j = i - 1;
                while (j >= 0 && ComparePreviewLayers(layers[j], layer) > 0)
                {
                    layers[j + 1] = layers[j];
                    j--;
                }

                layers[j + 1] = layer;
            }
        }

        private int ComparePreviewLayers(PreviewLayer a, PreviewLayer b)
        {
            int aLayer = SortingLayer.GetLayerValueFromID(a.renderer.sortingLayerID);
            int bLayer = SortingLayer.GetLayerValueFromID(b.renderer.sortingLayerID);
            if (aLayer != bLayer)
                return aLayer.CompareTo(bLayer);

            return a.renderer.sortingOrder.CompareTo(b.renderer.sortingOrder);
        }

        private void CompositePreviewLayer(
            PreviewLayer layer,
            Color[] targetPixels,
            int targetWidth,
            int targetHeight,
            Vector2 referencePivot)
        {
            Sprite sprite = layer.renderer.sprite;
            int sourceWidth;
            int sourceHeight;
            Color[] sourcePixels = GetSpritePixels(sprite, out sourceWidth, out sourceHeight);
            if (sourcePixels == null)
                return;

            bool flatColorMode = flatColorToggle != null && flatColorToggle.isOn;
            Vector2 offset = GetSpriteTextureRectOffset(sprite);
            int destinationX = Mathf.RoundToInt(referencePivot.x - sprite.pivot.x + offset.x);
            int destinationY = Mathf.RoundToInt(referencePivot.y - sprite.pivot.y + offset.y);

            for (int y = 0; y < sourceHeight; y++)
            {
                int targetY = destinationY + y;
                if (targetY < 0 || targetY >= targetHeight)
                    continue;

                for (int x = 0; x < sourceWidth; x++)
                {
                    int targetX = destinationX + x;
                    if (targetX < 0 || targetX >= targetWidth)
                        continue;

                    Color pixel = sourcePixels[y * sourceWidth + x];
                    if (pixel.a <= 0f)
                        continue;

                    if (flatColorMode)
                    {
                        pixel = new Color(
                            layer.tint.r,
                            layer.tint.g,
                            layer.tint.b,
                            pixel.a * layer.tint.a);
                    }
                    else
                    {
                        pixel *= layer.tint;
                    }
                    int targetIndex = targetY * targetWidth + targetX;
                    targetPixels[targetIndex] = AlphaBlend(targetPixels[targetIndex], pixel);
                }
            }
        }

        private Color[] BuildOutlinePreviewPixels(Color[] transparentPixels, Color[] bodyPixels, int width, int height)
        {
            if (gradientOutlineToggle != null && gradientOutlineToggle.isOn)
                return AddGradientOutline(transparentPixels, bodyPixels, width, height, gradientBrightness);

            if (glowOutlineToggle != null && glowOutlineToggle.isOn)
            {
                Color glowColor = Color.black;
                if (outlineColorText != null)
                    ColorUtility.TryParseHtmlString(outlineColorText.text, out glowColor);

                return AddGlowOutline(transparentPixels, bodyPixels, width, height, glowColor, glowThickness);
            }

            Color outlineColor = Color.black;
            if (outlineColorText != null)
                ColorUtility.TryParseHtmlString(outlineColorText.text, out outlineColor);

            return AddOutlineMask(transparentPixels, bodyPixels, width, height, outlineColor);
        }

        private Color[] GetSpritePixels(Sprite sprite, out int width, out int height)
        {
            Rect textureRect = GetSpriteTextureRect(sprite);
            int x = Mathf.RoundToInt(textureRect.x);
            int y = Mathf.RoundToInt(textureRect.y);
            width = Mathf.RoundToInt(textureRect.width);
            height = Mathf.RoundToInt(textureRect.height);

            if (width <= 0 || height <= 0)
                return null;

            if (unreadableOutlinePreviewTextures.Contains(sprite.texture))
                return ReadSpritePixelsFromRenderTexture(sprite, x, y, width, height);

            try
            {
                return sprite.texture.GetPixels(x, y, width, height);
            }
            catch (UnityException)
            {
                unreadableOutlinePreviewTextures.Add(sprite.texture);
                return ReadSpritePixelsFromRenderTexture(sprite, x, y, width, height);
            }
        }

        private Rect GetSpriteTextureRect(Sprite sprite)
        {
            try
            {
                return sprite.textureRect;
            }
            catch (UnityException)
            {
                return sprite.rect;
            }
        }

        private Vector2 GetSpriteTextureRectOffset(Sprite sprite)
        {
            try
            {
                return sprite.textureRectOffset;
            }
            catch (UnityException)
            {
                return Vector2.zero;
            }
        }

        private Color[] ReadSpritePixelsFromRenderTexture(Sprite sprite, int x, int y, int width, int height)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                sprite.texture.width,
                sprite.texture.height,
                0,
                RenderTextureFormat.ARGB32);

            Texture2D readable = null;
            try
            {
                Graphics.Blit(sprite.texture, temporary);
                RenderTexture.active = temporary;

                readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(x, y, width, height), 0, 0);
                readable.Apply();
                return readable.GetPixels();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
                if (readable != null)
                    Destroy(readable);
            }
        }

        private void UpdateOutlinePreviewSorting()
        {
            int maxBodyOrder = int.MinValue;
            int minEffectOrder = int.MaxValue;

            for (int i = 0; i < outlinePreviewLayers.Count; i++)
                maxBodyOrder = Mathf.Max(maxBodyOrder, outlinePreviewLayers[i].renderer.sortingOrder);

            AddEffectSortingOrder(slashRenderer, ref minEffectOrder);
            AddEffectSortingOrder(effectRenderer, ref minEffectOrder);
            AddEffectSortingOrder(effect2Renderer, ref minEffectOrder);

            int sortingOrder = maxBodyOrder == int.MinValue ? 0 : maxBodyOrder + 1;
            if (minEffectOrder != int.MaxValue && sortingOrder >= minEffectOrder)
                sortingOrder = minEffectOrder - 1;

            outlinePreviewRenderer.sortingOrder = sortingOrder;
        }

        private void AddEffectSortingOrder(SpriteRenderer renderer, ref int minEffectOrder)
        {
            if (IsRendererUsableForOutlinePreview(renderer))
                minEffectOrder = Mathf.Min(minEffectOrder, renderer.sortingOrder);
        }

  private void CacheInitialTMPValues()
        {
            lastTMPValues[shoesGearNameText] = shoesGearNameText.text;
            lastTMPValues[chestGearNameText] = chestGearNameText.text;
            lastTMPValues[beltGearNameText] = beltGearNameText.text;
            lastTMPValues[handsGearNameText] = handsGearNameText.text;
            lastTMPValues[legsGearNameText] = legsGearNameText.text;
            lastTMPValues[headGearNameText] = headGearNameText.text;
            lastTMPValues[shoesColorText] = shoesColorText.text;
            lastTMPValues[chestColorText] = chestColorText.text;
            lastTMPValues[handsColorText] = handsColorText.text;
            lastTMPValues[beltColorText] = beltColorText.text;
            lastTMPValues[legsColorText] = legsColorText.text;
            lastTMPValues[headColorText] = headColorText.text;
            lastTMPValues[shadowGearNameText] = shadowGearNameText.text;
            lastTMPValues[shadowColorText] = shadowColorText.text;
            lastTMPValues[slashNameText] = slashNameText.text;
            lastTMPValues[slashColorText] = slashColorText.text;
            lastTMPValues[effectNameText] = effectNameText.text;
            lastTMPValues[effectColorText] = effectColorText.text;
            lastTMPValues[weaponNameText] = weaponNameText.text;
            lastTMPValues[weaponColorText] = weaponColorText.text;
            lastTMPValues[backpackNameText] = backpackNameText.text;
            lastTMPValues[backpackColorText] = backpackColorText.text;
            lastTMPValues[shieldNameText] = shieldNameText.text;
            lastTMPValues[shieldColorText] = shieldColorText.text;
            lastTMPValues[mountNameText] = mountNameText.text;
            lastTMPValues[mountColorText] = mountColorText.text;
            lastTMPValues[skinColorText] = skinColorText.text;
            lastTMPValues[skinNameText] = skinNameText.text;
        }

        private void AnimateTMPChange(TextMeshProUGUI tmp)
        {
            if (!originalScales.ContainsKey(tmp))
                originalScales[tmp] = tmp.transform.localScale;

            if (runningAnimations.ContainsKey(tmp) && runningAnimations[tmp] != null)
                StopCoroutine(runningAnimations[tmp]);

            runningAnimations[tmp] = StartCoroutine(AnimateTMP(tmp));
        }


        private IEnumerator AnimateTMP(TextMeshProUGUI tmp)
        {
            Color originalColor = Color.white;
            if (tmp.color != Color.yellow)
                originalColor = tmp.color;

            Vector3 cachedScale = originalScales[tmp];

            tmp.color = Color.yellow;
            tmp.transform.localScale = cachedScale * 1.2f;

            yield return new WaitForSeconds(0.1f);

            tmp.color = originalColor;
            tmp.transform.localScale = cachedScale;
        }


        private void SetTMPWithChangeDetection(TextMeshProUGUI tmp, string newValue)
        {
            if (!lastTMPValues.ContainsKey(tmp))
                lastTMPValues[tmp] = tmp.text;

            if (lastTMPValues[tmp] != newValue)
            {
                tmp.text = newValue;
                tmp.color = Color.white; // ✅ Reset color BEFORE animating
                AnimateTMPChange(tmp);
                lastTMPValues[tmp] = newValue;
            }
            else
            {
                tmp.text = newValue;
            }
        }



        /// <summary>
        /// Updates the UI for the gear slots (Shoes, Chest, hands, belt, Legs, Head)
        /// by retrieving each part’s animator name and sprite color.
        /// </summary>
    public void UpdateGearUI()
        {
            if (shoesAnimator != null)
                SetTMPWithChangeDetection(shoesGearNameText, GetAnimatorName(shoesAnimator));
            if (shoesRenderer != null)
                SetTMPWithChangeDetection(shoesColorText, GetColorHex(shoesRenderer.color));
            if (!shoesRenderer.enabled)
            {
                SetTMPWithChangeDetection(shoesGearNameText, "None");
                SetTMPWithChangeDetection(shoesColorText, "#000000");
            }
            if (!skinColorRenderer.enabled)
            {
                SetTMPWithChangeDetection(skinColorText, "None");
                SetTMPWithChangeDetection(skinNameText, "#000000");
            }


            if (chestAnimator != null)
                SetTMPWithChangeDetection(chestGearNameText, GetAnimatorName(chestAnimator));
            if (chestRenderer != null)
                SetTMPWithChangeDetection(chestColorText, GetColorHex(chestRenderer.color));
            if (!chestRenderer.enabled)
            {
                SetTMPWithChangeDetection(chestGearNameText, "None");
                SetTMPWithChangeDetection(chestColorText, "#000000");
            }

            if (beltAnimator != null)
                SetTMPWithChangeDetection(beltGearNameText, GetAnimatorName(beltAnimator));
            if (beltRenderer != null)
                SetTMPWithChangeDetection(beltColorText, GetColorHex(beltRenderer.color));
            if (!beltRenderer.enabled)
            {
                SetTMPWithChangeDetection(beltGearNameText, "None");
                SetTMPWithChangeDetection(beltColorText, "#000000");
            }

            if (handsAnimator != null)
                SetTMPWithChangeDetection(handsGearNameText, GetAnimatorName(handsAnimator));
            if (handsRenderer != null)
                SetTMPWithChangeDetection(handsColorText, GetColorHex(handsRenderer.color));
            if (!handsRenderer.enabled)
            {
                SetTMPWithChangeDetection(handsGearNameText, "None");
                SetTMPWithChangeDetection(handsColorText, "#000000");
            }

            if (legsAnimator != null)
                SetTMPWithChangeDetection(legsGearNameText, GetAnimatorName(legsAnimator));
            if (legsRenderer != null)
                SetTMPWithChangeDetection(legsColorText, GetColorHex(legsRenderer.color));
            if (!legsRenderer.enabled)
            {
                SetTMPWithChangeDetection(legsGearNameText, "None");
                SetTMPWithChangeDetection(legsColorText, "#000000");
            }

            if (headAnimator != null)
                SetTMPWithChangeDetection(headGearNameText, GetAnimatorName(headAnimator));
            if (headRenderer != null)
                SetTMPWithChangeDetection(headColorText, GetColorHex(headRenderer.color));
            if (!headRenderer.enabled)
            {
                SetTMPWithChangeDetection(headGearNameText, "None");
                SetTMPWithChangeDetection(headColorText, "#000000");
            }

            if (shadowAnimator != null)
                SetTMPWithChangeDetection(shadowGearNameText, GetAnimatorName(shadowAnimator));
            if (shadowRenderer != null)
                SetTMPWithChangeDetection(shadowColorText, GetAlphaValue(shadowRenderer.color));
            if (!shadowRenderer.enabled)
            {
                SetTMPWithChangeDetection(shadowGearNameText, "None");
                SetTMPWithChangeDetection(shadowColorText, "0%");
            }

            if (slashToggle != null && !slashToggle.isOn)
            {
                SyncRendererToToggle(slashToggle, slashRenderer);
                SetTMPWithChangeDetection(slashNameText, "None");
                SetTMPWithChangeDetection(slashColorText, "#FFFFFF");
            }
            else
            {
                SyncRendererToToggle(slashToggle, slashRenderer);
                if (slashAnimator != null)
                    SetTMPWithChangeDetection(slashNameText, GetAnimatorName(slashAnimator));
                if (slashRenderer != null)
                    SetTMPWithChangeDetection(slashColorText, GetColorHex(slashRenderer.color));
                if (slashRenderer != null && !slashRenderer.enabled)
                {
                    SetTMPWithChangeDetection(slashNameText, "None");
                    SetTMPWithChangeDetection(slashColorText, "#000000");
                }
            }

            if (effectToggle != null && !effectToggle.isOn)
            {
                SyncRendererToToggle(effectToggle, effectRenderer);
                SetTMPWithChangeDetection(effectNameText, "None");
                SetTMPWithChangeDetection(effectColorText, "#FFFFFF");
            }
            else
            {
                SyncRendererToToggle(effectToggle, effectRenderer);
                if (effectAnimator != null)
                    SetTMPWithChangeDetection(effectNameText, GetAnimatorName(effectAnimator));
                if (effectRenderer != null)
                    SetTMPWithChangeDetection(effectColorText, GetColorHex(effectRenderer.color));
                if (effectRenderer != null && !effectRenderer.enabled)
                {
                    SetTMPWithChangeDetection(effectNameText, "None");
                    SetTMPWithChangeDetection(effectColorText, "#000000");
                }
            }
            if (effect2Toggle != null && !effect2Toggle.isOn)
            {
                SyncRendererToToggle(effect2Toggle, effect2Renderer);
                SetTMPWithChangeDetection(effect2NameText, "None");
                SetTMPWithChangeDetection(effect2ColorText, "#FFFFFF");
            }
            else
            {
                SyncRendererToToggle(effect2Toggle, effect2Renderer);
                if (effect2Animator != null)
                    SetTMPWithChangeDetection(effect2NameText, GetAnimatorName(effect2Animator));
                if (effect2Renderer != null)
                    SetTMPWithChangeDetection(effect2ColorText, GetColorHex(effect2Renderer.color));
                if (effect2Renderer != null && !effect2Renderer.enabled)
                {
                    SetTMPWithChangeDetection(effect2NameText, "None");
                    SetTMPWithChangeDetection(effect2ColorText, "#000000");
                }
            }
            UpdateShieldUI();
            UpdateWeaponUI();
            UpdateBackpackUI();
            UpdateMountUI();
            UpdateSkinColorUI();

            //Update left side fields
            shoesGearNameText2.text = shoesGearNameText.text;
            chestGearNameText2.text = chestGearNameText.text;
            beltGearNameText2.text  = beltGearNameText.text;
            handsGearNameText2.text = handsGearNameText.text;
            legsGearNameText2.text  = legsGearNameText.text;
            headGearNameText2.text  = headGearNameText.text;
            // new ones
            // if (skinNameText2   != null && skinNameText   != null) skinNameText2.text   = skinNameText.text;
            if (slashNameText2  != null && slashNameText  != null) slashNameText2.text  = slashNameText.text;
            if (effectNameText2 != null && effectNameText != null) effectNameText2.text = effectNameText.text;
            if (effect2NameText2 != null && effect2NameText != null) effect2NameText2.text = effect2NameText.text;
        }

        /// <summary>
        /// Updates the weapon UI by checking which weapon's SpriteRenderer is enabled.
        /// The enabled weapon is considered the active weapon.
        /// </summary>
        private void UpdateWeaponUI()
        {
            Weapon activeWeapon = GetActiveWeapon();
            if (activeWeapon != null)
            {
                SetTMPWithChangeDetection(weaponNameText, GetWeaponName(activeWeapon));
                SetTMPWithChangeDetection(weaponColorText, GetColorHex(activeWeapon.spriteRenderer.color));
            }
            else
            {
                SetTMPWithChangeDetection(weaponNameText, "None");
                SetTMPWithChangeDetection(weaponColorText, "#000000");
            }
        }


        /// <summary>
        /// Iterates through the weapons array to find the weapon whose SpriteRenderer is enabled.
        /// </summary>
        /// <returns>The active Weapon if found; otherwise, null.</returns>
        private Weapon GetActiveWeapon()
        {
            foreach (var weapon in weapons)
            {
                if (weapon.spriteRenderer != null && weapon.spriteRenderer.enabled)
                {
                    return weapon;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the backpack UI by checking which backpack's SpriteRenderer is enabled.
        /// The enabled backpack is considered the active backpack.
        /// </summary>
        private void UpdateBackpackUI()
        {
            Backpack activeBackpack = GetActiveBackpack();
            if (activeBackpack != null)
            {
                SetTMPWithChangeDetection(backpackNameText, GetItemName(activeBackpack.animator, activeBackpack.backpackGO));
                SetTMPWithChangeDetection(backpackColorText, GetColorHex(activeBackpack.spriteRenderer.color));
            }
            else
            {
                SetTMPWithChangeDetection(backpackNameText, "None");
                SetTMPWithChangeDetection(backpackColorText, "#000000");
            }
        }


        /// <summary>
        /// Iterates through the backpacks array to find the backpack whose SpriteRenderer is enabled.
        /// </summary>
        /// <returns>The active Backpack if found; otherwise, null.</returns>
        private Backpack GetActiveBackpack()
        {
            foreach (var backpack in backpacks)
            {
                if (backpack.spriteRenderer != null && backpack.spriteRenderer.enabled)
                {
                    return backpack;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the shield UI by checking which shield's SpriteRenderer is enabled.
        /// The enabled shield is considered the active shield.
        /// </summary>
        private void UpdateShieldUI()
        {
            Shield activeShield = GetActiveShield();
            if (activeShield != null)
            {
                SetTMPWithChangeDetection(shieldNameText, GetItemName(activeShield.animator, activeShield.shieldGO));
                SetTMPWithChangeDetection(shieldColorText, GetColorHex(activeShield.spriteRenderer.color));
            }
            else
            {
                SetTMPWithChangeDetection(shieldNameText, "None");
                SetTMPWithChangeDetection(shieldColorText, "#000000");
            }
        }


        /// <summary>
        /// Iterates through the shields array to find the shield whose SpriteRenderer is enabled.
        /// </summary>
        /// <returns>The active Shield if found; otherwise, null.</returns>
        private Shield GetActiveShield()
        {
            foreach (var shield in shields)
            {
                if (shield.shieldGO.activeInHierarchy &&
                    shield.animator != null &&
                    shield.animator.enabled &&
                    shield.spriteRenderer != null &&
                    shield.spriteRenderer.enabled &&
                    shield.animator.runtimeAnimatorController != null)
                {
                    return shield;
                }
            }
            return null;
        }


        /// <summary>
        /// Updates the mount UI by checking which mount's SpriteRenderer is enabled.
        /// The enabled mount is considered the active mount.
        /// </summary>
        private void UpdateMountUI()
        {
            Mount activeMount = GetActiveMount();
            if (activeMount != null)
            {
                SetTMPWithChangeDetection(mountNameText, GetItemName(activeMount.animator, activeMount.mountGO));
                SetTMPWithChangeDetection(mountColorText, GetColorHex(activeMount.spriteRenderer.color));
            }
            else
            {
                SetTMPWithChangeDetection(mountNameText, "None");
                SetTMPWithChangeDetection(mountColorText, "#000000");
            }
        }


        /// <summary>
        /// Iterates through the mounts array to find the mount whose SpriteRenderer is enabled.
        /// </summary>
        /// <returns>The active Mount if found; otherwise, null.</returns>
        private Mount GetActiveMount()
        {
            foreach (var mount in mounts)
            {
                if (mount.spriteRenderer != null && mount.spriteRenderer.enabled)
                {
                    return mount;
                }
            }
            return null;
        }


        private void UpdateSkinColorUI()
        {
            if (skinColorRenderer != null)
            {
                SetTMPWithChangeDetection(skinColorText, GetColorHex(skinColorRenderer.color));
                SetTMPWithChangeDetection(skinNameText, GetAnimatorName(skinAnimator));
            }
            else
            {
                SetTMPWithChangeDetection(skinColorText, "#000000");
            }
        }


        /// <summary>
        /// Retrieves the animator's name based on its runtimeAnimatorController.
        /// Falls back to the animator's GameObject name if not available.
        /// </summary>
        /// <param name="animator">The animator to query.</param>
        /// <returns>The name from the animator or its runtime controller.</returns>
        private string GetAnimatorName(Animator animator)
        {
            if (animator.runtimeAnimatorController != null)
            {
                string name = animator.runtimeAnimatorController.name;
                return name.EndsWith("Controller") ? name.Replace("Controller", "") : name;
            }
            return animator.name;
        }

        
        /// <summary>
        /// Retrieves the weapon's name using its animator or GameObject.
        /// </summary>
        /// <param name="weapon">The weapon to query.</param>
        /// <returns>The name of the weapon.</returns>
        private string GetWeaponName(Weapon weapon)
        {
            if (weapon.animator != null && weapon.animator.runtimeAnimatorController != null)
            {
                string name = weapon.animator.runtimeAnimatorController.name;
                return name.EndsWith("Controller") ? name.Replace("Controller", "") : name;
            }
            return weapon.weaponGO.name;
        }

        
        /// <summary>
        /// Retrieves the item name (for backpacks or shields) using its animator or GameObject.
        /// </summary>
        /// <param name="animator">The animator to query.</param>
        /// <param name="itemGO">The item GameObject.</param>
        /// <returns>The name of the item.</returns>
        private string GetItemName(Animator animator, GameObject itemGO)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                string name = animator.runtimeAnimatorController.name;
                return name.EndsWith("Controller") ? name.Replace("Controller", "") : name;
            }
            return itemGO.name;
        }

        
        /// <summary>
        /// Converts a Unity Color to a hexadecimal string (e.g., #RRGGBB).
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A hex string representing the color.</returns>
        private string GetColorHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        private string GetAlphaValue(Color color)
        {
            float alphaPercent = color.a * 100f;
            return alphaPercent.ToString("F0") + "%";
        }

        private void UpdateGlowThicknessUI(float value)
        {
            glowThickness = Mathf.RoundToInt(value);

            if (glowThicknessLabel != null)
                glowThicknessLabel.text = "" + glowThickness;
        }




    /// <summary>
    /// Combines individual spritesheets for each animation into new, composite spritesheets.
    /// It assumes a folder structure under "Spritesheets" where each gear item has a folder named
    /// exactly as its stored name (from the UI) and that folder contains the 15 spritesheets.
    /// The base layer ("NakedBody") is always used (and tinted by skin color).
    /// The layering order is:
    ///  0: NakedBody (with skin color), 1: Shoes, 2: Legs, 3: Chest, 4: belt, 5: hands 6: Shield, 7: Weapon, 8: Backpack, 9: Head.
    /// The resulting combined spritesheets are saved into a new folder so as not to overwrite any originals.
    /// </summary>

    // Row policy: for rows 0..7 (i.e., 1..8), true => Weapon is AFTER Shield (in front).
    [SerializeField]
    private bool[] weaponAfterShieldRows = new bool[8] {
        true, true, true,  // rows 1,2,3 -> Weapon after Shield
        false, false, false, false, // rows 4,5,6, 7 -> Shield after Weapon
        true          // rows 8 -> Weapon after Shield
    };

    // true => Backpack is on TOP of BOTH shield & weapon on that row.
    // You wanted rows 1–5 behind (false), rows 6–8 in front (true).
    [SerializeField]
    private bool[] backpackInFrontRows = new bool[8] {
        true, false, false, false, false,   // rows 1..5 → backpack behind both
        true,  true,  true                   // rows 6..8 → backpack in front of both
    };



      public void StartCombineSpritesheets()
    {
        StartCoroutine(CombineCharacterSpritesheetsCoroutine());
    }

    /// <summary>
    /// Combines individual spritesheets for each animation into new composite spritesheets.
    /// Layering order:
    /// 0: Shadow (tinted by its alpha),
    /// 1: NakedBody (tinted by skin color; skipped if skinToggle is off),
    /// 2: Shoes,
    /// 3: Legs,
    /// 4: Chest,
    /// 5: belt,
    /// 6: hands,
    /// 7: Shield,
    /// 8: Weapon,
    /// 9: Backpack,
    /// 10: Head.
    /// 11: Slash.
    /// 12: Effect.
    /// </summary>
private IEnumerator CombineCharacterSpritesheetsCoroutine()
{
    string rootPath = Application.isEditor
    ? Application.dataPath
    : Application.persistentDataPath;
    
    // Activate load screen and reset progress.
    if (loadScreenPanel != null)
    {
        loadScreenPanel.SetActive(true);
        if (loadProgressSlider != null)
            loadProgressSlider.value = 0f;
    }
    if (currentlyGeneratingTMP != null)
        currentlyGeneratingTMP.text = "Starting spritesheet generation...";

    // Define standard animation names.
    List<string> animList = new List<string>();

    if (attack1Toggle != null && attack1Toggle.isOn) animList.Add("Attack1");
    if (attack2Toggle != null && attack2Toggle.isOn) animList.Add("Attack2");
    if (attack3Toggle != null && attack3Toggle.isOn) animList.Add("Attack3");
    if (attack4Toggle != null && attack4Toggle.isOn) animList.Add("Attack4");
    if (attack5Toggle != null && attack5Toggle.isOn) animList.Add("Attack5");
    if (attack6Toggle != null && attack6Toggle.isOn) animList.Add("Attack6");
    if (kickToggle != null && kickToggle.isOn) animList.Add("Kick");
    if (rollingToggle != null && rollingToggle.isOn) animList.Add("Rolling");
    if (slideToggle != null && slideToggle.isOn) animList.Add("Slide");

    if (attackRunToggle != null && attackRunToggle.isOn) animList.Add("AttackRun");
    if (attackRun2Toggle != null && attackRun2Toggle.isOn) animList.Add("AttackRun2");
    if (rideIdleAttack1Toggle != null && rideIdleAttack1Toggle.isOn) animList.Add("RideIdleAttack1");
    if (rideRunAttack1Toggle != null && rideRunAttack1Toggle.isOn) animList.Add("RideRunAttack1");
    if (special1Toggle != null && special1Toggle.isOn) animList.Add("Special1");

    if (crouchIdleToggle != null && crouchIdleToggle.isOn) animList.Add("CrouchIdle");
    if (crouchRunToggle != null && crouchRunToggle.isOn) animList.Add("CrouchRun");
    if (dieToggle != null && dieToggle.isOn) animList.Add("Die");
    if (idleToggle != null && idleToggle.isOn) animList.Add("Idle");
    if (idle2Toggle != null && idle2Toggle.isOn) animList.Add("Idle2");
    if (idle3Toggle != null && idle3Toggle.isOn) animList.Add("Idle3");
    if (idle4Toggle != null && idle4Toggle.isOn) animList.Add("Idle4");

    if (runToggle != null && runToggle.isOn) animList.Add("Run");
    if (runBackwardsToggle != null && runBackwardsToggle.isOn) animList.Add("RunBackwards");
    if (strafeLeftToggle != null && strafeLeftToggle.isOn) animList.Add("StrafeLeft");
    if (strafeRightToggle != null && strafeRightToggle.isOn) animList.Add("StrafeRight");
    if (walkToggle != null && walkToggle.isOn) animList.Add("Walk");
    if (rideIdleToggle != null && rideIdleToggle.isOn) animList.Add("RideIdle");
    if (rideRunToggle != null && rideRunToggle.isOn) animList.Add("RideRun");

    if (takeDamageToggle != null && takeDamageToggle.isOn) animList.Add("TakeDamage");
    if (tauntToggle != null && tauntToggle.isOn) animList.Add("Taunt");

    string[] animations = animList.ToArray(); 

    // Get folder names from UI.
    string shoesFolder    = shoesGearNameText.text;
    string legsFolder     = legsGearNameText.text;
    string chestFolder    = chestGearNameText.text;
    string beltFolder    = beltGearNameText.text;
    string handsFolder    = handsGearNameText.text;
    string headFolder     = headGearNameText.text;
    string weaponFolder   = weaponNameText.text;
    string backpackFolder = backpackNameText.text;
    string shieldFolder   = shieldNameText.text;
    string mountFolder    = mountNameText.text; 
    string nakedBodyFolder = skinNameText.text;
    string shadowFolder   = shadowGearNameText.text;
    string slashFolder = slashNameText.text;
    string effectFolder = effectNameText.text;
    string effect2Folder = effect2NameText.text;

    // Setup paths…
    // In editor: use the editable asset folder
    parentFolder = ResolveSpritesheetSourceFolder();
    if (string.IsNullOrEmpty(parentFolder))
    {
        string message = "Could not find source spritesheets. In builds, include them in StreamingAssets/Spritesheets.";
        Debug.LogError(message);
        if (currentlyGeneratingTMP != null)
            currentlyGeneratingTMP.text = message;
        yield return new WaitForSeconds(2f);
        if (loadScreenPanel != null)
            loadScreenPanel.SetActive(false);
        yield break;
    }

    string outputParent;

    #if UNITY_EDITOR
    outputParent = Path.Combine(Application.dataPath, "SmallScaleInt/Character creator - Fantasy/Created Spritesheets");
    #elif UNITY_STANDALONE
    outputParent = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Created Spritesheets");
    #else
    outputParent = Path.Combine(Application.persistentDataPath, "Created Spritesheets");
    #endif

    if (!Directory.Exists(outputParent))
        Directory.CreateDirectory(outputParent);

    //Use input field for character name
    string userInputName = "Character";  // default fallback
    if (characterNameInput != null && !string.IsNullOrWhiteSpace(characterNameInput.text))
    {
        userInputName = characterNameInput.text.Trim();
    }

    // Clean illegal file characters (just in case)
    foreach (char c in Path.GetInvalidFileNameChars())
        userInputName = userInputName.Replace(c, '_');

    // Append date to name
    string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
    string configFolder = Path.Combine(outputParent, userInputName + "_" + timestamp);

    Directory.CreateDirectory(configFolder);

    bool extractShadow = extractShadowToggle != null && extractShadowToggle.isOn;
    bool extractEffects = extractEffectsToggle != null && extractEffectsToggle.isOn;
    string shadowExtractFolder = Path.Combine(configFolder, "shadow");
    string effectsExtractFolder = Path.Combine(configFolder, "effects");
#if UNITY_EDITOR
    bool hasExtractedShadowSheets = false;
    bool hasExtractedEffectsSheets = false;
#endif

    if (extractShadow)
        Directory.CreateDirectory(shadowExtractFolder);

    if (extractEffects)
        Directory.CreateDirectory(effectsExtractFolder);


    // slash setup…
    bool includeSlash = (slashToggle != null && slashToggle.isOn &&
                           !weaponNameText.text.StartsWith("Test") &&
                           !weaponNameText.text.StartsWith("Special2") &&
                           !weaponNameText.text.StartsWith("None"));
    Color slashTint = includeSlash ? ParseColorFromTMP(slashColorText.text) : Color.white;

    // effect setup…
    bool includeEffect = (effectToggle != null && effectToggle.isOn &&
                           !weaponNameText.text.StartsWith("Test") &&
                           !weaponNameText.text.StartsWith("Special2") &&
                           !weaponNameText.text.StartsWith("None"));
    Color effectTint = includeEffect ? ParseColorFromTMP(effectColorText.text) : Color.white;

    bool includeEffect2 = (effect2Toggle != null && effect2Toggle.isOn &&
                       !weaponNameText.text.StartsWith("Test") &&
                       !weaponNameText.text.StartsWith("Special2") &&
                       !weaponNameText.text.StartsWith("None"));
    Color effect2Tint = includeEffect2 ? ParseColorFromTMP(effect2ColorText.text) : Color.white;


    int totalAnimations = animations.Length;
    int processedAnimations = 0;

    // --- Standard animations loop ---
    foreach (string anim in animations)
    {
        if (currentlyGeneratingTMP != null)
            currentlyGeneratingTMP.text = "Generating " + anim + " spritesheet...";

        // Load naked body (for size) and shadow
        Texture2D nakedBodyTex = LoadTexture(Path.Combine(parentFolder, nakedBodyFolder, anim + ".png"));
        if (nakedBodyTex == null)
        {
            Debug.LogError("Missing NakedBody spritesheet for animation: " + anim);
            processedAnimations++;
            if (loadProgressSlider != null)
                loadProgressSlider.value = (float)processedAnimations / totalAnimations;
            yield return null;
            continue;
        }
        Texture2D shadowTex = (shadowFolder == "None")
            ? null
            : LoadTexture(Path.Combine(parentFolder, shadowFolder, anim + ".png"));
        float shadowAlpha = ParseAlphaPercentage(shadowColorText.text);

        

        // Slash is loaded separately
        Texture2D slashTex = null;
        if (includeSlash && slashFolder != "None")
            slashTex = LoadTexture(Path.Combine(parentFolder, slashFolder, anim + ".png"));

            // Effect is loaded separately
        Texture2D effectTex = null;
        if (includeEffect && effectFolder != "None")
            effectTex = LoadTexture(Path.Combine(parentFolder, effectFolder, anim + ".png"));
        Texture2D effect2Tex = null;
        if (includeEffect2 && effect2Folder != "None")
            effect2Tex = LoadTexture(Path.Combine(parentFolder, effect2Folder, anim + ".png"));

                

        // Prepare tints
        Color shoesTint    = shoesRenderer   != null ? shoesRenderer.color   : Color.white;
        Color legsTint     = legsRenderer    != null ? legsRenderer.color    : Color.white;
        Color mountTint    = ParseColorFromTMP(mountColorText.text);
        Color chestTint    = chestRenderer   != null ? chestRenderer.color   : Color.white;
        Color beltTint    = beltRenderer   != null ? beltRenderer.color   : Color.white;
        Color handsTint    = handsRenderer   != null ? handsRenderer.color   : Color.white;
        Color shieldTint   = ParseColorFromTMP(shieldColorText.text);
        Color weaponTint   = ParseColorFromTMP(weaponColorText.text);
        Color backpackTint = ParseColorFromTMP(backpackColorText.text);
        Color headTint     = headRenderer    != null ? headRenderer.color    : Color.white;

        bool flatColorMode = flatColorToggle != null && flatColorToggle.isOn;

        Texture2D shoesTex    = LoadAndTintTexture(shoesFolder, anim, shoesTint, flatColorMode);
        Texture2D legsTex     = LoadAndTintTexture(legsFolder, anim, legsTint, flatColorMode);
        Texture2D mountTex    = LoadAndTintTexture(mountFolder, anim, mountTint, flatColorMode);
        Texture2D chestTex    = LoadAndTintTexture(chestFolder, anim, chestTint, flatColorMode);
        Texture2D beltTex     = LoadAndTintTexture(beltFolder, anim, beltTint, flatColorMode);
        Texture2D handsTex    = LoadAndTintTexture(handsFolder, anim, handsTint, flatColorMode);
        Texture2D shieldTex   = LoadAndTintTexture(shieldFolder, anim, shieldTint, flatColorMode);
        Texture2D weaponTex   = LoadAndTintTexture(weaponFolder, anim, weaponTint, flatColorMode);
        Texture2D backpackTex = LoadAndTintTexture(backpackFolder, anim, backpackTint, flatColorMode);
        Texture2D headTex     = LoadAndTintTexture(headFolder, anim, headTint, flatColorMode);

        // Build layer arrays
        Texture2D[] layers = new Texture2D[] {
            shoesTex, legsTex, mountTex, chestTex, beltTex, handsTex,
            shieldTex, weaponTex, backpackTex, headTex
        };
        Color[] layerTints = new Color[] {
            shoesTint, legsTint, mountTint, chestTint, beltTint, handsTint,
            shieldTint, weaponTint, backpackTint, headTint
        };

        // Allocate buffers
        int W = nakedBodyTex.width, H = nakedBodyTex.height;
        Color[] finalPixels    = new Color[W * H];
        Color[] shadowPixels   = new Color[W * H];
        Color[] bodyGearPixels = new Color[W * H]; // will hold nakedBody+gear

        // 1) Shadow → finalPixels
        if (shadowTex != null)
        {
            var sp = shadowTex.GetPixels();
            for (int i = 0; i < sp.Length; i++)
            {
                sp[i].a *= shadowAlpha;
                shadowPixels[i] = sp[i];
            }
        }

        for (int i = 0; i < finalPixels.Length; i++)
        {
            finalPixels[i] = extractShadow ? new Color(0, 0, 0, 0) : shadowPixels[i];
        }

        // 2) NakedBody → bodyGearPixels
        if (skinToggle == null || skinToggle.isOn)
        {
            var nb = nakedBodyTex.GetPixels();
            Color skinCol = skinColorRenderer != null ? skinColorRenderer.color : Color.white;

            if (flatColorMode)
                nb = ApplyFlatColor(nb, skinCol);

            for (int i = 0; i < nb.Length; i++)
                bodyGearPixels[i] = nb[i] * skinCol;
        }
        else
        {
            for (int i = 0; i < bodyGearPixels.Length; i++)
                bodyGearPixels[i] = new Color(0, 0, 0, 0); // Fully transparent
        }


        // 3) Gear layers → bodyGearPixels (ROW-AWARE with dynamic ordering for Shield, Weapon, Backpack)

        // Pre-extract pixel arrays once (they are already tinted in LoadAndTintTexture)
        Color[][] layerPix = new Color[layers.Length][];
        for (int i = 0; i < layers.Length; i++)
            layerPix[i] = (layers[i] != null) ? layers[i].GetPixels() : null;

        // Indices into our layers[] array so we can build per-row order
        const int IDX_SHOES    = 0;
        const int IDX_LEGS     = 1;
        const int IDX_MOUNT    = 2;
        const int IDX_CHEST    = 3;
        const int IDX_BELT     = 4;
        const int IDX_HANDS    = 5;
        const int IDX_SHIELD   = 6;
        const int IDX_WEAPON   = 7;
        const int IDX_BACKPACK = 8;
        const int IDX_HEAD     = 9;

        int rows = 8;
        int rowHeight = H / rows;

        for (int r = 0; r < rows; r++)
        {
            int topDownRow = rows - 1 - r;

            List<int> order = new List<int>(10)
            {
                IDX_SHOES, IDX_LEGS, IDX_MOUNT, IDX_CHEST, IDX_BELT, IDX_HANDS
            };

            bool weaponAfterShield = (topDownRow < weaponAfterShieldRows.Length)
                ? weaponAfterShieldRows[topDownRow] : true;

            // Backpack on top for top-based rows: 0,4,5,6,7  (i.e., visual rows 1,5,6,7,8)
            bool backpackOnTop = (topDownRow == 0) || (topDownRow == 4) || (topDownRow >= 5);

            if (backpackOnTop)
            {
                // Backpack should be in FRONT of BOTH shield & weapon
                if (weaponAfterShield)
                {
                    order.Add(IDX_SHIELD);
                    order.Add(IDX_WEAPON);
                }
                else
                {
                    order.Add(IDX_WEAPON);
                    order.Add(IDX_SHIELD);
                }
                order.Add(IDX_BACKPACK);
            }
            else
            {
                // Backpack should be BEHIND BOTH shield & weapon
                order.Add(IDX_BACKPACK);
                if (weaponAfterShield)
                {
                    order.Add(IDX_SHIELD);
                    order.Add(IDX_WEAPON);
                }
                else
                {
                    order.Add(IDX_WEAPON);
                    order.Add(IDX_SHIELD);
                }
            }

            order.Add(IDX_HEAD);

            int yStart = r * rowHeight;
            for (int oi = 0; oi < order.Count; oi++)
            {
                int li = order[oi];
                var pix = (li >= 0 && li < layerPix.Length) ? layerPix[li] : null;
                if (pix == null) continue;

                for (int y = 0; y < rowHeight; y++)
                {
                    int rowOffset = (yStart + y) * W;
                    int end = rowOffset + W;
                    for (int p = rowOffset; p < end; p++)
                        bodyGearPixels[p] = AlphaBlend(bodyGearPixels[p], pix[p]);
                }
            }

            // Optional: quick sanity check
            // Debug.Log($"topRow {topDownRow+1}: backpackOnTop={backpackOnTop}, weaponAfterShield={weaponAfterShield}");
        }



        // 4) Composite body+gear → finalPixels
        for (int i = 0; i < finalPixels.Length; i++)
            finalPixels[i] = AlphaBlend(finalPixels[i], bodyGearPixels[i]);

        // 5) Save mask-copy for outlining
        Color[] bodyGearCopy = (Color[])bodyGearPixels.Clone();

        // 6) Create Texture2D
        Texture2D outTex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        outTex.SetPixels(finalPixels);
        outTex.Apply();

        // 7) Static idle tweak (optional)
        bool makeStaticAnimation = staticIdleAnimation != null && staticIdleAnimation.isOn &&
            (anim == "Idle" || anim == "Idle2" || anim == "Idle3" || anim == "Idle4" || anim == "CrouchIdle");

        if (makeStaticAnimation)
        {
            outTex = MakeTextureStatic(outTex, 15, 8);
        }

        // 8) Resize (if needed) — track it!
        bool didResize = (use64Toggle != null && use64Toggle.isOn);
        if (didResize)
            outTex = ResizeTexturePixelPerfect(outTex, outTex.width / 2, outTex.height / 2);

        if (extractShadow && shadowTex != null)
        {
            Texture2D shadowOutTex = BuildLayerTexture(shadowPixels, W, H, makeStaticAnimation, didResize);
            string shadowOutputPath = Path.Combine(shadowExtractFolder, anim + ".png");
            File.WriteAllBytes(shadowOutputPath, shadowOutTex.EncodeToPNG());
            ExportTilesetFrames(shadowOutTex, anim, shadowExtractFolder);
#if UNITY_EDITOR
            hasExtractedShadowSheets = true;
#endif
        }

        // 9) Apply outline (using a correctly‐sized mask)
        Color[] mask = didResize
            ? ResizePixelsPixelPerfect(bodyGearCopy, W, H, outTex.width, outTex.height)
            : bodyGearCopy;

        // ⬇️ NEW: if this is a “Magic” weapon, strip its pixels out of the mask
        if (weaponFolder.StartsWith("Magic"))
        {
            if (weaponTex != null)
            {
                // 1) grab the original weapon pixels and tint them
                Color[] weaponMask = weaponTex.GetPixels();
                for (int i = 0; i < weaponMask.Length; i++)
                    weaponMask[i] *= weaponTint;

                // 2) if we resized the main sheet, resize the weapon‐mask exactly the same
                if (didResize)
                    weaponMask = ResizePixelsPixelPerfect(
                        weaponMask, 
                        W, H,              // original dims
                        outTex.width, 
                        outTex.height
                    );

                // 3) carve out: wherever weaponMask is non-transparent, zero out mask α
                for (int i = 0; i < mask.Length; i++)
                {
                    if (weaponMask[i].a > 0f)
                        mask[i].a = 0f;
                }
            }
        }

        // 9.5) Finally, run your outline routines on that edited mask:
        if (gradientOutlineToggle != null && gradientOutlineToggle.isOn)
        {
            var px       = outTex.GetPixels();
            var outlined = AddGradientOutline(px, mask, outTex.width, outTex.height, gradientBrightness);
            outTex.SetPixels(outlined);
            outTex.Apply();
        }
        else if (glowOutlineToggle != null && glowOutlineToggle.isOn)
        {
            Color glowColor = Color.black;

            if (outlineColorText != null && ColorUtility.TryParseHtmlString(outlineColorText.text, out Color parsedGlow))
                glowColor = parsedGlow;

            int outlineW = outTex.width;
            int outlineH = outTex.height;

            // If resized, use already-resized versions of final+mask pixels
            Color[] basePixels = outTex.GetPixels(); // Always use resized texture as base
            Color[] resizedMask = didResize
                ? ResizePixelsPixelPerfect(bodyGearPixels, W, H, outlineW, outlineH)
                : bodyGearPixels;

            var outlined = AddGlowOutline(basePixels, resizedMask, outlineW, outlineH, glowColor, glowThickness);
            outTex.SetPixels(outlined);
            outTex.Apply();
        }

        else if (outlineToggle != null && outlineToggle.isOn)
        {
            Color outlineColor = Color.black;

            if (outlineColorText != null && ColorUtility.TryParseHtmlString(outlineColorText.text, out Color parsed))
                outlineColor = parsed;

            var px       = outTex.GetPixels();
            var outlined = AddOutlineMask(px, mask, outTex.width, outTex.height, outlineColor);
            outTex.SetPixels(outlined);
            outTex.Apply();
        }



        // 10) Effects are either merged into the main sheet or written to their own combined sheet.
        if (extractEffects)
        {
            Color[] effectPixels = new Color[outTex.width * outTex.height];
            CompositeTintedLayer(slashTex, slashTint, didResize, W, H, outTex.width, outTex.height, effectPixels);
            CompositeTintedLayer(effectTex, effectTint, didResize, W, H, outTex.width, outTex.height, effectPixels);
            CompositeTintedLayer(effect2Tex, effect2Tint, didResize, W, H, outTex.width, outTex.height, effectPixels);

            if (HasVisiblePixels(effectPixels))
            {
                Texture2D effectsOutTex = CreateTextureFromPixels(effectPixels, outTex.width, outTex.height);
                string effectsOutputPath = Path.Combine(effectsExtractFolder, anim + ".png");
                File.WriteAllBytes(effectsOutputPath, effectsOutTex.EncodeToPNG());
                ExportTilesetFrames(effectsOutTex, anim, effectsExtractFolder);
#if UNITY_EDITOR
                hasExtractedEffectsSheets = true;
#endif
            }
        }
        else
        {
            Color[] basePixels = outTex.GetPixels();
            CompositeTintedLayer(slashTex, slashTint, didResize, W, H, outTex.width, outTex.height, basePixels);
            CompositeTintedLayer(effectTex, effectTint, didResize, W, H, outTex.width, outTex.height, basePixels);
            CompositeTintedLayer(effect2Tex, effect2Tint, didResize, W, H, outTex.width, outTex.height, basePixels);
            outTex.SetPixels(basePixels);
            outTex.Apply();
        }


        // Save out
        string outputPath = Path.Combine(configFolder, anim + ".png");
        File.WriteAllBytes(outputPath, outTex.EncodeToPNG());
        ExportTilesetFrames(outTex, anim, configFolder);
        Debug.Log($"Saved combined spritesheet for {anim} at {outputPath}");

        processedAnimations++;
        if (loadProgressSlider != null)
            loadProgressSlider.value = (float)processedAnimations / totalAnimations;
        yield return null;
    }


    if (loadProgressSlider != null)
        loadProgressSlider.value = 1f;
    if (currentlyGeneratingTMP != null)
        currentlyGeneratingTMP.text = "Generation complete!";
    yield return new WaitForSeconds(0.5f);
    if (loadScreenPanel != null)
        loadScreenPanel.SetActive(false);

    #if UNITY_EDITOR
    if (sliceSpritesheets != null && sliceSpritesheets.isOn)
    {
        AssetDatabase.Refresh();
        string[] files = GetGeneratedSpritesheetFiles(configFolder, true);
        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();
        foreach (string file in files)
        {
            string assetPath = "Assets" + file.Substring(Application.dataPath.Length);
            TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti != null)
            {
                ti.spriteImportMode = SpriteImportMode.Multiple;
                ti.spritePixelsPerUnit = 100;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tex != null)
                {
                    int columns = 15;
                    int rows = 8;
                    float sliceWidth = tex.width / (float)columns;
                    float sliceHeight = tex.height / (float)rows;
                    List<SpriteRect> spriteRects = new List<SpriteRect>(columns * rows);
                    for (int y = 0; y < rows; y++)
                    {
                        for (int x = 0; x < columns; x++)
                        {
                            SpriteRect rect = new SpriteRect();
                            rect.name = Path.GetFileNameWithoutExtension(assetPath) + "_" + y + "_" + x;
                            rect.rect = new Rect(x * sliceWidth, y * sliceHeight, sliceWidth, sliceHeight);
                            rect.pivot = new Vector2(0.5f, 0.5f);
                            rect.alignment = SpriteAlignment.Custom;
                            rect.spriteID = GUID.Generate();
                            spriteRects.Add(rect);
                        }
                    }

                    var provider = factory.GetSpriteEditorDataProviderFromObject(ti);
                    if (provider != null)
                    {
                        provider.InitSpriteEditorDataProvider();
                        provider.SetSpriteRects(spriteRects.ToArray());
                        provider.Apply();
                    }
                }
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
    AssetDatabase.Refresh();
    #endif
    ApplyFrameCountOption(configFolder);
    Debug.Log($"use64Toggle.isOn = {use64Toggle.isOn}");
    #if UNITY_EDITOR
    if (createAnimatorToggle != null && createAnimatorToggle.isOn && animatorClipBuilder != null)
    {
        int targetColumns = GetTargetColumns();
        string shadowAnimatorPath = null;
        string effectsAnimatorPath = null;

        if (hasExtractedShadowSheets && Directory.Exists(shadowExtractFolder))
        {
            string[] shadowSheetPaths = Directory.GetFiles(shadowExtractFolder, "*.png");
            shadowAnimatorPath = animatorClipBuilder.GenerateClipsForSpritesheets(
                shadowSheetPaths,
                shadowExtractFolder,
                targetColumns,
                userInputName + "_" + timestamp + "_Shadow",
                false
            );
        }

        if (hasExtractedEffectsSheets && Directory.Exists(effectsExtractFolder))
        {
            string[] effectsSheetPaths = Directory.GetFiles(effectsExtractFolder, "*.png");
            effectsAnimatorPath = animatorClipBuilder.GenerateClipsForSpritesheets(
                effectsSheetPaths,
                effectsExtractFolder,
                targetColumns,
                userInputName + "_" + timestamp + "_Effects",
                false
            );
        }

        animatorClipBuilder.ConfigureExtractedPreviewLayers(shadowAnimatorPath, effectsAnimatorPath);

        string[] sheetPaths = Directory.GetFiles(configFolder, "*.png");
        #if UNITY_EDITOR
        animatorClipBuilder.GenerateClipsForSpritesheets(sheetPaths, configFolder, targetColumns, userInputName + "_" + timestamp);
        #endif
    }
    #endif

    yield break;
}

private Color[] ResizePixelsPixelPerfect(Color[] original, int origWidth, int origHeight, int newWidth, int newHeight)
{
    Color[] result = new Color[newWidth * newHeight];
    for (int y = 0; y < newHeight; y++)
    {
        for (int x = 0; x < newWidth; x++)
        {
            int origX = x * 2;
            int origY = y * 2;
            if (origX >= origWidth) origX = origWidth - 1;
            if (origY >= origHeight) origY = origHeight - 1;
            result[y * newWidth + x] = original[origY * origWidth + origX];
        }
    }
    return result;
}


private Texture2D ResizeTexturePixelPerfect(Texture2D source, int newWidth, int newHeight)
{
    Texture2D result = new Texture2D(newWidth, newHeight, source.format, false);
    for (int y = 0; y < newHeight; y++)
    {
        for (int x = 0; x < newWidth; x++)
        {
            // Calculate the source pixel to sample
            int srcX = Mathf.FloorToInt(x * (source.width / (float)newWidth));
            int srcY = Mathf.FloorToInt(y * (source.height / (float)newHeight));
            Color pixel = source.GetPixel(srcX, srcY);
            result.SetPixel(x, y, pixel);
        }
    }
    result.filterMode = FilterMode.Point; // Important: keep pixels sharp
    result.Apply();
    return result;
}

private Texture2D BuildLayerTexture(Color[] pixels, int width, int height, bool makeStaticAnimation, bool resizeHalf)
{
    Texture2D texture = CreateTextureFromPixels(pixels, width, height);

    if (makeStaticAnimation)
        texture = MakeTextureStatic(texture, 15, 8);

    if (resizeHalf)
        texture = ResizeTexturePixelPerfect(texture, texture.width / 2, texture.height / 2);

    return texture;
}

private Texture2D CreateTextureFromPixels(Color[] pixels, int width, int height)
{
    Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    texture.SetPixels(pixels);
    texture.filterMode = FilterMode.Point;
    texture.Apply();
    return texture;
}

private void CompositeTintedLayer(Texture2D source, Color tint, bool didResize, int originalWidth, int originalHeight, int targetWidth, int targetHeight, Color[] targetPixels)
{
    if (source == null)
        return;

    Color[] layerPixels = source.GetPixels();
    for (int i = 0; i < layerPixels.Length; i++)
        layerPixels[i] *= tint;

    if (didResize)
        layerPixels = ResizePixelsPixelPerfect(layerPixels, originalWidth, originalHeight, targetWidth, targetHeight);

    for (int i = 0; i < targetPixels.Length; i++)
        targetPixels[i] = AlphaBlend(targetPixels[i], layerPixels[i]);
}

private bool HasVisiblePixels(Color[] pixels)
{
    for (int i = 0; i < pixels.Length; i++)
    {
        if (pixels[i].a > 0.001f)
            return true;
    }

    return false;
}


// Helper to avoid boilerplate:
private Texture2D LoadTextureOrNull(string folder, string animName)
{
    return (folder == "None")
        ? null
        : LoadTexture(Path.Combine(parentFolder, folder, animName + ".png"));
}

private string ResolveSpritesheetSourceFolder()
{
    List<string> candidates = new List<string>();

    #if UNITY_EDITOR
    candidates.Add(Path.Combine(Application.dataPath, "SmallScaleInt/Character creator - Fantasy/Spritesheets"));
    candidates.Add(Path.Combine(Application.dataPath, "SmallScaleInt/Character creator - Fantasy/spritesheets"));
    #else
    candidates.Add(Path.Combine(Application.streamingAssetsPath, "Spritesheets"));
    candidates.Add(Path.Combine(Application.streamingAssetsPath, "spritesheets"));

    string buildRoot = Path.GetDirectoryName(Application.dataPath);
    if (!string.IsNullOrEmpty(buildRoot))
    {
        candidates.Add(Path.Combine(buildRoot, "Spritesheets"));
        candidates.Add(Path.Combine(buildRoot, "spritesheets"));
    }
    #endif

    foreach (string candidate in candidates)
    {
        if (!string.IsNullOrEmpty(candidate) && Directory.Exists(candidate))
            return candidate;
    }

    Debug.LogError("Checked spritesheet source folders:\n" + string.Join("\n", candidates));
    return null;
}
    
    
  private Texture2D LoadTexture(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return null;
        }
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (tex.LoadImage(fileData))
            return tex;
        return null;
    }

    private Color ParseColorFromTMP(string hex)
    {
        Color parsedColor = Color.white;
        if (!string.IsNullOrEmpty(hex))
        {
            ColorUtility.TryParseHtmlString(hex, out parsedColor);
        }
        return parsedColor;
    }

    private float ParseAlphaPercentage(string percentageString)
    {
        if (string.IsNullOrEmpty(percentageString))
            return 1f;
        percentageString = percentageString.Trim();
        if (percentageString.EndsWith("%"))
            percentageString = percentageString.Substring(0, percentageString.Length - 1);
        if (float.TryParse(percentageString, out float percent))
            return Mathf.Clamp01(percent / 100f);
        return 1f;
    }

    private Color AlphaBlend(Color bottom, Color top)
    {
        float alpha = top.a;
        return top * alpha + bottom * (1f - alpha);
    }

    private Texture2D MakeTextureStatic(Texture2D original, int columns, int rows)
    {
        int tileWidth = original.width / columns;
        int tileHeight = original.height / rows;
        Color[] origPixels = original.GetPixels();
        Color[] newPixels = new Color[original.width * original.height];

        for (int row = 0; row < rows; row++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                int globalY = row * tileHeight + y;
                for (int x = 0; x < tileWidth; x++)
                {
                    int srcIndex = globalY * original.width + x;
                    Color staticColor = origPixels[srcIndex];
                    for (int col = 0; col < columns; col++)
                    {
                        int globalX = col * tileWidth + x;
                        int destIndex = globalY * original.width + globalX;
                        newPixels[destIndex] = staticColor;
                    }
                }
            }
        }
        Texture2D staticTexture = new Texture2D(original.width, original.height, original.format, false);
        staticTexture.SetPixels(newPixels);
        staticTexture.Apply();
        return staticTexture;
    }

private Texture2D PackSpritesheet(Texture2D original, int targetColumns)
{
    int originalColumns = 15;
    int rows = 8;
    int tileWidth = original.width / originalColumns;
    int tileHeight = original.height / rows;

    int[] mapping;
    if (targetColumns == 15)
    {
        mapping = new int[15];
        for (int i = 0; i < 15; i++) mapping[i] = i;
    }
    else if (targetColumns == 14)
    {
        // Remove columns 3,6,9,12,15 (1-indexed) → indices 2,5,8,11,14.
        mapping = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
    }
    else if (targetColumns == 12)
    {
        // Remove columns 3,6,9,12,15 (1-indexed) → indices 2,5,8,11,14.
        mapping = new int[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 12, 13 };
    }
    else if (targetColumns == 10)
    {
        // Remove columns 3,6,9,12,15 (1-indexed) → indices 2,5,8,11,14.
        mapping = new int[] { 0, 1, 3, 4, 6, 7, 9, 10, 12, 13 };
    }
    else if (targetColumns == 8)
    {
        // Keep columns 1,3,5,7,9,11,13,15 (1-indexed) → indices 0,2,4,6,8,10,12,14.
        mapping = new int[] { 0, 2, 4, 6, 8, 10, 12, 14 };
    }
    else if (targetColumns == 6)
    {
        // Keep columns 1,3,5,7,9,11,13,15 (1-indexed) → indices 0,2,4,6,8,10,12,14.
        mapping = new int[] { 0, 3, 6, 9, 12, 14 };
    }
    else if (targetColumns == 4)
    {
        // Keep only columns 1,5,9,13 (1-indexed) → indices 0,5,10,14.
        mapping = new int[] { 0, 4, 8, 12 };
    }
    else
    {
        Debug.LogError("Unsupported targetColumns: " + targetColumns);
        return original;
    }

    int newWidth = tileWidth * targetColumns;
    int newHeight = tileHeight * rows;
    // Create the new texture explicitly in RGBA32 format.
    Texture2D packed = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

    for (int row = 0; row < rows; row++)
    {
        for (int newCol = 0; newCol < targetColumns; newCol++)
        {
            int origCol = mapping[newCol];
            int origX = origCol * tileWidth;
            int origY = row * tileHeight;
            Color[] tilePixels = original.GetPixels(origX, origY, tileWidth, tileHeight);
            int newX = newCol * tileWidth;
            int newY = row * tileHeight;
            packed.SetPixels(newX, newY, tileWidth, tileHeight, tilePixels);
        }
    }
    packed.Apply();
    return packed;
}

private int GetTargetColumns()
{
    int columns = 15;
    if (fourteenFramesToggle != null && fourteenFramesToggle.isOn) columns = 14;
    else if (twelveFramesToggle != null && twelveFramesToggle.isOn) columns = 12;
    else if (tenFramesToggle != null && tenFramesToggle.isOn) columns = 10;
    else if (eightFramesToggle != null && eightFramesToggle.isOn) columns = 8;
    else if (sixFramesToggle != null && sixFramesToggle.isOn) columns = 6;
    else if (fourFramesToggle != null && fourFramesToggle.isOn) columns = 4;
    return columns;
}

private string[] GetGeneratedSpritesheetFiles(string configFolder, bool includeExtractedFolders)
{
    List<string> files = new List<string>();

    if (Directory.Exists(configFolder))
        files.AddRange(Directory.GetFiles(configFolder, "*.png"));

    if (includeExtractedFolders)
    {
        string shadowFolder = Path.Combine(configFolder, "shadow");
        string effectsFolder = Path.Combine(configFolder, "effects");

        if (Directory.Exists(shadowFolder))
            files.AddRange(Directory.GetFiles(shadowFolder, "*.png"));

        if (Directory.Exists(effectsFolder))
            files.AddRange(Directory.GetFiles(effectsFolder, "*.png"));
    }

    return files.ToArray();
}


    private void ApplyFrameCountOption(string configFolder)
    {
        int targetColumns = 15; // default
        if (fourteenFramesToggle != null && fourteenFramesToggle.isOn) targetColumns = 14;
        else if (twelveFramesToggle != null && twelveFramesToggle.isOn) targetColumns = 12;
        else if (tenFramesToggle != null && tenFramesToggle.isOn) targetColumns = 10;
        else if (eightFramesToggle != null && eightFramesToggle.isOn) targetColumns = 8;
        else if (sixFramesToggle != null && sixFramesToggle.isOn) targetColumns = 6;
        else if (fourFramesToggle != null && fourFramesToggle.isOn) targetColumns = 4;
        Debug.Log("Target columns for packing: " + targetColumns);
        
        if (targetColumns < 15)
        {
            string[] files = GetGeneratedSpritesheetFiles(configFolder, true);
            foreach (string file in files)
            {
                #if UNITY_EDITOR
                string assetPath = "Assets" + file.Substring(Application.dataPath.Length);
                TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti != null)
                {
                    ti.isReadable = true;
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (tex != null)
                    {
                        Texture2D packed = PackSpritesheet(tex, targetColumns);
                        byte[] pngData = packed.EncodeToPNG();
                        File.WriteAllBytes(file, pngData);
                    }
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
                #else
                Texture2D tex = LoadTexture(file);
                if (tex != null)
                {
                    Texture2D packed = PackSpritesheet(tex, targetColumns);
                    byte[] pngData = packed.EncodeToPNG();
                    File.WriteAllBytes(file, pngData);
                }
                #endif
            }
        }
    }


    private void ExportTilesetFrames(Texture2D spritesheet, string baseName, string outputFolder)
    {
        if (exportTilesetToggle == null || !exportTilesetToggle.isOn)
            return;

        int columns = spritesheet.width / (use64Toggle.isOn ? 64 : 128);  // ✅ Always derive from width

        int rows = 8;
        int frameWidth = spritesheet.width / columns;
        int frameHeight = spritesheet.height / rows;

        // 🔸 Create subfolder for this animation
        string animationFolder = Path.Combine(outputFolder, baseName);
        if (!Directory.Exists(animationFolder))
            Directory.CreateDirectory(animationFolder);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Texture2D frame = new Texture2D(frameWidth, frameHeight, TextureFormat.RGBA32, false);
                frame.SetPixels(spritesheet.GetPixels(col * frameWidth, row * frameHeight, frameWidth, frameHeight));
                frame.Apply();

                string tileName = $"{baseName}_{row}_{col}.png";
                string tilePath = Path.Combine(animationFolder, tileName);
                File.WriteAllBytes(tilePath, frame.EncodeToPNG());
            }
        }

        Debug.Log($"✅ Exported {rows * columns} frames to: {animationFolder}");
    }


    /// <summary>
    /// Takes a flat RGBA pixel array plus its dimensions
    /// and returns a new array where any transparent
    /// neighbor of an opaque pixel gets painted with
    /// outlineColor (a single‑pixel orthogonal dilation).
    /// </summary>
    private Color[] AddOutline(Color[] pixels, int width, int height, Color outlineColor)
    {
        Color[] result = (Color[])pixels.Clone();
        // Build a mask of where the character lives
        bool[] mask = new bool[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
            mask[i] = pixels[i].a > 0f;

        // Offsets for 4‑neighborhood:
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        // For each pixel that is part of the character:
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                if (!mask[idx]) continue;

                // Check each neighbor:
                for (int k = 0; k < 4; k++)
                {
                    int nx = x + dx[k];
                    int ny = y + dy[k];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    int nIdx = ny * width + nx;
                    if (!mask[nIdx])
                    {
                        // Paint an outline pixel
                        result[nIdx] = outlineColor;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Paints a 1px black outline in finalPixels around any body pixel
    /// in bodyPixels, but ignores the shadow that’s already in finalPixels.
    /// </summary>
    private Color[] AddOutlineMask(Color[] finalPixels,
                                Color[] bodyPixels,
                                int width, int height,
                                Color outlineColor)
    {
        bool[] mask = new bool[bodyPixels.Length];
        for (int i = 0; i < mask.Length; i++)
            mask[i] = bodyPixels[i].a > 0f;

        int[] dx = { -1, 1, 0, 0 }, dy = { 0, 0, -1, 1 };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                if (!mask[idx]) continue;

                // for each 4‑neighbor:
                for (int k = 0; k < 4; k++)
                {
                    int nx = x + dx[k], ny = y + dy[k];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int nIdx = ny * width + nx;
                    if (!mask[nIdx])
                        finalPixels[nIdx] = outlineColor;
                }
            }
        }

        return finalPixels;
    }


    /// <summary>
/// Like AddOutlineMask, but each outline pixel is colored by
/// taking the nearest opaque body pixel and darkening it.
/// </summary>
public float redColor   = 0.5f;
public float greenColor = 0.5f;
public float blueColor  = 0.5f;

/// <summary>
/// Paints a 1px outline around any body pixel, sampling the nearest body pixel colour,
/// inverting it, and then lightening it toward white by lightenFactor.
/// finalPixels[] already contains shadow; bodyPixels[] is your nakedBody+gear composite.
/// </summary>
private Color[] AddGradientOutline(
    Color[] finalPixels,
    Color[] bodyPixels,
    int width,
    int height,
    float lightenFactor = 0.5f  // 0 = pure inverted colour; 1 = pure white
)
{
    Color[] result = (Color[])finalPixels.Clone();

    // build mask of where body exists
    bool[] mask = new bool[bodyPixels.Length];
    for (int i = 0; i < mask.Length; i++)
        mask[i] = bodyPixels[i].a > 0f;

    // 4‑neighbour offsets
    int[] dx = { -1, 1, 0, 0 }, dy = { 0, 0, -1, 1 };

    // for every body pixel, paint its transparent neighbours
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            int idx = y * width + x;
            if (!mask[idx]) continue;

            // sample the body pixel colour
            Color src = bodyPixels[idx];

            // invert it
            Color inverted = new Color(1f - src.r, 1f - src.g, 1f - src.b, 1f);

            // lighten toward white
            Color outlineCol = Color.Lerp(inverted, Color.white, lightenFactor);

            // paint 1‑px thick outline
            for (int k = 0; k < 4; k++)
            {
                int nx = x + dx[k], ny = y + dy[k];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                int nIdx = ny * width + nx;
                if (!mask[nIdx])
                {
                    result[nIdx] = outlineCol;
                }
            }
        }
    }

    return result;
}

/// <summary>
/// Adds a soft glow around the bodyPixels using the specified glow color.
/// The glow is a 1px soft blend outward from bodyPixels into transparent areas.
/// </summary>
private Color[] AddGlowOutline(
    Color[] finalPixels,
    Color[] bodyPixels,
    int width,
    int height,
    Color glowColor,
    int thickness = 4 // how far the glow spreads
)
{
    Color[] result = (Color[])finalPixels.Clone();

    // Build mask of where body exists
    bool[] mask = new bool[bodyPixels.Length];
    for (int i = 0; i < mask.Length; i++)
        mask[i] = bodyPixels[i].a > 0f;

    // Create distance field: 0 for body, large for far-away pixels
    int[] distance = new int[width * height];
    for (int i = 0; i < distance.Length; i++)
        distance[i] = mask[i] ? 0 : int.MaxValue;

    // 8-direction offsets for better smoothing
    int[] dx = { -1, 1,  0, 0, -1, -1, 1, 1 };
    int[] dy = {  0, 0, -1, 1, -1,  1, -1, 1 };

    // Breadth-first propagation (like flood fill) to calculate pixel distances
    Queue<int> frontier = new Queue<int>();
    for (int i = 0; i < mask.Length; i++)
    {
        if (mask[i])
            frontier.Enqueue(i);
    }

    while (frontier.Count > 0)
    {
        int idx = frontier.Dequeue();
        int x = idx % width;
        int y = idx / width;
        int d = distance[idx];

        for (int k = 0; k < dx.Length; k++)
        {
            int nx = x + dx[k], ny = y + dy[k];
            if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                continue;

            int nIdx = ny * width + nx;
            if (distance[nIdx] > d + 1 && d + 1 <= thickness)
            {
                distance[nIdx] = d + 1;
                frontier.Enqueue(nIdx);
            }
        }
    }

    // Paint glow using distance + alpha falloff
    for (int i = 0; i < result.Length; i++)
    {
        int d = distance[i];
        if (d > 0 && d <= thickness)
        {
            float alpha = 1f - ((float)(d - 1) / thickness); // solid at 1px, fades out
            Color c = glowColor;
            c.a *= alpha;
            result[i] = AlphaBlend(result[i], c);
        }
    }

    return result;
}


        private Color[] ApplyFlatColor(Color[] pixels, Color tint)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0.01f)
                    pixels[i] = new Color(tint.r, tint.g, tint.b, pixels[i].a);
            }
            return pixels;
        }


        private Texture2D LoadAndTintTexture(string folder, string anim, Color tint, bool useFlatColor)
        {
            Texture2D tex = (folder == "None") ? null : LoadTexture(Path.Combine(parentFolder, folder, anim + ".png"));
            if (tex == null) return null;
            Color[] pix = tex.GetPixels();
            for (int i = 0; i < pix.Length; i++)
                pix[i] *= tint;
            if (useFlatColor)
                pix = ApplyFlatColor(pix, tint);
            Texture2D result = new Texture2D(tex.width, tex.height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        

    }
}
