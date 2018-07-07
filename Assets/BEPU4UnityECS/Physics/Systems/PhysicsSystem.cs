using Unity.BEPUphysics.Jobs;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Unity.BEPUphysics
{
    public class PhysicsSystem : JobComponentSystem
    {
        [Inject] private MembershipDebugGroup _membershipDebugGroup;

        protected override void OnCreateManager(int capacity)
        {
            _entitiesWithMembershipChanges = new NativeList<Entity>(Allocator.Persistent);
            _requestedMembershipChanges = new NativeList<byte>(Allocator.Persistent);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var membershipHandle = ScheduleMembershipChanges(inputDeps);
            var handle = new MembershipDebugJob
            {
                EntityArray = _membershipDebugGroup.EntityArray,
            }.Schedule(_membershipDebugGroup.Length, 64, membershipHandle);
            JobHandle.ScheduleBatchedJobs();
            return handle;
        }

        protected override void OnDestroyManager()
        {
            _entitiesWithMembershipChanges.Dispose();
            _requestedMembershipChanges.Dispose();
        }

        #region Space Membership

        [Inject] private PhysicsMembershipBarrier _physicsMembershipBarrier;
        private static NativeList<Entity> _entitiesWithMembershipChanges;
        private static NativeList<byte> _requestedMembershipChanges;

        // This currently has no way to know if you've ever called ADD on it
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
            if (_entitiesWithMembershipChanges.Contains(entity))
            {
                var i = _entitiesWithMembershipChanges.IndexOf(entity);
                var request = _requestedMembershipChanges[i];
                if (request == desired)
                    return;
                else
                {
                    _entitiesWithMembershipChanges.RemoveAtSwapBack(i);
                    _requestedMembershipChanges.RemoveAtSwapBack(i);
                }
            }
            else
            {
                _entitiesWithMembershipChanges.Add(entity);
                _requestedMembershipChanges.Add(desired);
            }
        }

        private JobHandle ScheduleMembershipChanges(JobHandle inputDeps)
        {
            JobHandle membershipHandle = inputDeps;
            if (_entitiesWithMembershipChanges.Length > 0)
            {
                EntityCommandBuffer.Concurrent buffer = _physicsMembershipBarrier.CreateCommandBuffer();
                var membershipJob = new PhysicsMembershipJob
                {
                    CommandBuffer = buffer,
                    Entities = new NativeArray<Entity>(_entitiesWithMembershipChanges, Allocator.TempJob),
                    Adds = new NativeArray<byte>(_requestedMembershipChanges, Allocator.TempJob),
                };
                membershipHandle = membershipJob.Schedule(_entitiesWithMembershipChanges.Length, 32, inputDeps);
                _entitiesWithMembershipChanges.Clear();
                _requestedMembershipChanges.Clear();
            }
            return membershipHandle;
        }

        #endregion
    }

    public class PhysicsMembershipBarrier : BarrierSystem { }
}