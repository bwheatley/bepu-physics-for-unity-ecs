using System.Collections.Generic;
using Unity.BEPUphysics.Jobs;
using Unity.BEPUphysics.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;

namespace Unity.BEPUphysics
{
    public class PhysicsSystem : JobComponentSystem
    {
        [Inject] private SpaceMembershipBarrier _spaceMembershipBarrier;
        [Inject] private MembershipDebugGroup _membershipDebugGroup;
        public TimeStepSettings TimeStepSettings { get; set; }
        private static Dictionary<Entity, byte> _membershipRequestChanges;

        protected override void OnCreateManager(int capacity)
        {
            _membershipRequestChanges = new Dictionary<Entity, byte>();
        }

        public static void Add(Entity entity)
        {
            TryAddRequest(entity, true);
        }

        // This currently has no way to know if you've ever called ADD on it
        public static void Remove(Entity entity)
        {
            TryAddRequest(entity, false);
        }

        private static void TryAddRequest(Entity entity, bool add)
        {
            byte desired = (byte)(add ? 1 : 0);
            byte request;
            if (_membershipRequestChanges.TryGetValue(entity, out request))
            {
                if (request == desired)
                    return;
                else
                    _membershipRequestChanges.Remove(entity);
            }
            else
            {
                _membershipRequestChanges.Add(entity, desired);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var spaceMembershipHandle = RunSpaceMembershipJob(inputDeps);
            var membershipDebugHandle = new MembershipDebugJob
            {
                EntityArray = _membershipDebugGroup.EntityArray
            }.Schedule(_membershipDebugGroup.Length, 64, spaceMembershipHandle);
            JobHandle.ScheduleBatchedJobs();
            return membershipDebugHandle;
        }

        private JobHandle RunSpaceMembershipJob(JobHandle inputDeps)
        {
            JobHandle spaceMembershipHandle = inputDeps;
            if (_membershipRequestChanges.Count > 0)
            {
                var entities = _membershipRequestChanges.Keys.ToArray();
                var adds = _membershipRequestChanges.Values.ToArray();
                var spaceMembershipJob = new SpaceMembershipJob
                {
                    CommandBuffer = _spaceMembershipBarrier.CreateCommandBuffer(),
                    Entities = new NativeArray<Entity>(entities, Allocator.TempJob),
                    Adds = new NativeArray<byte>(adds, Allocator.TempJob)
                };
                spaceMembershipHandle = spaceMembershipJob.Schedule(inputDeps);
                _membershipRequestChanges.Clear();
                JobHandle.ScheduleBatchedJobs();
                spaceMembershipHandle.Complete();
                spaceMembershipJob.Entities.Dispose();
                spaceMembershipJob.Adds.Dispose();
            }
            return spaceMembershipHandle;
        }
    }

    public class SpaceMembershipBarrier : BarrierSystem { }
}