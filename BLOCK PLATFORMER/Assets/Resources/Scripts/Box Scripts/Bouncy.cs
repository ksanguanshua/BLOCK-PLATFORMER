using UnityEngine;

public class Bouncy : Box
{
    [SerializeField] float bounciness;

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody2D rb))
        {
            //Vector2 forceVector = collision.collider.bounds.center - collision.otherCollider.bounds.center;
            Vector2 forceVector = new Vector2(-collision.rigidbody.linearVelocityX, -collision.rigidbody.linearVelocityY / 2);
            rb.AddForce(forceVector.normalized * bounciness, ForceMode2D.Impulse);
        }
    }*/

    public override void OnTouch()
    {
        base.OnTouch();

        GameObject.FindGameObjectWithTag("Player").GetComponent<TongueScript>().M.pushBackForce += bounciness;
    }

    public override void OnHold()
    {
        base.OnHold();

        GameObject.FindGameObjectWithTag("Player").GetComponent<TongueScript>().M.pushBackForce -= bounciness;
    }
}
