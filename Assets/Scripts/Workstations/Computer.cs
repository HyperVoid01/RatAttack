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
    [SerializeField] private TMP_Text ovenLevelText;
    
    [SerializeField] private TMP_Text diningCostText;
    [SerializeField] private TMP_Text diningLevelText;
    
    [SerializeField] private TMP_Text interiorCostText;
    [SerializeField] private TMP_Text interiorLevelText;
    
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
        SoundPlayer.Instance.PlaySound(SoundID.ComputerButtonClick, transform.position);
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
        balance.text = "R" + GameManager.Instance.Money;
        float percentage = ReputationManager.Instance.GetPercentageToNextRating();
        starRating.text = (ReputationManager.Instance.starRating + percentage).ToString("F1");
        starProgressionSlider.value = 1 - percentage;
        
        UpdateCostsUI();
    }

    private void UpdateCostsUI()
    {
        float[] ovenCosts = UpgradeManager.Instance.GetOvenCosts();
        if (ovenCosts != null)
        {
            ovenCostText.text = $"R{ovenCosts[1]} || {ovenCosts[0]} stars";
            ovenLevelText.text = (UpgradeManager.Instance._ovenLevel).ToString();
        }
        else
        {
            ovenCostText.text = "MAX LEVEL";
        }
        
        float[] diningCosts = UpgradeManager.Instance.GetDiningCosts();
        if (diningCosts != null)
        {
            diningCostText.text = $"R{diningCosts[1]} || {diningCosts[0]} stars";
            diningLevelText.text = (UpgradeManager.Instance._diningLevel).ToString();
        }
        else
        {
            diningCostText.text = "MAX LEVEL";
        }
        
        float[] interiorCosts = UpgradeManager.Instance.GetInteriorCosts();
        if (interiorCosts != null)
        {
            interiorCostText.text = $"R{interiorCosts[1]} || {interiorCosts[0]} stars";
            interiorLevelText.text = (UpgradeManager.Instance._interiorLevel).ToString();
        }
        else
        {
            interiorCostText.text = "MAX LEVEL";
        }
    }

    public void OpenPestControlMenu()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ComputerButtonClick, transform.position);
        
        pestControlMenu.SetActive(true);
        upgradesMenu.SetActive(false);
    }

    public void OpenUpgradesMenu()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ComputerButtonClick, transform.position);
        
        upgradesMenu.SetActive(true);
        pestControlMenu.SetActive(false);
    }

    public void BackToHomeScreen()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ComputerButtonClick, transform.position);
        
        upgradesMenu.SetActive(false);
        pestControlMenu.SetActive(false);
    }

    public void BuyItem(int itemIndex)
    {
        if (GameManager.Instance.Money >= purchaseItems[itemIndex].price)
        {
            SoundPlayer.Instance.PlaySound(SoundID.PurchaseSuccessful, transform.position);
            
            GameManager.Instance.DecreaseMoney(purchaseItems[itemIndex].price);
            Instantiate(purchaseItems[itemIndex].prefab, spawnPoint.position, Quaternion.identity);
            UpdateUI();
        }
        else
        {
            SoundPlayer.Instance.PlaySound(SoundID.PurchaseFailed, transform.position);
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
