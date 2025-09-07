# DamageableObjects - 2.0.0

This plugin for LabAPI adds support for **damageable objects** in SCP: Secret Laboratory.
These objects can be either **MapEditorReborn schematics** or other GameObjects via modding,
players can damage and destroy this objects during gameplay.

> [!WARNING]
> **Plugin in beta**<br />
> It may not work properly, if you encounter any problems, please post it in Issues

## Features

- **Availble DamageSources**
  - Shots from firearms
  - Explosions
  - SCP-018 interactions
  - SCP-939 Lunge abilities
  - SCP-096 Charge ability
  - Inprogress:
    - SCP-939 Claw & SCP-096 Attack (LMB)
    - SCP-049-2 Attack ability
    - Micro HID

- **Damageable MapEditorReborn Schematics**
  Define custom schematics that can take damage and destroy after reaching 0 HP.

- **Damageable Doors** `(will be implemented in other plugin)`
  ~~Adds new ways, to deal damage doors, and allows you to customize, what damage sources can affect to a door with choosen DoorType~~

- **Customizable Settings**
  Configure health, damage types & Protection.

- **Moding**
  It can be used in other plugins as well, such as the shield plugin. But soon I plan to rework the code for better adaptability and versatility

## Dependencies
- Harmony
- [ProjectMER](https://github.com/Michal78900/ProjectMER) (optional)

