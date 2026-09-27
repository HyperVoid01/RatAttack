using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalCleaning : MonoBehaviour
{ 
    [SerializeField] private float cleanDuration;
    [SerializeField] private DecalProjector decalProjector;
    
    private Material[] decalMaterial;
    private Coroutine cleanUpRoutine;
    
    public void Initialize(Material[] materials)
    {
        decalMaterial = materials;
        SetRandomDecal();
    }

    private void SetRandomDecal()
    {
        if (decalMaterial.Length == 0 || decalProjector == null)
        {
            Debug.LogError("No Decal Material Set!");
            return;
        }
            
        
        int random = Random.Range(0, decalMaterial.Length);
        decalProjector.material = decalMaterial[random];
    }

    public void StartCleaning()
    {
        if (cleanUpRoutine != null)
            return;
        
        cleanUpRoutine = StartCoroutine(CleanUp());
    }

    public void StopCleaning()
    {
        if (cleanUpRoutine == null)
            return;
        
        StopCoroutine(cleanUpRoutine);
        cleanUpRoutine = null;

        // Snap back to full opacity
        decalProjector.fadeFactor = 1f;
    }

    private IEnumerator CleanUp()
    {
        float elapsed = 0f;
        float startFade = decalProjector.fadeFactor;

        while (elapsed < cleanDuration)
        {
            elapsed += Time.deltaTime;
            decalProjector.fadeFactor = Mathf.Lerp(startFade, 0f, elapsed / cleanDuration);
            yield return null;
        }

        GameManager.Instance.DirtyHands();
        decalProjector.fadeFactor = 0f;
        Destroy(gameObject);
    }
}