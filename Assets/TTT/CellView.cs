using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellView : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    int col;
    int row;

    public void SetColumnAndRow(int row, int col)       //////////// ADJUSTED TV
    {
        this.col = col;         //////////// ADJUSTED TV
        this.row = row;     //////////// ADJUSTED TV
    }

    public void SetText(string s)
    {
        text.text = s;
    }

    public void Click()
    {
        //print("Click on " + row + ", " + col);
        FindObjectOfType<TTT>().ChooseSpace(row, col);          //////////// ADJUSTED TV
    }
}
