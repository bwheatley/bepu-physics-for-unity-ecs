using Unity.Entities;
using Unity.BEPUphysics.Components;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

namespace Unity.BEPUphysics.Jobs
{
    public struct MembershipDebugJob : IJobParallelFor
    {
        [ReadOnly]
        public EntityArray EntityArray;

        public void Execute(int index)
        {
            Debug.LogFormat("Entity index: {0} version: {1}", EntityArray[index].Index, EntityArray[index].Version);
        }
    }

    public struct MembershipDebugGroup
    {
        [ReadOnly]
        public SharedComponentDataArray<BEPUPhysicsObject> PhysicsObjectArray;
        public EntityArray EntityArray;
        public int Length;
    }

}