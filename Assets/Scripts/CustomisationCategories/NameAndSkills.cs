using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{
    public class NameAndSkills : MonoBehaviour, ICustomizerFunctions
    {
        [SerializeField] private TMP_InputField characterName;
        [SerializeField] private Toggle showSkillsDescription;
        [SerializeField] private Hairstyles hair;
        
        [SerializeField] private int intelligence = 1;
        [SerializeField] private int strength = 1;
        [SerializeField] private int giftOfGab = 1;
        [SerializeField] private int hardiness = 1;
        [SerializeField] private int reflexes = 1;

        [SerializeField] private TextMeshProUGUI intText;
        [SerializeField] private TextMeshProUGUI strText;
        [SerializeField] private TextMeshProUGUI gabText;
        [SerializeField] private TextMeshProUGUI hrdText;
        [SerializeField] private TextMeshProUGUI rfxText;

        [SerializeField] private TextMeshProUGUI skillDescriptions;

        public Canvas nameAndSkillsScreen;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            nameAndSkillsScreen.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            intText.text = "INT: " + intelligence;
            strText.text = "STR: " + strength;
            gabText.text = "GAB: " + giftOfGab;
            hrdText.text = "HRD: " + hardiness;
            rfxText.text = "RFX: " + reflexes;
        }

        #region Reference Finder Functions
        public void FindAllObjectRefs()
        {
            GameObject nameAndSkillsCanvas = GameObject.Find("NameAndSkillsScreen");
            if (nameAndSkillsCanvas != null)
            {
                nameAndSkillsScreen = nameAndSkillsCanvas.GetComponent<Canvas>();
            }

            GameObject skillDescToggler = GameObject.Find("ShowSkillDescriptions");
            if(skillDescToggler != null)
            {
                showSkillsDescription = skillDescToggler.GetComponent<Toggle>();
            }
        }
        public void FindAllUIRefs()
        {
            GameObject description = GameObject.Find("SkillDescriptions");
            if(description != null)
            {
                skillDescriptions = description.GetComponent<TextMeshProUGUI>();
            }
            GameObject INT = GameObject.Find("Intelligence");
            if(INT != null)
            {
                intText = INT.GetComponent<TextMeshProUGUI>();
            }
            GameObject STR = GameObject.Find("Strength");
            if(STR != null)
            {
                strText = STR.GetComponent<TextMeshProUGUI>();
            }
            GameObject GAB = GameObject.Find("Gift Of Gab");
            if(GAB != null)
            {
                gabText = GAB.GetComponent<TextMeshProUGUI>();
            }
            GameObject HRD = GameObject.Find("Hardiness");
            if(HRD != null)
            {
                hrdText = HRD.GetComponent<TextMeshProUGUI>();
            }
            GameObject RFX = GameObject.Find("Reflexes");
            if(RFX != null)
            {
                rfxText = RFX.GetComponent<TextMeshProUGUI>();
            }
        }

        #endregion

        #region Option Toggler Functions
        public void ToggleOptionForward()
        {
           // Intentionally left Blank due to skill-specific function togglers needed.
        }
        public void ToggleOptionBackward()
        {
            // Intentionally left Blank due to skill-specific function togglers needed.
        }

        public void ToggleINTForward()
        {
            intelligence++;
            if (intelligence >= 11)
            {
                intelligence = 10;
            }
        }
        public void ToggleINTBackward()
        {
            intelligence--;
            if (intelligence <= 0)
            {
                intelligence = 1;
            }
        }
        public void ToggleSTRForward()
        {
            strength++;
            if (strength >= 11)
            {
                strength = 10;
            }
        }
        public void ToggleSTRBackward()
        {
            strength--;
            if (strength <= 0)
            {
                strength = 1;
            }
        }
        public void ToggleGABForward()
        {
            giftOfGab++;
            if (giftOfGab >= 11)
            {
                giftOfGab = 10;
            }
        }
        public void ToggleGABBackward()
        {
            giftOfGab--;
            if (giftOfGab <= 0)
            {
                giftOfGab = 1;
            }
        }
        public void ToggleHRDForward()
        {
            hardiness++;
            if (hardiness >= 11)
            {
                hardiness = 10;
            }
        }
        public void ToggleHRDBackward()
        {
            hardiness--;
            if (hardiness <= 0)
            {
                hardiness = 1;
            }
        }
        public void ToggleRFXForward()
        {
            reflexes++;
            if (reflexes >= 11)
            {
                reflexes = 10;
            }
        }
        public void ToggleRFXBackward()
        {
            reflexes--;
            if (reflexes <= 0)
            {
                reflexes = 1;
            }
        }

        #endregion

        public void ConfirmSelection()
        {
            Application.Quit();
        }

        public void ShowSkillDescription()
        {
            if(showSkillsDescription != null)
            {
                if(showSkillsDescription == true)
                {
                    skillDescriptions.enabled = true;
                }
                else
                {
                    skillDescriptions.enabled = false;
                }
            }
        }

        public void GoBack()
        {
            nameAndSkillsScreen.enabled = false;
            hair.hairstyleScreen.enabled = true;  
        }
    }

}
