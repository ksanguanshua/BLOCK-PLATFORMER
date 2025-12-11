using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    public Transform player;
    public Transform target;
    [SerializeField] float distanceThreshold;
    SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (Vector2.Distance(player.position, target.position) > distanceThreshold)
            {
                sr.enabled = true;
            }
            else
            {
                sr.enabled = false;
            }

            Vector3 dirVector = (target.position - player.position).normalized;
            transform.position = player.position + dirVector;
            Debug.Log(Mathf.Atan2(dirVector.y, dirVector.x));
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dirVector.y, dirVector.x));
        }
        else
        {
            sr.enabled = false;
        }
    }
}
