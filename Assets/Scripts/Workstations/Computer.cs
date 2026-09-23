using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    [SerializeField] private GameObject pestControlMenu;
    [SerializeField] private GameObject upgradesMenu;
    
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

    private void UpdateUI()
    {
        balance.text = "Balance: R" + GameManager.Instance.money;
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
