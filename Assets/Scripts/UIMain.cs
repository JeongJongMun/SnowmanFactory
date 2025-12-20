using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    [SerializeField] private Button buttonKill = null;
    [SerializeField] private Button buttonPass = null;
    [SerializeField] private Button buttonInstruction = null;
    
    [SerializeField] private Slider sliderTimer = null;
    
    [SerializeField] private UIScore scoreUI = null;
    [SerializeField] private UIInstruction instructionUI = null;

    [SerializeField] private GameController controller = null;
    
    public void SetTimerSlider(float value)
    {
        sliderTimer.value = value;
    }
    
    public void SetScore(int score, int goal)
    {
        scoreUI.SetScore(score, goal);
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
        
        buttonInstruction.onClick.AddListener(() =>
        {
            instructionUI.Toggle();
        });
    }
}
