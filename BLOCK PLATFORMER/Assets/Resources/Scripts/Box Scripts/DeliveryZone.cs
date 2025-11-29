using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
public class DeliveryZone : MonoBehaviour
{
    List<Sprite> items = new();
    int savedItemCount;

    List<GameObject> icons = new();

    [SerializeField] int orderSizeMin;
    [SerializeField] int orderSizeMax;

    [SerializeField] float orderTimer;
    float orderTimer_t;
    [SerializeField] float orderCooldown;

    bool onCooldown;
    bool firstOrder = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateOrder();
    }

    // Update is called once per frame
    void Update()
    {
        if (savedItemCount != items.Count)
        {
            ShowOrder();
            savedItemCount = items.Count;
        }
        
        CheckOrder();

        if (items.Count == 0)
        {
            if (!onCooldown)
            {
                onCooldown = true;
                firstOrder = false;
                Invoke("CreateOrder", orderCooldown);
            }

            transform.Find("Timer").GetComponent<TextMeshPro>().text = "";
        }
        else
        {
             OrderTimer();
             transform.Find("Timer").GetComponent<TextMeshPro>().text = Mathf.Round(orderTimer_t).ToString();
        }
    }

    void OrderTimer()
    {
        if (orderTimer_t > 0)
        {
            orderTimer_t -= Time.deltaTime;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void CreateOrder()
    {
        onCooldown = false;
        orderTimer_t = orderTimer;
        int orderSize = Random.Range(orderSizeMin, orderSizeMax);

        if (firstOrder) 
        {
            orderSize = 2;
        }

        for (int i = 0; i < orderSize; i++)
        {
            items.Add(BoxTypes.instance.labelSprites[Random.Range(0, BoxTypes.instance.labelSprites.Length)]);
            GameObject icon = new();
            icons.Add(icon);
        }
    }

    void CheckOrder()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position + (Vector3)(Vector2.up * 1.5f), Vector2.one * 3, 0);
        foreach(Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out Box box))
            {
                if (items.Contains(box.labelSprite))
                {
                    Debug.Log("CORRECT BOX");
                    items.Remove(box.labelSprite);
                    Destroy(icons[0]);
                    icons.RemoveAt(0);
                    Destroy(box.gameObject);
                }
            }
        }
    }

    void ShowOrder()
    {
        for (int i = 0; i < items.Count; i++)
        {
            GameObject icon = icons[i];
            icon.transform.position = new Vector2(transform.position.x - (items.Count / 2) + ((items.Count + 1) % 2 * 0.5f) + i, transform.position.y + 3);
            
            if (icon.TryGetComponent(out SpriteRenderer sr))
            {
                sr.sprite = items[i];
                continue;
            }

            sr = icon.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = items[i];
        }
    }
}
