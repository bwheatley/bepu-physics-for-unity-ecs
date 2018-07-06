using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.BEPUphysics.Components;

namespace Unity.BEPUphysics.Jobs
{
    public struct SpaceMembershipJob : IJob
    {
        public NativeArray<Entity> Entities;
        [ReadOnly]
        public NativeArray<byte> Adds;
        public EntityCommandBuffer CommandBuffer;

        public void Execute()
        {
            for (int index = 0; index < Entities.Length; index++)
            {
                var e = Entities[index];
                if (Adds[index] == 1)
                {
                    CommandBuffer.AddSharedComponent(e, new BEPUPhysicsObject { });
                }
                else
                {
                    CommandBuffer.RemoveComponent<BEPUPhysicsObject>(e);
                }
            }
        }
    }
}