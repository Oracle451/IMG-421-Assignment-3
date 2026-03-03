using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AsteroidDeform : MonoBehaviour
{
    public float deformAmount = 0.5f;

    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float noise = Mathf.PerlinNoise(v.x * 2f, v.y * 2f);
            vertices[i] += v.normalized * noise * deformAmount;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
