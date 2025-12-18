using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float fx = x / (float)size * 2 - 1;
                float fy = y / (float)size * 2 - 1;

                float v = Mathf.Max(Mathf.Abs(fx), Mathf.Abs(fy));
                map[x, y] = Evaluate(v);
            }
        }
        return map;
    }

    static float Evaluate(float x)
    {
        float a = 3f;
        float b = 2.2f;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(b - b * x, a));
    }
}
