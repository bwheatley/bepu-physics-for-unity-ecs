using Unity.Entities;

namespace Unity.BEPUphysics.Components
{
    /// <summary>
    /// Simply a marker that the Physics system should process this Entity
    /// </summary>
    public struct BEPUPhysicsObject : ISharedComponentData { }
}