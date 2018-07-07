using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.BEPUphysics.Components;

namespace Unity.BEPUphysics.Jobs
{
    public struct PhysicsMembershipJob : IJobParallelFor
    {
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Entity> Entities;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<byte> Adds;
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(int index)
        {
            if (Adds[index] == 1)
            {
                CommandBuffer.AddSharedComponent(Entities[index], new PhysicsObject { });
            }
            else
            {
                CommandBuffer.RemoveComponent<PhysicsObject>(Entities[index]);
            }
        }
    }
}