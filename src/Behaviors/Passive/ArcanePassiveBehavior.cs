using System;
using ArcaneEquipment.Attunement;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace ArcaneEquipment.Behaviors;

public abstract class ArcanePassiveBehavior : CollectibleBehavior
{
    protected ICoreAPI? Api;
    protected ICoreServerAPI? sApi;

    /// <summary>
    /// How often this item's effect ticks in milliseconds.
    /// Override in subclass or set via JSON attributes.
    /// </summary>
    protected virtual int TickRateMs => 2000;

    public ArcanePassiveBehavior(CollectibleObject collObj) : base(collObj) { }

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        Api = api;

        if (api.Side == EnumAppSide.Server)
        {
            sApi = api as ICoreServerAPI;
            sApi!.Event.RegisterGameTickListener(OnPassiveTick, TickRateMs);
        }
    }

    private void OnPassiveTick(float deltaTime)
    {
        if (sApi == null) return;

        foreach (IServerPlayer player in sApi.World.AllOnlinePlayers)
        {
            try
            {
                if (!IsEquippedBy(player)) continue;

                sApi.Logger.Debug("[ArcanePassive] {0} has item equipped, resolving tier.", player.PlayerName);
                AttunementData? data = ArcaneEquipmentModSystem.Attunement?.GetData(player);
                int tier = data != null
                    ? AttunementSystem.GetTierFromHours(data.AttunementHours)
                    : 1;
                sApi.Logger.Debug("[ArcanePassive] {0} tier={1}", player.PlayerName, tier);

                switch (tier)
                {
                    case 1: ApplyTier1Effect(player); break;
                    case 2: ApplyTier2Effect(player); break;
                    case 3: ApplyTier3Effect(player); break;
                }
            }
            catch (Exception ex)
            {
                sApi.Logger.Error("[ArcanePassive] Exception in tick for {0}: {1}", player.PlayerName, ex);
            }
        }
    }

    /// <summary>
    /// Checks if this collectible is currently equipped by the player.
    /// </summary>
    protected bool IsEquippedBy(IPlayer player)
    {
        IInventory? inventory = player.InventoryManager
            .GetOwnInventory(GlobalConstants.characterInvClassName);
        if (inventory == null) return false;

        foreach (ItemSlot slot in inventory)
        {
            if (slot.Itemstack?.Collectible == collObj)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Called every tick when item is equipped at Tier 1.
    /// Negative or no effects. Override in subclass.
    /// </summary>
    protected virtual void ApplyTier1Effect(IServerPlayer player) { }

    /// <summary>
    /// Called every tick when item is equipped at Tier 2.
    /// Mild positive effects. Override in subclass.
    /// </summary>
    protected virtual void ApplyTier2Effect(IServerPlayer player) { }

    /// <summary>
    /// Called every tick when item is equipped at Tier 3.
    /// Full power effects. Override in subclass.
    /// </summary>
    protected virtual void ApplyTier3Effect(IServerPlayer player) { }
}