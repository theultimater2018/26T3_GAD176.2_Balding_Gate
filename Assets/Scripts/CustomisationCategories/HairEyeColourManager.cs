using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{
    public class HairEyeColourManager : MonoBehaviour
    {
        [SerializeField] protected Material[] eyeAndHairColours;
        [SerializeField] protected int currentHairEyeColourIndex = 0;

        [SerializeField] private Eyes eyesScript;
        [SerializeField] private Hairstyles hairstyleScript;

        [SerializeField] protected Material red;
        [SerializeField] protected Material blue;
        [SerializeField] protected Material black;
        [SerializeField] protected Material pink;
        [SerializeField] protected Material yellow;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            eyesScript = FindFirstObjectByType<Eyes>();
            hairstyleScript = FindFirstObjectByType<Hairstyles>();
        }

        // Update is called once per frame
        void Update()
        {
            if(Keyboard.current.eKey.wasPressedThisFrame)
            {
                CycleColourForward();
            }
        }

        public void CycleColourForward()
        {
            if (eyeAndHairColours.Length == 0)
            {
                return;
            }
            currentHairEyeColourIndex = (currentHairEyeColourIndex + 1) % eyeAndHairColours.Length;


            if (eyesScript.eyesScreen.enabled)
            {
                ApplyColour();
                ShowColourName();
            }
            else if (hairstyleScript.hairstyleScreen.enabled)
            {
                ApplyColour();
                ShowColourName();
            }
        }

        public void CycleColourBackward()
        {
            if (eyeAndHairColours.Length == 0)
            {
                return;
            }
            currentHairEyeColourIndex = (currentHairEyeColourIndex - 1 + eyeAndHairColours.Length) % eyeAndHairColours.Length;

            if (eyesScript.eyesScreen.enabled)
            {
                ApplyColour();
                ShowColourName();
            }
            else if (hairstyleScript.hairstyleScreen.enabled)
            {
                ApplyColour();
                ShowColourName();
            }

        }

        private void ApplyColour()
        {
            if (eyesScript.eyesScreen.enabled)
            {
                Renderer[] eyeRenderers = eyesScript.parentEyes.GetComponentsInChildren<Renderer>();
                
                foreach(Renderer renderer in eyeRenderers)
                {
                    renderer.sharedMaterial = eyeAndHairColours[currentHairEyeColourIndex];
                }
            }
            else if (hairstyleScript.hairstyleScreen.enabled)
            {
                Renderer[] hairRenderers = hairstyleScript.parentHair.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in hairRenderers)
                {
                    renderer.sharedMaterial = eyeAndHairColours[currentHairEyeColourIndex];
                }
            }
        }

        public void ShowColourName() // Causes null refs with the nose script but adding it to the nose script breaks the button's function so I've just left it as is honestly
        {
            if (eyeAndHairColours != null && currentHairEyeColourIndex < eyeAndHairColours.Length && eyeAndHairColours[currentHairEyeColourIndex] != null)
            {
                if(eyesScript.eyesScreen.enabled)
                {
                    eyesScript.eyeColourText.text = eyeAndHairColours[currentHairEyeColourIndex].name;
                }
                else if (hairstyleScript.hairstyleScreen.enabled)
                {
                    hairstyleScript.hairColourText.text = eyeAndHairColours[currentHairEyeColourIndex].name;
                }

            }
        }
    }

}
