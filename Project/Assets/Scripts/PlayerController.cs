using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rbPlayer;
    public CamFollowPlayer camFollow;
    
    public float movementSpeed = 330.0f;
    float dodgeForce = 2f;
    float rollForce = 1.3f;
    float nextDodgeTime = 0f;
    float nextRollBackTime = 0f;
    Vector2 movement;

    Animator animator;
    string currentState;
    const string PLAYER_IDLE = "Player_Idle";
    const string PLAYER_WALK = "Player_Walk";
    const string PLAYER_DODGE_LEFT = "Player_Dodge_Left";
    const string PLAYER_DODGE_RIGHT = "Player_Dodge_Right";
    const string PLAYER_ROLLBACK = "Player_Rollback";

    Rigidbody2D rbEnemy;

    void Awake()
    {
        rbPlayer = this.GetComponent<Rigidbody2D>();
        rbEnemy = null;

        animator = this.GetComponent<Animator>();
        ChangeAnimationState(PLAYER_IDLE);

        camFollow.enabled = false;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (movement.x != 0 || movement.y != 0)
            ChangeAnimationState(PLAYER_WALK);
        else
            ChangeAnimationState(PLAYER_IDLE);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (movement.x != 0 && movement.y >= 0)
                Dodge();
            else
                RollBack();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (camFollow.enabled == true)
                camFollow.enabled = false;
            else
                camFollow.enabled = true;
        }

    }

    void FixedUpdate()
    {
        // move based on input
        rbPlayer.AddForce(movement * movementSpeed * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    void Dodge()
    {
        if (Time.time >= nextDodgeTime)
        {
            rbPlayer.AddForce(movement * dodgeForce, ForceMode2D.Impulse);
            nextDodgeTime = Time.time + 0.3f;
        }

        // play dodge animation
    }

    void RollBack()
    {
        if (Time.time >= nextRollBackTime)
        {
            rbPlayer.AddForce(new Vector2(0, movement.y) * rollForce, ForceMode2D.Impulse);
            nextRollBackTime = Time.time + 0.3f;
        }
        
        // play roll animation
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState)
            return;

        animator.Play(newState);
        currentState = newState;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            rbEnemy = collision.gameObject.GetComponent<Rigidbody2D>();
            rbEnemy.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (rbEnemy != null)
        {
            rbEnemy.constraints = RigidbodyConstraints2D.None;
        }
    }
}