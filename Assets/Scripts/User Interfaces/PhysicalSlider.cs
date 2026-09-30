using UnityEngine;

public class PhysicalSlider : MonoBehaviour
{
    [SerializeField] private GameObject knob;
    [SerializeField] private GameObject point1;
    [SerializeField] private GameObject point2;

    public void SetValue(float value)
    {
        knob.transform.position = Vector3.Lerp(point1.transform.position, point2.transform.position, value);
    }
}
