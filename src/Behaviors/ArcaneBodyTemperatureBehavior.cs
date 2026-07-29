using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace ArcaneEquipment.Behaviors;

public class ArcaneBodyTemperatureBehavior : EntityBehavior
{
    private const float CoolingTarget = 32.5f;
    private const bool DebugLogging = false; // set false before release

    private float accum = 0f;
    private const float TickInterval = 1.0f;

    public ArcaneBodyTemperatureBehavior(Entity entity) : base(entity) { }

    public override string PropertyName() => "arcanebodytemperature";

    public override void OnGameTick(float deltaTime)
    {
        if (entity.World.Side != EnumAppSide.Server) return;

        accum += deltaTime;
        if (accum < TickInterval) return;
        accum = 0f;

        if (entity is not EntityPlayer eplr) return;
        IPlayer? player = eplr.Player;
        if (player == null) return;

        bool hasCoolingItem = HasCoolingItem(player);

        if (hasCoolingItem)
        {
            var bh = entity.GetBehavior<EntityBehaviorBodyTemperature>();
            if (bh == null) return;

            if (bh.CurBodyTemperature > CoolingTarget)
            {
                bh.CurBodyTemperature = CoolingTarget;
                if (DebugLogging)
                    entity.World.Logger.Debug("[ArcaneBodyTemp] Clamped {0} bodytemp to {1:F2}", player.PlayerName, CoolingTarget);
            }
        }
    }

    private bool HasCoolingItem(IPlayer player)
    {
        var inv = player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
        if (inv == null) return false;

        foreach (ItemSlot slot in inv)
        {
            if (slot?.Itemstack?.Item?.Code?.Path == "frostwalkerboots" &&
                slot.Itemstack.Item.Code.Domain == "arcaneequipment")
                return true;
        }

        return false;
    }
}