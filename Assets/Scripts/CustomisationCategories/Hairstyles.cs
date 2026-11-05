using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{
    public class Hairstyles : HairEyeColourManager, ICustomizerFunctions
    {
        [SerializeField] private Eyes eyes;
        [SerializeField] private NameAndSkills nameAndSkills;

        [SerializeField] private GameObject[] hairstyles;
        [SerializeField] private int currentHairstyleIndex;

        [SerializeField] public GameObject parentHair;
        [SerializeField] public Canvas hairstyleScreen;

        [SerializeField] private Button toggleHairForward;
        [SerializeField] private Button toggleHairBackward;
        [SerializeField] private Button toggleHairColourForward;
        [SerializeField] private Button toggleHairColourBackward;
        [SerializeField] private Button confirmHairSelection;
        [SerializeField] private Button goBackToEyes;
        [SerializeField] private TextMeshProUGUI hairText;
        [SerializeField] public TextMeshProUGUI hairColourText;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            FindAllObjectRefs();
            FindAllUIRefs();
            hairstyleScreen.enabled = false;    
        }

        #region Object Reference Assigner Functions
        public void FindAllObjectRefs()
        {
            eyes = FindFirstObjectByType<Eyes>();
            nameAndSkills = FindFirstObjectByType<NameAndSkills>();

            GameObject hairParent = GameObject.Find("Hair");
            if (hairParent != null)
            {
                parentHair = hairParent.gameObject;
            }            
        }

        public void FindAllUIRefs()
        {
            GameObject hairstyleCanvas = GameObject.Find("HairstyleScreen");
            if (hairstyleCanvas != null)
            {
                hairstyleScreen = hairstyleCanvas.GetComponent<Canvas>();
            }
            GameObject hairForward = GameObject.Find("CycleHairstyleForward");
            if (hairForward != null)
            {
                toggleHairForward = hairForward.GetComponent<Button>();
            }
            GameObject hairBackward = GameObject.Find("CycleHairstyleBackward");
            if (hairBackward != null)
            {
                toggleHairBackward = hairBackward.GetComponent<Button>();
            }
            GameObject hairColourForward = GameObject.Find("CycleHairColourForward");
            if (hairColourForward != null)
            {
                toggleHairForward = hairColourForward.GetComponent<Button>();
            }
            GameObject hairColourBackward = GameObject.Find("CycleHairColourBackward");
            if (hairColourBackward != null)
            {
                toggleHairForward = hairColourBackward.GetComponent<Button>();
            }

            GameObject hairName = GameObject.Find("HairNameDisplayer");
            if (hairName != null)
            {
                hairText = hairName.GetComponent<TextMeshProUGUI>();
            }
            GameObject hairColourName = GameObject.Find("HairColourNameDisplayer");
            if (hairColourName != null)
            {
                hairColourText = hairColourName.GetComponent<TextMeshProUGUI>();
            }
            GameObject hairConfirm = GameObject.Find("ConfirmHairSelection");
            if (hairConfirm != null)
            {
                confirmHairSelection = hairConfirm.GetComponent<Button>();
            }
            GameObject goBackEyes= GameObject.Find("GoBackToEyesButton");
            if (goBackEyes != null)
            {
                goBackToEyes = goBackEyes.GetComponent<Button>();
            }
        }

        #endregion

        public void ToggleOptionForward()
        {
            if (hairstyles.Length == 0)
            {
                return;
            }

            if (hairstyles[currentHairstyleIndex] != null)
            {
                hairstyles[currentHairstyleIndex].SetActive(false);
            }
            currentHairstyleIndex = (currentHairstyleIndex + 1) % hairstyles.Length;

            if (hairstyles[currentHairstyleIndex] != null)
            {
                hairstyles[currentHairstyleIndex].SetActive(true);
                ShowHairName();
            }
        }
        
        public void ToggleOptionBackward()
        {
            if (hairstyles.Length == 0)
            {
                return;
            }

            if (hairstyles[currentHairstyleIndex] != null)
            {
                hairstyles[currentHairstyleIndex].SetActive(false);
            }
            currentHairstyleIndex = (currentHairstyleIndex - 1 + hairstyles.Length) % hairstyles.Length;

            if (hairstyles[currentHairstyleIndex] != null)
            {
                hairstyles[currentHairstyleIndex].SetActive(true);
                ShowHairName();
            }
        }
        public void ConfirmSelection()
        {
            hairstyleScreen.enabled = false;
            nameAndSkills.nameAndSkillsScreen.enabled = true;
        }

        public void GoBack()
        {
            hairstyleScreen.enabled = false;
            eyes.eyesScreen.enabled = true;
        }

        private void ShowHairName()
        {
            if (hairstyles != null && currentHairstyleIndex < hairstyles.Length && hairstyles[currentHairstyleIndex] != null)
            {
                hairText.text = hairstyles[currentHairstyleIndex].name;

            }
        }
    }

}
