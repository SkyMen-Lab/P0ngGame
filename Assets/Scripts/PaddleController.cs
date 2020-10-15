using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    private float _movement;

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
        
    }
    
    public void HandleClick(float mov)
    {
        _movement = mov;
        TransformSmoothly();
    }

    private void TransformSmoothly()
    {
        var position = transform.position;
        var y = position.y + _movement;
        var pos = new Vector3(position.x, y);
        position = Vector3.Lerp(position, pos, 0.5f * Time.deltaTime);
        transform.position = position;
    }
}
