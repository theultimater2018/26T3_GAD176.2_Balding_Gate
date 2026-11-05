using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{
    public class Eyes : HairEyeColourManager, ICustomizerFunctions
    {
        //[SerializeField] private Noses noseScript; Removed Dependency
        [SerializeField] private BodyType bodyScript;
        [SerializeField] private Hairstyles hair;

        [SerializeField] public GameObject parentEyes;
        [SerializeField] private GameObject[] eyes;
        [SerializeField] private int currentEyeIndex;

        [SerializeField] private Button toggleEyeForward;
        [SerializeField] private Button toggleEyeBackward;
        [SerializeField] private Button toggleEyeColourForward;
        [SerializeField] private Button toggleEyeColourBackward;
        [SerializeField] private Button confirmEyeSelection;
        [SerializeField] private Button goBackToNose;
        [SerializeField] private TextMeshProUGUI eyesText;
        [SerializeField] public TextMeshProUGUI eyeColourText;

        public Canvas eyesScreen;

        #region Unity Functions
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        void Start()
        {
            FindAllObjectRefs();
            FindAllUIRefs();
            eyesScreen.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region Option Toggler Functions
        public void ToggleOptionForward()
        {
            if (eyes.Length == 0)
            {
                return;
            }

            if (eyes[currentEyeIndex] != null)
            {
                eyes[currentEyeIndex].SetActive(false);
            }
            currentEyeIndex = (currentEyeIndex + 1) % eyes.Length;

            if (eyes[currentEyeIndex] != null)
            {
                eyes[currentEyeIndex].SetActive(true);
                ShowEyeName();
            }
        }
        public void ToggleOptionBackward()
        {
            if (eyes.Length == 0)
            {
                return;
            }

            if (eyes[currentEyeIndex] != null)
            {
                eyes[currentEyeIndex].SetActive(false);
            }
            currentEyeIndex = (currentEyeIndex - 1 + eyes.Length) % eyes.Length;

            if (eyes[currentEyeIndex] != null)
            {
                eyes[currentEyeIndex].SetActive(true);
                ShowEyeName();
            }
        }
        #endregion

        #region Reference Finder Functions
        public void FindAllObjectRefs()
        {
            //noseScript = FindFirstObjectByType<Noses>(); Removed dependency
            hair = FindFirstObjectByType<Hairstyles>();

            GameObject eyeParent = GameObject.Find("Eyes");
            if (eyeParent != null)
            {
                parentEyes = eyeParent.gameObject;
            }

        }
        
        public void FindAllUIRefs()
        {
            GameObject eyesCanvas = GameObject.Find("EyesScreen");
            if (eyesCanvas != null)
            {
                eyesScreen = eyesCanvas.GetComponent<Canvas>();
            }

            GameObject eyeForward = GameObject.Find("CycleEyesForward");
            if (eyeForward != null)
            {
                toggleEyeForward = eyeForward.GetComponent<Button>();
            }
            GameObject eyeBackward = GameObject.Find("CycleEyesBackward");
            if (eyeBackward != null)
            {
                toggleEyeBackward= eyeBackward.GetComponent<Button>();
            }
            GameObject eyeColourForward = GameObject.Find("CycleEyeColourForward");
            if (eyeColourForward != null)
            {
                toggleEyeForward = eyeColourForward.GetComponent<Button>();
            }
            GameObject eyeColourBackward = GameObject.Find("CycleEyeColourBackward");
            if (eyeColourBackward != null)
            {
                toggleEyeForward = eyeColourBackward.GetComponent<Button>();
            }

            GameObject eyeName = GameObject.Find("EyesNameDisplayer");
            if (eyeName != null)
            {
                eyesText = eyeName.GetComponent<TextMeshProUGUI>();
            }
            GameObject eyeColourName = GameObject.Find("EyesColourNameDisplayer");
            if (eyeColourName != null)
            {
                eyeColourText = eyeColourName.GetComponent<TextMeshProUGUI>();
            }
            GameObject eyeConfirm = GameObject.Find("ConfirmEyeSelection");
            if (eyeConfirm != null)
            {
               confirmEyeSelection = eyeConfirm.GetComponent<Button>();
            }
            GameObject goBackNose = GameObject.Find("GoBackToNoseButton");
            if (goBackNose != null)
            {
                goBackToNose = goBackNose.GetComponent<Button>();
            }

        }
        #endregion
        public void ConfirmSelection()
        {
            eyesScreen.enabled = false;
            hair.hairstyleScreen.enabled = true;
        }

        public void GoBack()
        {
            eyesScreen.enabled = false;
            //noseScript.noseScreen.enabled = true; Removed Dependency
            bodyScript.bodyTypeScreen.enabled = true;   
        }

        public void ShowEyeName()
        {
            if (eyes != null && currentEyeIndex < eyes.Length && eyes[currentEyeIndex] != null)
            {
                eyesText.text = eyes[currentEyeIndex].name;
                
            }
        }

    }

}
