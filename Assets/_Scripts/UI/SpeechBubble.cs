using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] SpriteRenderer _bubbleRenderer;

    public void Hide()
    {
        _bubbleRenderer.enabled = false;
    }

    public void Show()
    {
        _bubbleRenderer.enabled = true;
    }
}
