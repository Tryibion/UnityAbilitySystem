using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestCalculation : EffectCustomCalculation
{
    public override void RunCustomCalculation(GameplayEffect effect, GameplayEffect.EffectAttributeModifier modifier, GameplayAttribute attribute)
    {
        attribute.ChangeCurrentValue(attribute.currentValue - 1f);
    }
}

