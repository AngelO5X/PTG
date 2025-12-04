#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Worldgen.generated.h"

UCLASS()
class PTG_API UWorldgen : public UObject
{
    GENERATED_BODY()

public:

    // === Noise utils ===
    static float Lerp(float A, float B, float T);
    static float Fade(float T);
    static float Grad(int Hash, float X, float Y);
    static uint64 MixSeed(uint64 A, uint64 B);

    static float Smoothstep(float X);
    static float EdgeFalloff(int X, int Y, int W, int H, int Inner, int Margin);

    // === Perlin structure ===
    struct FPerlin
    {
        int P[512];
        FPerlin(uint64 Seed);
        float Noise(float X, float Y) const;
    };

    // === Main terrain mesh generator ===
    UFUNCTION(BlueprintCallable)
    static void GenerateTerrainMesh(int32 SizeX, int32 SizeY,
        float GridSpacing,
        float HeightScale,
        TArray<FVector>& OutVertices,
        TArray<int32>& OutTriangles,
        TArray<FVector>& OutNormals,
        TArray<FVector2D>& OutUVs);
};
