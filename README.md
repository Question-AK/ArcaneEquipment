# Arcane Equipment

Magical wearable items for Vintage Story, inspired by D&D and Pathfinder. Each item has a passive effect that grows stronger the longer you wear it.

## Attunement

Wearing an arcane item starts attuning you to it. Time in game hours accumulates while it is equipped and decays while it is not, and after a grace period an unworn item loses its attunement entirely. Attunement has three tiers:

| Tier | Hours worn | Effect |
| --- | --- | --- |
| 1 | 0 | The item resists you: a small penalty until it accepts you |
| 2 | 72 | The item's power begins to show |
| 3 | 144 | Full power |

Attunement is tracked per player on the server and saved with the world.

## Items

- **Frost Walker Boots** (feet): freeze water around you as you walk. Tier 1 slows you slightly and freezes a 1-block radius; tier 2 removes the penalty and freezes 2 blocks; tier 3 freezes 4 blocks. The boots also keep your body temperature cool.
- **Basic Amulet** (neck): a plain wearable, the base for future amulets.

Items are available from the creative inventory. There are no crafting recipes yet.

## Requirements

Vintage Story 1.22 or later. The `port/1.22-net10` branch targets .NET 10.

## Development

Open `ArcaneEquipment.csproj` with the Vintage Story mod template tooling. New passive effects derive from `ArcanePassiveBehavior` and implement one method per tier; register them in `ArcaneEquipmentModSystem.Start`.
