using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameplayAbility : MonoBehaviour
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

    /// <summary>
    /// These tags are used to identify this ability.
    /// </summary>
    [Header("Tags")]
    [Tooltip("These tags are used to identify this ability.")]
    public GameplayTagContainer abilityTags;

    /// <summary>
    /// These tags are granted to the owning ability component upon activation.
    /// </summary>
    [Tooltip("These tags are granted to the owning ability component upon activation.")]
    public GameplayTagContainer grantedTags;

    /// <summary>
    /// These tags are required for the ability component to own before activation.
    /// </summary>
    [Tooltip("These tags are required for the ability component to own before activation.")]
    public GameplayTagContainer activationRequiredTags;

    /// <summary>
    /// These tags on the ability component will prevent this ability from activating.
    /// </summary>
    [Tooltip("These tags on the ability component will prevent this ability from activating.")]
    public GameplayTagContainer activationBlockedTags;

    /// <summary>
    /// These tags will block other abilities from activating if they contain these tags.
    /// </summary>
    [Tooltip("These tags will block other abilities from activating if they contain these tags.")]
    public GameplayTagContainer blockAbilityTags;

    /// <summary>
    /// These tags will cancel active abilities if they contain these tags.
    /// </summary>
    [Tooltip("These tags will cancel active abilities if they contain these tags.")]
    public GameplayTagContainer cancelAbilityTags;

    /// <summary>
    /// This is the effect to apply the cost of an ability to an attribute.
    /// </summary>
    [Header("Cost")]
    [Tooltip("The attribute cost of the ability. Put a Gameplay Effect here.")]
    public GameObject abilityCost;

    /// <summary>
    /// This is the effect to apply the cooldown to an ability.
    /// </summary>
    [Header("Cooldown")]
    [Tooltip("This is the effect to apply the cooldown to an ability. This should be a \"Has Duration\" effect.")]
    public GameObject abilityCooldown;

    /// <summary>
    /// The amount of time before the ability object is destroyed once the ability has ended.
    /// </summary>
    [Header("General")]
    [Tooltip("The amount of time before the ability object is destroyed once the ability has ended.")]
    public float destroyTime = 1f;

    /// <summary>
    /// This function initialized the owner info and destroy time
    /// </summary>
    /// <param name="owningGameObject"></param>
    /// <param name="owningAbilityComponent"></param>
    public void InitializeAbility(GameObject owningGameObject, AbilityComponent owningAbilityComponent)
    {
        ownerInfo.owningGameObject = owningGameObject;
        ownerInfo.owningAbilityComponent = owningAbilityComponent;

        // clamp min on destroy time
        if (destroyTime < 0)
        {
            destroyTime = 0;
        }
    }

    /// <summary>
    /// This function checks to see if the ability can be activated.
    /// </summary>
    /// <returns></returns>
    public virtual bool CanActivateAbility()
    {
        return true;
    }


    /// <summary>
    /// Thic function is called to activate the ability.
    /// </summary>
    public virtual void ActivateAbility()
    {

    }

    /// <summary>
    /// This function is called to commit the ability cost and the ability cooldown.
    /// </summary>
    /// <returns></returns>
    public bool CommitAbility()
    {
        // grant ability cost effect
        if (abilityCost)
        {
            if (!ownerInfo.owningAbilityComponent.GrantEffect(abilityCost))
            {
                return false;
            }
        }

        // grant ability cooldown effect
        if (abilityCooldown)
        {
            if (!ownerInfo.owningAbilityComponent.GrantEffect(abilityCooldown))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// This function is called to run any logic needed once the ability has ended.
    /// </summary>
    public virtual void EndAbility()
    {
        ownerInfo.owningAbilityComponent.DeactivateAbility(this.gameObject);
    }

    /// <summary>
    /// This function is called ot run any logic needed if the ability is canceled.
    /// </summary>
    public virtual void CancelAbility()
    {
        ownerInfo.owningAbilityComponent.DeactivateAbility(this.gameObject);
    }
}
