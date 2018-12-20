using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using SharedTools_Stuff;
using Unity.Collections.LowLevel.Unsafe;

namespace NodeNotes_Visual.ECS {

    [ExecuteInEditMode]
    public class ECS_WeightlessObjects : JobComponentSystem {

        [BurstCompile]
        struct MovementJob : IJobProcessComponentData<Position, PhisicsArrayDynamic_Component> {
            public float deltaTime;
            [ReadOnly] public NativeArray<int> previousArray;

            public void Execute(ref Position pos, ref PhisicsArrayDynamic_Component dta) {
                dta.testValue += previousArray[0] * deltaTime;
            }
        }

        public static JobHandle jh;

        protected override JobHandle OnUpdate(JobHandle inputDeps) {

            MovementJob moveJob = new MovementJob {
                deltaTime = Time.deltaTime,
                previousArray = ECS_ObjectsToArray.previousPositions,
            };

            jh = moveJob.Schedule(this, inputDeps);

            return jh;
        }
    }

    [UpdateBefore(typeof(ECS_ObjectsToArray))]
    [UpdateBefore(typeof(ECS_WeightlessObjects))]
    [ExecuteInEditMode]
    public class ECS_ArrayFlipJob : JobComponentSystem {

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            
            ECS_ObjectsToArray.jh.Complete();
            ECS_WeightlessObjects.jh.Complete();
            ECS_ObjectsToArray.currentPositions.CopyTo(ECS_ObjectsToArray.previousPositions);

            ECS_ObjectsToArray.staticPositions.CopyTo(ECS_ObjectsToArray.currentPositions);
            return inputDeps;
        }
    }


 
    [ExecuteInEditMode]
    public class ECS_ObjectsToArray : JobComponentSystem {

        #region values
        const int width = 128;
        const int length = 128;
        const int height = 32;
        public const int size = width * length * height;
        public static NativeArray<int> previousPositions;
        public static NativeArray<int> currentPositions;
        public static NativeArray<int> staticPositions;
        #endregion

        [BurstCompile]
        struct MovementJob : IJobProcessComponentData<Position, PhisicsArrayDynamic_Component> {
            public float deltaTime;
            [ReadOnly]
            public NativeArray<int> previousArray;
            [WriteOnly]
            public NativeArray<int> currentArray;

            public void Execute(ref Position pos, ref PhisicsArrayDynamic_Component dta)
            {
                dta.testValue += deltaTime;

                currentArray[0] = previousArray[0] + 1;
            }
        }

        public static JobHandle jh;

        protected override JobHandle OnUpdate(JobHandle inputDeps) {

            MovementJob moveJob = new MovementJob {
                deltaTime = Time.deltaTime,
                previousArray = previousPositions,
                currentArray = currentPositions

            };

            jh  = moveJob.Schedule(this, inputDeps);

            return jh;
        }
        
        protected override void OnCreateManager() {
            int size = width * length * height;

            previousPositions = new NativeArray<int>(size, Allocator.Persistent);
            currentPositions = new NativeArray<int>(size, Allocator.Persistent);
            staticPositions = new NativeArray<int>(size, Allocator.Persistent);
            Debug.Log("Creating phisics array: {0}".F(size));

            base.OnCreateManager();
        }

        protected override void OnDestroyManager() {
            previousPositions.Dispose();
            currentPositions.Dispose();
            staticPositions.Dispose();
        }
    }

}