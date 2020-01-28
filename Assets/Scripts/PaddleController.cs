using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5;
    private Rigidbody2D _rigidbody2D;
    private Vector2 move;
    public Collider2D collider2d;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();
        move = _rigidbody2D.velocity * speed;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //HandlePresses();
    }

    void Update()
    {
        HandlePresses();
        _rigidbody2D.velocity = move;
    }

    void HandlePresses()
    {
        if (gameObject.name == "Paddle Left")
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                var y = transform.position.y +  5.6f;
                var pos = new Vector3(transform.position.x, y);
                transform.position = Vector3.Lerp(transform.position, pos, 0.5f * Time.deltaTime);
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                var y = transform.position.y -  5.6f;
                var pos = new Vector3(transform.position.x, y);
                transform.position = Vector3.Lerp(transform.position, pos, 0.5f * Time.deltaTime);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W))
                move.y = 1 * speed;
            else if (Input.GetKeyDown(KeyCode.S))
                move.y = -1 * speed;
        }
        
        //
        // if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.W) ||
        //     Input.GetKeyUp(KeyCode.S))
        //     move.y = 0;
    }
}
