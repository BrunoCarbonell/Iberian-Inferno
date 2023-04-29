using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum Type { FORCADO, MATADOR, CAVALEIRO }
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
    public bool lookingRigth = true;
    public Animator anim;
    private CapsuleCollider2D col;
    private GameManager gM;
    public ParticleSystem stun;
    public GameObject poofEffect;
    public ParticleSystem hiteffect;

    [Header("Forcado")]
    public float jumpForwardForce;
    public bool isHolding = false;
    public GameObject[] bodyParts;
    public bool isAttacking = false;


    [Header("Cavaleiro")]
    public int spawnPosition;
    public Transform spearSpawnPos;
    public GameObject spearPrefab;
    public float spearSpeed = 20;
    private Vector2 shootDirection;
    public float firerate = 1.5f;
    private float nextFire;
    public float delayShoot;
    private float nextAnimFire;
    public Sprite[] skins;
    public SpriteRenderer farpa;
    private Sprite currentSkin;

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
        col= GetComponent<CapsuleCollider2D>();
        target = GameObject.Find("HipTouro").transform;
        lookingRigth = true;
        gM = GameObject.FindObjectOfType<GameManager>();
        if(type == Type.FORCADO)
        {
            seeker = GetComponent<Seeker>();
            InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
        }

        if(type == Type.CAVALEIRO)
        {
            nextFire = Time.time + firerate;
            nextAnimFire = Time.time +firerate - delayShoot;
            if (farpa == null)
                farpa = spearSpawnPos.GetComponent<SpriteRenderer>();
            currentSkin = skins[Random.Range(0, skins.Length)];
        }
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

        if(type == Type.CAVALEIRO && !isDead)
        {
            if (target.position.x > transform.position.x && lookingRigth)
                Flip();
            if (target.position.x < transform.position.x && !lookingRigth)
                Flip();
        }
        

        if(fly)
        {
            hand.enabled = false;
            hand1.enabled = false;
            col.enabled = false;
            GetComponent<Balance>().enabled = false;
        }

        if(isDead && (hand.enabled == true || hand1.enabled == true))
        {
            hand.enabled = false;
            hand1.enabled = false;
        }

        if (atualHP <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(DestroyTimer(destroyTime));
            col.enabled = false;
            if(GetComponent<Balance>() != null)
                GetComponent<Balance>().enabled = false;
            followEnable = false;
            isHolding = false;
            directionLookEnable = false;
            gM.enemyList.Remove(parentGO);
            stun.Play(true);
            if (!fly)
            {
                foreach (GameObject go in bodyParts)
                {
                    go.layer = 10;
                }
            }
            
        }

        //var tmp = new Vector2()
        if (fly && isDead && rb.velocity.magnitude <= 0.1f)
        {
            fly = false;

            foreach (GameObject go in bodyParts)
            {
                go.layer = 10;
            }
        }
       
    }


    private void FixedUpdate()
    {
        if (type == Type.FORCADO)
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

                if (!isAttacking && !isHolding && !isDead)
                    Attack();
            }
        }else if(type == Type.CAVALEIRO)
        {
            CheckIfTimeToFire();
        }



        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));

    }

    public void Hit()
    {
        hiteffect.Play(true);
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

        if(collision.CompareTag("OUT OF BOUNDS"))
        {
            atualHP = 0;
        }
    }

    IEnumerator DestroyTimer(float time)
    {
        yield return new WaitForSeconds(time);
        var evaporate = Instantiate(poofEffect, transform.position, Quaternion.identity);
        Destroy(evaporate, 5);
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

    private void CheckIfTimeToFire()
    {
        
        if (Time.time >nextAnimFire && !isDead)
        {

            farpa.enabled = true;
            farpa.sprite = currentSkin;
            anim.SetTrigger("Shoot");
            nextAnimFire = Time.time + 5000;
        }

        if(Time.time > nextFire && !isDead)
        {
            Vector3 spearRot;
            spearRot.x = target.position.x - transform.position.x;
            spearRot.y = target.position.y - transform.position.y;

            float angle = (Mathf.Atan2(spearRot.y, spearRot.x) * Mathf.Rad2Deg)-90;
            var rot = Quaternion.Euler(new Vector3(0, 0, angle));

            var bullet = Instantiate(spearPrefab, spearSpawnPos.position, rot);
            bullet.GetComponent<SpearController>().player = target;
            bullet.GetComponent<SpearController>().ChangeSkin(currentSkin);
            shootDirection = (target.transform.position - spearSpawnPos.position).normalized * spearSpeed;
            bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(shootDirection.x, shootDirection.y);
            Destroy(bullet, 6f);
            nextFire = Time.time + firerate;
            nextAnimFire = Time.time + firerate - delayShoot;
            farpa.enabled = false;
            currentSkin = skins[Random.Range(0, skins.Length)];
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
