using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Oven : MonoBehaviour
{
    [SerializeField] private float[] cookingTimes = new float[3];
    private int _level = 1;
    //[SerializeField] public float cookingTime;
    public GameObject pizzaObject;
    private Pizza currentPizza;
    public ParticleSystem cookingParticles;
    
    private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pizza") && !pizzaObject && other.GetComponent<Pizza>().PizzaState == PizzaState.raw)
        {
            pizzaObject = other.gameObject;
            currentPizza = pizzaObject.GetComponent<Pizza>();
            StartCoroutine(Cooking());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (pizzaObject && other.gameObject == pizzaObject)
        {
            SoundPlayer.Instance.StopLoop(audioSource);
            
            StopAllCoroutines();
            cookingParticles.Stop();
            pizzaObject = null;
            currentPizza = null;
        }
    }

    private IEnumerator Cooking()
    {
        audioSource = SoundPlayer.Instance.PlayLoop(SoundID.OvenCooking, transform.position);
        
        cookingParticles.Play();
        yield return new WaitForSeconds(cookingTimes[_level - 1]);
        currentPizza.Cook();
        cookingParticles.Stop();
        SoundPlayer.Instance.StopLoop(audioSource);
        SoundPlayer.Instance.PlaySound(SoundID.PizzaDoneCooking, transform.position);
    }

    public void UpgradeOven(int newlevel)
    {
        _level = newlevel;
    }
}
