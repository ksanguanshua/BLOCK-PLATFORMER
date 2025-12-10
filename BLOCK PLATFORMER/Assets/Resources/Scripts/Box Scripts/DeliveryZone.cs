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

    GameManager.GameState currentGameState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        BoxTypes.instance.createdBoxes.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if (savedItemCount != items.Count)
        {
            savedItemCount = items.Count;
        }

        StateChanged();
        ShowOrder();

        if (GameManager.instance.gameState == GameManager.GameState.deliver)
        {
            if (BoxTypes.instance.createdBoxes.Count == 0)
            {
                return;
            }

            if (items.Count == 0)
            {
                if (!onCooldown)
                {
                    onCooldown = true;
                    firstOrder = false;
                    Invoke("CreateOrder", orderCooldown);
                }
            }
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
            orderSize = 1;
        }

        for (int i = 0; i < orderSize; i++)
        {
            Sprite randomLabel = BoxTypes.instance.createdBoxes[Random.Range(0, BoxTypes.instance.createdBoxes.Count)];
            items.Add(randomLabel);
            BoxTypes.instance.createdBoxes.Remove(randomLabel);

            GameObject icon = new();
            icons.Add(icon);
        }
    }

    public void CheckOrder()
    {
        if (items.Count == 0)
        {
            return;
        }

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

                collider.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
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
            GameManager.instance.CompletedOrder(items.Count);

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

        if (GameManager.instance.gameState != GameManager.GameState.deliver)
        {
            return;
        }

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

    void StateChanged()
    {
        if (GameManager.instance.gameState != currentGameState)
        {
            CancelInvoke();

            switch (GameManager.instance.gameState)
            {
                case GameManager.GameState.tutorial:
                    break;
                case GameManager.GameState.organize:

                    anim.SetBool("isClosed", true);

                    items.Clear();
                    foreach (GameObject icon in icons)
                    {
                        Destroy(icon);
                    }
                    icons.Clear();

                    break;
                case GameManager.GameState.deliver:

                    anim.SetBool("isClosed", false);

                    CreateOrder();
                    break;
                case GameManager.GameState.breakTime:
                    break;
            }

            currentGameState = GameManager.instance.gameState;
        }
    }
}
