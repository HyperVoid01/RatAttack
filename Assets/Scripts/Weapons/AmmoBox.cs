using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    [SerializeField] private int ammoAmount;
    public int Ammo => ammoAmount;
    
    public void LoadAmmo()
    {
        GameManager.Instance.IncreaseAmmo(ammoAmount);
        Destroy(gameObject);
    }
}
