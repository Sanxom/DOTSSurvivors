using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
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
            AddComponent<CharacterMoveDirection>(entity);
            AddComponent(entity, new CharacterMoveSpeed
            {
                Value = authoring.MoveSpeed,
            });
        }
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

public partial struct CharacterMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (transform, direction, speed) in SystemAPI.Query<RefRW<LocalTransform>, CharacterMoveDirection, CharacterMoveSpeed>())
        {
            var moveStep2D = direction.Value;
        }
    }
}