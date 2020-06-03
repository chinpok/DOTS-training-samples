﻿using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
struct BucketBrigadeConfig : IComponentData
{
    public float TemperatureIncreaseRate;
    public float FlashpointMax;
    public float FlashpointMin;
    public int2 GridDimensions;
    public float CellSize;

    public float WaterSourceRefillRate;
    public float BucketCapacity;
    public float BucketRadius;
    public float AgentRadius;
    public float AgentSpeed;
    public int NumberOfBuckets;
    
    public int NumberOfPassersInOneDirectionPerChain;
    public int NumberOfChains;
}
