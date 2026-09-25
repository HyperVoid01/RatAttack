using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

public class Computer : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPosition;
    
    [Header("Deliverables")]
    [SerializeField] private PurchaseItem[] purchaseItems;
    [SerializeField] private Transform spawnPoint;

    [Header("User Interfaces")]
    [SerializeField] private TMP_Text balance;
    [SerializeField] private TMP_Text starRating;
    [SerializeField] private Slider starProgressionSlider;
    [SerializeField] private GameObject pestControlMenu;
    [SerializeField] private GameObject upgradesMenu;
    
    [Header("Descriptions")]
    [SerializeField] private TMP_Text ovenCostText;
    [SerializeField] private Slider ovenLevelSlider;
    
    [SerializeField] private TMP_Text diningCostText;
    [SerializeField] private Slider diningLevelSlider;
    
    [SerializeField] private TMP_Text interiorCostText;
    [SerializeField] private Slider interiorLevelSlider;
    
    public void UseComputer()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        HUDManager.Instance.SetAllInActive();
        
        PlayerMovement.Instance.canMove = false;
        // CameraController.Instance.isActive = false;
        
        CameraController.Instance.SwitchPosition(cameraPosition, () =>
        {
            HUDManager.Instance.SwitchToComputer();
            UpdateUI();
        });
    }

    public void Exit()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        HUDManager.Instance.SetAllInActive();
        
        CameraController.Instance.ReturnToLastPosition(() =>
        {
            PlayerMovement.Instance.canMove = true;
            // CameraController.Instance.isActive = true;
            
            HUDManager.Instance.SwitchToPlayer();
        });
    }

    public void UpdateUI()
    {
        balance.text = "Balance: R" + GameManager.Instance.money;
        float percentage = ReputationManager.Instance.GetPercentageToNextRating();
        starRating.text = (ReputationManager.Instance.starRating + percentage).ToString();
        starProgressionSlider.value = 1 - percentage;
        
        UpdateCostsUI();
    }

    private void UpdateCostsUI()
    {
        float[] ovenCosts = UpgradeManager.Instance.GetOvenCosts();
        if (ovenCosts != null)
        {
            ovenCostText.text = $"R{ovenCosts[1]}\nREP:{ovenCosts[0]}";
            ovenLevelSlider.value = UpgradeManager.Instance._ovenLevel;
        }
        else
        {
            ovenCostText.text = "MAX LEVEL";
        }
        
        float[] diningCosts = UpgradeManager.Instance.GetDiningCosts();
        if (diningCosts != null)
        {
            diningCostText.text = $"R{diningCosts[1]}\nREP:{diningCosts[0]}";
            diningLevelSlider.value = UpgradeManager.Instance._diningLevel;
        }
        else
        {
            diningCostText.text = "MAX LEVEL";
        }
        
        float[] interiorCosts = UpgradeManager.Instance.GetInteriorCosts();
        if (interiorCosts != null)
        {
            interiorCostText.text = $"R{interiorCosts[1]}\nREP:{interiorCosts[0]}";
            interiorLevelSlider.value = UpgradeManager.Instance._interiorLevel;
        }
        else
        {
            interiorCostText.text = "MAX LEVEL";
        }
    }

    public void OpenPestControlMenu()
    {
        pestControlMenu.SetActive(true);
        upgradesMenu.SetActive(false);
    }

    public void OpenUpgradesMenu()
    {
        upgradesMenu.SetActive(true);
        pestControlMenu.SetActive(false);
    }

    public void BackToHomeScreen()
    {
        upgradesMenu.SetActive(false);
        pestControlMenu.SetActive(false);
    }

    public void BuyItem(int itemIndex)
    {
        if (GameManager.Instance.money >= purchaseItems[itemIndex].price)
        {
            GameManager.Instance.money -= purchaseItems[itemIndex].price;
            Instantiate(purchaseItems[itemIndex].prefab, spawnPoint.position, Quaternion.identity);
            UpdateUI();
        }
    }
}

[System.Serializable]
struct PurchaseItem
{
    public string name;
    public int price;
    public GameObject prefab;
}
