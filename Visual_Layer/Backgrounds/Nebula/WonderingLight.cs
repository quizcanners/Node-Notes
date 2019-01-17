using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;

[ExecuteInEditMode]
public class WonderingLight : MonoBehaviour {

    public float forceInterval = 0.2f;
    public float forceStrength = 1;
    public float blowbackStrength = 0.1f;
    public float toCenterForce = 0.1f;
    public float velocityLimit = 10;
    public float range = 1;
    float forceDelay;
    public Rigidbody rigidBody;
    Vector3 blowback = Vector3.zero;
    
    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            if (!rigidBody)
                rigidBody = GetComponent<Rigidbody>();
            if (!rigidBody)
                rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
        }
        blowback = Vector3.zero;
        rigidBody.velocity = Vector3.zero;
    }
    
    void Update () {

        if (rigidBody)
        {
            range = Mathf.Clamp(range, 1, 10000);

            float safeDeltaTime = Mathf.Clamp(Time.deltaTime, 0, 0.1f);

            forceDelay -= safeDeltaTime;
            
            if (forceDelay < 0)
            {
                rigidBody.AddForce(Vector3.one.GetRandomPointWithin() * forceStrength);

                forceDelay = forceInterval;
            }
            
            var centralForce = -transform.localPosition * toCenterForce;

            centralForce.Scale(new Vector3(1, 1, 1));

            centralForce *= Mathf.Pow(centralForce.magnitude/ range, 2);

            rigidBody.AddForce((-blowback + centralForce) * safeDeltaTime);
            
            var vel = rigidBody.velocity;

            vel.x *= Mathf.Abs(vel.x);
            vel.y *= Mathf.Abs(vel.y);
            vel.z *= Mathf.Abs(vel.z);

            if (vel.magnitude > velocityLimit)
                vel = vel.normalized * velocityLimit;
            
            blowback += vel * blowbackStrength * safeDeltaTime;
            
            if (blowback.magnitude > velocityLimit)
                blowback = blowback.normalized * velocityLimit;

        }
    }
}
