﻿using System;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct LaneOccupancy : IBufferElementData
{
    public bool Occupied;
}

// Update the track-tile occupancy after CarMovementSystem so that we have the latest car offsets.
// CarMovementSystem will use last frame's tile knowledge to avoid collisions.
[UpdateAfter(typeof(CarMovementSystem))]
public class TrackOccupancySystem : SystemBase
{
    // todo both these are readonly for now since we are not dealing with them changing in various systems
    // We would need to recompute the lane tiles, respawn cars etc.
    public readonly float TrackSize = 20;
    public readonly uint LaneCount = 4;
// todo the '64' needs to be the number of tiles we want to subdivide each lane (length of car + circumference of lane?)
    public readonly uint TilesPerLane = 64;

    unsafe void ResetBuffer(ref DynamicBuffer<LaneOccupancy> buffer)
    {
        buffer.ResizeUninitialized( (int)TilesPerLane);
        int size = UnsafeUtility.SizeOf<LaneOccupancy>();
        UnsafeUtility.MemSet(buffer.GetUnsafePtr(),
        0,
        buffer.Length * UnsafeUtility.SizeOf<bool>());
    }

    protected override void OnCreate()
    {
        for (uint i=0; i<LaneCount; i++)
        {
            var entity = EntityManager.CreateEntity(typeof(LaneOccupancy));
            EntityManager.SetName(entity, "Lane" + i);
            DynamicBuffer<LaneOccupancy> buffer = EntityManager.AddBuffer<LaneOccupancy>(entity);
            ResetBuffer(ref buffer);
        }
    }

    protected override void OnUpdate()
    {
// todo read the current 'Offset' and 'Lane' of each vehicle entitiy.
// todo based on that determine the 'tiles' that the car is in and block that tile for other cars.
// todo store this data on '4' "Lane" enitities that use a DynamicBuffer in its component?
// todo for cars NOT switching lanes, we only have to check our lane and the 'tile' in front our current tile.
// todo we can use last frames 'tile' information in "CarMovementSystem"
// todo cars that are going slower than desired (blocked) want to switch lanes and need to check the lane to the right
//      or to the left. We will alternative right and left every other frame so we don't ahve to worry about
//      two cars merging into the same lane.

        // Reset the occupancy for each lane to 0 for all tiles
        var lanes = GetEntityQuery(typeof(LaneOccupancy)).ToEntityArray(Allocator.TempJob);
        var buffers = new NativeArray<DynamicBuffer<LaneOccupancy>>(lanes.Length, Allocator.Temp);

        for(int i=0; i<lanes.Length; i++)
        {
// todo is this a copy or a reference (we need a reference)
            var buffer = EntityManager.GetBuffer<LaneOccupancy>(lanes[i]);
            buffers[i] = buffer;
            ResetBuffer(ref buffer);
        }

        uint tilesPerLane = TilesPerLane;

        Entities
            .ForEach((Entity vehicle, ref CarMovement movement) =>
            {
                float trackPos = movement.Offset;
                int myTile = (int) (trackPos * tilesPerLane);

                var buffer = buffers[(int)movement.Lane];
                var occ = buffer[myTile];
                occ.Occupied = true;
                buffer[myTile] = occ;
                buffers[(int)movement.Lane] = buffer;

            })
                .WithDisposeOnCompletion(lanes)
                .WithDisposeOnCompletion(buffers)
                .ScheduleParallel();

/*
var buffer = EntityManager.GetBuffer<ReferencedBiomes>(layer);
foreach (var element in buffer)
{
    var biomeID = element.BiomeID;
    if (!biomeIds.Contains(biomeID) &&
        m_BiomeIdToEntityAndContentHash.TryGetValue(biomeID, out var entityAndContentHash))
    {
        biomeIds.Add(biomeID);
        biomeEntities.Add(new EntityAndHash { Entity = entityAndContentHash.Entity, BiomeID = biomeID });
    }
}
*/
    }
}