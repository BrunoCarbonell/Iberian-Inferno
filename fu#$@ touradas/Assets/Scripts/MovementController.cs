using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{

    Rigidbody2D rb;
    public int speed;
    public float speedMultipl;

    bool btnPressed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float targetSpeed = speed * speedMultipl;

        rb.velocity = new Vector2 (targetSpeed, rb.velocity.y);
    }
   

    public void Move(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            btnPressed = true;
            speedMultipl = 1;
        }else if (value.canceled)
        {
            btnPressed = false;
            speedMultipl = 0;
        }
    }
}
