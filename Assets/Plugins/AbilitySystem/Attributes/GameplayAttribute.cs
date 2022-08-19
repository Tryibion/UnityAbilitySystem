using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// delegate for attribute change
/// </summary>
public delegate void OnAttributeChange(float newValue, float oldValue);

[Serializable]
public class GameplayAttributePair
{
    /// <summary>
    /// Name of an attribute to organize in the inspector
    /// </summary>
    [SerializeField]
    string inspectorName;

    /// <summary>
    /// The game object of an attribute
    /// </summary>
    public GameObject attribute;
}

public class GameplayAttribute : MonoBehaviour
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
    /// The Tag associated with this attribute
    /// </summary>
    public GameplayTag AttributeTag;

    /// <summary>
    /// The owner info that holds the owning game object and ability component
    /// </summary>
    [HideInInspector]
    public OwnerInfo ownerInfo;

    [Header("Attribute Values")]
    /// <summary>
    /// The current value of an attribute. Do not change this directly though code, change it using function.
    /// </summary>
    public float currentValue;
    public float OldCurrentValue { get; private set; }

    /// <summary>
    /// The minimum value of an attribute. Do not change this directly though code, change it using function.
    /// </summary>
    public float minimumValue;
    public float OldMinimumValue { get; private set; }

    /// <summary>
    /// The maximum value of an attribute. Do not change this directly though code, change it using function.
    /// </summary>
    public float maximumValue;
    public float OldMaximumValue { get; private set; }

    /// <summary>
    /// Event is tirggered when the current value is changed
    /// </summary>
    public event OnAttributeChange currentValueChange;

    /// <summary>
    /// Event is triggered when the minimum value is changed
    /// </summary>
    public event OnAttributeChange minimumValueChange;

    /// <summary>
    /// Event is triggered when the maximum value is changed
    /// </summary>
    public event OnAttributeChange maximumValueChange;

    /// <summary>
    /// A function that initializes the owner info variable on the attribute
    /// </summary>
    /// <param name="owningGameObject"></param>
    /// <param name="abilityComponent"></param>
    public void InitializeAttribute(GameObject owningGameObject, AbilityComponent abilityComponent)
    {
        ownerInfo.owningGameObject = owningGameObject;
        ownerInfo.owningAbilityComponent = abilityComponent;
    }

    /// <summary>
    /// Called to change the current value of an attribute
    /// </summary>
    /// <param name="value"></param>
    public void ChangeCurrentValue(float value)
    {
        PreAttributeChange();
        OldCurrentValue = currentValue;
        currentValue = Mathf.Clamp(value, minimumValue, maximumValue);
        currentValueChange?.Invoke(currentValue, OldCurrentValue);
        PostAttributeChange();
    }

    /// <summary>
    /// Called to change the minimum value of an attribute
    /// </summary>
    /// <param name="value"></param>
    public void ChangeMinimumValue(float value)
    {
        PreAttributeChange();
        OldMinimumValue = minimumValue;
        minimumValue = value;
        if (currentValue < minimumValue)
        {
            ChangeCurrentValue(minimumValue);
        }
        minimumValueChange?.Invoke(minimumValue, OldMinimumValue);
        PostAttributeChange();
    }

    /// <summary>
    /// Called to change the maximum value of an attribute
    /// </summary>
    /// <param name="value"></param>
    public void ChangeMaximumValue(float value)
    {
        PreAttributeChange();
        OldMaximumValue = maximumValue;
        maximumValue = value;
        if (currentValue > maximumValue)
        {
            ChangeCurrentValue(maximumValue);
        }
        maximumValueChange?.Invoke(maximumValue, OldMaximumValue);
        PostAttributeChange();
    }

    /// <summary>
    /// Called at the beginning of any value change
    /// </summary>
    protected virtual void PostAttributeChange()
    {

    }

    /// <summary>
    /// Called at the end of any value change
    /// </summary>
    protected virtual void PreAttributeChange()
    {

    }
}
