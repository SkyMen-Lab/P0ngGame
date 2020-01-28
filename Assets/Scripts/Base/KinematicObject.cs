using System;
using UnityEngine;

namespace Base
{
    public abstract class KinematicObject : MonoBehaviour
    {
        protected Vector2 Direction;
        protected float Speed;
        protected Rigidbody2D Body;
        protected abstract void ComputeVelocity();
        
        private void Update()
        {
            ComputeVelocity();
            NetworkManager.SendMessageToServer(transform.position.ToString());
        }

        private void Start()
        {
            Body = GetComponent<Rigidbody2D>();
        }
    }
}