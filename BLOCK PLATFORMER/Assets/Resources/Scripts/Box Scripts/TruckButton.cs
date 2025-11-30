using UnityEngine;

public class TruckButton : MonoBehaviour
{
    [SerializeField] DeliveryZone deliveryZone;
    [SerializeField] SpriteRenderer sr;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            deliveryZone.CheckOrder();
            sr.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sr.enabled = true;
        }
    }
}
