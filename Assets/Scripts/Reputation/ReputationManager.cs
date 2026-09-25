using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public float Reputation;
    [SerializeField] private float[] starRatingThresholds;
    public int starRating;
    
    public static ReputationManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public float GetPercentageToNextRating()
    {
        if (starRating >= starRatingThresholds.Length)
            return 0f;
        
        float percentage = Reputation / starRatingThresholds[starRating];

        if (percentage == 1)
            return 0f;
        
        return percentage;
    }

    public void IncreaseReputation(float amount)
    {
        Reputation += amount;

        if (starRating >= starRatingThresholds.Length)
            return;

        if (Reputation >= starRatingThresholds[starRating])
        {
            starRating++;
            HUDManager.Instance.UpdateStarRating();
        }
    }

    public void DecreaseReputation(float amount)
    {
        Reputation = Mathf.Clamp(Reputation - amount, 0f, 100);

        if (starRating > 0 && Reputation < starRatingThresholds[starRating - 1])
        {
            starRating--;
            HUDManager.Instance.UpdateStarRating();
        }
    }
}
