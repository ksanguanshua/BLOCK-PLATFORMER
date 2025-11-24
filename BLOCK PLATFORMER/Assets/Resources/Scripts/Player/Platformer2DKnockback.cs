using UnityEngine;

public class Platformer2DKnockback : MonoBehaviour
{
    Rigidbody2D c_rb;
    Platformer2DMovementBase c_mb;
    [SerializeField] public Transform c_spriteTrans;

    public void Start()
    {
        c_mb = GetComponent<Platformer2DMovementBase>();
        c_rb = GetComponent<Rigidbody2D>();
    }

    public void KnockbackEffect()
    {
        if (c_rb.linearVelocity.sqrMagnitude > 0.01f) // avoid division by zero
        {
            float angle = Mathf.Atan2(c_rb.linearVelocity.y, c_rb.linearVelocity.x) * Mathf.Rad2Deg;
            c_mb.ss_facing = new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle));
            print(c_mb.ss_facing);
            c_spriteTrans.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
