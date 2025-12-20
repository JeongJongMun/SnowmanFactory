using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    [SerializeField] private Button buttonKill = null;
    [SerializeField] private Button buttonPass = null;
    [SerializeField] private Slider sliderTimer = null;

    [SerializeField] private GameController controller = null;
    
    public void SetTimerSlider(float value)
    {
        sliderTimer.value = value;
    }
    
    private void Awake()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        buttonKill.onClick.AddListener(() =>
        {
            controller.Kill();
        });
        
        buttonPass.onClick.AddListener(() =>
        {
            controller.Pass();
        });
    }
}
