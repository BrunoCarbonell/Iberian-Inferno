using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum State { PAUSE, AIM, FORCE, MENU, NONE };

public class Shooter : MonoBehaviour
{

    public State state;
    public GameObject ragdoll;
    private Rigidbody2D torso;
    private Transform tmp_Body;
    public float force = 10;
    private PlayerBull playerInputActions;
    [SerializeField] private Vector2 direction;
    private Quaternion rotation;
    [SerializeField] private float angle;
    [SerializeField] private Transform trans;
    public bool drag = false;
    public Vector2 posMouseAim;
    public Vector2 posMouseAct;
    public Vector2 posMouseRelease;
    public float dist;

    // Start is called before the first frame update
    void Start()
    {
        trans = GetComponent<Transform>();
        tmp_Body = ragdoll.transform.Find("Body");
        torso = tmp_Body.GetComponent<Rigidbody2D>();
        playerInputActions = new PlayerBull();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Look.performed += LookAt;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            posMouseAim = posMouseAct;

        }

        if (Input.GetMouseButtonUp(0))
        {
            posMouseRelease = posMouseAct;
            GameObject bullet = Instantiate(ragdoll, transform.position, Quaternion.identity) as GameObject;
            tmp_Body = bullet.transform.Find("Body");
            torso = tmp_Body.GetComponent<Rigidbody2D>();
            if(!drag)
                torso.AddForce(transform.right * force * Time.deltaTime);
            else
            {
                
                force = 1000 * (Vector2.Distance(posMouseAim, posMouseRelease));
                torso.AddForce(transform.right * force * Time.deltaTime);
            }

        }


        

        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //if(state == State.FORCE)
        trans.rotation = rotation;
    }

    
    public void LookAt(InputAction.CallbackContext context)
    {
        

        if (context.control.name == "position")
        {
            direction = Camera.main.ScreenToWorldPoint(new Vector3(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y)) - transform.position;
            posMouseAct = Camera.main.ScreenToWorldPoint(new Vector3(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y));
        }
        else if (context.control.name == "rightStick")
        {
            if ((context.ReadValue<Vector2>().x > 0.1f || context.ReadValue<Vector2>().x < -0.1f) && (context.ReadValue<Vector2>().y > 0.1f || context.ReadValue<Vector2>().y < -0.1f))
                direction = context.ReadValue<Vector2>();
        }

        direction = direction.normalized;
    }
}
