using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class CharacterAuthoring : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    [field: SerializeField] public float MoveSpeed { get; private set; }
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion

    private class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<InitializeCharacterFlag>(entity);
            AddComponent<CharacterMoveDirection>(entity);
            AddComponent(entity, new CharacterMoveSpeed
            {
                Value = authoring.MoveSpeed,
            });
            AddComponent(entity, new FacingDirectionOverride
            {
                Value = 1f
            });
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct CharacterInitializationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<PhysicsMass> mass, 
            EnabledRefRW<InitializeCharacterFlag> shouldInitialize) in 
            SystemAPI.Query<RefRW<PhysicsMass>, 
            EnabledRefRW<InitializeCharacterFlag>>())
        {
            mass.ValueRW.InverseInertia = float3.zero;
            shouldInitialize.ValueRW = false;
        }
    }
}

public partial struct CharacterMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<PhysicsVelocity> velocity,
            RefRW<FacingDirectionOverride> facingDirection,
            CharacterMoveDirection direction, 
            CharacterMoveSpeed speed,
            Entity entity) in 
            SystemAPI.Query<RefRW<PhysicsVelocity>,
            RefRW<FacingDirectionOverride>,
            CharacterMoveDirection, 
            CharacterMoveSpeed>().WithEntityAccess())
        {
            float2 moveStep2D = direction.Value * speed.Value;
            velocity.ValueRW.Linear = new float3(moveStep2D, 0f);

            if (math.abs(moveStep2D.x) > 0.15f)
            {
                facingDirection.ValueRW.Value = math.sign(moveStep2D.x);
            }

            if (SystemAPI.HasComponent<PlayerTag>(entity))
            {
                RefRW<AnimationIndexOverride> animationOverride = SystemAPI.GetComponentRW<AnimationIndexOverride>(entity);
                var animationType = math.lengthsq(moveStep2D) > float.Epsilon ? PlayerAnimationIndex.Movement : PlayerAnimationIndex.Idle;
                animationOverride.ValueRW.Value = (float)animationType;
            }
        }
    }
}

public partial struct GlobalTimeUpdateSystem : ISystem
{
    private static int _globalTimeShaderPropertyID;

    public void OnCreate(ref SystemState state)
    {
        _globalTimeShaderPropertyID = Shader.PropertyToID("_GlobalTime");
    }

    public void OnUpdate(ref SystemState state)
    {
        Shader.SetGlobalFloat(_globalTimeShaderPropertyID, (float)SystemAPI.Time.ElapsedTime);
    }
}

public struct CharacterMoveDirection : IComponentData
{
    public float2 Value;
}

public struct CharacterMoveSpeed : IComponentData
{
    public float Value;
}

[MaterialProperty("_FacingDirection")]
public struct FacingDirectionOverride : IComponentData
{
    public float Value;
}

public struct InitializeCharacterFlag : IComponentData, IEnableableComponent
{
    // noop Flag
}