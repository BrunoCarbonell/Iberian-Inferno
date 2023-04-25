using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum Type { FORCADO, MATADOR, CAVALEIROW }
public class Enemy : MonoBehaviour
{

    public Type type;
    public GameObject parentGO;
    public Transform[] sprites;
    public CopyAnim[] ragdoll;
    public Collider2D[] ragCol;
    public bool isDead;
    public int hp;
    private int atualHP;
    public bool fly = false;
    private Rigidbody2D rb;
    public FixedJoint2D hand, hand1;
    public float damage = 5;
    public float destroyTime;
    private bool lookingRigth = true;
    public Animator anim;
    private CapsuleCollider2D col;
    private GameManager gM;

    [Header("Forcado")]
    public float jumpForwardForce;
    public bool isHolding = false;
    public GameObject[] bodyParts;
    public bool isAttacking = false;

    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 50f;
    public float pathUpdateSeconds = 0.5f;
    public float stopDistance = 5f;

    [Header("Phisics")]
    public float speed = 200f;
    public float nextWaypointDistance = 3f;
    public float jumpNoteHeigthRequirement = 0.8f;
    public float JumpModifier = 0.3f;
    public Transform groundCheck;
    public float gCDistance = 1.5f;
    public LayerMask whatIsGround;



    [Header("Custom Behavior")]
    public bool followEnable = true;
    public bool jumpEnable = true;
    public bool directionLookEnable = true;


    private Path path;
    private int currentWaypoint = 0;
    bool isGrounded = false;
    Seeker seeker;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        atualHP = hp;
        seeker = GetComponent<Seeker>();
        col= GetComponent<CapsuleCollider2D>();
        target = GameObject.Find("HipTouro").transform;
        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
        lookingRigth = true;
        gM = GameObject.FindObjectOfType<GameManager>();

    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || (type == Type.FORCADO && isHolding))
        {
            foreach(CopyAnim o in ragdoll)
            {
                o.enabled = false;
            }

            foreach(Collider2D c in ragCol)
            {
                c.enabled = true;
            }
            followEnable = false;
        }

        if(fly)
        {
            hand.enabled = false;
            hand1.enabled = false;
            col.enabled = false;
            GetComponent<Balance>().enabled = false;
        }


        if (atualHP <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(DestroyTimer(destroyTime));
            col.enabled = false;
            GetComponent<Balance>().enabled = false;
            followEnable = false;
            isHolding = false;
            directionLookEnable = false;
            gM.enemyList.Remove(parentGO);
        }

        //var tmp = new Vector2()
        if (fly && isDead && rb.velocity.magnitude <= 0.1f)
            fly = false;
       
    }


    private void FixedUpdate()
    {

        if (IsInStopDistance())
        {
            followEnable = false;
            anim.SetTrigger("Stop");
           
        }
        else if (!isDead || (type == Type.FORCADO && !isHolding))
        {
            followEnable = true;
        }

        if (TargetInDistance() && followEnable)
        {
            PathFollow();

        }

        if (IsInStopDistance())
        {
            if (!isHolding && !fly)
            {
                var tmp = new Vector2(rb.velocity.y, 0);
                rb.velocity = tmp;
                
            }
               
            if(!isAttacking && !isHolding && !isDead)
                Attack();
        }

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));

    }

    public void Hit()
    {
        atualHP--;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MF") && isDead && fly)
        {
            if (collision.GetComponent<Enemy>())
            {
                collision.GetComponent<Enemy>().Hit();
                fly = false;
            }
            
        }

        if(collision.CompareTag("Player") && collision.GetComponent<BullController>() && !fly && !isDead)
        {
            col.enabled = false;
            var tmp = target.GetComponent<BullController>().FreeHoldingSpots();
            target  .GetComponent<BullController>().mFStacks++;
            target.GetComponent<BullController>().dmgOverTime = damage;
            hand.enabled = true;
            hand1.enabled = true;
            hand.connectedBody = tmp.GetComponent<Rigidbody2D>();
            hand1.connectedBody = tmp.GetComponent<Rigidbody2D>();
            isHolding = true;
            anim.enabled = false;
            foreach(GameObject go in bodyParts)
            {
                go.layer = 8;
            }
            StopCoroutine(FinishAttack(0f));

        }


        if(collision.CompareTag("Headbutt"))
            Hit();
    }

    IEnumerator DestroyTimer(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(parentGO);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }


    private void UpdatePath()
    {
        if(followEnable && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    private void PathFollow()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
            return;


        //see if is colliding with anything
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, gCDistance, whatIsGround);

        //Calculate direction
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        
            
        //Jump
        if(jumpEnable && isGrounded)
        {
            if(direction.y > jumpNoteHeigthRequirement)
            {
                rb.AddForce(Vector2.up * speed * JumpModifier);
            }
        }

        //Movement
        
        rb.AddForce(force);

        

        //Next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
            currentWaypoint++;

        if(directionLookEnable)
        {
            if (rb.velocity.x > 0.05f && !lookingRigth)
                Flip();
            else if (rb.velocity.x < -0.05f && lookingRigth)
                Flip();
        }
    }

    public void Flip()
    {

        foreach (Transform t in sprites)
        {
            Vector3 currentScale = t.transform.localScale;
            currentScale.x *= -1;
            t.transform.localScale = currentScale;
        }
        lookingRigth = !lookingRigth;
    }

    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }


    private bool IsInStopDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < stopDistance;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void Attack()
    {
        anim.SetTrigger("Attack");
        StartCoroutine(FinishAttack(1f));
        isAttacking = true;

    }


    IEnumerator FinishAttack(float time)
    {

        yield return new WaitForSeconds(0.2f);
        if (!lookingRigth)
            rb.AddForce(Vector2.left * jumpForwardForce * speed);
        else
            rb.AddForce(Vector2.right * jumpForwardForce * speed);
        yield return new WaitForSeconds(time);
        isAttacking = false;
    }
    private void OnDrawGizmos()
    {
        
        Gizmos.DrawWireSphere(groundCheck.position, gCDistance);

    }
}
