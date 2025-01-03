using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Backmen.CameraFollow.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Backmen.CameraFollow.Components;
/// <summary>
/// Component to set eye offset to make camera follow player's mouse
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CameraFollowComponent : Component
{
    // TODO: Probably need to remove max distance cause it doesn't influence on max distance lol
    [DataField, AutoNetworkedField]
    public Vector2 MaxDistance = new(0.0001f, 0.0001f);

    [DataField, AutoNetworkedField]
    public Vector2 DefaultMaxDistance = new(0.0001f, 0.0001f);

    [DataField, AutoNetworkedField]
    public Vector2 Offset;

    [DataField("lerpTime")]
    public float LerpTime = 1f;

    // Max distance controller btw
    // I think its broken cause it looks weird
    [DataField("backStrength")]
    public float BackStrength = 10f;

    [DataField("defaultBackStrength")]
    public float DefaultBackStrength = 10f;

    [DataField("action")]
    public EntProtoId<InstantActionComponent> Action = "ActionToggleCamera";

    // Action entity to remove it from player on component remove
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public bool Enabled = false;

}
