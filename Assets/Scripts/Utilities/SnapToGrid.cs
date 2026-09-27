using UnityEngine;

[RequireComponent(typeof(Grid))]
public class SnapToGrid : MonoBehaviour
{
    private Grid grid;

    void Start()
    {
        grid = GetComponent<Grid>();
    }

    void LateUpdate()
    {
        // snap all children (ignoring the grid itself and UI elements) to the grid
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t == transform || t.GetComponent<RectTransform>() != null)
                continue;

            Vector3Int cell = grid.WorldToCell(t.position);
            t.position = grid.GetCellCenterWorld(cell);
        }
    }
}
