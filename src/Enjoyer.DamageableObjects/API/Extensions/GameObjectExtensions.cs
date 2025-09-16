using Enjoyer.DamageableObjects.API.Components;
using Enjoyer.DamageableObjects.Configs.Interfaces;
using UnityEngine;

namespace Enjoyer.DamageableObjects.API.Extensions;

public static class GameObjectExtensions
{
    public static T AddDamageableComponent<T>(this GameObject gameObject, IDoProperties properties) where T : DamageableComponent
    {
        T component = gameObject.AddComponent<T>();

        component.MaxHealth = properties.MaxHealth;
        component.ProtectionEfficacy = properties.DamageResistance;
        component.AllowedDamageTypes = properties.AllowedDamageTypes;
        component.DamageMultipliers = properties.DamageMultipliers;

        return component;
    }
}