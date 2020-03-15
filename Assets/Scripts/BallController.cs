using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class BallController : KinematicObject
{
    #region Events

    public delegate void BallScored(GameObject zone);
    public event BallScored OnBallScoredEvent;

    #endregion

    #region other Game objects

    public GameObject spawnPoint;
    private NetworkManager _networkManager;
    public static BallController Instance;

    #endregion

    #region Constants

    private const float InitialSpeed = 1.5f;

    #endregion
    public enum StartDirection
    {
        Left = -1,
        Right = 1
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Body = GetComponent<Rigidbody2D>();
        }
        else
        {
            Destroy(this);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if ((other.gameObject.name == "Paddle Right") || (other.gameObject.name == "Paddle Left"))
        {
            Speed += 0.1f;
        }
        HandleHit(other);
    }

    private void HandleHit(Collision2D col)
    {
        float y = (transform.position.y - col.transform.position.y) / col.collider.bounds.size.y;
        if (col.gameObject.name == "Paddle Right")
        {
            Direction = new Vector2(-1, y);
        } 
        else if (col.gameObject.name == "Paddle Left")
        {
            Direction = new Vector2(1, y);
        }
        else if (col.gameObject.CompareTag("Wall"))
        {
            Direction = new Vector2(Direction.x, Direction.y * -1);
        }
        Body.velocity = Direction * Speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //ball enter the goal zone
        OnBallScoredEvent?.Invoke(other.gameObject);
        var direction = other.gameObject.name == "Right" ? StartDirection.Left : StartDirection.Right;
        ResetBall(direction);
    }

    public void ResetBall(StartDirection startDirection)
    {
        var position = spawnPoint.transform.position;
        //transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 2);
        transform.position = position;
        Direction = new Vector2((float) startDirection, 0);
        Speed = InitialSpeed;
        Body.velocity = Direction * Speed;
    }

    public void StopTheBall()
    {
        transform.position = spawnPoint.transform.position;
        Speed = 0;
        Direction = Vector2.zero;
        Body.velocity = Direction * Speed;
    }
    
    private void CalculateDirection(Collision2D collision2D)
    {
        
    }

    protected override void ComputeVelocity()
    {
        
    }
}
