using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickBase : MonoBehaviour
{
    public Action OnStartActivateEvent = null;
    public Action OnFinishActivateEvent = null;
    public Action OnStartDeactivateEvent = null;
    public Action OnFinishDeactivateEvent = null;
    
    virtual public void Init()
    {
        
    }
    
    public void Activate()
    {
        OnStartActivateEvent?.Invoke();
        OnActivate();
    }

    virtual public void OnActivate()
    {
    }

    public void Deactivate()
    {
        OnStartDeactivateEvent?.Invoke();
        OnDeactivate();
    }
    
    virtual public void OnDeactivate()
    {
        
    }
}
