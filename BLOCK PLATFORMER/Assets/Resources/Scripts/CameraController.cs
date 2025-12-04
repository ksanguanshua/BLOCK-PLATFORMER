using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField] float camX;
    [SerializeField] float camY;
    [SerializeField] float camZ;

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
            camX = Mathf.Lerp(camX, target.position.x, lerpSpeed);
            transform.position = new Vector3(camX, camY, transform.position.z);
        }
    }
}
