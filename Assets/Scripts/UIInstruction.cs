using Core.Scripts;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIInstruction : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform = null;
    [SerializeField] private Image imageDim = null;

    private readonly Vector2 _showSize = new(896, 1500);
    private readonly Vector2 _hideSize = new(311, 520);
    
    private readonly Quaternion _showRotation = Quaternion.identity;
    private readonly Quaternion _hideRotation = Quaternion.Euler(0, 0, 20);
    
    private readonly Vector3 _showPosition = new(0, 0, 0);
    private readonly Vector3 _hidePosition = new(-413, -1122, 0);

    private bool _isShow = false;
    private float _toggleDuration = 0.5f;

    private void Start()
    {
        
        imageDim.gameObject.AddUIEvent((data =>
        {
            if (data.pointerCurrentRaycast.gameObject == imageDim.gameObject)
                Toggle();
        }));
    }

    public void Toggle()
    {
        if (_isShow)
        {
            Hide();
        }
        else
        {
            Show();
        }
        _isShow = !_isShow;
    }
    
    private void Show()
    {
        rectTransform.DOSizeDelta(_showSize, _toggleDuration).SetEase(Ease.OutBack);
        rectTransform.DORotateQuaternion(_showRotation, _toggleDuration).SetEase(Ease.OutBack);
        rectTransform.DOLocalMove(_showPosition, _toggleDuration).SetEase(Ease.OutBack);
        imageDim.DOFade(0.75f, _toggleDuration);
        imageDim.raycastTarget = true;
    }

    private void Hide()
    {
        rectTransform.DOSizeDelta(_hideSize, _toggleDuration).SetEase(Ease.OutBack);
        rectTransform.DORotateQuaternion(_hideRotation, _toggleDuration).SetEase(Ease.OutBack);
        rectTransform.DOLocalMove(_hidePosition, _toggleDuration).SetEase(Ease.OutBack);
        imageDim.DOFade(0f, _toggleDuration);
        imageDim.raycastTarget = false;
    }
}
