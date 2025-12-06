using UnityEngine;
using UnityEngine.InputSystem;
using SaintsField;
using System.Collections;
using Unity.VisualScripting;
using SaintsField.Playa;
using static UnityEditor.Experimental.GraphView.GraphView;

public class TongueScript : MonoBehaviour
{
    [System.Serializable]
    public struct Modifiers
    {
        [SerializeField] public float tongueLength;
        [SerializeField] public float tongueShootTime;
        [SerializeField] public float throwForce;
        [SerializeField] public float handLerpForce;
        [SerializeField] public AnimationCurve tongueExtensionCurve;
        [SerializeField] public float pushBackForce;

        [LayoutStart("Tongue Launching", ELayout.FoldoutBox)]
        [SerializeField] public float tonguePullForce;
        [SerializeField] public float launchAccel;
        [SerializeField] public float launchDeccel;
        [SerializeField][ReadOnly] public float baseAccel;
        [SerializeField][ReadOnly] public float baseDeccel;
    }

    [System.Serializable]
    public struct States
    {
        [LayoutStart("Box Holding", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public float holdInput;
        [SerializeField][ReadOnly] public Vector2 lastFacingDir;
        [SerializeField][ReadOnly] public bool canTurn;
        [SerializeField][ReadOnly] public Transform heldBox;
        [SerializeField][ReadOnly] public GameObject tongueBox;
        [SerializeField][ReadOnly] public Transform hand;
        [SerializeField][ReadOnly] public Transform grabbableBox;

        [LayoutEnd]
        [LayoutStart("Tongue States", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public bool tongueOut;
        [SerializeField][ReadOnly] public bool tonguePulling;
        [SerializeField][ReadOnly] public Vector2 tongueLastDir;
        [SerializeField][ReadOnly] public Vector2 tongueOffset;
        [SerializeField][ReadOnly] public bool tongueRetracting;
        [SerializeField][ReadOnly] public Vector2 tongueEndPoint;
        [SerializeField][ReadOnly] public float lerpTime;
    }

    [System.Serializable]
    public struct References
    {
        [SerializeField][ReadOnly] public Movement movement;
        [SerializeField][ReadOnly] public Animator anim;
        [SerializeField] public Transform playerSprite;
        [SerializeField] public LineRenderer lineRenderer;
        [SerializeField] public LineRenderer lineRendererVisualLayer1;
        [SerializeField] public LineRenderer lineRendererVisualLayer2;
        [SerializeField][ReadOnly] public Transform tongueTip;
        [SerializeField] public LayerMask layerBox;
        [SerializeField] public LayerMask layerGround;
    }

    [SerializeField][SaintsRow][RichLabel("Modifiers")] public Modifiers M;
    [SerializeField][SaintsRow][RichLabel("References")] public References R;
    [SerializeField][SaintsRow][RichLabel("States")] public States S;

    void Start()
    {
        S.canTurn = true;
        S.lastFacingDir = Vector2.right;
        S.hand = transform.Find("Hand");
        R.tongueTip = transform.Find("EndPointOfTongue");
        R.movement = GetComponent<Movement>();
        R.anim = GetComponent<Animator>();

        M.baseAccel = R.movement.M.accelerationAir;
        M.baseDeccel = R.movement.M.deccelerationAir;
    }
    void Update()
    {
        HandMovement();
        TongueUpdate();
        if (S.heldBox != null)
        {
            HeldBox();
        }
        R.anim.SetBool("tongueOut", S.tongueOut);
        R.anim.SetBool("grounded", R.movement.S.isGrounded);
        R.anim.SetFloat("velocityY", R.movement.R.rb.linearVelocityY);

        if (R.movement.S.movementInput.y >= 0)
        {
            R.movement.M.accelerationAir = M.baseAccel;
            R.movement.M.deccelerationAir = M.baseDeccel;
        }

        if (Mathf.Abs(R.movement.R.rb.linearVelocityX) >= 10)
        {
            R.movement.R.particleManager.StartPlay("PSRings");
            ParticleSystem PSthrow = R.movement.R.particleManager.GetParticleSystem("PSTrail");
            var main = PSthrow.main;
            main.startRotationY = R.playerSprite.eulerAngles.y * Mathf.Deg2Rad;
            var textureSheetAnimation = PSthrow.textureSheetAnimation;
            textureSheetAnimation.SetSprite(0, R.playerSprite.GetComponent<SpriteRenderer>().sprite);
            R.movement.R.particleManager.StartPlay("PSTrail");
        }
        else
        {
            R.movement.R.particleManager.StopParticle("PSRings");
            R.movement.R.particleManager.StopParticle("PSTrail");
        }
    }

    public void GrabInput(float input)
    {
        if (!S.tongueOut)
        {
            if (input == 1 && S.holdInput == 0)
            {
                R.anim.SetTrigger("tongueThrow");
                ThrowTongue(S.lastFacingDir);

                GameObject PSthrow = R.movement.R.particleManager.GetParticleSystem("PSTongueThrow").gameObject;
                float angle = Mathf.Atan2(S.lastFacingDir.y, S.lastFacingDir.x) * Mathf.Rad2Deg;
                PSthrow.transform.rotation = Quaternion.Euler(0, 0, angle);
                R.movement.R.particleManager.PlayParticle("PSTongueThrow");

                AudioManager.instance.PlayTongueOut();
            }
        }
        if (input == 0 && S.holdInput == 1 && S.heldBox != null) // holding box and ready to throw --> throw
        {
            //S.heldBox.GetComponent<Collider2D>().enabled = true;
            S.heldBox.gameObject.layer = LayerMask.NameToLayer("Box");
            S.heldBox.GetComponent<Box>().OnThrow();
            S.heldBox.GetComponent<Rigidbody2D>().gravityScale = 2;
            S.heldBox.GetComponent<Rigidbody2D>().AddForce(new Vector2(S.lastFacingDir.x, S.lastFacingDir.y + 1) * M.throwForce, ForceMode2D.Impulse);
            S.heldBox = null;

            GameObject PSthrow = R.movement.R.particleManager.GetParticleSystem("PSThrow").gameObject;
            float angle = Mathf.Atan2(S.lastFacingDir.y + 1, S.lastFacingDir.x) * Mathf.Rad2Deg;
            PSthrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            R.movement.R.particleManager.PlayParticle("PSThrow");
        }
        else if (input == 0 && S.holdInput == 1 && S.tongueBox != null) // pulling box with tongue
        {
            S.tongueBox.transform.parent = null;
            //S.tongueBox.GetComponent<Collider2D>().enabled = true;
            S.tongueBox.GetComponent<Rigidbody2D>().gravityScale = 2;
            S.tongueBox = null;
        }
        S.holdInput = input;
    }

    void HandMovement()
    {
        if (R.movement.S.movementInput != Vector2.zero && S.canTurn)
        {
            S.lastFacingDir = R.movement.S.movementInput;
            if (R.movement.S.movementInput.x < 0)
            {
                R.anim.SetBool("lookRight", false);
                R.anim.SetBool("moving", true);
                R.movement.R.particleManager.StartPlay("PSDustWalk");
            }
            else if (R.movement.S.movementInput.x > 0)
            {
                R.anim.SetBool("lookRight", true);
                R.anim.SetBool("moving", true);
                R.movement.R.particleManager.StartPlay("PSDustWalk");
            }
        }

        S.hand.position = Vector2.Lerp(S.hand.position, transform.position + (Vector3)S.lastFacingDir, M.handLerpForce);
        R.anim.SetFloat("inputX", S.lastFacingDir.x);
        R.anim.SetFloat("inputY", S.lastFacingDir.y);
        R.anim.SetFloat("inputHoldY", R.movement.S.movementInput.y);

        if (R.movement.S.movementInput.x == 0)
        {
            R.anim.SetBool("moving", false);
        }

        if (R.movement.S.movementInput.x == 0 || !R.movement.S.isGrounded)
        {
            R.movement.R.particleManager.StopParticle("PSDustWalk");
        }
    }

    void Grab(GameObject box)
    {
        S.hand.GetChild(0).position = box.transform.position;
        S.hand.GetChild(0).rotation = box.transform.rotation;
        S.hand.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        S.heldBox = box.transform;
        S.hand.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        S.grabbableBox = null;
    }

    void HeldBox()
    {
        //S.heldBox.GetComponent<Collider2D>().enabled = false;
        //S.heldBox.rotation = S.hand.rotation;
        //S.heldBox.position = S.hand.position;
        S.heldBox.gameObject.layer = LayerMask.NameToLayer("HeldBox");
        S.heldBox.GetComponent<Box>().BeingHeld();
        S.heldBox.GetComponent<Rigidbody2D>().MovePositionAndRotation(S.hand.position, S.hand.rotation);
        S.heldBox.GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    void ThrowTongue(Vector2 direction)
    {
        S.canTurn = false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, M.tongueLength, R.layerBox);
        if (hit)
        {
            GameObject boxHit = hit.collider.gameObject;

            boxHit.transform.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            boxHit.transform.GetComponent<Rigidbody2D>().gravityScale = 0;

            S.tongueOffset = boxHit.transform.position - transform.position;
            S.tongueOut = true;
            print("box hit : " + boxHit);
            S.tongueBox = boxHit;
            R.anim.SetFloat("tongueX", S.tongueOffset.x);
            R.anim.SetFloat("tongueY", S.tongueOffset.y);
        }
        else
        {
            print("box not hit");
            RaycastHit2D groundHit = Physics2D.Raycast(transform.position, direction, M.tongueLength, R.layerGround);
            if (groundHit)
            {
                S.tongueOffset = groundHit.point - (Vector2)transform.position + (groundHit.point - (Vector2)transform.position).normalized * 0.1f;
                S.tonguePulling = true;
            }
            else
            {
                S.tongueOffset = direction * M.tongueLength;
            }
            S.tongueOut = true;
            R.anim.SetFloat("tongueX", direction.x);
            R.anim.SetFloat("tongueY", direction.y);
        }

        StartCoroutine("TongueRetract");
    }

    void CheckCatchMidThrow(Vector2 direction)
    {

    }

    void BoxToTongueTip(GameObject box)
    {
        //box.GetComponent<Collider2D>().enabled = false;
        //box.transform.rotation = S.hand.rotation;
        box.layer = LayerMask.NameToLayer("HeldBox");
        box.GetComponent<Box>().OnHold();
        box.GetComponent<Rigidbody2D>().MoveRotation(S.hand.rotation);
        box.GetComponent<Rigidbody2D>().gravityScale = 0;
        box.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        box.transform.parent = R.tongueTip;
        box.transform.localPosition = Vector2.zero;
    }

    IEnumerator TongueRetract()
    {
        R.movement.S.state = Movement.State.Inactive;
        R.movement.S.acceleration = 0;
        R.movement.S.decceleration = 0;
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.gravityScale = 0;
        yield return new WaitForSeconds(0.05f);
        R.anim.SetBool("lookNEUTRAL", true);
        yield return new WaitForSeconds(0.1f);
        S.tongueRetracting = true;
        yield return new WaitForSeconds(0.05f);
        if (S.tongueBox != null)
            BoxToTongueTip(S.tongueBox);
        rigidbody2D.gravityScale = 1;
        R.movement.S.acceleration = R.movement.M.acceleration;
        R.movement.S.decceleration = R.movement.M.decceleration;
        R.movement.S.state = Movement.State.Base;

        S.tongueRetracting = true;
        S.tongueOut = false;
        S.tongueOffset = Vector2.zero;

        AudioManager.instance.PlayTongueIn();

        if (S.tongueBox != null)
        {
            GetComponent<Rigidbody2D>().AddForce(-S.lastFacingDir * M.pushBackForce, ForceMode2D.Impulse);
            ParticleSystem ps = R.movement.R.particleManager.GetParticleSystem("PSRing");
            var main = ps.main;
            //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            main.startRotationZ = (R.playerSprite.eulerAngles.z * -1) * Mathf.Deg2Rad;
            print((R.playerSprite.eulerAngles.z * -1) * Mathf.Deg2Rad);
            R.movement.R.particleManager.PlayParticle("PSRing");
            R.movement.R.particleManager.PlayParticle("PSBoxBurst");
            Grab(S.tongueBox);
            S.tongueBox.transform.parent = null;
            S.tongueBox = null;

            AudioManager.instance.PlayBoxHit();
        }
        else if (S.tonguePulling == true)
        {
            GetComponent<Rigidbody2D>().AddForce(S.lastFacingDir * M.tonguePullForce, ForceMode2D.Impulse);
            R.movement.M.accelerationAir = M.launchAccel;
            R.movement.M.deccelerationAir = M.launchDeccel;
            S.tonguePulling = false;

            AudioManager.instance.PlayDash();
        }
        S.canTurn = true;
        R.anim.SetBool("lookNEUTRAL", false);
    }

    void TongueUpdate()
    {
        S.tongueEndPoint = Vector3.Lerp(S.tongueEndPoint, S.tongueOffset, 0.5f);
        R.tongueTip.position = S.tongueEndPoint + (Vector2)transform.position;
        // S.lerpTime += Time.deltaTime;
        // float percent = Mathf.Clamp01(M.tongueExtensionCurve.Evaluate(S.lerpTime));
        // S.tongueEndPoint = Vector3.Lerp(transform.position, (Vector2)transform.position + S.tongueOffset, percent);
        R.lineRenderer.SetPosition(1, S.tongueEndPoint);
        R.lineRendererVisualLayer1.SetPosition(1, S.tongueEndPoint);
        R.lineRendererVisualLayer2.SetPosition(1, S.tongueEndPoint);
        if (S.tongueEndPoint == Vector2.zero)
        {
            S.tongueRetracting = false;
        }

        /*
        if (S.tongueRetracting)
        {
            print("huh");
            Collider2D collider = Physics2D.OverlapCircle(((Vector2)transform.position + S.tongueEndPoint), 1, R.layerBox);
            if (collider != null)
            {
                Rigidbody2D rigidbody2D = collider.GetComponent<Rigidbody2D>();
                print("almost");
                if (rigidbody2D.mass > 1)
                {
                    print("called");
                    GetComponent<Rigidbody2D>().AddForce((((Vector2)transform.position + S.tongueEndPoint) - (Vector2)transform.position) * 30f * Time.deltaTime, ForceMode2D.Impulse);
                }
                else
                {
                    print("called lower");
                    rigidbody2D.AddForce(((Vector2)transform.position - ((Vector2)transform.position + S.tongueEndPoint)) * 30f * Time.deltaTime, ForceMode2D.Impulse);
                }
            }
        }
        */
    }
}
