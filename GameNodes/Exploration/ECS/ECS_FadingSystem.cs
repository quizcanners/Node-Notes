using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
 
namespace NodeNotes_Visual { 

    [ExecuteInEditMode]
public class ECS_FadingSystem : JobComponentSystem {

        [BurstCompile]
        struct MovementJob : IJobProcessComponentData<Position, Rotation, ExplorationLerpData> {
            public float deltaTime;
            public void Execute(ref Position pos, ref Rotation rot, ref ExplorationLerpData dta) {
                dta.Value += deltaTime;

              //  Debug.Log("Executing");

            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
          //  Debug.Log("On Update");

            MovementJob moveJob = new MovementJob
            {
                deltaTime = Time.deltaTime
            };

            JobHandle moveHandle = moveJob.Schedule(this, inputDeps);

            return moveHandle;
        }
    }
}