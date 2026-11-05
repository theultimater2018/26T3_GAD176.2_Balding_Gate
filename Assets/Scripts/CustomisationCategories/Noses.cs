using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace SAE.GAD176.CharacterCustomizer.CustomisationOptions
{
    public class Noses : SkinColourManager, ICustomizerFunctions
    {
        [SerializeField] private Eyes eyes;

        public Canvas noseScreen;
        [SerializeField] private GameObject[] noses;
        [SerializeField] private int currentNoseIndex = 0;
        [SerializeField] private GameObject noseParent;

        [SerializeField] private TextMeshProUGUI noseText;
        [SerializeField] private Button cycleForwardNose;
        [SerializeField] private Button cycleBackwardNose;
        [SerializeField] private Button goBackToBody;
        [SerializeField] private Button confirmNose;

        void Start()
        {
            FindAllObjectRefs();
            FindAllUIRefs();

            noseScreen.enabled = false;
        }  

        // Update is called once per frame
        void Update()
        {
            SyncSkinColourBetweenNoses();
        }

        #region Reference Finder Functions
         new private void FindAllObjectRefs()
        {
            base.FindAllObjectRefs();

            eyes = FindFirstObjectByType<Eyes>();

            GameObject parentNose = GameObject.Find("Nose");
            if(parentNose != null)
            {
                noseParent = parentNose.gameObject;
            }
            //GameObject noseType1 = GameObject.Find("ButtonNose");
            //if (noseType1 != null)
            //{
            //    buttonNose = noseType1.gameObject;
            //}

            //GameObject noseType2 = GameObject.Find("PiggyNose");
            //if (noseType2 != null)
            //{
            //    piggyNose = noseType2.gameObject;
            //}

            //GameObject noseType3 = GameObject.Find("HookedNose");
            //if (noseType3 != null)
            //{
            //    hookedNose = noseType3.gameObject;
            //}

            //GameObject noseType4 = GameObject.Find("KittyNose");
            //if (noseType4 != null)
            //{
            //    buttonNose = noseType4.gameObject;
            //}
        }
        new public void FindAllUIRefs()
        {
            GameObject eyesCanvas = GameObject.Find("EyesScreen");
            if (eyesCanvas != null)
            {
                eyes.eyesScreen = eyesCanvas.GetComponent<Canvas>();
            }
            GameObject bodyTypeCanvas = GameObject.Find("BodyScreen");
            if (bodyTypeCanvas != null)
            {
                bodyTypeScreen = bodyTypeCanvas.GetComponent<Canvas>();
            }
            GameObject noseCanvas = GameObject.Find("NoseScreen");
            if(noseCanvas != null)
            {
                noseScreen = noseCanvas.GetComponent<Canvas>();
            }
            GameObject noseNext = GameObject.Find("CycleNoseForward");
            if (noseNext != null)
            {
                cycleForwardNose = noseNext.GetComponent<Button>();
                cycleForwardNose.onClick.AddListener(ToggleOptionForward);
            }
            GameObject nosePrev = GameObject.Find("CycleNoseBackward");
            if (nosePrev != null)
            {
                cycleBackwardNose = nosePrev.GetComponent<Button>();
                cycleBackwardNose.onClick.AddListener(ToggleOptionBackward);
            }
            GameObject goBack = GameObject.Find("GoBackToBodyButton");
            if (goBack != null)
            {
                goBackToBody = goBack.GetComponent<Button>();
                goBackToBody.onClick.AddListener(GoBack);
            }
            GameObject noseName = GameObject.Find("NoseNameDisplayer");
            if (noseName != null)
            {
                noseText = noseName.GetComponent<TextMeshProUGUI>();
            }
        }

        #endregion
        new public void ToggleOptionForward()
        {
            if (noses.Length == 0)
            {
                return;
            }

            if(noses[currentNoseIndex] != null)
            {
                noses[currentNoseIndex].SetActive(false);
            }
            currentNoseIndex = (currentNoseIndex + 1) % noses.Length;

            if (noses[currentNoseIndex] != null)
            {
                noses[currentNoseIndex].SetActive(true);
                ShowNoseName();
            }
        }
        new public void ToggleOptionBackward()
        {
            if (noses.Length == 0)
            {
                return;
            }

            if (noses[currentNoseIndex] != null)
            {
                noses[currentNoseIndex].SetActive(false);
            }
            currentNoseIndex = (currentNoseIndex - 1 + noses.Length) % noses.Length;

            if (noses[currentNoseIndex] != null)
            {
                noses[currentNoseIndex].SetActive(true);
                ShowNoseName();
            }

        }
       
        new public void ConfirmSelection()
        {
            noseScreen.enabled = false;
            eyes.eyesScreen.enabled = true;
        }

        private void ShowNoseName()
        {
            if (noses != null && currentNoseIndex < noses.Length && noses[currentNoseIndex] != null)
            {
                noseText.text = noses[currentNoseIndex].name;
            }
        }

        private void SyncSkinColourBetweenNoses()
        {
            if (skinColours.Length == 0)
            {
                return;
            }

            Renderer[] noseRenderers = noseParent.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in noseRenderers)
            {
                if (type1Body.activeSelf == true)
                {
                    renderer.material = type1BodyRenderer.material;
                }
                else if (type2Body.activeSelf == true)
                {
                    renderer.material = type2BodyRenderer.material;
                }
                else
                {
                    return;
                }
               
            }
        }
        public override void GoBack()
        {
            bodyTypeScreen.enabled = true;
            noseScreen.enabled = false;         
        }
    }
}

