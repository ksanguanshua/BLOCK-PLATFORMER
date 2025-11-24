using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Platformer2DMovementBase))]
public class Platformer2DLedge : MonoBehaviour
{
    #region Modifiers
    [Header("Wall Slide Ledge Settings")]
    [Range(-10.0f, 10.0f)][SerializeField] private float m_slideSpeed = -7f;
    [Header("Ledging Settings")]
    [SerializeField] private float ledgeCheckDistance = 1f; // Distance to detect a ledge
    [SerializeField] private LayerMask lm_ledgeLayer;       // Layer for ledges
    [SerializeField] private float ledgeJumpForce = 20f;        // Force applied during the vault
    [Header("Debug")]
    [SerializeField] private bool debugLedgeDetection = true;
    [SerializeField] private bool ch_canLedge = true;
    #endregion

    #region Components
    private Rigidbody2D c_rb;
    private Platformer2DMovementBase c_movementBase;
    #endregion

    #region Initialization
    private void Start()
    {
        ComponentGrab();
    }

    void ComponentGrab()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_movementBase = GetComponent<Platformer2DMovementBase>();
    }
    #endregion
    public void CheckLedge(Vector2 in_moveInput)
    {
        if (CanLedge((int)in_moveInput.x) && in_moveInput.y > -0.5 && c_movementBase.s_playerState != Platformer2DMovementBase.state.WallJumping && c_rb.linearVelocityY < 0.5)
        {
            c_movementBase.s_playerState = Platformer2DMovementBase.state.Ledging;
            c_rb.linearVelocity = Vector2.zero;
            c_rb.gravityScale = 0;
        }
        else if(c_movementBase.s_playerState == Platformer2DMovementBase.state.Ledging)
        {
            c_movementBase.s_playerState = Platformer2DMovementBase.state.Active;
        }
    }

    public bool CanLedge(int in_moveInput)
    {
        if (!ch_canLedge) return false;

        // Detect ledges based on movement input direction
        Vector2 direction = new Vector2(in_moveInput, 0);
        RaycastHit2D lowerHit = Physics2D.Raycast(transform.position + Vector3.up * 0.9f, direction, ledgeCheckDistance, lm_ledgeLayer);
        RaycastHit2D upperHit = Physics2D.Raycast(transform.position + Vector3.up * 1.2f, direction, ledgeCheckDistance, lm_ledgeLayer);

        bool ledgeDetected = lowerHit.collider != null && upperHit.collider == null;

        if (debugLedgeDetection)
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.9f, direction * ledgeCheckDistance, lowerHit.collider ? Color.green : Color.red);
            Debug.DrawRay(transform.position + Vector3.up * 1.2f, direction * ledgeCheckDistance, upperHit.collider ? Color.green : Color.red);
        }

        return ledgeDetected;
    }
}
