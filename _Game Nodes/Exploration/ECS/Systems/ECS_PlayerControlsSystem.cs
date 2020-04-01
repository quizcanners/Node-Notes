namespace NodeNotes_Visual.ECS
{
/*
    public class ECS_PlayerControlsSystem : JobComponentSystem
    {

        [BurstCompile]
        struct MovementJob : IJobForEach<Translation, PlayerControls_Component> {

            public float3 position;

            public void Execute(ref Translation pos, ref PlayerControls_Component dta) {

               pos.Value.xyz = position;  
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            
                MovementJob moveJob = new MovementJob {position = NodeNotes_Camera.inst ? NodeNotes_Camera.inst.transform.position : Vector3.zero};

                return moveJob.Schedule(this, inputDeps);

        }
    }*/
}
