using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : MonoBehaviour
{
    /// <summary>
    /// A list of the default attributes associtated with this component
    /// </summary>
    [Header("Attributes")]
    public List<GameplayAttributePair> defaultAttributes = new List<GameplayAttributePair>();

    /// <summary>
    /// A list of the attributes associated with this component
    /// </summary>
    [HideInInspector]
    public List<GameplayAttribute> attributes = new List<GameplayAttribute>();

    [Header("Abilities")]
    /// <summary>
    /// a list of the abilities the component will begin with
    /// </summary>
    public List<GameObject> defaultAbilities = new List<GameObject>();

    /// <summary>
    /// a list of the abilities that this component can use
    /// </summary>
    protected List<GameObject> grantedAbilities = new List<GameObject>();

    /// <summary>
    /// a list of the abilities that are active
    /// </summary>
    protected List<GameplayAbility> activeAbilities = new List<GameplayAbility>();

    [Header("Effects")]
    /// <summary>
    /// A list of the default gameplay effects
    /// </summary>
    public List<GameObject> defaultEffects = new List<GameObject>();

    /// <summary>
    /// A list of the active gameplay effects
    /// </summary>
    protected List<GameplayEffect> activeEffects =  new List<GameplayEffect>();

    /// <summary>
    /// A list of the active gameplay tags
    /// </summary>
    [HideInInspector]
    public GameplayTagContainer ownedGameplayTags = new GameplayTagContainer();

    // folder objects to store spawned objects under
    private GameObject abilitySystemFolderObject;
    private GameObject abilityFolderObject;
    private GameObject effectFolderObject;
    private GameObject attributeFolderObject;

    private void Awake()
    {
        CreateFolderObjects();
    }

    // Start is called before the first frame update
    void Start()
    {

        // create attributes
        foreach (var defaultAttribute in defaultAttributes)
        {
            CreateAttribute(defaultAttribute.attribute);
        }

        // grant default abilities
        foreach (var defaultAbility in defaultAbilities)
        {
            GrantAbility(defaultAbility);
        }

        // grant default effects
        foreach (var defaultEffect in defaultEffects)
        {
            GrantEffect(defaultEffect);
        }
    }

    /// <summary>
    /// This function creates the folder objects used to organize the ability system's game objects
    /// </summary>
    void CreateFolderObjects()
    {
        // create a gameobjects to be used as a folder for abilities
        abilitySystemFolderObject = new GameObject("Ability System");
        abilitySystemFolderObject.transform.parent = this.transform;

        attributeFolderObject = new GameObject("Attributes");
        attributeFolderObject.transform.parent = abilitySystemFolderObject.transform;

        abilityFolderObject = new GameObject("Abilities");
        abilityFolderObject.transform.parent = abilitySystemFolderObject.transform;

        effectFolderObject = new GameObject("Effects");
        effectFolderObject.transform.parent = abilitySystemFolderObject.transform;
    }

    /// <summary>
    /// This function instances a game object with the attribute component to be used with the ability system
    /// </summary>
    /// <param name="attributeObject"></param>
    private void CreateAttribute(GameObject attributeObject)
    {
        if (attributeObject)
        {
            GameplayAttribute attribute = attributeObject.GetComponent<GameplayAttribute>();
            if (attribute)
            {
                // check to make sure the attribute does not already exist by copmparing gameplay tags
                foreach (var activeAttribute in attributes)
                {
                    if (activeAttribute.AttributeTag == attribute.AttributeTag)
                    {
                        return;
                    }
                }

                // spawn attribute instance
                var attributeObjectInstance = Instantiate(attributeObject, attributeFolderObject.transform);
                if (attributeObjectInstance)
                {
                    GameplayAttribute attributeInstance = attributeObjectInstance.GetComponent<GameplayAttribute>();
                    if (attributeInstance)
                    {
                        // set attribute owner references
                        attributeInstance.InitializeAttribute(this.gameObject, this);
                        attributes.Add(attributeInstance);
                    }

                }
            }
        }
    }

    /// <summary>
    /// This function moves an ability into the granted list to be activated later
    /// </summary>
    /// <param name="abilityObject"></param>
    /// <returns></returns>
    public bool GrantAbility(GameObject abilityObject)
    {
        if (abilityObject)
        {
            if (grantedAbilities.Contains(abilityObject))
            {
                return false;
            }

            GameplayAbility ability = abilityObject.GetComponent<GameplayAbility>();
            if (ability)
            {
                grantedAbilities.Add(abilityObject);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Tries to activate an ability
    /// </summary>
    /// <param name="abilityObject"></param>
    /// <returns></returns>
    public bool TryActiveAbility(GameObject abilityObject)
    {
        if (!abilityObject)
        {
            return false;
        }

        foreach (var grantedAbilityObject in grantedAbilities)
        {
            if (grantedAbilityObject == abilityObject)
            {
                GameplayAbility grantedAbility = grantedAbilityObject.GetComponent<GameplayAbility>();
                if (!grantedAbility)
                {
                    return false;
                }

                // check if the abilty has the required tags
                if (!grantedAbility.activationRequiredTags.IsEmpty())
                {
                    if (!ownedGameplayTags.ContainsAllTags(grantedAbility.activationRequiredTags.GetGameplayTags()))
                    {
                        return false;
                    }
                }

                // check if the ability should be blocked by any existing tags
                if (!grantedAbility.activationBlockedTags.IsEmpty())
                {
                    if (ownedGameplayTags.ContainsAnyTag(grantedAbility.activationBlockedTags.GetGameplayTags()))
                    {
                        return false;
                    }
                }

                // check if ability should be blocked by other active abilities
                foreach (var activeAbility in activeAbilities)
                {
                    if (activeAbility.blockAbilityTags.ContainsAnyTag(grantedAbility.abilityTags.GetGameplayTags()))
                    {
                        return false;
                    }
                }

                // check if ability is on cooldown
                // TODO: move up in checking order
                if (grantedAbility.abilityCooldown)
                {
                    GameplayEffect cooldownEffect = grantedAbility.abilityCooldown.GetComponent<GameplayEffect>();
                    if (cooldownEffect)
                    {
                        if (ownedGameplayTags.ContainsAnyTag(cooldownEffect.grantedTags.GetGameplayTags()))
                        {
                            return false;
                        }
                    }
                }

                // instantiate the object
                GameObject abilityObjectInstance = Instantiate(grantedAbilityObject, abilityFolderObject.transform);
                if (!abilityObjectInstance)
                {
                    return false;
                }

                // get instance of ability
                GameplayAbility abilityInstance = abilityObjectInstance.GetComponent<GameplayAbility>();
                if (!abilityInstance)
                {
                    return false;
                }

                // cancel any active abilities with cancel tags
                if (!abilityInstance.cancelAbilityTags.IsEmpty())
                {
                    foreach (var activeAbility in activeAbilities)
                    {
                        if (activeAbility.abilityTags.ContainsAnyTag(abilityInstance.cancelAbilityTags.GetGameplayTags()))
                        {
                            activeAbility.CancelAbility();
                        }
                    }
                }

                // initialize and active ability
                abilityInstance.InitializeAbility(this.gameObject, this);
                if (abilityInstance.CanActivateAbility())
                {
                    activeAbilities.Add(abilityInstance);
                    ownedGameplayTags.AddAllTags(abilityInstance.grantedTags.GetGameplayTags());
                    abilityInstance.ActivateAbility();
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Deactivates an ability by removing it form active abilities and removing tags from owned tags
    /// </summary>
    /// <param name="abilityObject"></param>
    public void DeactivateAbility(GameObject abilityObject)
    {
        if (!abilityObject)
        {
            return;
        }

        GameplayAbility ability = abilityObject.GetComponent<GameplayAbility>();
        if (ability)
        {
            ownedGameplayTags.RemoveAllTags(ability.grantedTags.GetGameplayTags());
            activeAbilities.Remove(ability);
            Destroy(abilityObject, ability.destroyTime);
        }
    }

    /// <summary>
    /// Removes a granted ability from the list
    /// </summary>
    /// <param name="abilityObject"></param>
    /// <returns></returns>
    public bool RemoveAbility(GameObject abilityObject)
    {
        return grantedAbilities.Remove(abilityObject);
    }

    /// <summary>
    /// Checks to see if a certain ability is active
    /// </summary>
    /// <param name="abilityObject"></param>
    /// <returns></returns>
    public bool IsAbilityActive(GameObject abilityObject)
    {
        if (!abilityObject)
        {
            return false;
        }

        GameplayAbility ability = abilityObject.GetComponent<GameplayAbility>();
        if (ability)
        {
            foreach (var activeAbility in activeAbilities)
            {
                if (activeAbility.abilityTags.GetGameplayTags() == ability.abilityTags.GetGameplayTags())
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Get an attribute by using a gameplay tag, will return null if no attribute exists
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public GameplayAttribute GetAttributeByTag(GameplayTag tag)
    {
        foreach (var attribute in attributes)
        {
            if (attribute.AttributeTag == tag)
            {
                return attribute;
            }
        }
        return null;
    }

    /// <summary>
    /// This function is used to run a gameplay effect.
    /// </summary>
    /// <param name="effectObject"></param>
    /// <returns></returns>
    public bool GrantEffect(GameObject effectObject)
    {
        GameplayEffect effect = effectObject.GetComponent<GameplayEffect>();
        if (!effect)
        {
            return false;
        }

        // check to see if the system has any tags that would block this effect
        if (ownedGameplayTags.ContainsAnyTag(effect.activationBlockedTags.GetGameplayTags()))
        {
            return false;
        }

        GameObject effectObjectInstance = Instantiate(effectObject, effectFolderObject.transform);
        if (effectFolderObject)
        {
            GameplayEffect effectInstance = effectObjectInstance.GetComponent<GameplayEffect>();
            if (effectInstance)
            {
                effectInstance.InitailizeEffect(this.gameObject, this);
                ownedGameplayTags.AddAllTags(effectInstance.grantedTags.GetGameplayTags());
                activeEffects.Add(effectInstance);
                effectInstance.StartEffect();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// This function removes a gameplay effect and stops it
    /// </summary>
    /// <param name="effectObject"></param>
    public void RemoveEffect(GameObject effectObject)
    {
        GameplayEffect effect = effectObject.GetComponent<GameplayEffect>();
        if (!effect)
        {
            return;
        }

        foreach (var activeEffect in activeEffects)
        {
            if (activeEffect.effectTag == effect.effectTag)
            {
                ownedGameplayTags.RemoveAllTags(activeEffect.grantedTags.GetGameplayTags());
                activeEffects.Remove(activeEffect);
                activeEffect.EndEffect();
                break;
            }
        }
    }

}
