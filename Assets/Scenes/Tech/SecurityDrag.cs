using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]

public class SecurityDrag : MonoBehaviour
{
    public GameObject panel;
    
    void OnMouseDrag()
    {
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(/*Input.mousePosition.x*/0, Input.mousePosition.y, 0));
        panel.transform.position += newPosition;
    }    
}