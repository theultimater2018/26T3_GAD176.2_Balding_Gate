using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the scripts for toggling and displaying the skintone materials on the character body. Requires BodyType as its parent script to access the body 
/// game objects and their mesh renderers for cycling through materials on the objects mesh renderers accordingly. 
/// Materials used are assigned in the inspector accordingly.
/// </summary>

namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{
    public class SkinColourManager : BodyType
    {
        [SerializeField] protected Material[] skinColours;
        [SerializeField] protected int currentSkinColourIndex = 0;

        #region Unity Base Functions (Start)
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            base.FindAllObjectRefs();

            GameObject skinText = GameObject.Find("SkinColourNameDisplayer");
            if (skinText != null)
            {
                skinColourText = skinText.GetComponent<TextMeshProUGUI>();
            }

            ApplySkinColour();
            ShowSkinColourName();
        }

        private void Update()
        {
         
        }
        #endregion

        #region Skin Colour Navigator Functions
        public void CycleSkinColourForward()
        {
            if (type1Body.activeSelf == true && type2Body.activeSelf == false)
            {
                if (skinColours.Length == 0)
                {
                    return;
                }
                currentSkinColourIndex = (currentSkinColourIndex + 1) % skinColours.Length;

                ApplySkinColour();
                ShowSkinColourName();
            }
            else if (type2Body.activeSelf == true && type1Body.activeSelf == false)
            {
                if (skinColours.Length == 0)
                {
                    return;
                }
                currentSkinColourIndex = (currentSkinColourIndex + 1) % skinColours.Length;

                ApplySkinColour();
                ShowSkinColourName();
            }
            else
            {
                return;
            }
        }

        public void CycleSkinColourBackward()
        {
            if (type1Body.activeSelf == true && type2Body.activeSelf == false)
            {
                if (skinColours.Length == 0)
                {
                    return;
                }
                currentSkinColourIndex = (currentSkinColourIndex - 1 + skinColours.Length) % skinColours.Length;
                ApplySkinColour();
                ShowSkinColourName();
            }
            else if (type2Body.activeSelf == true && type1Body.activeSelf == false)
            {
                if (skinColours.Length == 0)
                {
                    return;
                }
                currentSkinColourIndex = (currentSkinColourIndex - 1 + skinColours.Length) % skinColours.Length;
                ApplySkinColour();
                ShowSkinColourName();
            }
            else
            {
                return;
            }
        }

        #endregion

        #region Skin Colour Applicator + Skin Colour UI Name Displayer Functions
        protected void ApplySkinColour()
        {
            if(type1Body.activeSelf == true && type2Body.activeSelf == false)
            {
                if(type1Body.activeSelf == true && type1BodyRenderer != null)
                {
                    type1BodyRenderer.sharedMaterial = skinColours[currentSkinColourIndex];   
                }
            }
            else if(type2Body.activeSelf == true && type1Body.activeSelf == false)
            {
                if(type2Body.activeSelf == true && type2BodyRenderer != null)
                {
                    type2BodyRenderer.sharedMaterial = skinColours[currentSkinColourIndex];   
                }
            }
            else
            {
                return;
            }
        }
        public void ShowSkinColourName() // Causes null refs with the nose script but adding it to the nose script breaks the button's function so I've just left it as is honestly
        {
            if (skinColours != null && currentSkinColourIndex < skinColours.Length && skinColours[currentSkinColourIndex] != null)
            {
                skinColourText.text = skinColours[currentSkinColourIndex].name;
            }
        } 
        #endregion
    }

}
