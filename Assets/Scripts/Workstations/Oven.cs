using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Oven : MonoBehaviour
{
    [SerializeField] private float[] cookingTimes = new float[3];
    private int _level = 1;
    //[SerializeField] public float cookingTime;
    public GameObject pizzaObject; // the pizza being cooked (or just cooked)
    private Pizza currentPizza;
    public ParticleSystem cookingParticles;
    
    private AudioSource audioSource;

    // Every pizza currently inside the oven
    private readonly List<Pizza> pizzasInside = new List<Pizza>();
    private Coroutine cookingRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Pizza") || !other.TryGetComponent(out Pizza pizza))
            return;

        if (!pizzasInside.Contains(pizza))
        {
            pizzasInside.Add(pizza);
        }

        TryStartCooking();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out Pizza pizza))
            return;

        pizzasInside.Remove(pizza);

        // The pizza being cooked (or just cooked) was taken out
        if (pizza == currentPizza)
        {
            StopCooking();
            pizzaObject = null;
            currentPizza = null;
        }

        // Another raw pizza may still be inside, so cook that one
        TryStartCooking();
    }

    private void OnDisable()
    {
        StopCooking();
        pizzasInside.Clear();
    }

    // Starts cooking the first raw pizza inside, if the oven is free
    private void TryStartCooking()
    {
        if (cookingRoutine != null)
            return;

        // Drop pizzas that were destroyed while inside (eaten, deleted, etc)
        pizzasInside.RemoveAll(p => p == null);

        foreach (Pizza pizza in pizzasInside)
        {
            if (pizza.PizzaState != PizzaState.raw)
                continue;

            currentPizza = pizza;
            pizzaObject = pizza.gameObject;
            cookingRoutine = StartCoroutine(Cooking(pizza));
            return;
        }
    }

    private IEnumerator Cooking(Pizza pizza)
    {
        audioSource = SoundPlayer.Instance.PlayLoop(SoundID.OvenCooking, transform.position);
        
        cookingParticles.Play();
        yield return new WaitForSeconds(GetCookingTime());

        StopCookingEffects();
        cookingRoutine = null;

        if (pizza != null)
        {
            pizza.Cook();
            SoundPlayer.Instance.PlaySound(SoundID.PizzaDoneCooking, transform.position);
        }
        else
        {
            // Destroyed mid-cook
            currentPizza = null;
            pizzaObject = null;
        }

        // Cook the next raw pizza if there is one waiting inside
        TryStartCooking();
    }

    // Cancels any cooking in progress and turns off its sound and particles
    private void StopCooking()
    {
        if (cookingRoutine != null)
        {
            StopCoroutine(cookingRoutine);
            cookingRoutine = null;
        }

        StopCookingEffects();
    }

    private void StopCookingEffects()
    {
        if (cookingParticles != null)
            cookingParticles.Stop();

        if (audioSource != null && SoundPlayer.Instance != null)
            SoundPlayer.Instance.StopLoop(audioSource);

        audioSource = null;
    }

    // Stays inside the cookingTimes array even if the oven level is higher than the array size
    private float GetCookingTime()
    {
        int index = Mathf.Clamp(_level - 1, 0, cookingTimes.Length - 1);
        return cookingTimes[index];
    }

    public void UpgradeOven(int newlevel)
    {
        _level = newlevel;
    }
}

// using System.Collections;
// using UnityEngine;
//
// [RequireComponent(typeof(BoxCollider))]
// public class Oven : MonoBehaviour
// {
//     [SerializeField] private float[] cookingTimes = new float[3];
//     private int _level = 1;
//     //[SerializeField] public float cookingTime;
//     public GameObject pizzaObject;
//     private Pizza currentPizza;
//     public ParticleSystem cookingParticles;
//     
//     private AudioSource audioSource;
//
//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Pizza") && !pizzaObject && other.GetComponent<Pizza>().PizzaState == PizzaState.raw)
//         {
//             pizzaObject = other.gameObject;
//             currentPizza = pizzaObject.GetComponent<Pizza>();
//             StartCoroutine(Cooking());
//         }
//     }
//
//     private void OnTriggerExit(Collider other)
//     {
//         if (pizzaObject && other.gameObject == pizzaObject)
//         {
//             SoundPlayer.Instance.StopLoop(audioSource);
//             
//             StopAllCoroutines();
//             cookingParticles.Stop();
//             pizzaObject = null;
//             currentPizza = null;
//         }
//     }
//
//     private IEnumerator Cooking()
//     {
//         audioSource = SoundPlayer.Instance.PlayLoop(SoundID.OvenCooking, transform.position);
//         
//         cookingParticles.Play();
//         yield return new WaitForSeconds(cookingTimes[_level - 1]);
//         currentPizza.Cook();
//         cookingParticles.Stop();
//         SoundPlayer.Instance.StopLoop(audioSource);
//         SoundPlayer.Instance.PlaySound(SoundID.PizzaDoneCooking, transform.position);
//     }
//
//     public void UpgradeOven(int newlevel)
//     {
//         _level = newlevel;
//     }
// }
