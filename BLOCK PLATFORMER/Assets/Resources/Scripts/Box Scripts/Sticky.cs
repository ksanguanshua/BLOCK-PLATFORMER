using UnityEngine;
using System.Collections.Generic;

public class Sticky : MonoBehaviour
{
    List<GameObject> connectedBoxes = new();
    [SerializeField] int maxConnections;
    bool isHeld;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Box box))
        {
            if (connectedBoxes.Count < maxConnections)
            {
                FixedJoint2D fj = gameObject.AddComponent<FixedJoint2D>();
                fj.connectedBody = collision.rigidbody;
                connectedBoxes.Add(collision.gameObject);
            }
            
        }
    }

    private void Update()
    {
        
    }
}
