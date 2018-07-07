using Unity.BEPUphysics.Jobs;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Unity.BEPUphysics
{
    public class PhysicsSystem : JobComponentSystem
    {
        [Inject] private MembershipDebugGroup _membershipDebugGroup;
        [Inject] private PhysicsPositionGroup _physicsPositionGroup;

        protected override void OnCreateManager(int capacity)
        {
            _entitiesWithMembershipChanges = new NativeList<Entity>(Allocator.Persistent);
            _requestedMembershipChanges = new NativeList<byte>(Allocator.Persistent);
            _positionsToUpdate = new NativeList<Position>(Allocator.Persistent);
            _targetPositions = new NativeList<float3>(Allocator.Persistent);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var preTimeStepHandle = SchedulePreTimeStepJobs(inputDeps);
            var timeStepHandle = ScheduleTimeStepJobs(preTimeStepHandle);
            JobHandle.ScheduleBatchedJobs();
            return timeStepHandle;
        }

        protected override void OnDestroyManager()
        {
            _entitiesWithMembershipChanges.Dispose();
            _requestedMembershipChanges.Dispose();
            _positionsToUpdate.Dispose();
            _targetPositions.Dispose();
        }

        #region Pre TimeStep

        private JobHandle SchedulePreTimeStepJobs(JobHandle inputDeps)
        {
            return ScheduleMembershipChanges(inputDeps);
        }

        #region Membership

        [Inject] private PhysicsMembershipBarrier _physicsMembershipBarrier;
        private static NativeList<Entity> _entitiesWithMembershipChanges;
        private static NativeList<byte> _requestedMembershipChanges;

        public static void Add(Entity entity)
        {
            TryAddRequest(entity, true);
        }

        public static void Remove(Entity entity)
        {
            TryAddRequest(entity, false);
        }

        // This currently has no way to know if you've ever previously added a component
        private static void TryAddRequest(Entity entity, bool add)
        {
            byte desired = (byte)(add ? 1 : 0);
            var i = _entitiesWithMembershipChanges.IndexOf(entity);
            if (i != -1)
            {
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

        #endregion

        #region TimeStep

        private JobHandle ScheduleTimeStepJobs(JobHandle inputDeps)
        {
            return SchedulePhysicsPositionUpdateJob(inputDeps);
        }

        private JobHandle SchedulePhysicsPositionUpdateJob(JobHandle inputDeps)
        {
            var positionHandle = inputDeps;
            if (_positionsToUpdate.Length > 0)
            {
                positionHandle = new PhysicsPositionUpdateJob
                {
                    Components = new NativeArray<Position>(_positionsToUpdate, Allocator.TempJob),
                    Positions = new NativeArray<float3>(_targetPositions, Allocator.TempJob)
                }.Schedule(_positionsToUpdate.Length, 64, inputDeps);
                _positionsToUpdate.Clear();
                _targetPositions.Clear();
            }
            return positionHandle;
        }

        #region Position Update

        private static NativeList<Position> _positionsToUpdate;
        private static NativeList<float3> _targetPositions;

        #endregion

        #endregion
    }

    public class PhysicsMembershipBarrier : BarrierSystem { }
}
