using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    private float movement;

    public string TeamCode { get; set; }
    private Rigidbody2D _rigidbody2D;
    public Collider2D collider2d;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();
    }
    
    private void FixedUpdate()
    {
        TransformSmoothly();
    }
    
    public void HandleClick(float mov)
    {
        movement = mov;
    }

    private void TransformSmoothly()
    {
        var y = transform.position.y + movement;
        var pos = new Vector3(transform.position.x, y);
        transform.position = Vector3.Lerp(transform.position, pos, 0.5f * Time.deltaTime);
    }
}
