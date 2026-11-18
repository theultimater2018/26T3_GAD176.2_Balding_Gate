using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro; // Required for TextMeshPro

namespace SmallScaleInc.CharacterCreatorFantasy
{
    public class AnimatorGearSwitcher : MonoBehaviour
    {
        [Header("Animator Controllers for Gear")]
        public List<RuntimeAnimatorController> animatorControllers;

        [Header("Randomization Exclusions")]
        public List<RuntimeAnimatorController> excludedControllers = new List<RuntimeAnimatorController>();
        public List<string> excludedNames = new List<string>(); // Exclude by animator name substring


        [Header("UI Buttons")]
        public Button nextButton;
        public Button previousButton;
        public Button randomButton; // Random button added

        [Header("Color Toggles")]
        public Toggle[] colorToggles = new Toggle[5];
        public Color[] toggleColors = new Color[5];
        // New Skin Color flag: if true, a random toggle from the above list is chosen.
        public bool isSkinColor;

        [Header("Weapon Settings")]
        public bool isWeapon; // Set true if this gear piece is a weapon.
        public Toggle[] weaponToggles; // List of toggles for weapons.

        [Header("Bag Settings")]
        public bool isBag; // Set true if this gear piece is a backpack.
        public Toggle[] bagToggles; // List of toggles for backpacks.

        [Header("Shield Settings")]
        public bool isShield; // Set true if this gear piece is a shield.
        public Toggle[] shieldToggles; // List of toggles for shields.

        [Header("Idle Variant Toggles")]
        public Toggle[] idleToggles;  // [0]=Idle, [1]=Idle2, [2]=Idle3, [3]=Idle4

        [Header("Player Controller Sync")]
        public TopDownCharacterController playerController;


        [Header("Global Color Swatch Panel (shared by all gear pieces)")]
        public GameObject colorSwatchPanel; // Shared panel for all gear pieces
        public Button[] colorSwatchButtons; // Swatch buttons with their assigned colors
        public Button closeColorPickerButton; // Button to close the panel without action

        [Header("UI Color Info")]
        public TextMeshProUGUI colorInfoText; // TextMeshPro object to display swatch info

        [Header("Weapon Color Picker")]
        // List of weapon SpriteRenderers that should all update when the weapon color picker is used.
        public List<SpriteRenderer> weaponSpriteRenderers;

        [Header("Color Mode")]
        public bool usePaletteColors = false;
        [Tooltip("A hand-picked palette of dark/fantasy colors.")]
        public Color[] paletteColors;

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private int currentAnimatorIndex = 0;
        private bool missingAnimatorWarningShown = false;
        private ColorSwatchSharedState swatchState;

        // What part of the body is the script attached to? Used on start to randomize the gear and color.
        public bool isHead; 
        public bool isChest;
        public bool isLegs;
        public bool isShoes;
        public bool isHands;
        public bool isBelt;
        // The previous "isSkin" field remains (if used elsewhere) but now we use isSkinColor for skin color randomization.

        void Start()
        {
            animator = ResolveAnimatorTarget();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Setup animator buttons.
            if (nextButton != null)
                nextButton.onClick.AddListener(NextAnimator);
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousAnimator);
            if (randomButton != null)
                randomButton.onClick.AddListener(RandomGear);

            // Initialize the first animator controller.
            if (HasAnimatorControllers())
                TryApplyAnimatorController(animatorControllers[currentAnimatorIndex], false);

            // Setup color toggles.
            for (int i = 0; i < colorToggles.Length; i++)
            {
                int index = i; // Local copy for the closure.
                if (colorToggles[index] != null)
                {
                    colorToggles[index].onValueChanged.AddListener(isOn =>
                    {
                        if (isOn)
                            ChangeColor(index);
                    });
                }
            }
            // Set colors of toggles.
            for (int i = 0; i < colorToggles.Length; i++)
            {
                if (colorToggles[i] != null && i < toggleColors.Length && colorToggles[i].targetGraphic != null)
                {
                    colorToggles[i].targetGraphic.color = toggleColors[i];
                }
            }
            
