using UnityEngine;
using System.Collections;

public class Pointer : MonoBehaviour
{
    Renderer _renderer;
    // Use this for initialization

    private readonly Color[] kColors = {
            Color.blue,
            Color.cyan,
            Color.green,
            Color.magenta,
            Color.red,
            Color.white,
            Color.yellow,
            Color.gray,
            Color.red,
            Color.black,
        };

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = kColors[Random.Range(0, kColors.Length)];

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        // Serialize the position and color
        if (stream.isWriting)
        {
            Color color = _renderer.material.color;

            stream.Serialize(ref color.r);
            stream.Serialize(ref color.g);
            stream.Serialize(ref color.b);
            stream.Serialize(ref color.a);
        }
        else
        {
            Color color = Color.white;

            stream.Serialize(ref color.r);
            stream.Serialize(ref color.g);
            stream.Serialize(ref color.b);
            stream.Serialize(ref color.a);

            _renderer.material.color = color;
        }
    }
}