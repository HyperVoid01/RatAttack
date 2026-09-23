using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interactable : MonoBehaviour, IHoldInteraction
{
    private Outline outline;
    public string message;

    public UnityEvent onInteraction;

    [Header("Hold (optional)")]
    [Tooltip("Leave empty if this object only responds to a single press.")]
    public UnityEvent onHoldStart;
    public UnityEvent onHoldEnd;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        DisableOutline();
    }

    public void Interact()
    {
        onInteraction.Invoke();
    }

    public void OnHoldStart()
    {
        onHoldStart.Invoke();
    }

    public void OnHoldEnd()
    {
        onHoldEnd.Invoke();
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

    public void EnableOutline()
    {
        if (!outline)
            return;

        outline.enabled = true;
    }
}