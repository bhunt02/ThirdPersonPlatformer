using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _readyToFall = true;
    private bool _readyToRise;
    private Vector3 _originalPosition;

    void Start()
    {
        _originalPosition = transform.position;
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!_readyToFall) return;
            _readyToFall = false;
            StartCoroutine(Fall());
        } else if (other.gameObject.CompareTag("Ground") && !_readyToFall)
        {
            _readyToRise = true;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") && !_readyToFall)
        {
            _readyToRise = true;
        }
    }

    private IEnumerator Fall()
    {
        // Delay fall
        yield return new WaitForSeconds(1.5f);   
        
        _rb.useGravity = true;
        while (!_readyToRise)
        {
            yield return new WaitForSeconds(0.1f);   
        }
        
        _rb.useGravity = false;
        while (transform.position.y < _originalPosition.y)
        {
            transform.position = new Vector3(_originalPosition.x, transform.position.y + 0.1f, _originalPosition.z);
            yield return new WaitForSeconds(0.01f);
        }

        _readyToFall = true;
        _readyToRise = false; 
        transform.position = _originalPosition;
        yield return null;
    }
}
