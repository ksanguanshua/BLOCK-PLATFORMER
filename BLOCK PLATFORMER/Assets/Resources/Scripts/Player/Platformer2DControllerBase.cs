using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Platformer2DControllerBase : MonoBehaviour
{
    Movement movement;
    TongueScript tongueScript;

    private void Start()
    {
        movement = GetComponent<Movement>();
        tongueScript = GetComponent<TongueScript>();
    }

    #region Input Functions
    public void OnMove(InputValue value)
    {
        movement.MoveInput(value.Get<Vector2>());
    }
    public void OnJump(InputValue value)
    {
        print(value.Get<float>());
        movement.JumpInput(value.Get<float>());
    }
    public void OnAttack(InputValue value)
    {
        print(value.Get<float>());
        tongueScript.GrabInput(value.Get<float>());
    }
    #endregion
}
