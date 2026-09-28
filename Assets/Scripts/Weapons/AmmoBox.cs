using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    [SerializeField] private int ammoAmount;
    public int Ammo => ammoAmount;
    
    public void LoadAmmo()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ShotgunReload, transform.position);
        GameManager.Instance.IncreaseAmmo(ammoAmount);
        Destroy(gameObject);
    }
}
