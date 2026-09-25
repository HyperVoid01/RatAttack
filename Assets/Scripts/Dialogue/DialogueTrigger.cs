using TMPro;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private float delay;
    [SerializeField] private bool startOnPlay;
    
    [Header("References")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;

    private void Start()
    {
        if (startOnPlay)
            TriggerDialogue();
    }

    private void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue, canvas, nameText, dialogueText, delay);
    }
}
