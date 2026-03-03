using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Neighborhood neighborhood;
    private Rigidbody rigid;
 
    // Use this for initialization
    void Awake ()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();
 
        // Set a random initial velocity
        vel = Random.onUnitSphere * Spawner.SETTINGS.velocity;
 
        Colorize();
        LookAhead();
    }

    // Change this function in your Boid script:
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Asteroid"))
        {
            Debug.Log($"Boid {gameObject.name} phased through asteroid {other.name}!");
        }
    }

    // FixedUpdate is called once per physics update (i.e., 50x/second)
    void FixedUpdate () 
    {
        BoidSettings bSet = Spawner.SETTINGS;
        
        // sumVel sums all the influences acting to change the Boid’s direction
        Vector3 sumVel = Vector3.zero;
 
        // ____ATTRACTOR____ - Move towards or away from the Attractor
        Vector3 delta = Attractor.POS - pos;
        // Check whether we’re attracted or avoiding the Attractor
        if (delta.magnitude > bSet.attractPushDist) {    
            sumVel += delta.normalized * bSet.attractPull;
        } else {
            sumVel -= delta.normalized * bSet.attractPush;
        }

        // ____OBSTACLE_AVOIDANCE____ (Smoothed Fan-Ray)
        Vector3 avoidanceForce = Vector3.zero;

        int rayCount = 7;
        float coneAngle = 90f;
        float maxAvoidDist = bSet.obstacleAvoidDist;
        float avoidWeight = bSet.obstacleAvoidWeight * 1.0f;

        Vector3 forward = vel.normalized;

        for (int i = 0; i < rayCount; i++)
        {
            // Note: Using transform.up instead of Vector3.up keeps the fan relative to the Boid's tilt
            float angle = -coneAngle / 2f + (coneAngle / (rayCount - 1)) * i;
            Vector3 rayDir = Quaternion.AngleAxis(angle, transform.up) * forward;

            if (Physics.Raycast(pos, rayDir, out RaycastHit hit, maxAvoidDist, bSet.obstacleLayer))
            {
                // Smooth exponential urgency
                float urgency = 1f - (hit.distance / maxAvoidDist);
                urgency = Mathf.Pow(urgency, 2f);

                // SMOOTH STEER: Instead of complex cross products, just push away from the surface normal
                // Adding the 'forward' vector ensures it glances off the rock instead of stopping
                Vector3 avoidDir = hit.normal + forward;

                // Add to our total avoidance force, keeping the urgency scale!
                avoidanceForce += avoidDir.normalized * urgency * avoidWeight;
            }
        }

        // Do NOT normalize avoidanceForce here. Let the 'urgency' scale dictate its size.
        if (avoidanceForce != Vector3.zero)
        {
            sumVel += avoidanceForce;
        }

        // ____COLLISION_AVOIDANCE____ – Avoid neighbors who are too near
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooNearPos = neighborhood.avgNearPos;
        // If the response is Vector3.zero, then no need to react
        if (tooNearPos != Vector3.zero) {
            velAvoid = pos - tooNearPos;
            velAvoid.Normalize();
            sumVel += velAvoid * bSet.nearAvoid;
        }
 
        // ____VELOCITY_MATCHING____ – Try to match velocity with neighbors
        Vector3 velAlign = neighborhood.avgVel;
        // Only do more if the velAlign is not Vector3.zero
        if (velAlign != Vector3.zero) {

        // In Code Listing 27.8, we’ll add additional influences to sumVel here

            // We’re really interested in direction, so normalize the velocity
            velAlign.Normalize();
            // and then set it to the speed we chose
            sumVel += velAlign * bSet.velMatching;
        }

        // ____FLOCK_CENTERING____ – Move towards the center of local neighbors
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero) {
            velCenter -= transform.position;
            velCenter.Normalize();
            sumVel += velCenter * bSet.flockCentering;
        }
 
        // ____INTERPOLATE VELOCITY____ - Between normalized vel & sumVel
        sumVel.Normalize();
        vel = Vector3.Lerp(vel.normalized, sumVel, bSet.velocityEasing);
        // Set the magnitude of vel to the velocity set on Spawner.SETTINGS
        vel *= bSet.velocity;
       
        // Look in the direction of the new velocity
        LookAhead();
    }
 
    // Orients the Boid to look at the direction it’s flying
    void LookAhead()
    {
        transform.LookAt(pos + rigid.velocity);
    }
 
    // Give the Boid a random color, but make sure it’s not too dark
    void Colorize()
    {
        // Gray metallic look (adjust gray value as desired)
        Color baseColor = new Color(0.6f, 0.6f, 0.6f);  // Medium gray — try 0.4f–0.8f range
        float metallic   = 0.8f;
        float smoothness = 0.6f;

        MeshRenderer[] meshRends = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer r in meshRends)
        {
            Material mat = r.material;

            // Set base color (tint/albedo)
            mat.color = baseColor;

            // Metallic & smoothness
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
        }

        // Trail gradient
        TrailRenderer trend = GetComponent<TrailRenderer>();

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.yellow, 0.0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
                new GradientColorKey(Color.red, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );

        trend.colorGradient = gradient;
        trend.endWidth = 0f;
    }
 
    // Property used to easily get and set the position of this Boid
    public Vector3 pos 
    {
        get { return transform.position; }
        private set { transform.position = value; }

    }
     
     // Property used to easily get and set the velocity of this Boid
    public Vector3 vel 
    {
        get { return rigid.velocity; }
        private set { rigid.velocity = value; }
    }
}
