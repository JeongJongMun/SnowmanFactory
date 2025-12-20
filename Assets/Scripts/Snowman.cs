    using System;
using Core.Scripts.Table;
using DG.Tweening;
using UnityEngine;

public struct MovementData
{
    public Vector3 targetPosition;
    public float duration;
    public Action doneCallback;
}

public class Snowman : MonoBehaviour
{
    [SerializeField] private int id = 0;
    [SerializeField] private Animator animator = null;

    private MovementData _cachedMovementData;
    
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

    public void Move()
    {
        Move(_cachedMovementData);
    }

    public void MoveImmediate()
    {
        _cachedMovementData.duration = 0.2f;
        Move(_cachedMovementData);
    }

    public void Move(MovementData data)
    {
        _cachedMovementData = data;
        
        transform.DOMoveX(data.targetPosition.x, data.duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Destroy(gameObject);
                data.doneCallback?.Invoke();
            });
    }

    public void Stop()
    {
        transform.DOKill();
    }
}