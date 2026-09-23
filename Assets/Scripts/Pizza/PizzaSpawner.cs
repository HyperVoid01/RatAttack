using System.Collections;
using UnityEngine;

public class PizzaSpawner : MonoBehaviour
{
    [SerializeField] private GameObject pizzaPrefab;
    [SerializeField] private float pizzaSpawnRate; // Time between spawns
    [SerializeField] private Transform pizzaSpawnPoint;

    private Coroutine spawnRoutine;
    private GameObject pizza;

    private void Start()
    {
        pizza = Instantiate(pizzaPrefab, pizzaSpawnPoint.position, Quaternion.identity);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == pizza)
            pizza = null;
    }

    private IEnumerator SpawnPizza()
    {
        yield return new WaitForSeconds(pizzaSpawnRate);
        pizza = Instantiate(pizzaPrefab, pizzaSpawnPoint);
        spawnRoutine = null;
    }

    public void SpawnButton()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnPizza());
    }
}
