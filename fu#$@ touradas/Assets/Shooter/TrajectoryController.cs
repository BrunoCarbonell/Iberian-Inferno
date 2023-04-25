using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Line Renderer variables")]
    public LineRenderer line;
    public int resolution;

    [Header("Formula Variables")]
    public Vector2 velocity;
    public float yLimit;
    private float g;

    [Header("Linecast Variables")]
    [Range(2, 30)]
    public int linecastResolution;
    public LayerMask canHit;

    private void Start()
    {
        g = Mathf.Abs(Physics2D.gravity.y);
    }

    private void Update()
    {
        StartCoroutine(RenderArc());
    }

    public Vector2 HitPosition()
    {
        var lowerTimeValue = MaxTimeY() / linecastResolution;
        for(int i = 0; i < linecastResolution; i++)
        {
            var t = lowerTimeValue * i;
            var tt = lowerTimeValue * (i + 1);

            var hit = Physics2D.Linecast(CalculateLinePoint(t), CalculateLinePoint(tt), canHit);

            if (hit)
                return hit.point;
        }
        return CalculateLinePoint(MaxTimeY());
    }


    private IEnumerator RenderArc()
    {
        line.positionCount = resolution + 1;
        line.SetPositions(CalculateLineArray());
        yield return null; 
    }

    private Vector3[] CalculateLineArray()
    {
        Vector3[] lineArray = new Vector3[resolution + 1];

        var lowestTimeValue = MaxTimeX() / resolution;

        for (int i = 0; i < lineArray.Length; i++)
        {
            var t = lowestTimeValue * i;
            lineArray[i] = CalculateLinePoint(t);
        }

        return lineArray;
    }

    private Vector3 CalculateLinePoint(float t)
    {

        float x = velocity.x * t;
        float y = (velocity.y * t) - (g * Mathf.Pow(t, 2)/2);
       return new Vector3(x + transform.position.x, y + transform.position.y);
    }


    private float MaxTimeY()
    {
        var v = velocity.y;
        var vv =  v * v;

        var t = (v + Mathf.Sqrt(vv + 2 * g * (transform.position.y - yLimit))) / g;
        return t;
    }

    private float MaxTimeX()
    {
        var x = velocity.x;
        var t =(HitPosition().x - transform.position.x) /x;
        return t;
    }
}
