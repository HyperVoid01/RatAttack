using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int money = 100;
    
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
