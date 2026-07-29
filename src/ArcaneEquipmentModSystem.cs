using ArcaneEquipment.Behaviors;
using ArcaneEquipment.Attunement;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ArcaneEquipment;

public class ArcaneEquipmentModSystem : ModSystem
{
    public static AttunementSystem? Attunement;

    public override void Start(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass("FrostWalkerBehavior", typeof(FrostWalkerBehavior));
        api.RegisterEntityBehaviorClass("arcanebodytemperature", typeof(ArcaneBodyTemperatureBehavior));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        Attunement = api.ModLoader.GetModSystem<AttunementSystem>();
        api.Logger.Notification("[ArcaneEquipment] StartServerSide called, attunement system linked.");
    }
}
