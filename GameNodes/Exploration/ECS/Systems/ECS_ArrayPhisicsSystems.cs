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


namespace NodeNotes_Visual.ECS {
    
    [ExecuteInEditMode]
    public class ECS_WeightlessObjects : JobComponentSystem {

        [BurstCompile]
        struct MovementJob : IJobForEach<Translation, PhisicsArrayDynamic_Component> {
            public float deltaTime;
            [ReadOnly] public NativeArray<int> previousArray;

            public void Execute(ref Translation pos, ref PhisicsArrayDynamic_Component dta) {
                dta.testValue += previousArray[0] * deltaTime;
            }
        }

        public static JobHandle jh;

        protected override JobHandle OnUpdate(JobHandle inputDeps) {

            if (ECS_ObjectsToArray.enabled) {

                MovementJob moveJob = new MovementJob {
                    deltaTime = Time.deltaTime,
                    previousArray = ECS_ObjectsToArray.previousPositions,
                };

                jh = moveJob.Schedule(this, inputDeps);
            }

            return jh;
        }
    }

    [UpdateBefore(typeof(ECS_ObjectsToArray))]
    [UpdateBefore(typeof(ECS_WeightlessObjects))]
    [ExecuteInEditMode]
    public class ECS_ArrayFlipJob : JobComponentSystem {

        protected override JobHandle OnUpdate(JobHandle inputDeps) {

            if (ECS_ObjectsToArray.enabled) {

                ECS_ObjectsToArray.jh.Complete();
                ECS_WeightlessObjects.jh.Complete();
                ECS_ObjectsToArray.currentPositions.CopyTo(ECS_ObjectsToArray.previousPositions);

                ECS_ObjectsToArray.staticPositions.CopyTo(ECS_ObjectsToArray.currentPositions);
            }

            return inputDeps;
        }
    }
    
   
    [ExecuteInEditMode]
    public class ECS_ObjectsToArray : JobComponentSystem {

        #region values

        public static bool enabled;
        private bool initialized;
        const int width = 8;
        const int length = 8;
        const int height = 2;
        public const int size = width * length * height;
        public static NativeArray<int> previousPositions;
        public static NativeArray<int> currentPositions;
        public static NativeArray<int> staticPositions;
        #endregion

        [BurstCompile]
        struct MovementJob : IJobForEach<Translation, PhisicsArrayDynamic_Component> {
            public float deltaTime;
            [ReadOnly]
            public NativeArray<int> previousArray;
            [WriteOnly]
            public NativeArray<int> currentArray;

            public void Execute(ref Translation pos, ref PhisicsArrayDynamic_Component dta)
            {
                dta.testValue += deltaTime;

                currentArray[0] = previousArray[0] + 1;
            }
        }

        public static JobHandle jh;

        protected override JobHandle OnUpdate(JobHandle inputDeps) {

            if (enabled) {

                if (!initialized)
                    Initialize();

                MovementJob moveJob = new MovementJob {
                    deltaTime = Time.deltaTime,
                    previousArray = previousPositions,
                    currentArray = currentPositions
                };

                jh = moveJob.Schedule(this, inputDeps);
            }

            return jh;
        }

        protected void Initialize()
        {
            if (!initialized) {

                initialized = true;

                int size = width * length * height;

                previousPositions = new NativeArray<int>(size, Allocator.Persistent);
                currentPositions = new NativeArray<int>(size, Allocator.Persistent);
                staticPositions = new NativeArray<int>(size, Allocator.Persistent);

                initialized = true;
            }
        }
        
        protected override void OnDestroy() {
            if (initialized) {
                initialized = false;

                previousPositions.Dispose();
                currentPositions.Dispose();
                staticPositions.Dispose();
               
            }
        }
    }
    
}