            // The panel owns the shared state, so it is reset naturally with the scene.
            ColorSwatchSharedState sharedState = ResolveSwatchState();
            if (sharedState != null && !sharedState.IsSetup && colorSwatchButtons != null)
            {
                SetupColorSwatchButtons();
                SetupCloseButton();
                sharedState.IsSetup = true;
                colorSwatchPanel.SetActive(false);
            }

            // If this gear piece should automatically randomize on start, include all possible types.
            if (isHead || isChest || isLegs || isShoes || isHands || isBelt || isWeapon || isBag || isShield || isSkinColor)
            {
                RandomGear();
            }
        }

        void NextAnimator()
        {
            if (!HasAnimatorControllers()) return;
            currentAnimatorIndex = (currentAnimatorIndex + 1) % animatorControllers.Count;
            TryApplyAnimatorController(animatorControllers[currentAnimatorIndex], true);
        }

        void PreviousAnimator()
        {
            if (!HasAnimatorControllers()) return;
            currentAnimatorIndex--;
            if (currentAnimatorIndex < 0)
                currentAnimatorIndex = animatorControllers.Count - 1;
            TryApplyAnimatorController(animatorControllers[currentAnimatorIndex], true);
        }

        private bool HasAnimatorControllers()
        {
            return animatorControllers != null && animatorControllers.Count > 0;
        }

        private Animator ResolveAnimatorTarget()
        {
            Animator target = GetComponent<Animator>();
            if (target != null)
                return target;

            Animator[] childAnimators = GetComponentsInChildren<Animator>(true);
            return childAnimators.Length == 1 ? childAnimators[0] : null;
        }

        private bool TryApplyAnimatorController(RuntimeAnimatorController controller, bool warnIfMissing)
        {
            if (animator == null)
            {
                if (warnIfMissing)
                    WarnMissingAnimatorTarget();

                return false;
            }

            animator.runtimeAnimatorController = controller;
            RefreshPlayerIdleIfAnimatorIsBody();
            return true;
        }

        private void RefreshPlayerIdleIfAnimatorIsBody()
        {
            if (animator == null) return;

            TopDownCharacterController controller = animator.GetComponent<TopDownCharacterController>();
            if (controller == null && playerController != null && animator == playerController.GetComponent<Animator>())
                controller = playerController;

            if (controller != null)
                controller.RefreshIdleAnimatorState();
        }

        private void ApplyIdleVariantToPlayer(int variant)
        {
            TopDownCharacterController controller = playerController != null
                ? playerController
                : FindAnyObjectByType<TopDownCharacterController>();

            if (controller != null)
                controller.ApplyIdleVariantImmediately(variant);
        }

        private void WarnMissingAnimatorTarget()
        {
            if (missingAnimatorWarningShown)
                return;

            missingAnimatorWarningShown = true;
            Debug.LogWarning(
                $"{nameof(AnimatorGearSwitcher)} on '{gameObject.name}' could not find a unique Animator target. " +
                "Add an Animator to this GameObject, or leave controller switching to the child option objects.",
                this);
        }

        void ChangeColor(int toggleIndex)
        {
            if (spriteRenderer != null && toggleColors.Length > toggleIndex)
            {
                spriteRenderer.color = toggleColors[toggleIndex];
            }
        }

