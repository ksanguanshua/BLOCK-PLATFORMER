using UnityEngine;

public abstract class BaseMovementParent : MonoBehaviour
{
    abstract public void Launch(Vector2 direction, float force, bool resetForce);
}
