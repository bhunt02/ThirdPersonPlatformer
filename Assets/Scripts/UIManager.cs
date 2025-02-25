using TMPro;
using UnityEngine;

public class UIManager: MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _dashText;
    
    private void Start()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            switch (child.gameObject.name)
            {
                case "ScoreText":
                    _scoreText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "DashText":
                    _dashText = child.GetComponent<TextMeshProUGUI>();
                    break;
            }
        }
        
        gameManager?.AddScoreChangeListener(UpdateScoreText);
        gameManager?.AddDashCooldownListener(UpdateDashText);
        UpdateScoreText(0);
        UpdateDashText(0);
    }

    private void UpdateScoreText(int score)
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {score}";   
        }
    }

    private void UpdateDashText(float cooldown)
    {
        if (_dashText != null)
        {
            _dashText.text = cooldown switch
            {
                > 0f => $"{Mathf.CeilToInt(cooldown)}",
                _ => "Press SHIFT to Dash",
            };
        }
    }
}