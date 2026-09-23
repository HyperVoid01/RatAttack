using System;
using UnityEngine;

// Wraps the "press to start, release to stop, force-stop if the target changed
// out from under us" bookkeeping for one hold-button. Not a MonoBehaviour -
// create one per feature (e.g. one for E-on-looked-at-object, one for
// Mouse1-on-held-item) and call Tick() on it from another script's Update().
public class HoldInputBinding
{
    private readonly KeyCode key;
    private readonly Func<IHoldInteraction> resolveTarget;
    private IHoldInteraction active;

    public HoldInputBinding(KeyCode key, Func<IHoldInteraction> resolveTarget)
    {
        this.key = key;
        this.resolveTarget = resolveTarget;
    }

    public void Tick()
    {
        IHoldInteraction current = resolveTarget();

        // What we'd be holding has changed (dropped item, looked away, etc.) - force stop the old one
        if (active != null && active != current)
        {
            active.OnHoldEnd();
            active = null;
        }

        if (current == null)
            return;

        if (Input.GetKeyDown(key))
        {
            current.OnHoldStart();
            active = current;
        }
        else if (Input.GetKeyUp(key))
        {
            current.OnHoldEnd();
            if (active == current)
                active = null;
        }
    }

    // Call this if the owning object is disabled/destroyed mid-hold so nothing is left dangling
    public void ForceStop()
    {
        if (active == null)
            return;

        active.OnHoldEnd();
        active = null;
    }
}