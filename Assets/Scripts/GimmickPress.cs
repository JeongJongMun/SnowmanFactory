using System.Collections;
using Core.Scripts.Manager;
using Core.Scripts;
using DG.Tweening;
using UnityEngine;

public class GimmickPress : GimmickBase
{
    [SerializeField] private Animator animator = null;
    
    public override void Init()
    {
        base.Init();
        
        animator.speed = 2.5f;
    }
    
    public override void OnActivate()
    {
        base.OnActivate();
        
        transform.DOKill();
        StartCoroutine(PlayAndCallback());
    }
    
    public IEnumerator PlayAndCallback()
    {
        animator.SetTrigger("doPlay");
        Managers.Sound.Play("SFX_button", Define.ESoundType.SFX, volume: 2f);

        yield return null;

        float duration = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration / 2.0f);

        OnFinishActivateEvent?.Invoke(); 
        Deactivate();
        
        yield return new WaitForSeconds(duration / 2.0f);
        
        OnFinishDeactivateEvent?.Invoke();
    }
    
    public override void OnDeactivate()
    {   
        base.OnDeactivate();

    }
}