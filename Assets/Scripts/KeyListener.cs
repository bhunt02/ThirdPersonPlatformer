using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyListener : MonoBehaviour
{
    private static KeyCode[] KEY_CODES =
    {
        KeyCode.W,
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.UpArrow,
        KeyCode.LeftArrow,
        KeyCode.DownArrow,
        KeyCode.RightArrow,
        KeyCode.Space,
    };
    
    public Dictionary<KeyCode, UnityEvent<bool>> KeyEvents = new();
    public UnityEvent onInitialized = new();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        foreach(var k in KEY_CODES)
        {
            KeyEvents.Add(k, new UnityEvent<bool>());
        }
        onInitialized.Invoke();
    }

    // Update is called once per frame
    private void Update()
    {
        foreach (var k in KEY_CODES)
        {
            if (Input.GetKeyDown(k))
            {
                KeyEvents[k].Invoke(true);
            } 
            else if (Input.GetKeyUp(k))
            {
                KeyEvents[k].Invoke(false);
            }
        }
    }
}
