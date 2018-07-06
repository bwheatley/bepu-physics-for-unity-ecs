using Unity.Entities;
using Unity.Mathematics;

namespace Unity.BEPUphysics.Components
{
    public struct LinearVelocity : IComponentData
    {
        public float3 Value;

        public LinearVelocity(float3 velocity)
        {
            Value = velocity;
        }
    }
}