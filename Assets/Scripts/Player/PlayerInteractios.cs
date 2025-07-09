using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractios : MonoBehaviour
{
   // public Transform startPosition; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GunAmmo"))
        {
            GameManager.Instance.gunAmmo += other.gameObject.GetComponent<AmmoBox>().ammo;

            Destroy(other.gameObject);
        }


    }
}
