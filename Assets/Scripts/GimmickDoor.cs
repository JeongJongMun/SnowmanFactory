using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GimmickDoor : GimmickBase
{
    [SerializeField] private Transform leftDoor = null;
    [SerializeField] private Transform rightDoor = null;

    [SerializeField] private Ease openEase = Ease.OutQuad;
    [SerializeField] private Ease closeEase = Ease.InQuad;

    private Vector2 _leftDoorInitialPos;
    private Vector2 _rightDoorInitialPos;
    
    private float _moveDuration = 0.2f;
    private float _moveDistance = 284.0f;
    private bool _isAnimating = false;

    public override void Init()
    {
        base.Init();
        
        _leftDoorInitialPos = leftDoor.position;
        _rightDoorInitialPos = rightDoor.position;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        
        if (_isAnimating) return;
        
        _isAnimating = true;

        leftDoor.DOMoveX(-_moveDistance, _moveDuration).SetEase(closeEase);
        rightDoor.DOMoveX(_moveDistance, _moveDuration).SetEase(closeEase);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        
        leftDoor.DOMoveX(_leftDoorInitialPos.x, _moveDuration).SetEase(openEase);
        rightDoor.DOMoveX(_rightDoorInitialPos.x, _moveDuration).SetEase(openEase)
            .OnComplete(() =>
            {
                _isAnimating = false;
            });

    }   
}