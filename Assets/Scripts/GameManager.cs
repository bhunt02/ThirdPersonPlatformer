using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager: MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject platforms;

    private UnityEvent<int> _onScoreChange = new();
    public void AddScoreChangeListener(UnityAction<int> call) => _onScoreChange.AddListener(call);
    
    private UnityEvent<float> _onDashCooldownChanged = new();
    public void AddDashCooldownListener(UnityAction<float> call) => _onDashCooldownChanged.AddListener(call);
    
    private int _score;
    
    private List<Coin> _coins = new();
    
    private void Start()
    {
        player?.AddDashCooldownListener(_onDashCooldownChanged.Invoke);
        _coins = SpawnCoins();
    }

    private List<Coin> SpawnCoins()
    {
        var coins = new List<Coin>();

        for (var i = 0; i < platforms.transform.childCount; i++)
        {
            var platform = platforms.transform.GetChild(i);

            switch (platform.gameObject.name)
            {
                case "Platform":
                    if (Random.Range(1, 11) < 5)
                    {
                        var coin = CreateCoin(platform, Vector3.zero);
                        coins.Add(coin);   
                    }
                    break;
                case "BigPlatform":
                    var count = Random.Range(5,10);
                    var angleInc = 2 * Mathf.PI / count;
                    var radius = platform.transform.lossyScale.x / 2;
                    for (var j = 0; j < count; j++)
                    {
                        var angle = j * angleInc;
                        var position = new Vector3(radius * Mathf.Cos(angle), 0, radius * Mathf.Sin(angle));
                        var c = CreateCoin(platform,position);
                        coins.Add(c);
                    }
                    break;
            }
        }
        
        return coins;
    }

    private Coin CreateCoin(Transform center, Vector3 positionOffset)
    {
        var coin = Instantiate(coinPrefab);
        coin.transform.position = center.position + positionOffset + new Vector3(0, center.lossyScale.y / 2 + 1, 0);
        var scr = coin.GetComponent<Coin>();
        scr.AddCollectedListener(() =>
        {
            ++_score;
            _onScoreChange.Invoke(_score);
        });
        return scr;
    }
}