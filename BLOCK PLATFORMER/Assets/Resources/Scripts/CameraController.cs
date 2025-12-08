using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField] float camX;
    [SerializeField] float camY;
    [SerializeField] float camZ;

    [SerializeField] Vector2 maxBound;
    [SerializeField] Vector2 minBound;

    [SerializeField] float lerpSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camX = transform.position.x;
        camY = transform.position.y;
        camZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            camX = Mathf.Lerp(camX, Mathf.Clamp(target.position.x, minBound.x, maxBound.x), lerpSpeed);
            camY = Mathf.Lerp(camY, Mathf.Clamp(target.position.y, minBound.y, maxBound.y), lerpSpeed);
            transform.position = new Vector3(camX, camY, transform.position.z);
        }
    }
}
