using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigidBodyComponent;

    //movement variables
    private bool upKey;
    private bool attackKey;
    private bool dashKey;
    private int jumpHeld = 0; //0 = ignore, 1 = holding down, 2 = let go
    private bool inJump;
    private int hasJump;
    private float horizontalInput;
    private bool isLaunching;

    //layers
    [SerializeField] Transform groundCheck;
    [SerializeField] private LayerMask SpeedPlatform;
    [SerializeField] private LayerMask JumpPad;


    //sprite
    private bool isFacingRight;

    //attacking variables
    private bool canAttack;
    private bool attacking;
    private float attackingPower = 90f;
    private float attackingTime = 0.2f;
    [SerializeField] private TrailRenderer tr; //this is attached to the square which is a child of the actual player
    public bool hit;
    public bool isAttacking;

    //dashing variables
    private bool canDash;
    private bool isDashing;
    private float dashingPower = 60f;
    private float dashingTime = 0.2f;

    //camera
    [SerializeField] private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        rigidBodyComponent = GetComponent<Rigidbody2D>();
        cam = FindObjectOfType<Camera>();

        rigidBodyComponent.gravityScale = 15;

        hasJump = 1;
        attackKey = false;
        isDashing = false;
        attacking = false;
        canDash = true;
        canAttack = true;
        hit = false;

        isAttacking = false;
    }

    // Update is called once per frame
    void Update()
    {
        //stops other inputs from occuring while dashing
        if (attacking || isDashing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            upKey = true;
        }

        if (inJump)
        {
            jumpHeld = 1;
        }

        else if (Input.GetKey(KeyCode.Space))
        {
            jumpHeld = 1;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpHeld = 2;
        }

        if (Input.GetMouseButtonDown(0))
        {
            attackKey = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dashKey = true;
        }


        horizontalInput = Input.GetAxis("Horizontal");
        flip();

    }

    private void FixedUpdate()
    {
        
        //dash cancel
        if (isDashing && Input.GetMouseButton(0))          
        {
            attackingPower = 120f;
            StopCoroutine(Dash());
            DashReset();
            StartCoroutine(Attack());
            attackingPower = 90f;
        }


        //dashing and attacking
        if (dashKey == true && canDash == true)
        {
            StartCoroutine(Dash());
        }

        

        if (attackKey == true && canAttack == true)
        {
            StartCoroutine(Attack());
        }


        attackKey = false;
        dashKey = false;

        if (onJumpPad() && !IsSpeeding())
        {
            Debug.Log("pad");
            StartCoroutine(JumpPadLaunch());
        }



        if (attacking || isDashing || isLaunching)
        {
            return;

        }


        //for variable jump
        if (jumpHeld == 2 && rigidBodyComponent.velocity.y > 0f)
        {
            rigidBodyComponent.velocity = Vector2.zero;
            jumpHeld = 0;
        }



        //horizontal movement
        if (IsSpeeding() == false)
        {
            rigidBodyComponent.velocity = new Vector2(horizontalInput * 30, rigidBodyComponent.velocity.y);
        }

    


        //falling gravity
        if (rigidBodyComponent.velocity.y < -1f)
        {
            rigidBodyComponent.gravityScale = 25;
        }

        //jumping, the first 'else' is the double jump


        if (upKey == true && hasJump == 1)
        {
            rigidBodyComponent.velocity = Vector2.zero;
            rigidBodyComponent.velocity = Vector2.up * 70f;

            StartCoroutine(VariableJump());

            

            upKey = false;
            hasJump = 0;

        }

        else if (upKey == true && hasJump == 2)
        {
            rigidBodyComponent.velocity = Vector2.zero;
            rigidBodyComponent.velocity = Vector2.up * 70f;
            upKey = false;
            hasJump = 0;
        }

        else if (upKey == true)
        {
            StartCoroutine(JumpBuffer());
        }

        if (IsSpeeding())
        {
            Debug.Log("speeding");
            rigidBodyComponent.velocity = new Vector2(transform.localScale.x * 110f + 50f, 0f);
        }




    }

    //resetting jumps and dashes
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //resets movement
        hasJump = 1;
        rigidBodyComponent.gravityScale = 15;
        canAttack = true;
        canDash = true;

        //arrays for the different rebounds of the different angled enemy blocks
        string[] enemyTypes = new string[] { "Enemy", "TopEnemy", "TopRightEnemy", "TopLeftEnemy", "BottomRightEnemy", "BottomLeftEnemy", "RightEnemy", "LeftEnemy" };
        float[,] bounceValues = { {0,0,-40,40,-40,40,-80,80},{70,-30,-20,-20,50,50,20,20} };

        if (isAttacking)
        {
            for (int i = 0; i < 8; i++)
            {
                if (collision.gameObject.CompareTag(enemyTypes[i]))
                {
                    rigidBodyComponent.velocity = new Vector2(bounceValues[0,i],bounceValues[1,i]);
                }
            }
        }  
    }


    //same as above, but these blocks wont have any physics unless u attack them
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hasJump = 1;
        rigidBodyComponent.gravityScale = 15;
        canAttack = true;
        canDash = true;

        string[] enemyTypes = new string[] { "Enemy", "TopEnemy", "TopRightEnemy", "TopLeftEnemy", "BottomRightEnemy", "BottomLeftEnemy", "RightEnemy", "LeftEnemy" };
        float[,] bounceValues = { { 0, 0, -40, 40, -40, 40, -80, 80 }, { 70, -30, -20, -20, 50, 50, 20, 20 } };

        if (isAttacking)
        {
            for (int i = 0; i < 8; i++)
            {
                if (collision.gameObject.CompareTag(enemyTypes[i]))
                {
                    rigidBodyComponent.velocity = new Vector2(bounceValues[0, i], bounceValues[1, i]);
                }
            }
        }
    }

    private void flip()
    {
        if(isFacingRight && horizontalInput > 0f || !isFacingRight && horizontalInput < 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator JumpBuffer()
    {
        yield return new WaitForSeconds(0.2f);
        upKey = false;
    }

    private IEnumerator AttackBuffer()
    {
        yield return new WaitForSeconds(0.02f);
        if (isDashing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(Attack());
            }
        }
    }


    private IEnumerator Attack()
    {
        canAttack = false;
        attacking = true;
        rigidBodyComponent.gravityScale = 0;
        rigidBodyComponent.velocity = Vector2.zero;

        //getting positions
        Vector2 mWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = rigidBodyComponent.position;

        //making the attack vector
        Vector2 worldPosition = Vector2.zero;
        worldPosition.x = mWorldPosition.x - playerPos.x;
        worldPosition.y = mWorldPosition.y - playerPos.y;
        worldPosition = worldPosition.normalized;

        if (isFacingRight && worldPosition.x > 0f || !isFacingRight && worldPosition.x < 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        //attacking
        rigidBodyComponent.velocity = worldPosition * attackingPower;
        tr.emitting = true;
        isAttacking = true;
        yield return new WaitForSeconds(attackingTime);

        //resetting
        AttackReset();

        
        
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rigidBodyComponent.gravityScale = 0;

        StartCoroutine(AttackBuffer());

        //dashing
        rigidBodyComponent.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);

        //resetting
        DashReset();

    }

    private IEnumerator JumpPadLaunch()
    {
        isLaunching = true;
        rigidBodyComponent.gravityScale = 0;

        //dashing
        rigidBodyComponent.velocity = new Vector2(transform.localScale.x * 70f, transform.localScale.x * 40f);
        tr.emitting = true;
        yield return new WaitForSeconds(0.2f);

        //resetting
        LaunchReset();

    }

    private void DashReset()
    {
        dashKey = false;
        rigidBodyComponent.gravityScale = 15;
        tr.emitting = false;
        rigidBodyComponent.velocity = rigidBodyComponent.velocity * 0.2f;
        isDashing = false;
    }

    private void LaunchReset()
    {
        rigidBodyComponent.gravityScale = 15;
        tr.emitting = false;
        rigidBodyComponent.velocity = rigidBodyComponent.velocity * 0.2f;
        isLaunching = false;
    }

    private void AttackReset()
    {
        isAttacking = false;
        attackKey = false;
        rigidBodyComponent.gravityScale = 15;
        tr.emitting = false;
        rigidBodyComponent.velocity = rigidBodyComponent.velocity * 0.2f;
        attacking = false;
    }


    private IEnumerator VariableJump()
    {
        inJump = true;
        yield return new WaitForSeconds(0.02f);
        inJump = false;    
    }
    
    private bool IsSpeeding()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.4f, SpeedPlatform) && Input.GetMouseButton(0) && (rigidBodyComponent.velocity.x > 0);
    }

    private bool onJumpPad()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.4f, JumpPad);
    }
}