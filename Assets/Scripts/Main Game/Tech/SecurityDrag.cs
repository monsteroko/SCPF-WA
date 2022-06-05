using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]

public class SecurityDrag : MonoBehaviour
{
    public GameObject panel;

    private Vector3 move, startPos;

    private void Update()
    {
        panel.transform.position += move;
    }

    private void OnMouseDown()
    {
        startPos = Input.mousePosition;
        move.Set(0, 0, 0);
    }

    private void OnMouseDrag()
    {
        move = Input.mousePosition - startPos;
    }    
}