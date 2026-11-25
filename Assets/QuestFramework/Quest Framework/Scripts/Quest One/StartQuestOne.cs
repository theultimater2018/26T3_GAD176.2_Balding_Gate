using UnityEngine;
using UnityEngine.InputSystem;

namespace QuestFramework
{
    public class StartQuestOne : QuestFramework
    {

        public bool isQuestOneOver = false; // Checks if the first quest is over, used to start the second quest

        public GameObject questTitleBox; // Unchecked Quest Title Box
        public GameObject completedQuestTitleBox; // Checked Quest Title Box

        public bool speech1finished;

        public Q1Objective objectiveScript;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            completedQuestTitleBox.SetActive(false);
            questTargetPrompt.SetActive(true);
            speech1finished = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (OnSpacePressedInteract != null)
                {
                    OnSpacePressedInteract.Invoke();

                }
            }

            if (hasQuestBeenAccepted == true && objectiveScript.hasQuestObjectiveBeenCompleted == true && isQuestOneOver == false)
            {
                questTargetPrompt.SetActive(true);
            }
        }

        private void QuestAccepted() // Accepts the quest when the player finishes the dialogue with the quest giver, and sets the quest objective text to the first quest objective
        {

            if (isPlayerTalkingToQuestGiver == true && hasQuestBeenAccepted == false && isQuestOneOver == false)
            {
                hasQuestBeenAccepted = true;

                dialogueText.text = "Ah! New recruit, hm yes i got something here for you.. *Rustles Through Papers* ";
                dialogueSpeakerTilte.SetActive(true);
                dialogueBox.SetActive(true);
                questTargetPrompt.SetActive(true);
                speech1finished = true;
            }

            else if (speech1finished == true && isPlayerTalkingToQuestGiver == true && isQuestOneOver == false)
            {
                dialogueText.text = "Theres a smell coming from the south west training room, go figure it out ";
                dialogueSpeakerTilte.SetActive(true);
            }

            if (isPlayerTalkingToQuestGiver == true && hasQuestBeenAccepted == true && isQuestOneOver == false)
            {
                questObejctiveText.text = "Find the awful smell from the training room, located somwehere in the southwest of the castle";
                dialogueSpeakerTilte.SetActive(true);
                questTargetPrompt.SetActive(false);
            }
        }

        private void QuestOnebeenHandedIn()
        {
            if (isPlayerTalkingToQuestGiver == true && objectiveScript.hasQuestObjectiveBeenCompleted == true && hasQuestOnebeenHandedIn == false)
            {
                questTargetPrompt.SetActive(false);
                dialogueText.text = "Dead bodies? Never seen em. Ill let the jaintor know, alright next.. ";
                dialogueSpeakerTilte.SetActive(true);
                completedQuestTitleBox.SetActive(true);
                questTitleBox.SetActive(false);
                isQuestOneOver = true;
                dialogueBox.SetActive(true);
                hasQuestOnebeenHandedIn = true;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) // Created the prompts for the player to know its an interactable object, and sets the bool for the player talking to the quest giver to true
        {

            if (collision.gameObject.CompareTag("Player"))
            {
                UIInteractPrompt.SetActive(true);

                isPlayerTalkingToQuestGiver = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision) // Removes the prompts for the player to know its an interactable object when the player leaves the area, and sets the bool for the player talking to the quest giver to false to turn off those interactions 
        {

            if (collision.gameObject.CompareTag("Player"))
            {
                UIInteractPrompt.SetActive(false);

                dialogueSpeakerTilte.SetActive(false);
                dialogueBox.SetActive(false);

                isPlayerTalkingToQuestGiver = false;
            }
        }


        public void OnEnable()
        {
            OnSpacePressedInteract.AddListener(QuestAccepted);
            OnSpacePressedInteract.AddListener(QuestOnebeenHandedIn);

        }

        public void OnDisable()
        {
            OnSpacePressedInteract.RemoveListener(QuestAccepted);
            OnSpacePressedInteract.RemoveListener(QuestOnebeenHandedIn);
        }
    }
    }
