using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public float Reputation;

    [Tooltip("Reputation needed for each star, in ascending order. The number of entries is the MAX number of stars.")]
    [SerializeField] private float[] starRatingThresholds;

    [Tooltip("Stars needed to win. The player can keep earning stars after winning, up to the max.")]
    [SerializeField] private int starsToWin = 5;

    [Tooltip("Reputation can never go above this. It is raised automatically to the last threshold if it is lower.")]
    [SerializeField] private float maxReputation = 100f;

    public int starRating;

    public int MaxStars => starRatingThresholds != null ? starRatingThresholds.Length : 0;
    public bool HasWon { get; private set; }
    
    public static ReputationManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ValidateSettings();
    }

    // Progress between the current star's threshold and the next one (0 to 1)
    public float GetPercentageToNextRating()
    {
        if (starRating >= MaxStars)
            return 0f;

        float start = starRating > 0 ? starRatingThresholds[starRating - 1] : 0f;
        float end = starRatingThresholds[starRating];
        float range = end - start;

        if (range <= 0f)
            return 0f;

        return Mathf.Clamp01((Reputation - start) / range);
    }

    public void IncreaseReputation(float amount)
    {
        Reputation = Mathf.Clamp(Reputation + amount, 0f, maxReputation);

        ClampStarRating();

        bool gainedStar = false;

        // Gain every star whose threshold has now been reached
        while (starRating < MaxStars && Reputation >= starRatingThresholds[starRating])
        {
            starRating++;
            gainedStar = true;
        }

        if (!gainedStar)
            return;

        SoundPlayer.Instance.PlaySound(SoundID.StarRatingIncrease, transform.position);
        HUDManager.Instance.UpdateStarRating();

        CheckWin();
    }

    public void DecreaseReputation(float amount)
    {
        Reputation = Mathf.Clamp(Reputation - amount, 0f, maxReputation);

        ClampStarRating();

        bool lostStar = false;

        // Lose every star whose threshold the reputation is now below
        while (starRating > 0 && Reputation < starRatingThresholds[starRating - 1])
        {
            starRating--;
            lostStar = true;
        }

        if (!lostStar)
            return;

        SoundPlayer.Instance.PlaySound(SoundID.StarRatingDecrease, transform.position);
        HUDManager.Instance.UpdateStarRating();
    }

    private void ValidateSettings()
    {
        if (starRatingThresholds != null && starRatingThresholds.Length > 0)
        {
            float lastThreshold = starRatingThresholds[starRatingThresholds.Length - 1];

            // A star above the max reputation could never be reached
            if (maxReputation < lastThreshold)
            {
                Debug.LogWarning($"{name}: Max Reputation ({maxReputation}) is below the last star threshold ({lastThreshold}). Raising it.");
                maxReputation = lastThreshold;
            }

            for (int i = 1; i < starRatingThresholds.Length; i++)
            {
                if (starRatingThresholds[i] < starRatingThresholds[i - 1])
                {
                    Debug.LogWarning($"{name}: Star Rating Thresholds must be in ascending order (see element {i}).");
                    break;
                }
            }
        }

        // The win can't need more stars than exist
        if (starsToWin > MaxStars)
        {
            Debug.LogWarning($"{name}: Stars To Win ({starsToWin}) is higher than the max stars ({MaxStars}), so the game can never be won. Clamping.");
            starsToWin = MaxStars;
        }

        if (starsToWin < 1)
        {
            starsToWin = 1;
        }

        ClampStarRating();
    }

    // Keeps starRating inside 0 - thresholds length so indexing can never go out of range
    private void ClampStarRating()
    {
        if (starRating > MaxStars)
        {
            Debug.LogWarning($"{name}: starRating ({starRating}) is higher than the thresholds array length ({MaxStars}). Clamping.");
            starRating = MaxStars;
        }

        if (starRating < 0)
        {
            starRating = 0;
        }
    }

    // Wins once, the first time the player reaches Stars To Win. Earning more stars afterwards
    // (or dropping below and climbing back) does not trigger the win again.
    [ContextMenu("CheckWin")]
    private void CheckWin()
    {
        if (HasWon || MaxStars == 0)
            return;

        if (starRating >= starsToWin)
        {
            HasWon = true;
            GameManager.Instance.WinGame();
        }
    }
}