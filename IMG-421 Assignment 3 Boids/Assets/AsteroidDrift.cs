using UnityEngine;

public class AsteroidDrift : MonoBehaviour
{
    public float driftSpeed = 1f;
    public float bounds = 100f;

    private Vector3 driftDir;

    void Start()
    {
        driftDir = Random.onUnitSphere;
    }

    void Update()
    {
        transform.position += driftDir * driftSpeed * Time.deltaTime;

        if (transform.position.magnitude > bounds)
        {
            transform.position = -transform.position.normalized * bounds;
        }
    }
}