using TMPro;
using UnityEngine;

public class UIScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textScore = null;

    public void SetScore(int score, int goal)
    {
        textScore.text = $"{score} / {goal}";
    }
}
