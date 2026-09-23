using UnityEngine;

public class Pizza : MonoBehaviour
{
    [SerializeField] private GameObject raw;
    [SerializeField] private GameObject[] pizzaVariants = new GameObject[6];
    [SerializeField] private MeshRenderer[] meshRenderers = new MeshRenderer[6];

    [SerializeField] private Material rawMaterial;
    [SerializeField] private Material cookedMaterial;

    private PizzaState pizzaState = PizzaState.raw;
    private Flavour flavour = Flavour.Plain;

    public PizzaState PizzaState { get => pizzaState; }
    public Flavour Flavour { get => flavour; }

    // Plain has no entry in pizzaVariants (it lives in `raw` instead), so
    // every lookup goes through here instead of indexing (int)f - 1 directly.
    private GameObject GetVisualFor(Flavour f)
    {
        if (f == Flavour.Plain)
            return raw;
        
        return pizzaVariants[(int)f];
    }

    public void Cook()
    {
        pizzaState = PizzaState.cooked;
        GetVisualFor(flavour).transform.GetChild(0).GetComponent<MeshRenderer>().material = cookedMaterial;
    }

    public void ChangeFlavor(Flavour newFlavour)
    {
        GetVisualFor(flavour).SetActive(false);
        flavour = newFlavour;
        GetVisualFor(flavour).SetActive(true);
    }

    public void DisablePickup()
    {
        gameObject.layer = 0;
        GetComponent<Interactable>().enabled = false;
    }
}

public enum PizzaState
{
    cooked,
    raw
}

/*
 Different combos:
 0 - Plain
 1 - Pep
 2 - Onions
 3 - Veg
 4 - Pep, Onions
 5 - Pep, Veg
 6 - Onions, Veg
*/

public enum Flavour
{
    Plain,    // No Toppings
    Pepperoni, // Pep
    Onion, // Onions
    Veg, // Veg
    PepperoniAndOnions, // Pep , Onions
    PepperoniAndVeg, // Pep, Veg
    OnionAndVeg  // Onions, Veg
}
