# DamageableObjects - 2.2.0

This plugin for LabAPI adds support for **damageable objects** in SCP: Secret Laboratory.
These objects can be either **MapEditorReborn schematics** or other GameObjects via modding,
players can damage and destroy this objects during gameplay.

#### documentation:

[RU](https://github.com/enjoyer-cs/Enjoyer.DamageableObjects/tree/master/.github/docs/ru/Configuring.md)
| [EN](https://github.com/enjoyer-cs/Enjoyer.DamageableObjects/tree/master/.github/docs/en/Configuring.md)

## Features

- **Available DamageSources**
  - Shots from firearms
  - Explosions
  - SCP-018 interactions
  - SCP-939 Claw & Lunge abilities
  - SCP-096 Attack & Charge abilities
  - Scp-049-2 Attack ability
  - SCP-3114 Slap ability
  - _[New]_ SCP-1509
  - _[New]_ Jailbird

- **Damageable ProjectMER Schematics**
  Define custom schematics that can take damage and destroy after reaching 0 HP.

- **Damageable Doors**
  Adds new ways to deal with damage doors, and allows you to customize, what damage sources can affect to a door with chosen DoorType

- **Customizable Settings**
  Configure health, damage types & Protection.

- **Modding**
  It can be used in other plugins as well, such as the shield plugin. But soon I plan to rework the code for better adaptability and versatility

## Dependencies
- Harmony
- [ProjectMER](https://github.com/Michal78900/ProjectMER) (optional)
