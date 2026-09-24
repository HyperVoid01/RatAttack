using UnityEngine;

public class Upgradeable : MonoBehaviour
{
    private int level = 0;
    [SerializeField] private UpgradeCollectionType type;
    [SerializeField] private GameObject[] variants = new GameObject[3];
    
    private void Start()
    {
        if (type == UpgradeCollectionType.Interior)
            UpgradeManager.Instance.upgradeInterior += Upgrade;
        else if (type == UpgradeCollectionType.Dining)
            UpgradeManager.Instance.upgradeDining += Upgrade;
    }

    private void OnDisable()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.upgradeDining -= Upgrade;
            UpgradeManager.Instance.upgradeInterior -= Upgrade;
        }
    }

    private void Upgrade()
    {
        if (level == 2)
            return;
        
        variants[level].SetActive(false);
        level++;
        variants[level].SetActive(true);
    }

    enum UpgradeCollectionType
    {
        Dining,
        Interior
    }
}
