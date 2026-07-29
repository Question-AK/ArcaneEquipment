---
name: arcaneequipment
description: Project-specific conventions and implementation notes for the ArcaneEquipment Vintage Story mod.
---

# ArcaneEquipment Project Skill

## Attunement Tier System
- Tier thresholds are based on cumulative equipped time:
  - Tier 1 at 0 in-game hours
  - Tier 2 at 72 in-game hours
  - Tier 3 at 144 in-game hours
- Attunement decays after 3 in-game months unequipped.

## Frost Walker Boots Behavior
Implement per-tier effects:
- Tier 1: walk speed penalty (no meaningful freeze radius)
- Tier 2: small freeze radius
- Tier 3: full freeze radius

## Tick Handler Pattern
- Use `RegisterGameTickListener` in a `ModSystem` for periodic logic.
- Do **not** use `Event.Timer` for this mod’s gameplay loops.

## Inventory Polling Pattern
- Use `characterInvClassName` checks for passive equipment detection.

## Known Wiki Gaps
Treat these as known gaps to verify carefully in context:
- `CharacterSystem.HasTrait`
- `EnumRenderStage.Ortho`

## Wearable Items — Required Fields

Despite `ItemWearable` being deprecated in VS 1.21 in favour of the `Wearable` collectible
behavior, `"class": "ItemWearable"` must still be present as a top-level field in the item
JSON. The two serve different purposes and both are required:

- `"class": "ItemWearable"` — drives GUI thumbnail rendering in inventory and gear slots.
  Without it the slot shows a white box regardless of shape/texture configuration.
- `{ "name": "Wearable" }` in the behaviors array — drives stat modifiers (walkspeed,
  warmth, protection) and the wearable stats pipeline.

Minimum working wearable item JSON structure:
```json
{
    "code": "youritem",
    "class": "ItemWearable",
    "storageFlags": 128,
    "shape": { "base": "yourmod:yourshape" },
    "textures": {
        "seraph": { "base": "game:block/transparent" },
        "yourtexturekey": { "base": "yourmod:path/to/texture" }
    },
    "guiTransform": { ... },
    "attributes": {
        "clothescategory": "foot",
        "wearableAttachment": true
    },
    "behaviors": [
        { "name": "Wearable" }
    ]
}
```

Shape texture size must match `textureWidth`/`textureHeight` declared in the shape JSON.
If the shape declares `16x16`, the texture file must be 16x16.

Deploy always uses full assets xcopy — individual file xcopy has proven unreliable:
```shell
xcopy /E /Y /I "C:\Users\Kjol\Documents\02 Dev Projects\Vintage Story Projects\VSProjects\Mods\ArcaneEquipment\assets" "%APPDATA%\VintagestoryData\Mods\ArcaneEquipment\assets"
```