using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    public Rigidbody Ball;

    public bool ReadyToHit = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnTriggerStay(Collider other)
    {
        if (!ReadyToHit && other.tag != "Club") {
            float friction = other.material.dynamicFriction;
            Vector3 friction_force = Ball.velocity * friction * -0.2f;
            friction_force.y = friction_force.y / 2;
            Ball.AddForce(friction_force);
            Ball.AddTorque(Ball.angularVelocity * friction * -0.2f);
            if (Ball.velocity.magnitude < 0.3) {
                FreezeBall();
            }
        }
    }
    
    public void FreezeBall()
    {
        Ball.constraints = RigidbodyConstraints.FreezeAll;
        Ball.maxAngularVelocity = 0;
        ReadyToHit = true;
    }

    public void UnfreezeBall()
    {
        Ball.maxAngularVelocity = 7;
        Ball.constraints = RigidbodyConstraints.None;
        ReadyToHit = false;
    }
    public void OnTriggerEnter(Collider other)
    {

    }
}
