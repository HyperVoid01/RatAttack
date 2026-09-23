using UnityEngine;

public class TableManager : MonoBehaviour
{
    [SerializeField] private Table[] tables;
    
    public static TableManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Table GetTable()
    {
        foreach (Table table in tables)
        {
            if (!table.AvailableSlots())
                continue;

            return table;
        }
        
        Debug.LogWarning("No available slots");
        return null;
    }

    public int TotalSlots()
    {
        int totalSeats = 0;
        foreach (Table table in tables)
        {
            totalSeats += table.SeatCount;
        }
        
        return totalSeats;
    }

    public int TotalAvailableSlots()
    {
        int totalAvailableSeats = 0;
        foreach (Table table in tables)
        {
            if (!table.AvailableSlots())
                continue;

            totalAvailableSeats += table.SeatCount - table.CustomerCount;
        }
    
        return totalAvailableSeats;
    }
}
