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
        struct MovementJob : IJobForEach<Translation, PlayerControls_Component, Acceleration_Component> {

            public float2 movement;
            public float3 preservation;

            public void Execute(ref Translation pos, ref PlayerControls_Component dta, ref Acceleration_Component accel) {
                float3 acc = accel.value;
              
                acc = acc * preservation + acc * 0.5f;

                acc.xz += movement * dta.acceleration;
                
                float speed = math.length(acc);

                if (speed > dta.maxSpeed)
                    acc *= dta.maxSpeed / speed;

          
                accel.value = acc;

                pos.Value.xz += acc.xz;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            float x = (Input.GetKey(KeyCode.A) ? -1f : 0f) +
                      (Input.GetKey(KeyCode.D) ? 1f : 0f);

            float y = (Input.GetKey(KeyCode.W) ? 1f : 0f) +
                      (Input.GetKey(KeyCode.S) ? -1f : 0f);

            float3 preserve = new float3(Mathf.Abs(x), 0 , Mathf.Abs(y)); 
            float2 move = new float2(x*Time.deltaTime, y*Time.deltaTime);

            MovementJob moveJob = new MovementJob {
                movement = move,
                preservation = preserve,
            };
            

            return moveJob.Schedule(this, inputDeps); 
        }
    }
}
