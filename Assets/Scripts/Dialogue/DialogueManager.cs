using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Manages dialogue
public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences;
    private GameObject textBoxObject;
    private TMP_Text dialogueTextBox;
    private int lastAdvanceFrame = -1;

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

    // Returns true if the first sentence was shown
    public bool StartDialogue(Dialogue dialogue, GameObject textBoxObject, TMP_Text nameText, TMP_Text dialogueText)
    {
        sentences.Clear();
        this.textBoxObject = textBoxObject;
        textBoxObject.SetActive(true);
        dialogueTextBox = dialogueText;
        nameText.text = dialogue.speakerName;

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        // Reset so the first sentence always shows
        lastAdvanceFrame = -1;
        return DisplayNextSentence();
    }

    // Returns true only if a new sentence was actually displayed
    public bool DisplayNextSentence()
    {
        // Ignore a second call in the same frame
        if (lastAdvanceFrame == Time.frameCount)
        {
            Debug.LogWarning("DisplayNextSentence called twice in one frame - ignoring duplicate.\n" + StackTraceUtility.ExtractStackTrace());
            return false;
        }
        lastAdvanceFrame = Time.frameCount;

        if (sentences.Count == 0)
        {
            EndDialogue();
            return false;
        }

        string sentence = sentences.Dequeue();
        dialogueTextBox.text = sentence;
        
        if (sentence == "")
            EndDialogue();
        
        return true;
    }

    private void EndDialogue()
    {
        if (textBoxObject != null)
            textBoxObject.SetActive(false);
        
        GameManager.Instance.StartGame();
    }
}