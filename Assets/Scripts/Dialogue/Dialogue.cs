using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    public string speakerName;
    
    [TextArea(3, 10)]
    public string[] sentences;
}
