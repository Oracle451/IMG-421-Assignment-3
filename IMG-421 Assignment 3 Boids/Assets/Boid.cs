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
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Asteroid"))
        {
            Debug.Log($"Sir, a boid has hit the second {collision.collider.name}!");
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

        // ____OBSTACLE_AVOIDANCE____
        Vector3 avoidanceForce = Vector3.zero;

        // Number of rays to cast from the boid in the fan
        int rayCount = 7;
        // Total spread angle of the ray fan
        float coneAngle = 90f;
        // Maximum distance to consider an obstacle threatening
        float maxAvoidDist = bSet.obstacleAvoidDist;
        // How strongly to react to obstacles
        float avoidWeight = bSet.obstacleAvoidWeight * 1.0f;

        // Current forward direction
        Vector3 forward = vel.normalized;

        for (int i = 0; i < rayCount; i++)
        {
            // Spread rays evenly across cone angle
            float angle = -coneAngle / 2f + (coneAngle / (rayCount - 1)) * i;
            
            // Horizontal and Vertical rays around the boids axis
            Vector3 rayDirH = Quaternion.AngleAxis(angle, transform.up) * forward;
            Vector3 rayDirV = Quaternion.AngleAxis(angle, transform.right) * forward;

            // Process both directions to cover a full 3d cone
            foreach (Vector3 rayDir in new[] { rayDirH, rayDirV })
            {
                // Sphere radius to cover the boid 
                float sphereRadius = 1.5f;

                if (Physics.SphereCast(pos, sphereRadius, rayDir, out RaycastHit hit, maxAvoidDist, bSet.obstacleLayer))
                {
                    // 0 = Asteroid at max distance for detection and 1 = its at the front door
                    float urgency = 1f - (hit.distance / maxAvoidDist);
                    // Square it to have an exponential increase
                    urgency = Mathf.Pow(urgency, 2f);

                    // hit.normal pushes away from the surface, adding forward creates a glancing angle so the boid slides off rather than stopping completely
                    Vector3 avoidDir = hit.normal + forward;
                    // Add this ray's contribution (multiple rays hitting multiple asteroids will add up)
                    avoidanceForce += avoidDir.normalized * urgency * avoidWeight;

                    // Red = this ray is detecting an obstacle
                    Debug.DrawRay(pos, rayDir * hit.distance, Color.red, Time.fixedDeltaTime);
                }
                else
                {
                    // Green = this ray does not detect any obstacles
                    Debug.DrawRay(pos, rayDir * maxAvoidDist, Color.green, Time.fixedDeltaTime);
                }
            }
        }

        if (avoidanceForce != Vector3.zero)
        {
            // Add the total avoidance steering force to the velocity sum
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
