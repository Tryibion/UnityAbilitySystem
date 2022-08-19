using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectCustomCalculation : MonoBehaviour
{
    public struct OwnerInfo
    {
        /// <summary>
        /// The owning game object
        /// </summary>
        public GameObject owningGameObject;

        /// <summary>
        /// The owning ability component
        /// </summary>
        public AbilityComponent owningAbilityComponent;
    }

    /// <summary>
    /// Holds the owner gameobject and ability component
    /// </summary>
    [HideInInspector]
    public OwnerInfo ownerInfo;

    public virtual void RunCustomCalculation(GameplayEffect effect, GameplayEffect.EffectAttributeModifier modifier, GameplayAttribute attribute)
    {

    }
}
