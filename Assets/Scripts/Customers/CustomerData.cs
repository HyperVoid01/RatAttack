using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerData", menuName = "Scriptable Objects/Customer Data")]
public class CustomerData : ScriptableObject
{
    public float walkSpeed;
    public float orderWaitTime; // How long customer will wait for order till they leave
    public float eatTime;
    public float ratDetectionRadius; // How far customer can see rats
    public int ratTolerance; // How many seconds customer must see rat for to leave
}
