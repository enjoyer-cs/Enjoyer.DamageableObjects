> [!NOTE]
> The basic functionality of the plugin already includes the ability to configure damage according to MER schematics,
> as well as doors. Below you can find out more about the plugin's out-of-the-box configuration options.

## Basic parameters

DamageableObjects have parameters that can be used to configure:

- Maximum health `max_health`
- Resistance to damage types that armor protects against `damage_resistance`
- Which damage sources can affect the object `allowed_damage_types`
- Multipliers for different damage types `damage_multipliers`

They are all configured in the config file, separately for each object. Below is a more detailed description of them.

### `damage_resistance`

This parameter only affects damage types that take armor into account.
In the plugin, this is implemented for those damage sources that take it into account in the game itself:

- Gunshots
- Explosions
- Damage from SCP-939

The final damage formula takes into account the DamageableObject resistance and the penetration power of the damage source.

#### Damage formula with resistance

```text
resist = damage_resistance / 100
penetration = penetration / 100
damage = baseDamage * (1f - efficacy * (1f - penetration));
```

### `allowed_damage_types`

This is simply a list of allowed damage sources; if it is empty, the filter is not applied.
It can contain values from this list: [DamageTypes](https://github.com/enjoyer-cs/Enjoyer.DamageableObjects/tree/master/.github/docs/enums/DamageTypes.md)

Example:

```yaml
allowed_damage_types:
    - Firearm
    - Scp018
```

### `damage_multipliers`

Used to configure damage multipliers for different sources. The multiplier can be a fractional number.

Example:

```yaml
damage_multipliers:
    Scp939: 3.5
    Scp096: 2.5
```

## Doors

Doors in the plugin are defined by two things, first by DoorName, and then by their zone.
[List of values](https://github.com/enjoyer-cs/Enjoyer.DamageableObjects/tree/master/.github/docs/enums/Doors.md)
The plugin affects all doors corresponding to DoorName and all doors where DoorName = None, but the zone is the same as in the config.
**The zone is checked by the door itself**, not by its location, so you can filter MER doors.

Example:

```yaml
damageable_doors:
    Entrance:
        max_health: 500...

```

### `not_affect_to_damage`

This is a door parameter that prevents the door's behavior from changing when working with the specified damage types.
They are specified here with a comma, for example: `not_affect_to_damage: Grenade, Weapon, ParticleDisruptor`.
Currently, these are all damage sources that can be excluded using this plugin.

## Schematics

To work with them, you need to install the version with the suffix `-MER`,
otherwise you will not have the `damageable_schematics` block in your config.
Schematics are filtered only by their name.

Example:

```yaml
damageable_schematics:
    Example:
        max_health: 1000...

```
