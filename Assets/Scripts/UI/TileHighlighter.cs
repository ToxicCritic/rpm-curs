using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Vector3 offset = new Vector3(0.5f, 0.5f, 0); // Смещение для центровки на тайле

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = Vector3Int.FloorToInt(mousePosition);
        gridPosition.z = -1;

        // Центрируем выделение на тайле
        Vector3 highlightPosition = gridPosition + offset;
        transform.position = highlightPosition;
    }
}
