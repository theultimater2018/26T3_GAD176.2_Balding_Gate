using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace QuestFramework
{
    public class Q1Objective : QuestFramework
    {

        public StartQuestOne questOneScriptNPC;

        public bool isPlayerAtTheBones = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            UIInteractPrompt.SetActive(false);
            questTargetPrompt.SetActive(false);
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

            if (questOneScriptNPC.hasQuestBeenAccepted == true && hasQuestObjectiveBeenCompleted == false)
            {
                questTargetPrompt.SetActive(true);
            }
        }

        private void OnEnable()
        {
            OnSpacePressedInteract.AddListener(questObjectiveCompleted);
        }

        private void OnDisable()
        {
            OnSpacePressedInteract.RemoveListener(questObjectiveCompleted);
        }

        private void questObjectiveCompleted()
        {
            if (hasQuestObjectiveBeenCompleted == false && questOneScriptNPC.hasQuestBeenAccepted == true && isPlayerAtTheBones == true)
            {
                dialogueText.text = "Well i found the smell.. they should really clean out the dead bodies, half of these have turned to dust and bone ";
                hasQuestObjectiveBeenCompleted = true;
                questObejctiveText.text = "inform the guard the cause of the smell ";

                dialogueBox.SetActive(true);
                UIInteractPrompt.SetActive(false);
                questTargetPrompt.SetActive(false);
            }

        }

        private void OnCollisionEnter2D(Collision2D collision)
        {

            if (collision.gameObject.CompareTag("Player") && questOneScriptNPC.hasQuestBeenAccepted == true && hasQuestObjectiveBeenCompleted == false)
            {
                UIInteractPrompt.SetActive(true);
                questTargetPrompt.SetActive(false);
                isPlayerAtTheBones = true;
                BoxesOff();
            }

            else if (collision.gameObject.CompareTag("Player"))
            {
                UIInteractPrompt.SetActive(false);
                questTargetPrompt.SetActive(false);
                isPlayerAtTheBones = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                UIInteractPrompt.SetActive(false);
                questTargetPrompt.SetActive(false);
                isPlayerAtTheBones = false;
                BoxesOff();
            }

            if (collision.gameObject.CompareTag("Player") && questOneScriptNPC.hasQuestBeenAccepted == true && hasQuestObjectiveBeenCompleted == false)
            {
                UIInteractPrompt.SetActive(false);
                questTargetPrompt.SetActive(true);
                isPlayerAtTheBones = false;
                BoxesOff();
            }
        }
    }
}
