using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayEffect : MonoBehaviour
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

    public enum ActivationType
    {
        Instant,
        HasDuration,
        Infinite
    }

    public enum EffectModifyType
    {
        Add,
        Multiply,
        Divide,
        Custom
    }

    public enum AttributeValue
    {
        CurrentValue,
        MinimumValue,
        MaximumValue
    }

    [System.Serializable]
    public struct EffectAttributeModifier
    {
        /// <summary>
        /// The attribute tag to find the attribute to modify.
        /// </summary>
        [Tooltip("The attribute tag to find the attribute to modify.")]
        public GameplayTag attributeTag;

        /// <summary>
        /// The modify type
        /// </summary>
        [Tooltip("The type of math modification.")]
        public EffectModifyType modifyType;

        /// <summary>
        /// The attribute value to change
        /// </summary>
        [Tooltip("The attribute value to change.")]
        public AttributeValue attributeValue;

        /// <summary>
        /// The value change the attribute by if modify type is not set to custom
        /// </summary>
        [Tooltip("The value change the attribute by if modify type is not set to custom.")]
        public float modifyValue;

        /// <summary>
        /// The effect calculation. Only effective on custom modify type.
        /// </summary>
        [Tooltip("The effect calculation. Only effective on \"custom\" modify type.")]
        public GameObject effectCalculation;
    }

    /// <summary>
    /// Holds the owner gameobject and ability component
    /// </summary>
    [HideInInspector]
    public OwnerInfo ownerInfo;

    /// <summary>
    /// The time that the effect has been active.
    /// </summary>
    [HideInInspector]
    public float activeTime;

    /// <summary>
    /// The type of the effect activation
    /// </summary>
    [Header("General Effect")]
    [Tooltip("The type of effect activation.")]
    public ActivationType activationType;

    /// <summary>
    /// The duration of the effect in seconds. Only valid for Has Duration type.
    /// </summary>
    [Tooltip("The duration of the effect in seconds. Only valid for Has Duration type.")]
    public float duration = 1f;

    /// <summary>
    /// How often the timer will fire and activate the effect. Only valid for effects that have a duration.
    /// </summary>
    [Tooltip("How often the timer will fire and activate the effect. Only valid for effects that have a duration.")]
    public float activationFiringRate = 0.01f;

    /// <summary>
    /// This tag is used to identify this gameplay effect.
    /// </summary>
    [Header("Tags")]
    [Tooltip("This tag is used to identify this gameplay effect.")]
    public GameplayTag effectTag;

    /// <summary>
    /// These tags are granted to the owning ability component upon activation.
    /// </summary>
    [Tooltip("These tags are granted to the owning ability component upon activation.")]
    public GameplayTagContainer grantedTags;

    /// <summary>
    /// These tags on the ability component will prevent this effect from activating.
    /// </summary>
    [Tooltip("These tags on the ability component will prevent this effect from activating.")]
    public GameplayTagContainer activationBlockedTags;

    /// <summary>
    /// The list of attributes to modify and what modification to do.
    /// </summary>
    [Header("Attribute Modification")]
    [Tooltip("A list of attributes to modify.")]
    public List<EffectAttributeModifier> attributeModification;

    /// <summary>
    /// Used to count the duration of the effect.
    /// </summary>
    private float durationCounter = 0f;

    /// <summary>
    /// Used to cancel a \"Has Duration\" or \"Infinite\" effect.
    /// </summary>
    private bool endEffect = false;

    /// <summary>
    /// This function initializes owner info for this effect
    /// </summary>
    /// <param name="owningObject"></param>
    /// <param name="owningAbilityComponent"></param>
    public void InitailizeEffect(GameObject owningObject, AbilityComponent owningAbilityComponent)
    {
        ownerInfo.owningGameObject = owningObject;
        ownerInfo.owningAbilityComponent = owningAbilityComponent;
    }

    /// <summary>
    /// This function is called by the ability component to start an effect.
    /// </summary>
    public void StartEffect()
    {
        durationCounter = 0f;
        endEffect = false;
        activeTime = 0f;
        StartCoroutine(RunEffect());
    }


    /// <summary>
    /// This is the coroutine to loop through an effect if desired.
    /// </summary>
    /// <returns></returns>
    IEnumerator RunEffect()
    {
        switch (activationType)
        {
            case ActivationType.Instant:
                {
                    ApplyEffect();
                    yield return null;
                    DestroyEffect();
                }
                break;
            case ActivationType.HasDuration:
                {
                    while ((durationCounter < duration) && !endEffect)
                    {
                        ApplyEffect();
                        yield return new WaitForSeconds(activationFiringRate);
                        durationCounter += activationFiringRate;
                        activeTime += activationFiringRate;
                    }
                    DestroyEffect();
                }
                break;
            case ActivationType.Infinite:
                {
                    while (!endEffect)
                    {
                        ApplyEffect();
                        yield return new WaitForSeconds(activationFiringRate);
                        activeTime += activationFiringRate;
                    }
                    DestroyEffect();
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// This function is the main function that modifies an attribute
    /// </summary>
    void ApplyEffect()
    {
        foreach (EffectAttributeModifier modifier in attributeModification)
        {
            GameplayAttribute attribute = ownerInfo.owningAbilityComponent.GetAttributeByTag(modifier.attributeTag);
            if (!attribute)
            {
                continue;
            }

            // apply change to what attribute value was chosen
            switch (modifier.attributeValue)
            {
                case AttributeValue.CurrentValue:
                    {
                        switch (modifier.modifyType)
                        {
                            case EffectModifyType.Add:
                                attribute.ChangeCurrentValue(attribute.currentValue + modifier.modifyValue);
                                break;
                            case EffectModifyType.Multiply:
                                attribute.ChangeCurrentValue(attribute.currentValue * modifier.modifyValue);
                                break;
                            case EffectModifyType.Divide:
                                attribute.ChangeCurrentValue(attribute.currentValue / modifier.modifyValue);
                                break;
                            case EffectModifyType.Custom:
                                SetupCustomCalculation(modifier, attribute);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case AttributeValue.MinimumValue:
                    switch (modifier.modifyType)
                    {
                        case EffectModifyType.Add:
                            attribute.ChangeMinimumValue(attribute.minimumValue + modifier.modifyValue);
                            break;
                        case EffectModifyType.Multiply:
                            attribute.ChangeMinimumValue(attribute.minimumValue * modifier.modifyValue);
                            break;
                        case EffectModifyType.Divide:
                            attribute.ChangeMinimumValue(attribute.minimumValue / modifier.modifyValue);
                            break;
                        case EffectModifyType.Custom:
                            SetupCustomCalculation(modifier, attribute);
                            break;
                        default:
                            break;
                    }
                    break;
                case AttributeValue.MaximumValue:
                    switch (modifier.modifyType)
                    {
                        case EffectModifyType.Add:
                            attribute.ChangeMaximumValue(attribute.maximumValue + modifier.modifyValue);
                            break;
                        case EffectModifyType.Multiply:
                            attribute.ChangeMaximumValue(attribute.maximumValue * modifier.modifyValue);
                            break;
                        case EffectModifyType.Divide:
                            attribute.ChangeMaximumValue(attribute.maximumValue / modifier.modifyValue);
                            break;
                        case EffectModifyType.Custom:
                            SetupCustomCalculation(modifier, attribute);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// This function spawns a custom calculation and runs it.
    /// </summary>
    /// <param name="modifier"></param>
    /// <param name="attribute"></param>
    private void SetupCustomCalculation(EffectAttributeModifier modifier, GameplayAttribute attribute)
    {
        if (!modifier.effectCalculation)
        {
            return;
        }

        EffectCustomCalculation customCalculation = modifier.effectCalculation.GetComponent<EffectCustomCalculation>();
        if (!customCalculation)
        {
            return;
        }

        // create instance
        GameObject calculationObjectInstance = Instantiate(modifier.effectCalculation, this.gameObject.transform);
        if (calculationObjectInstance)
        {
            EffectCustomCalculation calculationInstance = calculationObjectInstance.GetComponent<EffectCustomCalculation>();
            if (calculationInstance)
            {
                // run the custom calculation
                calculationInstance.RunCustomCalculation(this, modifier, attribute);
                Destroy(calculationObjectInstance);
            }
        }
    }

    /// <summary>
    /// THis function is called to end the effect.
    /// </summary>
    public void EndEffect()
    {
        endEffect = true;
    }

    /// <summary>
    /// This function removes the effect from the ability component and destroys the effect object.
    /// </summary>
    void DestroyEffect()
    {
        ownerInfo.owningAbilityComponent.RemoveEffect(this.gameObject);
        Destroy(this.gameObject, 0.1f);
    }
}
