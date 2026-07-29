using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace ArcaneEquipment.Attunement;

public class AttunementSystem : ModSystem
{
    private ICoreServerAPI? sapi;

    // Stores attunement data per player UID
    private Dictionary<string, AttunementData> playerData = new();
    private double lastTickTotalHours = -1.0;

    // In-game hours before decay begins (3 in-game months)
    // Default VS calendar: 1 month = 9 in-game days, 1 day = 24 hours
    // 3 months = 27 days = 648 hours
    private const double DecayGraceHours = 648.0;

    // Tier thresholds in in-game hours
    // 72 hours = Tier 2, 144 hours = Tier 3
    public const float Tier2Hours = 72f;
    public const float Tier3Hours = 144f;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;

        api.Event.SaveGameLoaded += OnSaveGameLoaded;
        api.Event.GameWorldSave += OnGameWorldSave;

        // Tick every 60 seconds to update attunement
        api.Event.RegisterGameTickListener(OnAttunementTick, 60000);

        api.Logger.Notification("[ArcaneEquipment] AttunementSystem started.");
    }

    private void OnSaveGameLoaded()
    {
        byte[] data = sapi!.WorldManager.SaveGame.GetData("arcaneequipment_attunement");
        playerData = data == null
            ? new Dictionary<string, AttunementData>()
            : SerializerUtil.Deserialize<Dictionary<string, AttunementData>>(data);

        sapi.Logger.Notification($"[ArcaneEquipment] Loaded attunement data for {playerData.Count} players.");
    }

    private void OnGameWorldSave()
    {
        sapi!.WorldManager.SaveGame.StoreData(
            "arcaneequipment_attunement",
            SerializerUtil.Serialize(playerData)
        );
    }

    private void OnAttunementTick(float deltaTime)
    {
        if (sapi == null) return;

        double currentHours = sapi.World.Calendar.TotalHours;
        if (lastTickTotalHours < 0) lastTickTotalHours = currentHours;
        float elapsedHours = (float)(currentHours - lastTickTotalHours);
        lastTickTotalHours = currentHours;

        foreach (IServerPlayer player in sapi.World.AllOnlinePlayers)
        {
            string uid = player.PlayerUID;
            if (!playerData.ContainsKey(uid))
                playerData[uid] = new AttunementData();

            AttunementData data = playerData[uid];
            string? equippedArcaneItem = GetEquippedArcaneItemCode(player);

            if (equippedArcaneItem != null)
            {
                // No attuned item yet — start tracking this one
                if (data.AttunedItemCode == null)
                {
                    data.AttunedItemCode = equippedArcaneItem;
                    data.AttunementHours = 0f;
                    data.LastEquippedTotalHours = currentHours;
                    sapi.Logger.Notification($"[ArcaneEquipment] {player.PlayerName} began attuning to {equippedArcaneItem}.");
                }

                // Attuned item is equipped — accumulate hours
                if (equippedArcaneItem == data.AttunedItemCode)
                {
                    float previousHours = data.AttunementHours;
                    data.AttunementHours += elapsedHours;
                    data.LastEquippedTotalHours = currentHours;
                    CheckTierUp(player, data, previousHours);
                }
            }
            else if (data.AttunedItemCode != null)
            {
                // Attuned item not equipped — check for decay
                double hoursSinceEquipped = currentHours - data.LastEquippedTotalHours;
                if (hoursSinceEquipped > DecayGraceHours)
                {
                    // Decay at 1 hour per tick
                    data.AttunementHours = System.Math.Max(0f, data.AttunementHours - elapsedHours);
                    if (data.AttunementHours <= 0f)
                    {
                        sapi.Logger.Notification($"[ArcaneEquipment] {player.PlayerName} lost attunement to {data.AttunedItemCode}.");
                        data.AttunedItemCode = null;
                    }
                }
            }
        }
    }

    private void CheckTierUp(IServerPlayer player, AttunementData data, float previousHours)
    {
        int previousTier = GetTierFromHours(previousHours);
        int currentTier = GetTierFromHours(data.AttunementHours);

        if (currentTier > previousTier)
        {
            string message = currentTier switch
            {
                2 => "The item feels less foreign. Something stirs within it.",
                3 => "A deep resonance flows through the item. It has accepted you.",
                _ => ""
            };

            if (message != "")
                sapi!.SendMessage(player, GlobalConstants.GeneralChatGroup, message, EnumChatType.Notification);

            sapi!.Logger.Notification($"[ArcaneEquipment] {player.PlayerName} reached Tier {currentTier} with {data.AttunedItemCode}.");
        }
    }

    public static int GetTierFromHours(float hours)
    {
        if (hours >= Tier3Hours) return 3;
        if (hours >= Tier2Hours) return 2;
        return 1;
    }

    /// <summary>
    /// Returns the code of the first equipped arcane item found, or null if none.
    /// Only returns an item if it belongs to the arcaneequipment domain.
    /// </summary>
    private string? GetEquippedArcaneItemCode(IPlayer player)
    {
        IInventory? inventory = player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
        if (inventory == null) return null;

        foreach (ItemSlot slot in inventory)
        {
            string? domain = slot.Itemstack?.Collectible?.Code?.Domain;
            if (domain == "arcaneequipment")
                return slot.Itemstack!.Collectible.Code.ToString();
        }

        return null;
    }

    /// <summary>
    /// Public accessor so behavior classes can read a player's attunement data.
    /// </summary>
    public AttunementData? GetData(IPlayer player)
    {
        playerData.TryGetValue(player.PlayerUID, out AttunementData? data);
        return data;
    }
}