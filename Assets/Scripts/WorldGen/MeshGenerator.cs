using UnityEngine;

public static class MeshGenerator
{
    // Funkcja generuje mesh z "Ghost Border" (niewidzialn¹ ramk¹) dla idealnych ³¹czeñ
    public static Mesh GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, int uvOffsetX, int uvOffsetY, int worldSize)
    {
        // Mapa ma ramkê (border), wiêc jest szersza o 2 punkty ni¿ wynikowy mesh
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2;

        Vector3[] vertices = new Vector3[meshSize * meshSize];
        Vector3[] normals = new Vector3[meshSize * meshSize];
        Vector2[] uvs = new Vector2[meshSize * meshSize];
        Vector4[] tangents = new Vector4[meshSize * meshSize];
        int[] triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];

        int triIndex = 0;

        for (int y = 0; y < meshSize; y++)
        {
            for (int x = 0; x < meshSize; x++)
            {
                // Indeksy
                int vertexIndex = y * meshSize + x;
                // Przesuniêcie o +1 w mapie wysokoœci (bo pomijamy ramkê border)
                int mapX = x + 1;
                int mapY = y + 1;

                // 1. POZYCJA
                float currentHeight = heightMap[mapX, mapY] * heightMultiplier;
                vertices[vertexIndex] = new Vector3(x, currentHeight, y);

                // 2. GLOBALNE UV (Naprawia uciête tekstury na ³¹czeniach)
                // U¿ywamy pozycji w œwiecie, a nie w chunku
                float globalU = (x + uvOffsetX) / (float)worldSize;
                float globalV = (y + uvOffsetY) / (float)worldSize;
                uvs[vertexIndex] = new Vector2(globalU, globalV);

                // 3. NORMALNE (Naprawia cienie na krawêdziach)
                // Pobieramy wysokoœæ s¹siadów z ramki (których nie ma w meshu, ale s¹ w danych)
                float hL = heightMap[mapX - 1, mapY] * heightMultiplier;
                float hR = heightMap[mapX + 1, mapY] * heightMultiplier;
                float hD = heightMap[mapX, mapY - 1] * heightMultiplier;
                float hU = heightMap[mapX, mapY + 1] * heightMultiplier;

                Vector3 tangentX = new Vector3(2f, hR - hL, 0f).normalized;
                Vector3 tangentZ = new Vector3(0f, hU - hD, 2f).normalized;
                // Iloczyn wektorowy daje idealn¹ normaln¹, spójn¹ miêdzy chunkami
                normals[vertexIndex] = Vector3.Cross(tangentZ, tangentX);

                // 4. TANGENTY (Wymagane dla shaderów, ustawione na sztywno dla g³adkoœci)
                tangents[vertexIndex] = new Vector4(1, 0, 0, -1);

                // 5. TRÓJK¥TY
                if (x < meshSize - 1 && y < meshSize - 1)
                {
                    triangles[triIndex] = vertexIndex;
                    triangles[triIndex + 1] = vertexIndex + meshSize;
                    triangles[triIndex + 2] = vertexIndex + 1;

                    triangles[triIndex + 3] = vertexIndex + 1;
                    triangles[triIndex + 4] = vertexIndex + meshSize;
                    triangles[triIndex + 5] = vertexIndex + meshSize + 1;

                    triIndex += 6;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.tangents = tangents;

        mesh.RecalculateBounds();

        return mesh;
    }
}