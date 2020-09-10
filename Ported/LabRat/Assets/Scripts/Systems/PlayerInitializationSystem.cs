﻿using System;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

public class PlayerInitializationSystem : SystemBase
{
    private EntityCommandBufferSystem ECBSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        ECBSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ticks = DateTime.Now.Ticks;
        var ecb = ECBSystem.CreateCommandBuffer().AsParallelWriter();
        Entities.ForEach((int entityInQueryIndex, in Entity playerInitializationEntity, in PlayerInitialization playerInitialization) =>
        {
            var playerCount = playerInitialization.PlayerCount;
            for (int i = 0; i < playerInitialization.PlayerCount; i++)
            {
                var playerEntity = ecb.Instantiate(entityInQueryIndex, playerInitialization.PlayerPrefab);
                if (i == 0 && !playerInitialization.AIOnly)
                {
                    ecb.AddComponent<HumanPlayerTag>(entityInQueryIndex, playerEntity);

                    var playerArrowPreviewEntity = ecb.Instantiate(entityInQueryIndex, playerInitialization.HumanPlayerArrowPreview);
                    ecb.AddComponent<HumanPlayerTag>(entityInQueryIndex, playerArrowPreviewEntity);

                    ecb.SetComponent(entityInQueryIndex, playerEntity, new Name { Value = "You" });
                }
                else
                {
                    ecb.AddComponent(entityInQueryIndex, playerEntity, new AIPlayerLastDecision { Value = ticks });
                    ecb.SetComponent(entityInQueryIndex, playerEntity, new Name { Value = $"Computer {i}" });
                }
                ecb.SetComponent(entityInQueryIndex, playerEntity, new ColorAuthoring() { Color = UnityEngine.Color.HSVToRGB(i / (float)playerCount, 1, 1) });
            }
        
            ecb.AddComponent<Disabled>(entityInQueryIndex, playerInitializationEntity);
        }).ScheduleParallel();
        ECBSystem.AddJobHandleForProducer(Dependency);
    }
}