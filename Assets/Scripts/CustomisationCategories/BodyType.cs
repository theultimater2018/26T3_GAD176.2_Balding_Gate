using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Covers everything to do with the Body Type section of the character customiser, such as UI Logic and how it toggles the body type options accordingly.
/// </summary>

namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{

    public class BodyType : MonoBehaviour, ICustomizerFunctions
    {
        #region BodyType Variables
        [SerializeField] private SkinColourManager skinColourManager;

        //[SerializeField] private Noses noseScript; Removed Dependency
        [SerializeField] private Eyes eyeScript;

        private GameObject parentBody;
        [SerializeField] protected GameObject type1Body;
        [SerializeField] protected GameObject type2Body;
        [SerializeField] protected MeshRenderer type1BodyRenderer;
        [SerializeField] protected MeshRenderer type2BodyRenderer;

       [SerializeField] public Canvas bodyTypeScreen;

        private Button bodyType1Button;
        private Button bodyType2Button;
        [SerializeField] protected Button skinColourForwardButton;
        [SerializeField] protected Button skinColourBackwardButton;
        private Button completeBodySelection;

        [SerializeField] protected TextMeshProUGUI skinColourText;
        #endregion

        #region Unity Functions (Start + Awake)
        void Start()
        {
            FindAllObjectRefs();
            FindAllUIRefs();
            skinColourManager = FindFirstObjectByType<SkinColourManager>();

            bodyTypeScreen.enabled = true;
        }

        #endregion

        #region Reference Finder Functions
        public void FindAllObjectRefs()
        {
            //noseScript = FindFirstObjectByType<Noses>(); Removed Dependency
            
            GameObject bodyTypeParent = GameObject.Find("Body");
            if(bodyTypeParent != null) 
            {
                parentBody = bodyTypeParent.gameObject;
            }

            GameObject bodyType1 = parentBody.transform.GetChild(0).gameObject;
            if (bodyType1 != null)
            {
                type1Body = bodyType1.gameObject;
            }

            MeshRenderer type1Renderer = type1Body.GetComponent<MeshRenderer>();
            if (type1Renderer != null)
            {
                type1BodyRenderer = type1Renderer.GetComponent<MeshRenderer>();
            }

            GameObject bodyType2 = parentBody.transform.GetChild(1).gameObject;
            if (bodyType2 != null)
            {
                type2Body = bodyType2.gameObject;
            }

            MeshRenderer type2Renderer = type2Body.GetComponent<MeshRenderer>();
            if (type2Renderer != null)
            {
                type2BodyRenderer = type2Renderer.GetComponent<MeshRenderer>();
            }
        }

        public virtual void FindAllUIRefs()
        {
            GameObject bodyTypeCanvas = GameObject.Find("BodyScreen");
            if (bodyTypeCanvas != null)
            {
                bodyTypeScreen = bodyTypeCanvas.GetComponent<Canvas>();
            }

            GameObject leftButton = GameObject.Find("BodyType1Button");
            if (leftButton != null)
            {
                bodyType1Button = leftButton.GetComponent<Button>();
                bodyType1Button.onClick.AddListener(ToggleOptionForward);
            }

            GameObject rightButton = GameObject.Find("BodyType2Button");
            if (rightButton != null)
            {
                bodyType2Button = rightButton.GetComponent<Button>();
                bodyType2Button.onClick.AddListener(ToggleOptionBackward);
            }

            GameObject skinToneRightButton = GameObject.Find("CycleSkinColourForward");
            if(skinToneRightButton != null)
            {
                skinColourForwardButton = skinToneRightButton.GetComponent<Button>();
            }
            if(skinColourManager != null)
            {
                skinColourForwardButton.onClick.AddListener(skinColourManager.CycleSkinColourForward);
            }

            GameObject skinToneLeftButton = GameObject.Find("CycleSkinColourBackward");
            if (skinToneLeftButton != null)
            {
                skinColourBackwardButton = skinToneLeftButton.GetComponent<Button>();
            }
            if (skinColourManager != null)
            {
                skinColourForwardButton.onClick.AddListener(skinColourManager.CycleSkinColourBackward);
            }

            GameObject confirmBody = GameObject.Find("ConfirmBodySelection");
            if (confirmBody != null)
            {
                completeBodySelection = confirmBody.GetComponent<Button>();
                completeBodySelection.onClick.AddListener(ConfirmSelection);
            }

            GameObject skinText = GameObject.Find("SkinColourNameDisplayer");
            if (skinText != null)
            {
                skinColourText = skinText.GetComponent<TextMeshProUGUI>();
            }
        }
        #endregion

        #region Button Functions (sans skintone navigators, which are in SkinColourManager)
        public void ToggleOptionForward() // Body Type 1 Button
        {
            if(type1Body != null && type2Body != null)
            {
                type1Body.gameObject.SetActive(true);
                type2Body.gameObject.SetActive(false);
            } 
        }
        public void ToggleOptionBackward() // Body Type 2 Button
        {
            if (type1Body != null && type2Body != null)
            {
                type1Body.gameObject.SetActive(false);
                type2Body.gameObject.SetActive(true);
            }  
        }

         public void ConfirmSelection()
        {
            bodyTypeScreen.enabled = false;
            //noseScript.noseScreen.enabled = true; Removed Dependency
            eyeScript.eyesScreen.enabled = true;
        }

        public virtual void GoBack()
        {
            Application.Quit(); 
        }

        #endregion
    }
}


