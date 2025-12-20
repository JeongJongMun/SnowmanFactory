using Core.Scripts.Table;
using System.Diagnostics;
using UnityEngine;

public class ScoreData
{
    public int normalPassCount = 0;
    public int defectKillCount = 0;
    public int normalKillCount = 0;
    public int defectPassCount = 0;
    public int spyKillCount = 0;

    private ScoreTd _scoreTable => Tables.Instance.GetTable<ScoreTd>();

    private RankTd _rankTable => Tables.Instance.GetTable<RankTd>();

    public int GetBaseScore()
    {
        var scoreTable = Tables.Instance.GetTable<ScoreTd>();
        return (normalPassCount * scoreTable.GetScore("normal_pass")) +
               (defectKillCount * scoreTable.GetScore("defect_kill")) +
               (normalKillCount * scoreTable.GetScore("normal_kill")) +
               (defectPassCount * scoreTable.GetScore("defect_pass")) +
               (spyKillCount * scoreTable.GetScore("spy_kill"));
    }

    public int GetTimeBonus(float remainingTime)
    {
        if (remainingTime >= 30f) return _scoreTable.GetScore("time_bonus_30");
        if (remainingTime >= 20f) return _scoreTable.GetScore("time_bonus_20");
        if (remainingTime >= 10f) return _scoreTable.GetScore("time_bonus_10");
        return 0;
    }

    public int GetPerfectBonus()
    {
        if (defectPassCount == 0 && normalKillCount == 0)
            return _scoreTable.GetScore("perfect_play");

        if (defectPassCount == 0)
            return _scoreTable.GetScore("zero_defect");

        return 0;
    }

    public int GetTotalScore(float remainingTime)
    {
        int baseScore = GetBaseScore();
        int timeBonus = GetTimeBonus(remainingTime);
        int perfectBonus = GetPerfectBonus();

        return baseScore + timeBonus + perfectBonus;
    }

    public string GetRank(int totalScore, bool missionSuccess)
    {
        return _rankTable.GetRank(totalScore, defectPassCount, missionSuccess);
    }

    public void Reset()
    {
        normalPassCount = 0;
        defectKillCount = 0;
        normalKillCount = 0;
        defectPassCount = 0;
        spyKillCount = 0;
    }
}