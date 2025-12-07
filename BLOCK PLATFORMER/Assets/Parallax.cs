using UnityEngine;

public class Parallax : MonoBehaviour
{
    Vector2 size;
    Vector2 startPos;
    public GameObject cam;
    public float parallaxEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        parallaxEffect = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 distance = cam.transform.position * parallaxEffect;

        transform.position = new Vector3(startPos.x + distance.x, startPos.y + distance.y, transform.position.z);
    }
}
