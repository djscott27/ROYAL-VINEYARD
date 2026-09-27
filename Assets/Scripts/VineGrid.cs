using UnityEngine;

[RequireComponent(typeof(SnapToGrid))]
public class VineGrid : MonoBehaviour
{
    private Grid grid;

    // Grid dimensions
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 5;

    // Grid prefab
    private GameObject vineGridSpace;

    void Start()
    {
        grid = GetComponent<Grid>();
        vineGridSpace = Resources.Load<GameObject>("VineGridSpace");
        GenerateGrid();
    }

    void GenerateGrid()
    {
        float topLeftX = grid.cellSize.x * (-gridWidth/2) + transform.position.x;
        float topLeftY = grid.cellSize.y * (gridHeight/2) + transform.position.y;

        for (int xi = 0; xi < gridWidth; xi++)
        {
            float x = topLeftX + (grid.cellSize.x * xi);
            for (int yi = 0; yi < gridHeight; yi++)
            {
                float y = topLeftY - (grid.cellSize.y * yi);
                GameObject newSpace = Instantiate(vineGridSpace, transform);
                newSpace.transform.position = new Vector2(x,y);
            }
        }
    }
}
