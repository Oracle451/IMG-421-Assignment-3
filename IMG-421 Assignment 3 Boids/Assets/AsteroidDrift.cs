using UnityEngine;

public class AsteroidDrift : MonoBehaviour
{
    // How fast the asteroid moves
    public float driftSpeed = 1f;
    // When an asteroid goes beyond this distance from origin, it wraps around to the opposite side
    public float bounds = 100f;

    // The direction in which this asteroid will drift
    private Vector3 driftDir;

    void Start()
    {
        // Choose a completely random direction in 3D space once when the asteroid is created
        driftDir = Random.onUnitSphere;
    }

    void Update()
    {
        // Move the asteroid smoothly in its chosen direction
        transform.position += driftDir * driftSpeed * Time.deltaTime;

        // If outside bounds then teleport to opposite side of sphere
        if (transform.position.magnitude > bounds)
        {
            transform.position = -transform.position.normalized * bounds;
        }
    }
}