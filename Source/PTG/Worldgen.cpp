#include "Worldgen.h"
#include <random>
#include <algorithm>
#include <cmath>

// ======================================================
// UTILS
// ======================================================

float UWorldgen::Lerp(float A, float B, float T)
{
    return A + T * (B - A);
}

float UWorldgen::Fade(float T)
{
    return T * T * T * (T * (T * 6 - 15) + 10);
}

float UWorldgen::Grad(int Hash, float X, float Y)
{
    int H = Hash & 3;
    float U = (H < 2) ? X : Y;
    float V = (H < 2) ? Y : X;
    return ((H & 1) ? -U : U) + ((H & 2) ? -V : V);
}

uint64 UWorldgen::MixSeed(uint64 A, uint64 B)
{
    A ^= B + 0x9e3779b97f4a7c15ULL + (A << 6) + (A >> 2);
    A ^= (A << 13);
    A ^= (A >> 7);
    A ^= (A << 17);
    return A;
}

float UWorldgen::Smoothstep(float X)
{
    return X * X * (3 - 2 * X);
}

float UWorldgen::EdgeFalloff(int X, int Y, int W, int H, int Inner, int Margin)
{
    int InnerX1 = (W - Inner) / 2;
    int InnerY1 = (H - Inner) / 2;

    int InnerX2 = InnerX1 + Inner;
    int InnerY2 = InnerY1 + Inner;

    int DX = 0;
    if (X < InnerX1) DX = InnerX1 - X;
    else if (X > InnerX2) DX = X - InnerX2;

    int DY = 0;
    if (Y < InnerY1) DY = InnerY1 - Y;
    else if (Y > InnerY2) DY = Y - InnerY2;

    int D = FMath::Max(DX, DY);

    if (D <= 0) return 1.0f;
    if (D >= Margin) return 0.0f;

    float T = 1.0f - (float)D / Margin;
    return Smoothstep(T);
}

// ======================================================
// PERLIN
// ======================================================

UWorldgen::FPerlin::FPerlin(uint64 Seed)
{
    TArray<int> Perm;
    Perm.SetNum(256);

    for (int i = 0; i < 256; i++)
        Perm[i] = i;

    std::mt19937_64 RNG(Seed);
    std::shuffle(Perm.begin(), Perm.end(), RNG);

    for (int i = 0; i < 256; i++)
        P[i] = P[256 + i] = Perm[i];
}

float UWorldgen::FPerlin::Noise(float X, float Y) const
{
    int X0 = FMath::FloorToInt(X) & 255;
    int Y0 = FMath::FloorToInt(Y) & 255;

    float XF = X - FMath::FloorToFloat(X);
    float YF = Y - FMath::FloorToFloat(Y);

    float U = UWorldgen::Fade(XF);
    float V = UWorldgen::Fade(YF);

    int A = P[X0] + Y0;
    int B = P[X0 + 1] + Y0;

    float R1 = UWorldgen::Lerp(
        UWorldgen::Grad(P[A], XF, YF),
        UWorldgen::Grad(P[B], XF - 1, YF),
        U);

    float R2 = UWorldgen::Lerp(
        UWorldgen::Grad(P[A + 1], XF, YF - 1),
        UWorldgen::Grad(P[B + 1], XF - 1, YF - 1),
        U);

    return UWorldgen::Lerp(R1, R2, V);
}

// ======================================================
// MAIN MESH GENERATOR
// ======================================================

void UWorldgen::GenerateTerrainMesh(int32 SizeX, int32 SizeY,
    float GridSpacing,
    float HeightScale,
    TArray<FVector>& OutVertices,
    TArray<int32>& OutTriangles,
    TArray<FVector>& OutNormals,
    TArray<FVector2D>& OutUVs)
{
    OutVertices.Empty();
    OutTriangles.Empty();
    OutNormals.Empty();
    OutUVs.Empty();

    uint64 Seed = 123456;
    FPerlin Noise(Seed);

    for (int y = 0; y < SizeY; y++)
    {
        for (int x = 0; x < SizeX; x++)
        {
            float H = Noise.Noise(x * 0.05f, y * 0.05f);
            H = (H + 1.f) * 0.5f; // normalize to 0-1

            OutVertices.Add(FVector(x * GridSpacing, y * GridSpacing, H * HeightScale));
            OutUVs.Add(FVector2D((float)x / SizeX, (float)y / SizeY));
            OutNormals.Add(FVector::UpVector);
        }
    }

    for (int y = 0; y < SizeY - 1; y++)
    {
        for (int x = 0; x < SizeX - 1; x++)
        {
            int I = x + y * SizeX;

            OutTriangles.Add(I);
            OutTriangles.Add(I + SizeX);
            OutTriangles.Add(I + 1);

            OutTriangles.Add(I + 1);
            OutTriangles.Add(I + SizeX);
            OutTriangles.Add(I + SizeX + 1);
        }
    }
}
