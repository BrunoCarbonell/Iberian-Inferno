using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyAnim : MonoBehaviour
{
    public Transform target;
    public float force = 500;
    private float targetRot;
    private Rigidbody2D rb2d;

    public void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        targetRot = target.eulerAngles.z;
        rb2d.MoveRotation(Mathf.LerpAngle(rb2d.rotation, targetRot, force * Time.fixedDeltaTime));
    }
}
