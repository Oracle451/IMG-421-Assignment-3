using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Asteroid Settings")]
    // The prefab that will be instantiated to create each asteroid
    public GameObject asteroidPrefab;
    // Number of asteroids to spawn in the scene
    public int asteroidCount = 30;
    // Creates a spherical spawn volume with this radius
    public float spawnRadius = 50f;
    // Helps prevent asteroids from overlapping or being too close
    public float minAsteroidSpacing = 10f;
    // Minimum and maximum scale that asteroids can randomly get
    public float minScale = 1f;
    public float maxScale = 5f;

    void Start()
    {
        // When the object is created, spawn the asteroids 
        SpawnAsteroids();
    }

    void SpawnAsteroids()
    {
        // Keep track of how many asteroids we've successfully placed
        int spawned = 0;
        // Counter to prevent infinite loops if we can't find good positions
        int attempts = 0;

        // Store the position and effective radius of each placed asteroid
        System.Collections.Generic.List<(Vector3 pos, float radius)> placedAsteroids = new System.Collections.Generic.List<(Vector3, float)>();

        // Keep trying until we either place all asteroids or run out of reasonable attempts
        while (spawned < asteroidCount && attempts < asteroidCount * 20)
        {
            attempts++;

            Vector3 pos;

            // Generate random positions, but reject any that are too close to the center
            do
            {
                // Random point inside a sphere of radius spawnRadius
                pos = Random.insideUnitSphere * spawnRadius;
            }
            while (pos.magnitude < 15f);

            // Random scale applied to the asteroid
            float scale = Random.Range(minScale, maxScale);

            // Apply the scale to the asteroid
            float asteroidRadius = 0.5f * scale;

            // Assume this position is good unless we find a conflict
            bool validPosition = true;

            // Check against every previously placed asteroid
            foreach (var placed in placedAsteroids)
            {
                // Minimum distance required = radius of new + radius of old + spacing buffer
                float requiredDistance = asteroidRadius + placed.radius + minAsteroidSpacing;

                // If too close to any existing asteroid then set invalid position
                if (Vector3.Distance(pos, placed.pos) < requiredDistance)
                {
                    validPosition = false;
                    break;
                }
            }

            // If no conflicts found then its a valid spot
            if (validPosition)
            {
                // Create the asteroid in the scene
                GameObject asteroid = Instantiate(
                    asteroidPrefab,
                    pos,
                    Random.rotation
                );

                // Apply the randomly chosen size
                asteroid.transform.localScale = Vector3.one * scale;

                int obstacleLayer = LayerMask.NameToLayer("Obstacle");
                asteroid.layer = obstacleLayer;
                foreach (Transform child in asteroid.GetComponentsInChildren<Transform>())
                    child.gameObject.layer = obstacleLayer;

                asteroid.tag = "Asteroid";

                // Remember this asteroid so future ones can avoid it
                placedAsteroids.Add((pos, asteroidRadius));

                // Update Spawn count
                spawned++;
            }
        }
    }
}