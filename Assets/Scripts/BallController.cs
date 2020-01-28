using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class BallController : KinematicObject
{
    // Start is called before the first frame update
    public float speed = 5;
    private Text RightScoreField, LeftScoreField;
    private int RightTeamScore = 0;
    private int LeftTeamScore = 0;
    void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
        RightScoreField = GameObject.Find("RightSideScore").GetComponent<Text>();
        LeftScoreField = GameObject.Find("LeftSideScore").GetComponent<Text>();
        Direction = Body.velocity * speed;
        Direction.x = -1 * speed;
        Body.velocity = Direction;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if ((other.gameObject.name == "Paddle Right") || (other.gameObject.name == "Paddle Left"))
        {
            speed += (float) 0.2;
            HandleHit(other);
        }
    }

    private void HandleHit(Collision2D col)
    {
        float y = (transform.position.y - col.transform.position.y) / col.collider.bounds.size.y;
        if (col.gameObject.name == "Paddle Right")
        {
            Direction = new Vector2(1, y).normalized;
        } 
        if (col.gameObject.name == "Paddle Left")
        {
            Direction = new Vector2(-1, y).normalized;
        }
        Body.velocity = Direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("GoalLine")) return;

        if (other.gameObject.name == "Right") LeftTeamScore++;
        else if (other.gameObject.name == "Left") RightTeamScore++;

        gameObject.SetActive(false);
        UpdateScore();
        var spawnPoint = GameObject.Find("SpawnPoint");

        transform.position = spawnPoint.transform.position;
        gameObject.SetActive(true);
        Direction = Vector2.right;
        speed = 5;
        Body.velocity = Direction * speed;
    }

    private void UpdateScore()
    {
        RightScoreField.text = RightTeamScore.ToString();
        LeftScoreField.text = LeftTeamScore.ToString();
    }

    private void CalculateDirection(Collision2D collision2D)
    {
        
    }

    protected override void ComputeVelocity()
    {
        
    }
}
