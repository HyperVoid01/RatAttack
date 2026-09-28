using UnityEngine;

public class EnvironmentalEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticles;
    
    public static EnvironmentalEffects Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ClearDust()
    {
        dustParticles.Stop();
        RenderSettings.fog = false;
    }
}
