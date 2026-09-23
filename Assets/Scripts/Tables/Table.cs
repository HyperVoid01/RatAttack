using System;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField] public Transform pizzaSlot;
    [SerializeField] private Transform[] seatSlots; // Positions of seats
    private List<GameObject> customersAtTable = new List<GameObject>(); // Customers occupying seats

    public int CustomerCount { get => customersAtTable.Count; } // Gets last index
    public int SeatCount { get => seatSlots.Length; } // Amount of seats

    // Checks if there are any seats available for customer
    public bool AvailableSlots()
    {
        if (customersAtTable.Count < seatSlots.Length)
            return true;
        
        return false;
    }
    
    public Transform TakeSeat(GameObject customer)
    {
        customersAtTable.Add(customer);
        return seatSlots[CustomerCount - 1];
    }

    public void LeaveSeat(GameObject customer)
    {
        customersAtTable.Remove(customer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Transform seat in seatSlots)
        {
            Gizmos.DrawSphere(seat.position, 0.1f);
        }
    }
}