using UnityEngine;

// Spawns a random model under the customer when the customer is created.
// Runs before other scripts' Awake so the Outline picks up the model's renderers.
[DefaultExecutionOrder(-100)]
public class CustomerModelPicker : MonoBehaviour
{
    [SerializeField] private bool isInspector;
    [SerializeField] private GameObject inspectorPrefab;
    [SerializeField] private GameObject[] modelPrefabs;
    [Tooltip("Where the model is placed. Leave empty to use this object.")]
    [SerializeField] private Transform modelParent;

    public Animator Animator { get; private set; }

    private void Awake()
    {
        if (modelParent == null)
        {
            modelParent = transform;
        }

        GameObject prefab;
        
        if (isInspector && inspectorPrefab != null)
            prefab = inspectorPrefab;
        else
            prefab = modelPrefabs[Random.Range(0, modelPrefabs.Length)];
        

        if (prefab == null)
        {
            Debug.LogError($"{name}: A model prefab slot is empty.");
            return;
        }

        GameObject model = Instantiate(prefab, modelParent);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        Animator = model.GetComponentInChildren<Animator>();

        if (Animator == null)
        {
            Debug.LogError($"{name}: Model {prefab.name} has no Animator.");
        }
    }
}