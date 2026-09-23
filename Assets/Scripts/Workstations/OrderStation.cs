using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderStation : MonoBehaviour
{
    [SerializeField] public Transform[] lineSlots; // Spots for customers to stand in
    [SerializeField] public Image orderImage;
    [SerializeField] private Sprite[] orderSprites;

    public List<CustomerBehaviour> customersInLine = new();
    private int reservedSlots;

    // Increments every time a customer leaves the line, so waiting customers
    // know to shift forward exactly once (instead of re-triggering every frame).
    public int QueueVersion { get; private set; }

    public static OrderStation Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        orderImage.enabled = false;
    }

    private void Update()
    {
        if (customersInLine.Count > 0 && !customersInLine[0].orderTaken && customersInLine[0].interactable != null)
        {
            customersInLine[0].interactable.enabled = true;
            orderImage.enabled = true;
            orderImage.sprite = GetOrderImage(customersInLine[0].Order);
        }
        else if (customersInLine.Count == 0)
        {
            orderImage.enabled = false;
        }
    }

    private Sprite GetOrderImage(Flavour flavour)
    {
        switch (flavour)
        {
            case Flavour.Plain:
            {
                return orderSprites[0];
            }
            
            case Flavour.Pepperoni:
            {
                return orderSprites[1];
            }
            
            case Flavour.Onion:
            {
                return orderSprites[2];
            }
            
            case Flavour.Veg:
            {
                return orderSprites[3];
            }
            
            case Flavour.PepperoniAndOnions:
            {
                return orderSprites[4];
            }
            
            case Flavour.PepperoniAndVeg:
            {
                return orderSprites[5];
            }
            
            case Flavour.OnionAndVeg:
            {
                return orderSprites[6];
            }
        }
        
        orderImage.enabled = false;
        return null;
    }

    // Called once per customer at spawn time so two customers spawning in the
    // same frame can never be handed the same physical slot.
    public int ReserveSlot()
    {
        if (reservedSlots >= lineSlots.Length)
            return -1;
        
        int slot = reservedSlots;
        reservedSlots++;
        return slot;
    }

    public void JoinQueue(CustomerBehaviour customer)
    {
        customersInLine.Add(customer);
    }

    // Shared bookkeeping for anyone leaving the physical line, whether they
    // were served (LeaveQueue) or left early (fled, timed out before
    // joining, etc. via RemoveFromLine directly). Removing from the list is
    // a no-op if the customer hadn't joined yet (still walking to their
    // slot), but the slot still needs to be released either way.
    public void RemoveFromLine(CustomerBehaviour customer)
    {
        customersInLine.Remove(customer);
        reservedSlots = Mathf.Max(0, reservedSlots - 1);
        QueueVersion++;
    }

    public void LeaveQueue(CustomerBehaviour customer)
    {
        RemoveFromLine(customer);
        PickupStation.Instance.waitingCustomers.Add(customer);
    }
}

// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class OrderStation : MonoBehaviour
// {
//     [SerializeField] public Transform[] lineSlots; // Spots for customers to stand in
//     [SerializeField] public Image orderImage;
//     [SerializeField] private Sprite[] orderSprites;
//
//     public List<CustomerBehaviour> customersInLine = new();
//     private int reservedSlots;
//
//     // Increments every time a customer leaves the line, so waiting customers
//     // know to shift forward exactly once (instead of re-triggering every frame).
//     public int QueueVersion { get; private set; }
//
//     public static OrderStation Instance;
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
//     private void Start()
//     {
//         orderImage.enabled = false;
//     }
//
//     private void Update()
//     {
//         if (customersInLine.Count > 0 && !customersInLine[0].orderTaken && customersInLine[0].interactable != null)
//         {
//             customersInLine[0].interactable.enabled = true;
//             orderImage.enabled = true;
//             orderImage.sprite = GetOrderImage(customersInLine[0].Order);
//         }
//         else if (customersInLine.Count == 0)
//         {
//             orderImage.enabled = false;
//         }
//     }
//
//     private Sprite GetOrderImage(Flavour flavour)
//     {
//         switch (flavour)
//         {
//             case Flavour.Plain:
//             {
//                 return orderSprites[0];
//             }
//             
//             case Flavour.Pepperoni:
//             {
//                 return orderSprites[1];
//             }
//             
//             case Flavour.Onion:
//             {
//                 return orderSprites[2];
//             }
//             
//             case Flavour.Veg:
//             {
//                 return orderSprites[3];
//             }
//             
//             case Flavour.PepperoniAndOnions:
//             {
//                 return orderSprites[4];
//             }
//             
//             case Flavour.PepperoniAndVeg:
//             {
//                 return orderSprites[5];
//             }
//             
//             case Flavour.OnionAndVeg:
//             {
//                 return orderSprites[6];
//             }
//         }
//         
//         orderImage.enabled = false;
//         return null;
//     }
//
//     // Called once per customer at spawn time so two customers spawning in the
//     // same frame can never be handed the same physical slot.
//     public int ReserveSlot()
//     {
//         if (reservedSlots >= lineSlots.Length)
//             return -1;
//         
//         // if (customersInLine.Count == 0)
//         //     return 0;
//         
//         int slot = reservedSlots;
//         reservedSlots++;
//         return slot;
//     }
//
//     public void JoinQueue(CustomerBehaviour customer)
//     {
//         customersInLine.Add(customer);
//     }
//
//     public void LeaveQueue(CustomerBehaviour customer)
//     {
//         customersInLine.Remove(customer);
//         PickupStation.Instance.waitingCustomers.Add(customer);
//         reservedSlots--;
//         QueueVersion++;
//     }
// }