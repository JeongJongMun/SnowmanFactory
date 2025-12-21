using Core.Scripts.UI;
using TMPro;
using UnityEngine;

public class MissionPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI textTitle = null;
    [SerializeField] private TextMeshProUGUI textDescription = null;
    
    public void Init(int time, int goal)
    {
        Init();
        textTitle.text = $"MissionTitle";
        textDescription.text = $"제품 설명서를 참고하여 눈사람 {goal}개를 {time}초 내에 납품하라.";
    }
}
