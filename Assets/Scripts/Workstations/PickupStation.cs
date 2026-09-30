using System.Collections.Generic;
using UnityEngine;

public class PickupStation : MonoBehaviour
{
    [SerializeField] private Transform pickupPoint;
    
    public List<CustomerBehaviour> waitingCustomers = new List<CustomerBehaviour>();
    public GameObject currentPizzaObject;
    public Pizza currentPizza;
    
    public static PickupStation Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pizza") && !currentPizza && other.gameObject != currentPizzaObject)
        {
            if (other.GetComponent<Pizza>().PizzaState == PizzaState.raw)
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

    // Called when a customer leaves before collecting the pizza they were given.
    // Makes the pizza grabbable again and re-registers it as the station's current pizza.
    public void ReturnPizza(GameObject pizza)
    {
        if (!pizza || !pizza.TryGetComponent(out Pizza returned))
            return;

        returned.EnablePickup();

        // Re-enabling the collider doesn't reliably re-fire OnTriggerEnter, so register it directly
        if (!currentPizza)
        {
            currentPizzaObject = pizza;
            currentPizza = returned;
        }
    }

    public void CallCustomer()
    {
        SoundPlayer.Instance.PlaySound(SoundID.BellRing, transform.position);
        
        if (!currentPizzaObject || waitingCustomers.Count == 0)
            return;
        
        // Clears destroyed customers and ones already on their way out
        // (they stay alive while walking to the exit, so a null check alone isn't enough)
        waitingCustomers.RemoveAll(c => c == null || c.movement.HasLeft);
        
        foreach (CustomerBehaviour customer in waitingCustomers)
        {
            if (customer.Order == currentPizza.Flavour)
            {
                GameObject pizzaToDeliver = currentPizzaObject;
                currentPizza.DisablePickup();
                
                currentPizza = null;
                currentPizzaObject = null;
                
                if (pizzaToDeliver.TryGetComponent(out Collider pizzaCol))
                    pizzaCol.enabled = false;
                
                if (pizzaToDeliver.TryGetComponent(out Rigidbody pizzaRb))
                    pizzaRb.isKinematic = true;
                
                // Runs on the customer, so it stops if they leave or are destroyed
                customer.movement.BeginPickup(pickupPoint, pizzaToDeliver);
                waitingCustomers.Remove(customer);
                return;
            }
        }
    }
}

// using System.Collections.Generic;
// using UnityEngine;
//
// public class PickupStation : MonoBehaviour
// {
//     [SerializeField] private Transform pickupPoint;
//     
//     public List<CustomerBehaviour> waitingCustomers = new List<CustomerBehaviour>();
//     public GameObject currentPizzaObject;
//     public Pizza currentPizza;
//     
//     public static PickupStation Instance;
//
//     private void Awake()
//     {
//         if (Instance != null)
//         {
//             Destroy(gameObject);
//             return;
//         }
//
//         Instance = this;
//     }
//     
//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Pizza") && !currentPizza && other.gameObject != currentPizzaObject)
//         {
//             if (other.GetComponent<Pizza>().PizzaState == PizzaState.raw)
//                 return;
//             
//             currentPizzaObject = other.gameObject;
//             currentPizza = currentPizzaObject.GetComponent<Pizza>();
//         }
//     }
//
//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Pizza") && other.gameObject == currentPizzaObject)
//         {
//             currentPizza = null;
//             currentPizzaObject = null;
//         }
//     }
//
//     public void CallCustomer()
//     {
//         SoundPlayer.Instance.PlaySound(SoundID.BellRing, transform.position);
//         
//         if (!currentPizzaObject || waitingCustomers.Count == 0)
//             return;
//         
//         // Clears empty customer slots
//         waitingCustomers.RemoveAll(c => c == null);
//         
//         foreach (CustomerBehaviour customer in waitingCustomers)
//         {
//             if (customer.Order == currentPizza.Flavour)
//             {
//                 GameObject pizzaToDeliver = currentPizzaObject;
//                 currentPizza.DisablePickup();
//                 
//                 currentPizza = null;
//                 currentPizzaObject = null;
//                 
//                 if (pizzaToDeliver.TryGetComponent(out Collider pizzaCol))
//                     pizzaCol.enabled = false;
//                 
//                 if (pizzaToDeliver.TryGetComponent(out Rigidbody pizzaRb))
//                     pizzaRb.isKinematic = true;
//                 
//                 StartCoroutine(customer.movement.PickupPizza(pickupPoint, pizzaToDeliver));
//                 waitingCustomers.Remove(customer);
//                 return;
//             }
//         }
//     }
// }
