using System;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private Oven currentOven;
    public int _ovenLevel = 1;

    public Action upgradeDining;
    public int _diningLevel = 1;
    
    public Action upgradeInterior;
    public int _interiorLevel = 1;
    
    public static UpgradeManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpgradeOven()
    {
        if (currentOven == null || _ovenLevel == 3)
        {
            Debug.Log("Oven not found or max level");
            return;
        }
        _ovenLevel++;
        currentOven.UpgradeOven(_ovenLevel);
    }
    
    public void UpgradeDining()
    {
        if (_diningLevel == 3)
            return;
        
        _diningLevel++;
        upgradeDining?.Invoke();
    }

    public void UpgradeInterior()
    {
        if (_interiorLevel == 3)
            return;
        
        _interiorLevel++;
        upgradeInterior?.Invoke();
    }
}
