using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class SavingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool _saving;
    public void OnPointerDown( PointerEventData eventData )
    {
        _saving = true;
        Debug.Log(_saving);
    }
    public void OnPointerUp( PointerEventData eventData )
    {
        _saving = false;
        Debug.Log(_saving);
    }
    // Use this for initialization
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
    }
}
