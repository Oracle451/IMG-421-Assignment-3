using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoidSettings 
{
    // These fields allow you to adjust the flocking behavior of the Boids
    public int velocity = 32;
    public int neighborDist = 10;
    public int nearDist = 4;
    public int attractPushDist = 5;
    
    [Header("These \"influences\" are floats, usually from [0…4]")]
    public float velMatching = 1.5f;
    public float flockCentering = 1f;
    public float nearAvoid = 2f;
    public float attractPull = 1f;
    public float attractPush = 20f; // Very strong to spread out the Boids
   
    [Header("This determines how quickly Boids can turn and is [0…1]")]
    public float velocityEasing = 0.03f;

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidDist = 10f;
    public float obstacleAvoidWeight = 5f;
    public LayerMask obstacleLayer;
}


public class Spawner : MonoBehaviour
{
    // SETTINGS and BOIDS are both static to allow easy access
    static public BoidSettings  SETTINGS;
    static public List<Boid>    BOIDS;

    // These fields allow you to adjust the spawning behavior of the Boids
    [Header("Inscribed: Settings for Spawning Boids")]
    public GameObject BoidPrefab;
    public Transform boidAnchor;   

    public int numBoids = 100;
    public float spawnRadius = 100f;
    public float spawnDelay = 0.1f;

    [Header("Inscribed: Settings for Spawning Boids")]
    public BoidSettings boidSettings;
 
    void Awake () 
    {
        // Assign the boidSettings of this instance to the static SETTINGS
        Spawner.SETTINGS = boidSettings;
        // Start instantiation of the Boids
        BOIDS = new List<Boid>();
        InstantiateBoid();
    }

    public void InstantiateBoid() 
    {
        GameObject go = Instantiate<GameObject>(BoidPrefab);
        go.transform.position = Random.insideUnitSphere * spawnRadius;
        Boid b = go.GetComponent<Boid>();
        b.transform.SetParent(boidAnchor);
        BOIDS.Add( b );
        if (BOIDS.Count < numBoids) 
        {
            Invoke( "InstantiateBoid", spawnDelay );
        }
    }
}
