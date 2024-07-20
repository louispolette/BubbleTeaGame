using System.Xml.Serialization;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AfterImageEmitter : MonoBehaviour
{
    [Space]

    [SerializeField] SpriteRenderer _rendererToCopy;

    [Space]

    [SerializeField] bool _startEnabled = false;

    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        UpdateImage();

        if (_startEnabled)
        {
            EnableAfterImage();
        }
        else
        {
            DisableAfterImage();
        }
    }

    public void EnableAfterImage()
    {
        var emission = _particleSystem.emission;

        emission.enabled = true;
    }

    public void DisableAfterImage()
    {
        var emission = _particleSystem.emission;

        emission.enabled = false;
    }

    [ContextMenu("UpdateImage")]
    public void UpdateImage()
    {
        ParticleSystem particleSystem = null;

        if (_particleSystem != null)
        {
            particleSystem = _particleSystem;
        }
        else
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        Sprite sprite = _rendererToCopy.sprite;

        var tex = particleSystem.textureSheetAnimation;

        tex.SetSprite(0, sprite);

        var main = particleSystem.main;

        main.startSize = new ParticleSystem.MinMaxCurve(sprite.rect.width / sprite.pixelsPerUnit * _rendererToCopy.transform.localScale.x);
    }
}
