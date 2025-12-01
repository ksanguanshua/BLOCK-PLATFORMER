using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
public class DeliveryZone : MonoBehaviour
{

    List<Sprite> items = new();
    int savedItemCount;

    List<GameObject> icons = new();

    Animator anim;

    [SerializeField] int orderSizeMin;
    [SerializeField] int orderSizeMax;

    [SerializeField] float orderTimer;
    float orderTimer_t;
    [SerializeField] float orderCooldown;

    bool onCooldown;
    bool firstOrder = true;

    bool correctDelivery;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        CreateOrder();
    }

    // Update is called once per frame
    void Update()
    {
        if (savedItemCount != items.Count)
        {
            savedItemCount = items.Count;
        }

        ShowOrder();

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
        anim.SetBool("isClosed", false);

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

    public void CheckOrder()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position + (Vector3)(Vector2.up * 1.5f), Vector2.one * 3, 0);

        List<Sprite> order = new();
        List<Sprite> deliveries = new();

        anim.SetBool("isClosed", true);

        foreach (Sprite label in items)
        {
            order.Add(label);
        }

        foreach(Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out Box box))
            {
                if (collider.TryGetComponent(out SpriteRenderer sr))
                {
                    sr.enabled = false;
                }
                
                deliveries.Add(box.labelSprite);
            }
        }

        foreach(Sprite label in order)
        {
            if (deliveries.Contains(label))
            {
                deliveries.Remove(label);
            }
            else
            {
                correctDelivery = false;
                return;
            }
        }

        if (deliveries.Count == 0)
        {
            correctDelivery = true;

            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out Box box))
                {
                    Destroy(box.gameObject);
                }
            }
        }
    }

    public void FinishedAnimation()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position + (Vector3)(Vector2.up * 1.5f), Vector2.one * 3, 0);

        if (!correctDelivery)
        {
            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out Box box))
                {
                    if (collider.TryGetComponent(out SpriteRenderer sr))
                    {
                        sr.enabled = true;
                    }
                }
            }

            anim.SetBool("isClosed", false);
        }
        else
        {
            items.Clear();

            foreach(GameObject icon in icons)
            {
                Destroy(icon);
            }

            icons.Clear();
        }
    }

    void PlayCloseSound()
    {
        AudioManager.instance.PlayDoorClose(); 
    }

    void ShowOrder()
    {
        for (int i = 0; i < items.Count; i++)
        {
            GameObject icon = icons[i];
            icon.transform.position = new Vector2(transform.position.x - (items.Count / 2) + ((items.Count + 1) % 2 * 0.5f) + i, transform.Find("Icons").position.y);
            
            if (icon.TryGetComponent(out SpriteRenderer sr))
            {
                sr.sprite = items[i];
                continue;
            }

            sr = icon.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = items[i];
            sr.sortingOrder = 1;
        }
    }
}
