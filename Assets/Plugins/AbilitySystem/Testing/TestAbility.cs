using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAbility : GameplayAbility
{
    public override void ActivateAbility()
    {
        CommitAbility();
        // Add your code here
        
    }

    public override void EndAbility()
    {
        // Add your code here
        
        base.EndAbility();
    }

    public override void CancelAbility()
    {
        // Add your code here
        
        base.CancelAbility();
    }
    
}