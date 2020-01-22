using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class BallController : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5;
    private Rigidbody2D _rigidbody2D;
    private Vector2 move;
    private Text RightScoreField, LeftScoreField;
    private int RightTeamScore = 0;
    private int LeftTeamScore = 0;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        RightScoreField = GameObject.Find("RightSideScore").GetComponent<Text>();
        LeftScoreField = GameObject.Find("LeftSideScore").GetComponent<Text>();
        move = _rigidbody2D.velocity * speed;
        move.x = -1 * speed;
        _rigidbody2D.velocity = move;
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
            move = new Vector2(1, y).normalized;
        } 
        if (col.gameObject.name == "Paddle Left")
        {
            move = new Vector2(-1, y).normalized;
        }
        _rigidbody2D.velocity = move * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("GoalLine")) return;

        if (other.gameObject.name == "Right") LeftTeamScore++;
        else if (other.gameObject.name == "Left") RightTeamScore++;

        gameObject.SetActive(false);
        //Thread.Sleep(300);
        UpdateScore();
        var spawnPoint = GameObject.Find("SpawnPoint");

        transform.position = spawnPoint.transform.position;
        gameObject.SetActive(true);
        move = Vector2.right;
        speed = 5;
        _rigidbody2D.velocity = move * speed;
    }

    private void UpdateScore()
    {
        RightScoreField.text = RightTeamScore.ToString();
        LeftScoreField.text = LeftTeamScore.ToString();
    }

    private void CalculateDirection(Collision2D collision2D)
    {
        
    }
}
