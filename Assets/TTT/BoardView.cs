using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    CellView[,] cells;

    [SerializeField] GameObject rowPrefab;
    [SerializeField] GameObject cellPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeBoard(int rows, int cols)     //////////// ADJUSTED TV
    {
        cells = new CellView[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            GameObject row = Instantiate(rowPrefab, transform);
            for (int j = 0; j < cols; j++)
            {
                GameObject cell = Instantiate(cellPrefab, row.transform);

                cells[i, j] = cell.GetComponent<CellView>();                                                    ////////// ADJUSTED TV
                cells[i, j].SetColumnAndRow(i, j);                                                                      ///////// ADJUSTED TV
            }
        }
    }

    public void UpdateCellVisual(int row, int col, PlayerOption player)                             //////////// ADJUSTED TV
    {
        string symbol = "";
        if (player == PlayerOption.X)
            symbol = "X";
        else if (player == PlayerOption.O)
            symbol = "O";
        cells[row, col].SetText(symbol);
    }
}
