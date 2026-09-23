using UnityEngine;

[CreateAssetMenu(fileName = "NewRatData", menuName = "Scriptable Objects/Rat Data")]
public class RatData : ScriptableObject
{
    public float speed;
    public int maxHealth;
    public float wanderRadius;
    public float minIdleTime;
    public float maxIdleTime;
    public float fleeRadius; // How close player must be to trigger fleeing
    public float fleeDistance; // How far the rat tries to run
    public float pizzaDetectRadius; // How close rat detects pizza
    public float eatDuration; // How long till pizza is eaten
    public float sizeGrowthMultiplier; // How much rat grows after eating pizza
    public float cleanUpDuration; // How long it takes to clean up rat body
}
