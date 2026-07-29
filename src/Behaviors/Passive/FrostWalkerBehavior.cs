using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using System;

namespace ArcaneEquipment.Behaviors;

public class FrostWalkerBehavior : ArcanePassiveBehavior
{
    // Tick every 2 seconds
    protected override int TickRateMs => 2000;

    // Freeze radius per tier (circular, in blocks)
    private const int Tier1Radius = 1;
    private const int Tier2Radius = 2;
    private const int Tier3Radius = 4;

    private const bool DebugLogging = false; // set false before release

    public FrostWalkerBehavior(CollectibleObject collObj) : base(collObj) { }

    /// <summary>
    /// Tier 1 — boots feel wrong. Minor movement penalty.
    /// </summary>
    protected override void ApplyTier1Effect(IServerPlayer player)
    {
        // Slight walk speed penalty to represent discomfort
        player.Entity.Stats.Set("walkspeed", "frostwalker_penalty", -0.1f, true);

        FreezeSurroundingWater(player, Tier1Radius);

        if (DebugLogging && sApi != null)
        {
            sApi.Logger.Debug("[FrostWalker] Tier=1 walkspeed_stat={0:F3} bodytemp_write_removed=true",
                player.Entity.Stats.GetBlended("walkspeed"));
        }
    }

    /// <summary>
    /// Tier 2 — boots beginning to attune. Small freeze radius.
    /// Remove walk speed penalty.
    /// </summary>
    protected override void ApplyTier2Effect(IServerPlayer player)
    {
        // Remove tier 1 penalty
        player.Entity.Stats.Remove("walkspeed", "frostwalker_penalty");
        FreezeSurroundingWater(player, Tier2Radius);
    }

    /// <summary>
    /// Tier 3 — full power. Large freeze radius.
    /// </summary>
    protected override void ApplyTier3Effect(IServerPlayer player)
    {
        player.Entity.Stats.Remove("walkspeed", "frostwalker_penalty");
        FreezeSurroundingWater(player, Tier3Radius);
    }

    private void FreezeSurroundingWater(IServerPlayer player, int radius)
    {
        if (sApi == null) return;

        Block? iceBlock = sApi.World.GetBlock(new AssetLocation("game:lakeice"));
        if (iceBlock == null || iceBlock.BlockId == 0)
        {
            sApi.Logger.Warning("[ArcaneEquipment] FrostWalker: could not resolve game:lakeice.");
            return;
        }

        BlockPos feetPos = player.Entity.ServerPos.AsBlockPos;
        int radiusSq = radius * radius;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dz = -radius; dz <= radius; dz++)
            {
                if ((dx * dx) + (dz * dz) > radiusSq) continue;

                for (int dy = -2; dy <= 2; dy++)
                {
                    BlockPos pos = new BlockPos(
                        feetPos.X + dx,
                        feetPos.Y + dy,
                        feetPos.Z + dz
                    );
                    if (IsFreezableWater(sApi.World, pos))
                        sApi.World.BlockAccessor.SetBlock(iceBlock.BlockId, pos);
                }
            }
        }
    }

    private bool IsFreezableWater(IWorldAccessor world, BlockPos pos)
    {
        Block block = world.BlockAccessor.GetBlock(pos);
        string? path = block.Code?.Path;
        if (block.Code?.Domain != "game") return false;
        if (path == null || !path.StartsWith("water-still", System.StringComparison.Ordinal)) return false;
        if (!path.EndsWith("-7", System.StringComparison.Ordinal)) return false;
        BlockPos abovePos = pos.UpCopy();
        Block aboveBlock = world.BlockAccessor.GetBlock(abovePos);
        return aboveBlock.BlockId == 0;
    }

}