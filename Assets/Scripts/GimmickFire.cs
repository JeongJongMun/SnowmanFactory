using System.Collections;
using UnityEngine;

public class GimmickFire : GimmickBase
{
    [SerializeField] private Animator animator = null;
    
    public override void Init()
    {
        base.Init();
        
        animator.speed = 2.5f;
    }
    
    override public void OnActivate()
    {
        base.OnActivate();
        
        StartCoroutine(PlayAndCallback());
    }
    
    override public void OnDeactivate()
    {
        base.OnDeactivate();
    }
    
    public IEnumerator PlayAndCallback()
    {
        animator.SetTrigger("doPlay");
        
        // TODO : Fire sound
        
        yield return null;

        var duration = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration / 2.0f);

        OnFinishActivateEvent?.Invoke(); 
        Deactivate();
        
        yield return new WaitForSeconds(duration / 2.0f);
        
        OnFinishDeactivateEvent?.Invoke();
    }
}
