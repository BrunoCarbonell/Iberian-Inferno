using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public float jumpForce;
    public bool haveDoublejumped;
    public LayerMask whatIsGround;
    private bool isGrounded;
    private float xDir;
    public Transform groundCheck;
    [Space(5)]
    [Range(0f, 2f)] public float raicastDistance = 1.5f;
    public GameObject jumpEffect;
    private Vector3 m_Velocity = Vector3.zero;
    private int timer = 0;

    [Header("Attacks")]
    public float explosionRadius = 5;
    public float exploxionForceMulti = 5;
    Collider2D[] inExplosionRadius = null;
    public Transform explosionCenter;
    public BoxCollider2D headButtCol;

    [Header("Status")]
    public GameObject[] holdingSpots;
    public int mFStacks;
    [Space(5)]
    [Range(0f, 100f)] public float mFSlow;
    [SerializeField] private float atualSlow;
    public float dmgOverTime = 5;
    public float maxHP;
    public float atualHP;



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
        StartCoroutine(time());
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && !facingRigth)
            Flip();
        else if (Input.GetKeyDown(KeyCode.A) && facingRigth)
            Flip();
        gM.UpdateHpBar(atualHP, maxHP);

    }

    private void FixedUpdate()
    {
        Movement();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, raicastDistance, whatIsGround);
        if(isGrounded)
            haveDoublejumped = false;

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
    }

    public void Movement()
    {
        Vector3 targetVelocity = new Vector2(xDir * atualMovForce, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, 0.05f);
    }

    public void Move(InputAction.CallbackContext context)
    {


        xDir = context.ReadValue<Vector2>().x;
        //rb.velocity = new Vector2(xDir *(movementForce * Time.deltaTime), rb.velocity.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(context.started && isGrounded)
        {
            //rb.velocity = new Vector2(rb.velocity.x, 0);
            //rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.deltaTime);
            jumpEffect.GetComponentInParent<ParticleSystem>().Play();
            //jumpEffect.SetActive(false);
            //jumpEffect.SetActive(true);
            //rb.AddForce(Vector2.up * jumpForce * movementForce);
            StartCoroutine(JumpTimer(0.2f));
            anim.SetTrigger("Jump");
        }

        if(context.started && !isGrounded && !haveDoublejumped)
        {
            //rb.velocity = new Vector2(rb.velocity.x, 0);
            //rb.AddForce(Vector2.up * jumpForce * movementForce);
            //rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.deltaTime);
            jumpEffect.GetComponentInParent<ParticleSystem>().Play();
            StartCoroutine(JumpTimer(0.2f));
            haveDoublejumped = true;
            //jumpEffect.SetActive(false);
            //jumpEffect.SetActive(true);
            anim.SetTrigger("Jump");

        }
    }
    
    public void HeadButting(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            headButtCol.enabled = true;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackDelay(0.4f));
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
            case 4:
                return holdingSpots[4];
            default:
                return null;
        }
    }

    IEnumerator ExplosionDelay(float waitTime, Rigidbody2D o_rigid, Collider2D o, Vector2 distanceVec)
    {
        yield return new WaitForSeconds(waitTime);

        if (o.GetComponent<Enemy>().isHolding)
        {
            float explosionForce = exploxionForceMulti;
            o_rigid.AddForce(distanceVec.normalized * explosionForce);
            if (o.GetComponent<Enemy>())
            {
                o.GetComponent<Enemy>().Hit();
                o.GetComponent<Enemy>().fly = true;
            }
            mFStacks = 0;
        }

        
    }

    IEnumerator JumpTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        rb.AddForce(Vector2.up * jumpForce * movementForce);


    }

    IEnumerator AttackDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
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
        atualHP = atualHP - (dmgOverTime * mFStacks);
    }

    
}
