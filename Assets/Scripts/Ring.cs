
using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SonicController sonic = other.GetComponentInParent<SonicController>();

        if (sonic != null)
        {
            sonic.CollectRing();
            Destroy(gameObject);
        }
    }
}