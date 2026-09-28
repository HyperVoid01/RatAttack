using System;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private Oven currentOven;
    public int _ovenLevel = 1;
    [SerializeField] private float[] ovenUpgradeReputationCost = new float[3];
    [SerializeField] private int[] ovenUpgradeMoneyCost = new int[3];

    public Action upgradeDining;
    public int _diningLevel = 1;
    [SerializeField] private float[] diningUpgradeReputationCost = new float[3];
    [SerializeField] private int[] diningUpgradeMoneyCost = new int[3];

    public Action upgradeInterior;
    public int _interiorLevel = 1;
    [SerializeField] private float[] interiorUpgradeReputationCost = new float[3];
    [SerializeField] private int[] interiorUpgradeMoneyCost = new int[3];

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

    public float[] GetOvenCosts()
    {
        if (_ovenLevel == 4)
            return null;
        
        float[] costs =
        {
            ovenUpgradeReputationCost[_ovenLevel - 1],
            ovenUpgradeMoneyCost[_ovenLevel - 1]
        };
        
        return costs;
    }

public void UpgradeOven()
    {
        if (currentOven == null || _ovenLevel == 4)
            return;

        if (ReputationManager.Instance.Reputation < ovenUpgradeReputationCost[_ovenLevel - 1] ||
            GameManager.Instance.Money < ovenUpgradeMoneyCost[_ovenLevel - 1])
            return;
        
        GameManager.Instance.DecreaseMoney(ovenUpgradeMoneyCost[_ovenLevel - 1]);
        
        _ovenLevel++;
        currentOven.UpgradeOven(_ovenLevel);
        
        SoundPlayer.Instance.PlaySound(SoundID.PurchaseSuccessful, transform.position);
    }

    public float[] GetDiningCosts()
    {
        if (_diningLevel == 4)
            return null;
        
        float[] costs =
        {
            diningUpgradeReputationCost[_diningLevel - 1],
            diningUpgradeMoneyCost[_diningLevel - 1]
        };
        
        return costs;
    }
    
    public void UpgradeDining()
    {
        if (_diningLevel == 4)
            return;
        
        if (ReputationManager.Instance.Reputation < diningUpgradeReputationCost[_diningLevel - 1] ||
            GameManager.Instance.Money < diningUpgradeMoneyCost[_diningLevel - 1])
            return;
        
        GameManager.Instance.DecreaseMoney(diningUpgradeMoneyCost[_diningLevel - 1]);
        
        _diningLevel++;
        upgradeDining?.Invoke();
        
        SoundPlayer.Instance.PlaySound(SoundID.PurchaseSuccessful, transform.position);
    }
    
    public float[] GetInteriorCosts()
    {
        if (+_interiorLevel == 4)
            return null;
        
        float[] costs =
        {
            interiorUpgradeReputationCost[_interiorLevel - 1],
            interiorUpgradeMoneyCost[_interiorLevel - 1]
        };
        
        return costs;
    }

    public void UpgradeInterior()
    {
        if (_interiorLevel == 4)
            return;
        
        if (ReputationManager.Instance.Reputation < interiorUpgradeReputationCost[_interiorLevel - 1] ||
            GameManager.Instance.Money < interiorUpgradeMoneyCost[_interiorLevel - 1])
            return;
        
        GameManager.Instance.DecreaseMoney(interiorUpgradeMoneyCost[_interiorLevel - 1]);
        
        _interiorLevel++;
        upgradeInterior?.Invoke();

        if (_interiorLevel == 2)
        {
            EnvironmentalEffects.Instance.ClearDust();
        }
        
        SoundPlayer.Instance.PlaySound(SoundID.PurchaseSuccessful, transform.position);
    }
}