        // Picks a random animator/color or random toggle (weapon, bag, shield, skin color) if applicable.
        public void RandomGear()
        {
            bool randomizationApplied = false;

            // Weapon randomization.
            if (isWeapon && weaponToggles != null && weaponToggles.Length > 0)
            {
                foreach (Toggle toggle in weaponToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomWeaponIndex = Random.Range(0, weaponToggles.Length);
                if (weaponToggles[randomWeaponIndex] != null)
                    weaponToggles[randomWeaponIndex].isOn = true;
                randomizationApplied = true;
            }

            // Bag randomization.
            if (isBag && bagToggles != null && bagToggles.Length > 0)
            {
                foreach (Toggle toggle in bagToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomBagIndex = Random.Range(0, bagToggles.Length);
                if (bagToggles[randomBagIndex] != null)
                    bagToggles[randomBagIndex].isOn = true;
                randomizationApplied = true;
            }

            // Shield randomization.
            if (isShield && shieldToggles != null && shieldToggles.Length > 0)
            {
                foreach (Toggle toggle in shieldToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomShieldIndex = Random.Range(0, shieldToggles.Length);
                if (shieldToggles[randomShieldIndex] != null)
                    shieldToggles[randomShieldIndex].isOn = true;
                randomizationApplied = true;
            }

            // Idle variant randomization
            if (idleToggles != null && idleToggles.Length > 0)
            {
                // turn them all off:
                foreach (var t in idleToggles)
                    if (t != null) t.isOn = false;

                // pick one at random and turn it on:
                int randomIdle = Random.Range(0, idleToggles.Length);
                if (idleToggles[randomIdle] != null)
                    idleToggles[randomIdle].isOn = true;

                ApplyIdleVariantToPlayer(randomIdle + 1);
                randomizationApplied = true;
            }


            // Skin Color randomization using the colorToggles if isSkinColor is true.
            if (isSkinColor && colorToggles != null && colorToggles.Length > 0)
            {
                foreach (Toggle toggle in colorToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomSkinIndex = Random.Range(0, colorToggles.Length);
                if (colorToggles[randomSkinIndex] != null)
                    colorToggles[randomSkinIndex].isOn = true;
                randomizationApplied = true;
            }

            // If none of the toggle types are active, perform the default clothing/gear randomization.
            if (!randomizationApplied)
            {
                if (HasAnimatorControllers())
                {
                    // Filter out excluded controllers
                    List<RuntimeAnimatorController> validControllers = new List<RuntimeAnimatorController>();

                    foreach (var ctrl in animatorControllers)
                    {
                        if (ctrl == null) continue;
                        if (excludedControllers.Contains(ctrl)) continue;

                        bool nameExcluded = false;
                        foreach (string namePart in excludedNames)
                        {
                            if (!string.IsNullOrEmpty(namePart) && ctrl.name.Contains(namePart))
                            {
                                nameExcluded = true;
                                break;
                            }
                        }

                        if (!nameExcluded)
                            validControllers.Add(ctrl);
                    }

                    // Pick random from filtered list
                    if (validControllers.Count > 0)
                    {
                        currentAnimatorIndex = Random.Range(0, validControllers.Count);
                        TryApplyAnimatorController(validControllers[currentAnimatorIndex], true);
                    }
                    else
                    {
                        Debug.LogWarning($"No valid animators found after applying exclusions on: {gameObject.name}");
                    }
                }

                // instead of always Random.ColorHSV(...)
                Color chosenColor;

                // if your “usePaletteColors” flag is on and you actually have a palette:
                if (usePaletteColors && paletteColors != null && paletteColors.Length > 0)
                {
                    chosenColor = paletteColors[Random.Range(0, paletteColors.Length)];
                }
                else
                {
                    // fully random fallback
                    chosenColor = Random.ColorHSV(
                        0f, 1f,  // hue
                        0f, 1f,  // saturation
                        0f, 1f,  // value
                        1f, 1f   // alpha
                    );
                }

                if (spriteRenderer != null)
                    spriteRenderer.color = chosenColor;
            }
        }

        /// <summary>
        /// Called by a Toggle to enable or disable palette color mode.
        /// </summary>
        /// <param name="usePalette">If true, use predefined palette colors. If false, use fully random colors.</param>
        public void SetUsePaletteColors(bool usePalette)
        {
            usePaletteColors = usePalette;
        }


        // Sets up the swatch buttons to update the currently active target(s).
        void SetupColorSwatchButtons()
        {
            foreach (Button swatchButton in colorSwatchButtons)
            {
                if (swatchButton != null && swatchButton.targetGraphic != null)
                {
                    Color swatchColor = swatchButton.targetGraphic.color;
                    
                    // Add click listener: when clicked, update the appropriate target(s).
                    swatchButton.onClick.AddListener(() =>
                    {
                        // For weapons
                        if (swatchState.CurrentWeaponTargets != null && swatchState.CurrentWeaponTargets.Count > 0)
                        {
                            foreach (SpriteRenderer sr in swatchState.CurrentWeaponTargets)
                                if (sr != null) sr.color = swatchColor;

                            swatchState.CurrentWeaponTargets = null;
                        }
                        // For gear parts
                        else if (swatchState.CurrentTarget != null)
                        {
                            swatchState.CurrentTarget.color = swatchColor;
                            swatchState.CurrentTarget = null;
                        }
                        // For TMP color field (like outline)
                        else if (swatchState.CurrentTMPColorTarget != null)
                        {
                            swatchState.CurrentTMPColorTarget.text = "#" + ColorUtility.ToHtmlStringRGB(swatchColor);
                            swatchState.CurrentTMPColorTarget = null;
                        }

                        colorSwatchPanel.SetActive(false);
                    });


                    // Set up pointer enter and exit events for showing swatch info.
                    EventTrigger trigger = swatchButton.gameObject.GetComponent<EventTrigger>();
                    if (trigger == null)
                    {
                        trigger = swatchButton.gameObject.AddComponent<EventTrigger>();
                    }

                    // Pointer Enter event: display button name and hex code.
                    EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                    entryEnter.eventID = EventTriggerType.PointerEnter;
                    entryEnter.callback.AddListener((data) =>
                    {
                        if (colorInfoText != null)
                        {
                            string hexCode = "#" + ColorUtility.ToHtmlStringRGB(swatchColor);
                            colorInfoText.text = swatchButton.gameObject.name + ": " + hexCode;
                        }
                    });
                    trigger.triggers.Add(entryEnter);

                    // Pointer Exit event: clear the text.
                    EventTrigger.Entry entryExit = new EventTrigger.Entry();
                    entryExit.eventID = EventTriggerType.PointerExit;
                    entryExit.callback.AddListener((data) =>
                    {
                        if (colorInfoText != null)
                        {
                            colorInfoText.text = "";
                        }
                    });
                    trigger.triggers.Add(entryExit);
                }
            }
        }

        // Sets up the close button to hide the swatch panel.
        void SetupCloseButton()
        {
            if (closeColorPickerButton != null)
            {
                closeColorPickerButton.onClick.AddListener(CloseColorPicker);
            }
        }

        // Call this method from a gear piece’s own "Open Color Picker" button.
        // It marks that gear piece as the current target and shows the swatch panel.
        public void OpenColorPicker()
        {
            ColorSwatchSharedState sharedState = ResolveSwatchState();
            if (sharedState == null) return;

            sharedState.CurrentTarget = spriteRenderer;
            sharedState.CurrentWeaponTargets = null;
            sharedState.CurrentTMPColorTarget = null;
            if (colorSwatchPanel != null)
            {
                colorSwatchPanel.SetActive(true);
            }
        }

        // Call this method from the weapon color picker button.
        // It assigns the weapon SpriteRenderers list to the shared panel state and opens the swatch panel.
        public void OpenWeaponColorPicker()
        {
            ColorSwatchSharedState sharedState = ResolveSwatchState();
            if (sharedState == null) return;

            sharedState.CurrentWeaponTargets = weaponSpriteRenderers;
            sharedState.CurrentTarget = null;
            sharedState.CurrentTMPColorTarget = null;
            if (colorSwatchPanel != null)
            {
                colorSwatchPanel.SetActive(true);
            }
        }

        // Closes the color swatch panel without applying any color.
        public void CloseColorPicker()
        {
            if (colorSwatchPanel != null)
            {
                colorSwatchPanel.SetActive(false);
            }

            ColorSwatchSharedState sharedState = ResolveSwatchState();
            if (sharedState == null) return;

            sharedState.CurrentTarget = null;
            sharedState.CurrentWeaponTargets = null;
            sharedState.CurrentTMPColorTarget = null;
        }

        //Method to open color picker for outline
        public void OpenTMPColorPicker(TextMeshProUGUI tmpTarget)
        {
            ColorSwatchSharedState sharedState = ResolveSwatchState();
            if (sharedState == null) return;

            sharedState.CurrentTMPColorTarget = tmpTarget;
            sharedState.CurrentTarget = null;
            sharedState.CurrentWeaponTargets = null;

            if (colorSwatchPanel != null)
                colorSwatchPanel.SetActive(true);
        }

        private ColorSwatchSharedState ResolveSwatchState()
        {
            if (swatchState != null)
                return swatchState;

            if (colorSwatchPanel == null)
                return null;

            swatchState = colorSwatchPanel.GetComponent<ColorSwatchSharedState>();
            if (swatchState == null)
                swatchState = colorSwatchPanel.AddComponent<ColorSwatchSharedState>();

            return swatchState;
        }

    }
}
