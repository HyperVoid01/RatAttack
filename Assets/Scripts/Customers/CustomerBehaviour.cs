using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerBehaviour : MonoBehaviour
{
    [SerializeField] private CustomerData data;
    [SerializeField] private LayerMask ratLayer;
    private int ratSightingMeter = 0;
    private Flavour order;
    public bool orderTaken;
    public bool inLine;
    public bool joinedQueue;

    public Flavour Order => order;
    public Interactable interactable;
    public CustomerMovement movement;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        movement = GetComponent<CustomerMovement>();
        //StartCoroutine(CheckForRats());
    }

    private void Start()
    {
        if (interactable != null)
            interactable.enabled = false;

        // Sets a random order
        order = (Flavour)Random.Range(1, 7);
    }

    public void GetServed()
    {
        if (orderTaken)
            return;

        // Secure a table BEFORE committing to anything irreversible.
        // If no table is free, bail out and leave this customer exactly
        // as they were — still in queue, still interactable — so the
        // player (or the next call) can retry once a seat opens up.
        Table table = TableManager.Instance.GetTable();
        if (table == null)
        {
            Debug.Log("No table available yet, customer stays in line");
            return;
        }

        orderTaken = true;
        HUDManager.Instance.AddOrderText(this, MakeOrderString());

        if (interactable != null)
        {
            interactable.DisableOutline();
            interactable.enabled = false;
        }

        movement.LeaveLine(table);
    }

    private String MakeOrderString()
    {
        switch (order)
        {
            case Flavour.Plain:
            {
                return "Plain Pizza";
            }
            
            case Flavour.Pepperoni:
            {
                return "Pepperoni Pizza";
            }
            
            case Flavour.Onion:
            {
                return "Onion Pizza";
            }
            
            case Flavour.Veg:
            {
                return "Veg Pizza";
            }
            
            case Flavour.PepperoniAndOnions:
            {
                return "Pepperoni & Onion Pizza";
            }
            
            case Flavour.PepperoniAndVeg:
            {
                return "Pepperoni & Veg Pizza";
            }
            
            case Flavour.OnionAndVeg:
            {
                return "Onion & Veg Pizza";
            }
        }

        return "";
    }

    public IEnumerator EatPizza(GameObject pizza)
    {
        yield return new WaitForSeconds(data.eatTime);
        Destroy(pizza);
        
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(movement.Leave());
        GameManager.Instance.money += 50;
    }

    private IEnumerator CheckForRats()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (!Physics.CheckSphere(transform.position, data.ratDetectionRadius, ratLayer))
            {
                ratSightingMeter = Mathf.Clamp(ratSightingMeter - 1, 0, data.ratTolerance);
                continue;
            }
            
            ratSightingMeter++;

            // Customer has seen rats too much and leaves restaurant
            if (ratSightingMeter >= data.ratTolerance)
            {
                Debug.Log("Customer saw a rat!");
                StartCoroutine(movement.Leave());
                yield break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.ratDetectionRadius);
    }
}