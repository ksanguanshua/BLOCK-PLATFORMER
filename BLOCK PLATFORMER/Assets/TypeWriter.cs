using UnityEngine;
using TMPro;

public class TypeWriter : MonoBehaviour
{
    TextMeshProUGUI text;
    [SerializeField] int value;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        text.maxVisibleCharacters = value;
    }
}
