using UnityEngine;

public static class MeshGenerator
{
    public static Mesh GenerateTerrainMesh(float[,] heightMap, float heightMultiplier)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        Vector2[] uv = new Vector2[vertices.Length];

        int triIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;

                float h = heightMap[x, y] * heightMultiplier;
                vertices[i] = new Vector3(x, h, y);

                uv[i] = new Vector2((float)x / width, (float)y / height);

                if (x < width - 1 && y < height - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + width;
                    triangles[triIndex + 2] = i + 1;

                    triangles[triIndex + 3] = i + 1;
                    triangles[triIndex + 4] = i + width;
                    triangles[triIndex + 5] = i + width + 1;

                    triIndex += 6;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }
}
