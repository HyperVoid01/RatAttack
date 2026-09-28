using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("Interactions")]
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private GameObject dirtyHandsText;
    
    [Header("HUDs")]
    [SerializeField] private GameObject playerHud;
    [SerializeField] private GameObject computerUI;
    
    [Header("Customer Orders")]
    [SerializeField] private Transform orderTextRoot;
    [SerializeField] private GameObject orderDetails;

    [Header("StarRating")] 
    [SerializeField] private Transform starRatingOrigin;   // parent transform stars live under
    [SerializeField] private GameObject starRatingImage;   // star prefab
    private const int MaxStars = 18;
    private GameObject[] starRatingObjects = new GameObject[MaxStars];
    
    [Header("Ammo")]
    [SerializeField] private GameObject ammoCounterObject;
    [SerializeField] private TextMeshProUGUI ammoCounterText;
    
    Dictionary<CustomerBehaviour, GameObject> customerOrders = new Dictionary<CustomerBehaviour, GameObject>();

    public static HUDManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeStarPool();
        UpdateStarRating();
    }

    private void InitializeStarPool()
    {
        for (int i = 0; i < MaxStars; i++)
        {
            GameObject star = Instantiate(starRatingImage, starRatingOrigin);
            star.SetActive(false);
            starRatingObjects[i] = star;
        }
    }

    public void UpdateStarRating()
    {
        int rating = Mathf.Clamp(ReputationManager.Instance.starRating, 0, MaxStars);

        for (int i = 0; i < starRatingObjects.Length; i++)
        {
            if (starRatingObjects[i] == null)
                continue;

            starRatingObjects[i].SetActive(i < rating);
        }
    }

    public void UpdateAmmoCounter(int ammoAddition = 0)
    {
        ammoCounterText.text = ammoAddition > 0 ? $"{GameManager.Instance.Ammo} + {ammoAddition}" : GameManager.Instance.Ammo.ToString();
    }
    
    public void EnableInteractionText(string text, bool showAmmoCount, bool dirtyHands)
    {
        if (showAmmoCount)
        {
            ammoCounterObject.SetActive(true);
            UpdateAmmoCounter();
        }
        else
        {
            ammoCounterObject.SetActive(false);
        }

        if (Input.GetMouseButton(0)) // Ignores enabling when holding items
            return;

        if (dirtyHands)
        {
            dirtyHandsText.SetActive(true);
            interactionText.gameObject.SetActive(false);
        }
        else
        {
            dirtyHandsText.SetActive(false);
            interactionText.text = text;
            interactionText.gameObject.SetActive(true);
        }
    }

    public void DisableInteractionText()
    {
        interactionText.gameObject.SetActive(false);
        ammoCounterObject.SetActive(false);
        
        dirtyHandsText.SetActive(false);
    }

    public void AddOrderText(CustomerBehaviour customer, string text)
    {
        GameObject detailsObject = Instantiate(orderDetails);
        TMP_Text details = detailsObject.GetComponent<TMP_Text>();
        details.transform.SetParent(orderTextRoot, false);
        details.text = text;
        
        SoundPlayer.Instance.PlaySound(SoundID.WritingInNotepad, transform.position);
        
        customerOrders.Add(customer, detailsObject);
    }

    public void RemoveOrderDetails(CustomerBehaviour customer)
    {
        if (!customerOrders.ContainsKey(customer))
            return; // already removed, nothing to do
        
        GameObject oldOrder = customerOrders[customer];
        customerOrders.Remove(customer);
        Destroy(oldOrder);
    }

    public void SwitchToComputer()
    {
        SetAllInActive();
        computerUI.SetActive(true);
    }

    public void SwitchToPlayer()
    {
        SetAllInActive();
        playerHud.SetActive(true);
    }

    public void SetAllInActive()
    {
        playerHud.SetActive(false);
        computerUI.SetActive(false);
    }
}
