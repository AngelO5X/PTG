using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterMeshWaves : MonoBehaviour
{
    public float waveHeight = 0.2f;
    public float waveSpeed = 1f;
    public float waveScale = 0.8f;

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] vertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
        vertices = new Vector3[baseVertices.Length];
        baseVertices.CopyTo(vertices, 0);
    }

    void Update()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = baseVertices[i];
            v.y += Mathf.Sin(Time.time * waveSpeed + v.x * waveScale + v.z * waveScale) * waveHeight;
            vertices[i] = v;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
