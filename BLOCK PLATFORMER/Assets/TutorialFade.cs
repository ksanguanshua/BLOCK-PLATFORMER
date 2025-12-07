using UnityEngine;
using TMPro;
using System.Collections;

public class FadeOnPlayerTrigger : MonoBehaviour
{
    public float fadeTime = 0.5f;
    public bool startHidden = true;

    SpriteRenderer sprite;
    TextMeshPro text;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        text = GetComponent<TextMeshPro>();

        if (startHidden)
            SetAlpha(0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            StartFade(1);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            StartFade(0);
    }

    void StartFade(float target)
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(target));
    }

    IEnumerator FadeTo(float target)
    {
        float start = GetAlpha();
        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start, target, t / fadeTime);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(target);
    }

    float GetAlpha()
    {
        if (sprite != null) return sprite.color.a;
        if (text != null) return text.color.a;
        return 1;
    }

    void SetAlpha(float a)
    {
        if (sprite != null)
        {
            Color c = sprite.color;
            c.a = a;
            sprite.color = c;
        }

        if (text != null)
        {
            Color c = text.color;
            c.a = a;
            text.color = c;
        }
    }
}
