using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

namespace QuestFramework
{
    public class QuestFramework : MonoBehaviour
    {
        
        public bool isPlayerTalkingToQuestGiver = false; // Checks if the player is currently in the collider for talking to the quest giver
        public bool hasQuestBeenAccepted = false; // Checks if the player has accepted the quest, by finisnhing the dialogue with the quest giver
        public bool hasQuestObjectiveBeenCompleted = false; // Checks if the player has completed the quest objective, by collecting the required items or completing the required task
        public bool hasQuestOnebeenHandedIn = false; // Checks if the player has handed in the quest, by finishing the dialogue with the quest giver after completing the quest objective to start the next quest
        
      

        public GameObject dialogueSpeakerTilte; // The title of the dialogue speaker, which is usually the quest giver or an NPC
        public GameObject UIInteractPrompt; // The UI prompt that appears when the player is in range of the quest giver or an NPC to interact with them, usuanlly a button prompt to press a key to interact with them (Space)
        public GameObject questTargetPrompt; // The floor prompt that appears when the player is in range of the quest giver or an NPC to interact with them, usuanlly a zone that highlights the area where the player can interact with them

        public GameObject questInfoBox; // The quest info box that appears when the player presses the tab key, which shows the current quest objective
        public bool isQuestInfoBoxActive = false; // Checks if the quest info box is currently active or not, used in this script below
        public TextMeshProUGUI questObejctiveText; // The text that appears in the quest info box, which shows the current quest objective

        public TextMeshProUGUI dialogueText; // The text that appears in the dialogue box, which shows the current dialogue from the quest giver, player character or an NPC
        public GameObject dialogueBox; // The dialogue box that appears when the player is talking to the quest giver or an NPC, which shows the current dialogue from the quest giver, player character or an NPC

        public UnityEvent OnSpacePressedInteract = new UnityEvent(); // The event that is invoked when the player presses the space key to do basically anything, can be changed a lot 

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

            dialogueSpeakerTilte.SetActive(false);
            UIInteractPrompt.SetActive(false);

            questInfoBox.SetActive(false);

            dialogueBox.SetActive(false);

        }

        // Update is called once per frame
        void Update()
        {
            ControlQuestInfoBox();
        }

        private void ControlQuestInfoBox()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame && isQuestInfoBoxActive == false)
            {
                questInfoBox.SetActive(true);
                isQuestInfoBoxActive = true;
            }


            else if (Keyboard.current.tabKey.wasPressedThisFrame && isQuestInfoBoxActive == true)
            {
                questInfoBox.SetActive(false);
                isQuestInfoBoxActive = false;
            }
        }

        public void BoxesOn()
        {
            dialogueBox.SetActive(true);
            dialogueSpeakerTilte.SetActive(true);


        }

        public void BoxesOff()
        {
            dialogueBox.SetActive(false);
            dialogueSpeakerTilte.SetActive(false);
        }
    }
}
