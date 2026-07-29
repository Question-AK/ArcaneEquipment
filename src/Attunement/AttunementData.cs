using ProtoBuf;

namespace ArcaneEquipment.Attunement;

[ProtoContract]
public class AttunementData
{
    /// <summary>
    /// The item code currently being attuned to (e.g. "arcaneequipment:frostwalkerboots").
    /// Null if no item is being tracked.
    /// </summary>
    [ProtoMember(1)]
    public string? AttunedItemCode { get; set; }

    /// <summary>
    /// Total in-game hours accumulated toward attunement.
    /// </summary>
    [ProtoMember(2)]
    public float AttunementHours { get; set; }

    /// <summary>
    /// The in-game total hours timestamp of when the item was last equipped.
    /// Used to calculate decay grace period.
    /// </summary>
    [ProtoMember(3)]
    public double LastEquippedTotalHours { get; set; }
}