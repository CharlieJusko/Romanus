using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class PhysicsModifier : MonoBehaviour
    {
        public bool modifyGravity = false;
        public float gravityFactor = 1f;

        private new Rigidbody rigidbody;


        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            ModifyGravity();
        }

        void ModifyGravity()
        {
            if (modifyGravity)
            {
                rigidbody.useGravity = false;
                rigidbody.AddForce(Physics.gravity * (rigidbody.mass * rigidbody.mass * gravityFactor));
            }
        }
    }
}
