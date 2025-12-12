using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    public void DrawMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];

        int triIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                vertices[y * width + x] = new Vector3(x, h, y);

                if (x < width - 1 && y < height - 1)
                {
                    triangles[triIndex + 0] = y * width + x;
                    triangles[triIndex + 1] = y * width + x + width;
                    triangles[triIndex + 2] = y * width + x + 1;

                    triangles[triIndex + 3] = y * width + x + 1;
                    triangles[triIndex + 4] = y * width + x + width;
                    triangles[triIndex + 5] = y * width + x + width + 1;

                    triIndex += 6;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
