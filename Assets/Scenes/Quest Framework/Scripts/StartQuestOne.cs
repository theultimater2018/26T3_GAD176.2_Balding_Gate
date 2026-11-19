using UnityEngine;

namespace QuestFramework
{
    public class StartQuestOne : QuestFramework
    {

        public bool isQuestOneOver = false; // Checks if the first quest is over, used to start the second quest

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void QuestAccepted() // Accepts the quest when the player finishes the dialogue with the quest giver, and sets the quest objective text to the first quest objective
        {
    
            if (isPlayerTalkingToQuestGiver == true && hasQuestBeenAccepted == false && isQuestOneOver == false)
            {
                hasQuestBeenAccepted = true;

                dialogueText.text = "INPUT DIALOGUE HERE ";
                dialogueSpeakerTilte.SetActive(true);
                dialogueBox.SetActive(true);
            }

            if (isPlayerTalkingToQuestGiver == true && hasQuestBeenAccepted == true && isQuestOneOver == false)
            {
                questObejctiveText.text = "The Tv has spoken... clean the space pipes on the south platform";
            }
        }

        private void QuestObjectiveCompleted()
        {
           // NEEDS STUFF // 
        }

        private void OnCollisionEnter2D(Collision2D collision) // Created the prompts for the player to know its an interactable object, and sets the bool for the player talking to the quest giver to true
        {
        
            if (collision.gameObject.CompareTag("Player"))
            {
                UIInteractPrompt.SetActive(true);
                floorInteractPrompt.SetActive(true);

                isPlayerTalkingToQuestGiver = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision) // Removes the prompts for the player to know its an interactable object when the player ;eaves the area, and sets the bool for the player talking to the quest giver to false to turn off those interactions 
        {
        
            if (collision.gameObject.CompareTag("Player"))
            {
                UIInteractPrompt.SetActive(false);
                floorInteractPrompt.SetActive(false);

                dialogueSpeakerTilte.SetActive(false);
                dialogueBox.SetActive(false);

                isPlayerTalkingToQuestGiver = false;
            }
        }
    }
}
