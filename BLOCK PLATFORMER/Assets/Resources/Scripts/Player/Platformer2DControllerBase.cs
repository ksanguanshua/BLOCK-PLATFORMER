using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Platformer2DControllerBase : MonoBehaviour
{
    Movement movement;

    private void Start()
    {
        movement = GetComponent<Movement>();
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
    #endregion
}
