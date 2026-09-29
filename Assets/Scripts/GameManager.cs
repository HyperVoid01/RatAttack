using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int money = 100;
    [SerializeField] private int ammo = 10;
    [SerializeField] private bool areHandsClean = true;
    private bool _gameStarted = false;
    
    public int Ammo => ammo;
    public int Money => money;
    
    public bool AreHandsClean => areHandsClean;
    
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

    public void StartGame()
    {
        if (_gameStarted)
            return;
        
        _gameStarted = true;
        CustomerSpawner.Instance.Initialize();
        RatSpawner.Instance.Initialize();
    }

    public void IncreaseAmmo(int amount)
    {
        ammo += amount;
        HUDManager.Instance.UpdateAmmoCounter();
    }

    public void DecreaseAmmo(int amount)
    {
        ammo = Mathf.Max(ammo - amount, 0);
        HUDManager.Instance.UpdateAmmoCounter();
    }
    
    public void IncreaseMoney(int amount)
    {
        SoundPlayer.Instance.PlaySound(SoundID.ReceiveMoney, transform.position);
        
        money += amount;
    }

    public void DecreaseMoney(int amount)
    {
        money = Mathf.Max(money - amount, 0);
    }

    public void CleanHands()
    {
        areHandsClean = true;
    }

    public void DirtyHands()
    {
        areHandsClean = false;
    }
}
