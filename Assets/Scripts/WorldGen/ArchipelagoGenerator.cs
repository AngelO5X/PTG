using UnityEngine;

public class ArchipelagoGenerator : MonoBehaviour
{
    public int size = 256;
    public float noiseScale = 40f;

    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    public float heightMultiplier = 10f;

    public AnimationCurve heightCurve;
    public bool autoUpdate;

    private float[,] falloffMap;

    void Start()
    {
        falloffMap = GenerateFalloffMap(size);
        Generate();
    }

    public void Generate()
    {
        float[,] noiseMap = GenerateNoiseMap(size, size);

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.DrawMesh(noiseMap, heightMultiplier, heightCurve);
    }

    float[,] GenerateNoiseMap(int width, int height)
    {
        float[,] map = new float[width, height];

        System.Random prng = new System.Random();
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + offsetX) / noiseScale * frequency;
                    float sampleY = (y + offsetY) / noiseScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                float falloff = falloffMap[x, y];
                map[x, y] = Mathf.Clamp01((noiseHeight + 1) / 2f - falloff);
            }
        }

        return map;
    }

    // ---- FALL OFF MAP -----------------------------------------------------
    float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)size * 2 - 1;
                float ny = y / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));
                map[x, y] = Evaluate(value);
            }
        }
        return map;
    }

    float Evaluate(float x)
    {
        // Kształt wygładzania brzegu
        float a = 3;
        float b = 2.2f;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(b - b * x, a));
    }
}
