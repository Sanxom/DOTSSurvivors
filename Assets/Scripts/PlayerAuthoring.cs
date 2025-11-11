using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public enum PlayerAnimationIndex : byte
{
    Movement = 0,
    Idle = 1,
    None = byte.MaxValue
}

public class PlayerAuthoring : MonoBehaviour
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
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(entity);
            AddComponent<InitializeCameraTargetTag>(entity);
            AddComponent<CameraTarget>(entity);
            AddComponent<AnimationIndexOverride>(entity);
        }
    }
}

public partial class PlayerInputSystem : SystemBase
{
    private GameInput _input;

    protected override void OnCreate()
    {
        _input = new();
        _input.Enable();
    }

    protected override void OnUpdate()
    {
        float2 currentInput = (float2)_input.Player.Move.ReadValue<Vector2>();

        foreach (RefRW<CharacterMoveDirection> direction 
            in SystemAPI.Query<RefRW<CharacterMoveDirection>>()
            .WithAll<PlayerTag>())
        {
            direction.ValueRW.Value = currentInput;
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct CameraInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitializeCameraTargetTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (CameraTargetSingleton.Instance == null) return;
        Transform cameraTargetTransform = CameraTargetSingleton.Instance.transform;

        EntityCommandBuffer ecb = new(state.WorldUpdateAllocator);

        foreach ((RefRW<CameraTarget> cameraTarget, 
            Entity entity) in 
            SystemAPI.Query<RefRW<CameraTarget>>()
            .WithAll<InitializeCameraTargetTag, 
            PlayerTag>()
            .WithEntityAccess())
        {
            cameraTarget.ValueRW.CameraTransform = cameraTargetTransform;
            ecb.RemoveComponent<InitializeCameraTargetTag>(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}

[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct MoveCameraSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach ((LocalToWorld transform, 
            CameraTarget cameraTarget) in 
            SystemAPI.Query<LocalToWorld, 
            CameraTarget>()
            .WithAll<PlayerTag>().WithNone<InitializeCameraTargetTag>())
        {
            cameraTarget.CameraTransform.Value.position = transform.Position;
        }
    }
}

[MaterialProperty("_AnimationIndex")]
public struct AnimationIndexOverride : IComponentData
{
    public float Value;
}

public struct CameraTarget : IComponentData
{
    public UnityObjectRef<Transform> CameraTransform;
}

public struct PlayerTag : IComponentData
{
    // noop tag
}

public struct InitializeCameraTargetTag : IComponentData
{
    // noop Tag
}