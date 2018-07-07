using Unity.BEPUphysics.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Unity.BEPUphysics.Jobs
{
    public struct PhysicsPositionUpdateJob : IJobParallelFor
    {
        [WriteOnly] [DeallocateOnJobCompletion] public NativeArray<Position> Components;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<float3> Positions;

        public void Execute(int index)
        {
            Components[index] = new Position { Value = Positions[index] };
        }
    }

    public struct PhysicsPositionGroup
    {
        [WriteOnly] public ComponentDataArray<Position> PositionComponents;
        [ReadOnly] public SharedComponentDataArray<PhysicsObject> PhysicsObjects;
        [ReadOnly] public int Length;
    }
}
