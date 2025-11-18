using UnityEngine;

[RequireComponent(typeof(Platformer2DMovementBase))]
public class Platformer2DRunning : MonoBehaviour
{
    #region Running Settings
    [Header("Running Settings")]
    [SerializeField] private float runSpeedMax = 1.5f; //running speed
    [SerializeField] private float runAcceleration = 1.2f; //running acceleration

    [Header("Modes of Activation")]
    [SerializeField] private bool VELOCITYTRIGGER;
    [SerializeField] private float velocityThreshold = 8f; // Velocity required to trigger running
    [SerializeField] private bool BUTTONTRIGGER;
    [SerializeField] private KeyCode runButton = KeyCode.LeftShift; // Button to trigger running
    [SerializeField] private bool DASHTRIGGER;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false; // Enable debug logs for development
    #endregion

    #region Components
    private Platformer2DMovementBase c_mb;
    private Rigidbody2D c_rb;
    #endregion

    #region State
    private int runDirection = 0;
    private bool isRunning = false; // Whether the player is currently running
    private bool dashTriggeredRun = false; // Whether running was triggered by a dash
    private float originalMoveSpeed; // Cached base move speed
    private float originalAcceleration; // Cached base acceleration
    #endregion

    #region Initialization
    private void Start()
    {
        c_mb = GetComponent<Platformer2DMovementBase>();
        c_rb = GetComponent<Rigidbody2D>();

        // Cache the original movement speed and acceleration
        originalMoveSpeed = c_mb.m_movementSpeed;
        originalAcceleration = c_mb.m_groundAcceleration;
    }
    #endregion
    
    public void CheckRun(int p_xInput)
    {
        if (isRunning)
        {
            if(runDirection != p_xInput || Mathf.Abs(c_rb.linearVelocity.x) < c_mb.m_movementSpeed)
            {
                isRunning = false;
                c_mb.m_movementSpeed = originalMoveSpeed;
                runDirection = 0;
            }
        }
        else
        {
            if (VELOCITYTRIGGER)
            {
                if (c_mb.m_movementSpeed < Mathf.Abs(c_rb.linearVelocity.x))
                {
                    isRunning = true;
                    c_mb.m_movementSpeed = Mathf.Clamp(Mathf.Abs(c_rb.linearVelocity.x), originalMoveSpeed, runSpeedMax);
                    runDirection = p_xInput;
                }
            }
        }
    }

    }
