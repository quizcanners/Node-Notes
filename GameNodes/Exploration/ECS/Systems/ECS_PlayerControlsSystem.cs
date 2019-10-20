using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using QuizCannersUtilities;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Rendering;

namespace NodeNotes_Visual.ECS
{

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

           // if (NodeNotes_Camera.inst) {

                MovementJob moveJob = new MovementJob {position = NodeNotes_Camera.inst.transform.position};

                return moveJob.Schedule(this, inputDeps);
           // }

            //return inputDeps;
        }
    }
}
