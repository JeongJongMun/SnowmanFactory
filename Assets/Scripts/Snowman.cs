using System;
using DG.Tweening;
using UnityEngine;

public class Snowman : MonoBehaviour
{
    [SerializeField] private int id = 0;
    [SerializeField] private Animator animator = null;
    [SerializeField, Range(0.05f, 0.3f)] private float immediateMoveDuration = 0.2f;

    private Vector3 _moveTargetPosition = Vector3.zero;
    private Action _moveDoneCallback = null;
    
    public void Init(ISnowmanData data)
    {
        id = data.id;
        
        var path = $"Animations/Snowman/{data.fullbody_animator_name}/{data.fullbody_animator_name}";
        var controller = Resources.Load<RuntimeAnimatorController>(path);
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
    }

    public void MoveImmediate()
    {
        Move(_moveTargetPosition, immediateMoveDuration, _moveDoneCallback);
    }
    
    public void Move(Vector3 targetPosition, float duration, Action doneCallback = null)
    {
        _moveTargetPosition = targetPosition;
        _moveDoneCallback = doneCallback;

        transform.DOMoveX(targetPosition.x, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Destroy(gameObject);
                doneCallback?.Invoke();
            });
    }
    
    public void Stop()
    {
        transform.DOKill();
    }
}