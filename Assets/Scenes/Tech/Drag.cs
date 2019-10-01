using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]

public class Drag : MonoBehaviour
{

    private Vector3 offset;

    void OnMouseDown()
    {
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(/*Input.mousePosition.x*/0, Input.mousePosition.y, 10.0f));
    }

    void OnMouseDrag()
    {
        Vector3 newPosition = new Vector3(/*Input.mousePosition.x*/0, Input.mousePosition.y, 10.0f);
        transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
    }
}