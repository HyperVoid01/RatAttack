using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class IngredientStation : MonoBehaviour
{
    [SerializeField] private Interactable topping1;
    [SerializeField] private Interactable topping2;
    [SerializeField] private Interactable topping3;
    
    private GameObject currentPizzaObject;
    private Pizza currentPizza;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pizza") && !currentPizza && other.gameObject != currentPizzaObject)
        {
            // You can only add toppings to uncooked pizza
            if (other.GetComponent<Pizza>().PizzaState != PizzaState.raw)
                return;
            
            currentPizzaObject = other.gameObject;
            currentPizza = currentPizzaObject.GetComponent<Pizza>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pizza") && other.gameObject == currentPizzaObject)
        {
            currentPizza = null;
            currentPizzaObject = null;
        }
    }

    public void AddTopping1()
    {
        if (!currentPizza)
            return;

        switch (currentPizza.Flavour)
        {
            case Flavour.Plain:
            {
                currentPizza.ChangeFlavor(Flavour.Pepperoni);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }

            case Flavour.Onion:
            {
                currentPizza.ChangeFlavor(Flavour.PepperoniAndOnions);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }

            case Flavour.Veg:
            {
                currentPizza.ChangeFlavor(Flavour.PepperoniAndVeg);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }
            
        }
    }

    public void AddTopping2()
    {
        if (!currentPizza)
            return;
        
        switch (currentPizza.Flavour)
        {
            case Flavour.Plain:
            {
                currentPizza.ChangeFlavor(Flavour.Onion);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }
            
            case Flavour.Pepperoni:
            {
                currentPizza.ChangeFlavor(Flavour.PepperoniAndOnions);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }
            
            case Flavour.Veg:
            {
                currentPizza.ChangeFlavor(Flavour.OnionAndVeg);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }
        }
    }

    public void AddTopping3()
    {
        if (!currentPizza)
            return;
        
        switch (currentPizza.Flavour)
        {
            case Flavour.Plain:
            {
                currentPizza.ChangeFlavor(Flavour.Veg);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }
            
            case Flavour.Pepperoni:
            {
                currentPizza.ChangeFlavor(Flavour.PepperoniAndVeg);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }
            
            case Flavour.Onion:
            {
                currentPizza.ChangeFlavor(Flavour.OnionAndVeg);
                SoundPlayer.Instance.PlaySound(SoundID.AddToppings, transform.position);
                break;
            }
        }
    }
}
