using System;
using UnityEngine;

namespace Base
{
    public abstract class KinematicObject : MonoBehaviour
    {
        public abstract void ComputeVelocity();
        
        private void Update()
        {
            
        }

        private void Start()
        {
            throw new NotImplementedException();
        }
    }
}