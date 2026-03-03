using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public GameObject asteroidPrefab;
    public int asteroidCount = 30;
    public float spawnRadius = 50f;
    public float minAsteroidSpacing = 10f;
    public float minScale = 1f;
    public float maxScale = 5f;

    void Start()
    {
        SpawnAsteroids();
    }

    void SpawnAsteroids()
    {
        int spawned = 0;
        int attempts = 0;

        // Store placed asteroid data
        System.Collections.Generic.List<(Vector3 pos, float radius)> placedAsteroids =
            new System.Collections.Generic.List<(Vector3, float)>();

        while (spawned < asteroidCount && attempts < asteroidCount * 20)
        {
            attempts++;

            Vector3 pos;

            // Keep asteroids away from center (boid spawn zone)
            do
            {
                pos = Random.insideUnitSphere * spawnRadius;
            }
            while (pos.magnitude < 15f);

            // Random scale BEFORE spacing check
            float scale = Random.Range(minScale, maxScale);

            // Assume prefab base radius is 0.5 (Unity sphere default)
            float asteroidRadius = 0.5f * scale;

            bool validPosition = true;

            foreach (var placed in placedAsteroids)
            {
                float requiredDistance =
                    asteroidRadius + placed.radius + minAsteroidSpacing;

                if (Vector3.Distance(pos, placed.pos) < requiredDistance)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
            {
                GameObject asteroid = Instantiate(
                    asteroidPrefab,
                    pos,
                    Random.rotation
                );

                asteroid.transform.localScale = Vector3.one * scale;

                placedAsteroids.Add((pos, asteroidRadius));

                spawned++;
            }
        }
    }
}