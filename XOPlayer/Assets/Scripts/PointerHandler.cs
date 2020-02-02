using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerManager playerManager;

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        playerManager.OnPointerUp();
    }
}
