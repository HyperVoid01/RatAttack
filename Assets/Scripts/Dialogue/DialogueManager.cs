using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences;
    private GameObject textBoxObject;
    private TMP_Text dialogueTextBox;
    private float displayDelay;
    
    public static DialogueManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue, GameObject textBoxObject, TMP_Text nameText, TMP_Text dialogueText, float delay)
    {
        sentences.Clear();
        this.textBoxObject = textBoxObject;
        textBoxObject.SetActive(true);
        nameText.text = dialogue.speakerName;
        dialogueTextBox = dialogueText;
        displayDelay = delay;

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        nameText.text = dialogue.speakerName;
        
        StartCoroutine(RunDialogue());
    }

    private void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        string sentence = sentences.Dequeue();
        dialogueTextBox.text = sentence;
    }

    private void EndDialogue()
    {
        textBoxObject.SetActive(false);
    }

    private IEnumerator RunDialogue()
    {
        while (sentences.Count > 0)
        {
            DisplayNextSentence();
            yield return new WaitForSeconds(displayDelay);
        }
        
        EndDialogue();
    }
}
