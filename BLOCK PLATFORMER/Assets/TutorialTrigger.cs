using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialTrigger : MonoBehaviour
{
    public float fadeTime = 0.5f;
    public bool hideOnStart = true;

    public List<GameObject> objectsToFade;

    void Start()
    {
        if (hideOnStart)
        {
            foreach (var obj in objectsToFade)
            {
                if (obj == null) continue;

                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                TextMeshPro tmp = obj.GetComponent<TextMeshPro>();

                SetAlpha(sr, tmp, 0); // force transparent at start
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            StartFadeOnAll(1);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            StartFadeOnAll(0);
    }

    void StartFadeOnAll(float targetAlpha)
    {
        StopAllCoroutines();

        // start new fades
        foreach (GameObject obj in objectsToFade)
        {
            if (obj == null) continue;
            StartCoroutine(FadeObject(obj, targetAlpha));
        }
    }

    IEnumerator FadeObject(GameObject obj, float targetAlpha)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        TextMeshPro tmp = obj.GetComponent<TextMeshPro>();

        float startAlpha = GetAlpha(sr, tmp);
        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeTime);
            SetAlpha(sr, tmp, a);
            yield return null;
        }

        SetAlpha(sr, tmp, targetAlpha);
    }

    float GetAlpha(SpriteRenderer sr, TextMeshPro tmp)
    {
        if (sr != null) return sr.color.a;
        if (tmp != null) return tmp.color.a;
        return 1;
    }

    void SetAlpha(SpriteRenderer sr, TextMeshPro tmp, float a)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = a;
            tmp.color = c;
        }
    }
}
