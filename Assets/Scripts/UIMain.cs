using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    [SerializeField] private Button buttonKill = null;
    [SerializeField] private Button buttonPass = null;

    [SerializeField] private GameController controller = null;
    
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
