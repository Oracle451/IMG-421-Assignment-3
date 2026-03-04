using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AsteroidDeform : MonoBehaviour
{
    // Amount to deform the asteroids
    public float deformAmount = 0.5f;

    void Start()
    {
        // Get the mesh and its verticies
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // Apply perlin noise to all of the asteroids verticies
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float noise = Mathf.PerlinNoise(v.x * 2f, v.y * 2f);
            vertices[i] += v.normalized * noise * deformAmount;
        }

        // Apply the new verticies to the mesh
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc != null)
        {
            sc.center = mesh.bounds.center;
            sc.radius = mesh.bounds.extents.magnitude;
        }
    }
}
