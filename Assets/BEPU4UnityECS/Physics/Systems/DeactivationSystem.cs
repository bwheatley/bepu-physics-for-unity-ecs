using Unity.Entities;
using Unity.Jobs;

public class DeactivationSystem : JobComponentSystem
{


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return base.OnUpdate(inputDeps);
    }
}
