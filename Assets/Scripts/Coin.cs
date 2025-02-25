using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Coin : MonoBehaviour
{
    private bool _isCollected;
    private UnityEvent _onCollected = new();
    public void AddCollectedListener(UnityAction call) => _onCollected.AddListener(call);
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_isCollected) return;
            
            _onCollected.Invoke();
            _isCollected = true;
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!this.IsDestroyed())
        {
            transform.Rotate(new Vector3(0, Time.deltaTime * 90f, 0), Space.World);
        }
    }
}
