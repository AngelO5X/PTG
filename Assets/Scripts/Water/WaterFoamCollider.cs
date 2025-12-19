using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterFoamCollider : MonoBehaviour
{
    public float foamDistance = 0.5f;  // jak blisko obiektu pojawia siê piana
    public LayerMask foamLayer;         // warstwa z obiektami, które powoduj¹ pianê
    public Material waterMaterial;

    private Mesh mesh;
    private Vector3[] vertices;
    private float[] foamMask;          // 0-1 maska piany

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        foamMask = new float[vertices.Length];
    }

    void Update()
    {
        // reset maski
        for (int i = 0; i < foamMask.Length; i++)
            foamMask[i] = 0;

        Vector3 worldPos;
        for (int i = 0; i < vertices.Length; i++)
        {
            worldPos = transform.TransformPoint(vertices[i]);
            Collider[] hits = Physics.OverlapSphere(worldPos, foamDistance, foamLayer);
            if (hits.Length > 0)
            {
                foamMask[i] = 1;  // punkt piany
            }
        }

        // przekazujemy maskê do shaderu
        // najlepiej u¿yæ Textury 1D lub ComputeBuffer, tu uproszczenie:
        // np. Shader.SetFloat("_FoamMask", average), lub w wersji zaawansowanej vertex color
        float avg = 0;
        foreach (var f in foamMask) avg += f;
        avg /= foamMask.Length;
        waterMaterial.SetFloat("_FoamMask", avg);
    }
}
