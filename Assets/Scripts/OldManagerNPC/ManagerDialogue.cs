using TMPro;
using UnityEngine;

// Dialogue Controller for Old Manager NPC
public class ManagerDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;

    [Header("References")]
    [SerializeField] private ManagerController managerController;
    [SerializeField] private ManagerAnimatorController animatorController;
    [SerializeField] private DialogueVoiceOver voiceOver;
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject canvasPivot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Camera targetCamera;

    private int lastAdvanceFrame = -1;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (voiceOver == null)
        {
            voiceOver = GetComponent<DialogueVoiceOver>();
        }

        if (animatorController == null)
        {
            animatorController = GetComponent<ManagerAnimatorController>();
        }

        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }

        if (voiceOver == null)
        {
            Debug.LogError($"{name}: No DialogueVoiceOver found. Add one to this NPC or assign it in the inspector.");
        }

        TriggerDialogue();
    }

    private void LateUpdate()
    {
        canvasPivot.transform.rotation = Quaternion.LookRotation(canvasPivot.transform.position - targetCamera.transform.position);

        // Hide the outline while walking, even if the player script turned it on this frame
        if (outline != null && managerController.IsWalking)
        {
            outline.enabled = false;
        }
    }

    private void TriggerDialogue()
    {
        if (voiceOver != null)
        {
            voiceOver.ResetSequence();
        }

        if (animatorController != null)
        {
            animatorController.ResetSequence();
        }

        // The first sentence is shown here, so move, animate and speak for it too
        if (DialogueManager.Instance.StartDialogue(dialogue, canvas, nameText, dialogueText))
        {
            managerController.NextPosition();

            if (animatorController != null)
            {
                animatorController.NextAnimation();
            }

            if (voiceOver != null)
            {
                voiceOver.PlayNext();
            }
        }
    }

    public void DisplayNextSentence()
    {
        // Can't skip while the NPC is walking
        if (managerController.IsWalking)
            return;

        // Ignore duplicate calls in the same frame. A duplicate makes
        // DialogueManager return false, which would wrongly stop the voice over.
        if (lastAdvanceFrame == Time.frameCount)
            return;

        lastAdvanceFrame = Time.frameCount;

        // Only move, animate and speak when a new sentence was actually shown
        if (DialogueManager.Instance.DisplayNextSentence())
        {
            managerController.NextPosition();

            if (animatorController != null)
            {
                animatorController.NextAnimation();
            }

            if (voiceOver != null)
            {
                // Stops the previous clip and plays the next one
                voiceOver.PlayNext();
            }
        }
        else if (voiceOver != null)
        {
            // Dialogue really ended, cut off any voice over still playing
            voiceOver.Stop();
        }
    }
}