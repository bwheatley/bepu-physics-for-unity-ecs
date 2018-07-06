using Unity.Entities;
using Unity.Mathematics;

namespace Unity.BEPUphysics.Components
{
    public struct AngularVelocity : IComponentData
    {
        public float3 Value;

        public AngularVelocity(float3 velocity)
        {
            Value = velocity;
        }
    }
}
