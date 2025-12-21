using System;
using Core.Scripts;
using Core.Scripts.UI;
using TMPro;
using UnityEngine;

public class ResultPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI textDescription;
    [SerializeField] private TextMeshProUGUI textNormalPassCount;
    [SerializeField] private TextMeshProUGUI textDefectKillCount;
    [SerializeField] private TextMeshProUGUI textNormalKillCount;
    [SerializeField] private TextMeshProUGUI textDefectPassCount;
    [SerializeField] private TextMeshProUGUI textSpyKillCount;
    [SerializeField] private TextMeshProUGUI textTotalScore;
    [SerializeField] private TextMeshProUGUI textRank;
    
    public void Init(ScoreData data, string rank, int totalScore, int elapsedTime, Action onClickClose = null)
    {
        buttonClose.interactable = false;
        
        OnAfterCloseAction += onClickClose;
        
        StartCoroutine(Utils.NumberCountingEffect(textNormalPassCount, 0, data.normalPassCount));
        StartCoroutine(Utils.NumberCountingEffect(textDefectKillCount, 0, data.defectKillCount));
        StartCoroutine(Utils.NumberCountingEffect(textNormalKillCount, 0, data.normalKillCount));
        StartCoroutine(Utils.NumberCountingEffect(textDefectPassCount, 0, data.defectPassCount));
        StartCoroutine(Utils.NumberCountingEffect(textSpyKillCount, 0, data.spyKillCount));
        StartCoroutine(Utils.NumberCountingEffect(textTotalScore, 0, totalScore, doneCallback: () =>
        {
            buttonClose.interactable = true;
        }));
        
        textRank.text = rank;
        textDescription.text = $"{elapsedTime}초 동안 {data.normalPassCount}개 납품";
    }
}