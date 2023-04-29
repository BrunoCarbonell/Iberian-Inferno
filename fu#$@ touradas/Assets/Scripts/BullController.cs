using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class BullController : MonoBehaviour
{
    private PlayerBull inputAction;
    public Animator anim;
    private bool facingRigth = true;
    private Rigidbody2D rb;
    public Transform[] sprites;
    public CopyAnim[] ragdoll;
    public PolygonCollider2D[] ragCol;
    private GameManager gM;

    [Header("Movement")]
    public float movementForce;
    private float atualMovForce;
    private float xDir;
    private Vector3 m_Velocity = Vector3.zero;
    private int timer = 0;

    [Header("Jump")]
    public float jumpForce;
    public float secondaryJumpForce;
    public int maxJumps = 2;
    public float maxButtonHoldTime;
    public float holdForce;
    public float maxJumpSpeed;
    public float maxFallSpeed;
    public float fallSpeed;
    public float gravityMultipler;
    public LayerMask whatIsGround;
    private bool isGrounded;
    public Transform groundCheck;
    [Space(5)]
    [Range(0f, 2f)] public float raicastDistance = 1.5f;
    public GameObject jumpEffect;

    private bool jumpPressed;
    private bool jumpHeld;
    private float buttonHoldTime;
    private float originalGravity;
    [SerializeField]private int numberOfJumpsLeft;
    private bool isJumping;



    [Header("Attacks")]
    public float explosionRadius = 5;
    public float exploxionForceMulti = 5;
    Collider2D[] inExplosionRadius = null;
    public Transform explosionCenter;
    public BoxCollider2D headButtCol;
    public GameObject headbutEffect;
    public GameObject explosionEffect;
    public float attackCooldown = 0.7f;
    private bool canAttack = true;

    [Header("Status")]
    public GameObject[] holdingSpots;
    public int mFStacks;
    [Space(5)]
    [Range(0f, 100f)] public float mFSlow;
    [SerializeField] private float atualSlow;
    public float dmgOverTime = 5;
    public float maxHP;
    public float atualHP;
    public List<Enemy> mfAtacheds = new List<Enemy>();



    private void Awake()
    {
        inputAction = new PlayerBull();
        //anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        atualMovForce = movementForce;
        
    }

    private void Start()
    {
        gM = GameObject.FindObjectOfType<GameManager>();
        originalGravity = rb.gravityScale;
        buttonHoldTime = maxButtonHoldTime;
        numberOfJumpsLeft = maxJumps;
        StartCoroutine(time());
    }


    private void Update()
    {

        if (gM.state == GameState.PAUSE)
        {
            anim.speed = 0;
            return;
        }
        else
            anim.speed = 1;


        if (Input.GetKeyDown(KeyCode.D) && !facingRigth)
            Flip();
        else if (Input.GetKeyDown(KeyCode.A) && facingRigth)
            Flip();
        gM.UpdateHpBar(atualHP, maxHP);

        CheckForJump();

        if (isGrounded)
        {

            numberOfJumpsLeft = maxJumps;
            rb.gravityScale = originalGravity;
        }
    }

    private void FixedUpdate()
    {
        Movement();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, raicastDistance, whatIsGround);

        atualSlow = 1-((mFSlow * mFStacks)/100);

        if (mFStacks > 0)
            atualMovForce = atualSlow * movementForce;
        else
            atualMovForce = movementForce;
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));

        if (atualHP <= 0)
        {
            gM.state = GameState.GAMEOVER;
            foreach (PolygonCollider2D c in ragCol)
            {
                c.enabled = true;
            }

            foreach (CopyAnim r in ragdoll)
            {
                r.enabled = false;
                GetComponent<CapsuleCollider2D>().enabled = false;
                this.enabled = false;
            }

            
        }

        IsJumping();
    }

    public void Movement()
    {
        Vector3 targetVelocity = new Vector2(xDir * atualMovForce, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, 0.05f);
    }

    public void Move(InputAction.CallbackContext context)
    {
        xDir = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {

        if (context.started)
        {
            jumpPressed = true;
            jumpHeld = true;
        }
    }

    public void CheckForJump()
    {
        if (jumpPressed)
        {
            if (!isGrounded && numberOfJumpsLeft == maxJumps)
            {
                isJumping = false;
                //return;
            }
            numberOfJumpsLeft--;
            if (numberOfJumpsLeft >= 0)
            {
                rb.gravityScale = originalGravity;
                rb.velocity = new Vector2(rb.velocity.x, 0);
                buttonHoldTime = maxButtonHoldTime;
                if (!isJumping)
                {
                    jumpEffect.GetComponentInParent<ParticleSystem>().Play();
                    anim.SetTrigger("Jump");
                }
                isJumping = true;
            }
            jumpPressed = false;

        }
    }

    private void IsJumping()
    {
        if (isJumping && numberOfJumpsLeft == maxJumps)
        {
            rb.AddForce(Vector2.up * jumpForce);
            AdditionalAir();
        }else if(isJumping && numberOfJumpsLeft < maxJumps)
        {
            rb.AddForce(Vector2.up * secondaryJumpForce);
            AdditionalAir();
        }
        if(rb.velocity.y > maxJumpSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxJumpSpeed);
        }
        Falling();
    }
    
    private void AdditionalAir()
    {
        if (jumpHeld)
        {
            buttonHoldTime -= Time.deltaTime;
            if (buttonHoldTime <= 0)
            {
                buttonHoldTime = 0;
                isJumping = false;
                jumpHeld = false;
            }
            else
                rb.AddForce(Vector2.up * holdForce);
        }
        else
        {
            isJumping = false;
        }
    }
    private void Falling()
    {
        if(!isJumping && rb.velocity.y < fallSpeed)
        {
            rb.gravityScale = gravityMultipler;
        }
        if (rb.velocity.y < maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
        }
    }

    public void HeadButting(InputAction.CallbackContext context)
    {
        if (context.started && canAttack)
        {
            headButtCol.enabled = true;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackDelay(0.4f));
            canAttack = false;
            StartCoroutine(delayNextAtack(attackCooldown));
        }
    }

    public void Flip()
    {
        foreach(Transform t in sprites)
        {
            Vector3 currentScale = t.transform.localScale;
            currentScale.x *= -1;
            t.transform.localScale = currentScale;
        }
        facingRigth = !facingRigth;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(explosionCenter.position, explosionRadius);
        Gizmos.DrawWireSphere(groundCheck.position, raicastDistance);

    }


    public void Explode(InputAction.CallbackContext context)
    {
        if (context.started && mFStacks >0)
        {
            anim.SetTrigger("AOE");
            inExplosionRadius = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach(Collider2D o in inExplosionRadius)
            {
                Rigidbody2D o_rigid = o.GetComponent<Rigidbody2D>();
                if(o_rigid != null && o.GetComponent<Enemy>()!=null)
                {
                    Vector2 distanceVec = o.transform.position - explosionCenter.position;
                    if(distanceVec.magnitude > 0 && !o.CompareTag("Player"))
                    {
                        mFStacks = 0;
                        StartCoroutine(ExplosionDelay(0.2f, o_rigid, o, distanceVec));
                    }
                }
            }
        }
    }

    public GameObject FreeHoldingSpots()
    {
        switch (mFStacks)
        {
            case 0:
                return holdingSpots[0];
            case 1:
                return holdingSpots[1];
            case 2:
                return holdingSpots[2];
            case 3:
                return holdingSpots[3];
            default:
                return null;
        }
    }

    IEnumerator ExplosionDelay(float waitTime, Rigidbody2D o_rigid, Collider2D o, Vector2 distanceVec)
    {
        yield return new WaitForSeconds(waitTime);
        explosionEffect.GetComponentInParent<ParticleSystem>().Play(true);
        if (o.GetComponent<Enemy>().isHolding)
        {
            float explosionForce = exploxionForceMulti;
            o_rigid.AddForce(distanceVec.normalized * explosionForce);
            if (o.GetComponent<Enemy>())
            {
                o.GetComponent<Enemy>().Hit();
                o.GetComponent<Enemy>().fly = true;
            }

        }

        
    }

    IEnumerator delayNextAtack(float delay)
    {
        yield return new WaitForSeconds(delay);
        canAttack = true;
    }

    IEnumerator AttackDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        headbutEffect.GetComponentInParent<ParticleSystem>().Play();

        headButtCol.enabled = false;
    }
    IEnumerator time()
    {
        while(true)
        {
            if(mFStacks >0)
                timeCount();

            yield return new WaitForSeconds(1);
        }
    }

    void timeCount()
    {

        if(mFStacks == mfAtacheds.Count)
            atualHP = atualHP - (dmgOverTime * mFStacks);
    }

    public void Hit( float damage)
    {
        atualHP -= damage;
    }

    public void Heal(int amount)
    {
        atualHP += amount;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flower"))
        {
            if(atualHP != maxHP)
                Heal(collision.GetComponentInParent<Heal>().Use());
        }
    }
}
