using TMPro;
using UnityEngine;

// Dialogue Controller for Old Manager NPC
public class ManagerDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;

    [Header("References")]
    [SerializeField] private ManagerController managerController;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject canvasPivot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Camera targetCamera;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        TriggerDialogue();
    }

    private void LateUpdate()
    {
        canvasPivot.transform.rotation = Quaternion.LookRotation(canvasPivot.transform.position - targetCamera.transform.position);
    }

    private void TriggerDialogue()
    {
        // The first sentence is shown here, so move for it too
        if (DialogueManager.Instance.StartDialogue(dialogue, canvas, nameText, dialogueText))
        {
            managerController.NextPosition();
        }
    }

    public void DisplayNextSentence()
    {
        // Only move when a new sentence was actually shown
        if (DialogueManager.Instance.DisplayNextSentence())
        {
            managerController.NextPosition();
        }
    }
}