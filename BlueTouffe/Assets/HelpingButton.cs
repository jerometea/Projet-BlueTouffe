using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
public class HelpingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool _saving;
    public void OnPointerDown(PointerEventData eventData)
    {
        _saving = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _saving = false;
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