using Core.Scripts.Manager;
using Core.Scripts;
using DG.Tweening;
using UnityEngine;

public class GimmickPress : GimmickBase
{
    [SerializeField] private Transform transformTarget = null;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float pressionTime = 0.5f;
    
    private Vector3 _originPosition = Vector3.zero;
    
    public override void Init()
    {
        base.Init();
        
        _originPosition = transform.position;
    }
    
    public override void OnActivate()
    {
        base.OnActivate();
        
        transform.DOKill();

        Managers.Sound.Play("SFX_button", Define.ESoundType.SFX, volume: 2f);

        var seq = DOTween.Sequence();

        seq.Append(transform.DOMoveY(transformTarget.position.y, moveDuration).SetEase(Ease.OutQuad));
        seq.AppendInterval(pressionTime);
        seq.AppendCallback(() => {
            OnFinishActivateEvent?.Invoke(); 
            Deactivate();
        });
    }
    
    public override void OnDeactivate()
    {   
        base.OnDeactivate();

        transform.DOMoveY(_originPosition.y, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                OnFinishDeactivateEvent?.Invoke();
            });
    }
}