using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    [SerializeField] BoardView board;
    int moveCount = 0;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        //RunSimulation();        //////////////////***************!!!!!!!!!!!!!!!!!!!!!DO NOT RUN ON WEAK DEVICE!!!!!!!!!!!!!!!!!!!!!!!!!!*****************************************///////////////////
        if (Input.GetKeyDown(KeyCode.R))
            ResetGame();
    }
    private void RunSimulation()    //////////////////***************!!!!!!!!!!!!!!!!!!!!!DO NOT RUN ON WEAK DEVICE!!!!!!!!!!!!!!!!!!!!!!!!!!*****************************************///////////////////
    {
        if (GetWinner() != PlayerOption.NONE)
        {
            print("END GAME, NOT IMPOSSIBLE FOR ENDLESS");
            EditorApplication.isPaused = true;
        }
        else
        {
            if (moveCount == 9)
                ResetGame();
            else
                MakeOptimalMove();
        }
    }
    private void ResetGame()
    {
        print("************************************************");
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    private class L     // private Location class
    {
        public int row, col;
        public L()
        {
            row = -1;
            col = -1;
        }
        public L(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
        public bool isEqual(L other)
        { return row == other.row && col == other.col; }
        public bool isNull()
        { return row == -1 && col == -1; }
        public string toString()
        { return "(" + row + ", " + col + ")"; }
    }

    private void Helper_MakeRandomMove()   // ignore this
    {
        L[] openLocations = new L[9 - moveCount];   // Get a list of open spots
        int index = 0;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (cells[i, j].current == PlayerOption.NONE)
                {
                    openLocations[index] = new L(i, j);
                    index++;
                }
        L randomLocation = openLocations[Random.Range(0, openLocations.Length)];    // Choose a random spot in the list
        print("Turn count: " + moveCount + ". Random move, move at: " + randomLocation.toString() + ". Possible moves in total: " + openLocations.Length);
        ChooseSpace(randomLocation.row, randomLocation.col);
    }

    private int Helper_CountSpecialCase(L[] caseType, PlayerOption type)
    {
        int count = 0;
        foreach (L spot in caseType)
            if (cells[spot.row, spot.col].current == type)
                count++;
        return count;
    }
    private L[] Helper_GetSpecialCaseOpenSpot(L[] caseType)
    {
        int listLength = 0;
        foreach (L spot in caseType)
            if (cells[spot.row, spot.col].current == PlayerOption.NONE)
                listLength++;
        L[] openLocations = new L[listLength];
        int index = 0;
        foreach (L spot in caseType)
            if (cells[spot.row, spot.col].current == PlayerOption.NONE)
            {
                openLocations[index]  = spot;
                index++;
            }
        return openLocations;
    }
    private L[] Helper_GetSpecicalCaseLocations(L[] caseType, PlayerOption type)
    {
        int listLength = 0;
        foreach (L spot in caseType)
            if (cells[spot.row, spot.col].current == type)
                listLength++;
        L[] locations = new L[listLength];
        int index = 0;
        foreach (L spot in caseType)
            if (cells[spot.row, spot.col].current == type)
            { 
                locations[index] = spot;
                index++;
            }
        return locations;
    }
    private L[] Helper_GetSpecialCase2OpenSpot(L sideValue, L[] caseType)   // case Type must be mids
    {
        int listLength = 0;
        foreach (L spot in caseType)
            if (cells[spot.row, spot.col].current == PlayerOption.NONE)
                if (sideValue.row + spot.row == 3 && sideValue.col + spot.col == 3) { } // Created unique for special case  2
                else
                    listLength++;
        L[] openLocations = new L[listLength];
        int index = 0;
        foreach (L spot in caseType)
            if (cells[spot.row, spot.col].current == PlayerOption.NONE)
                if (sideValue.row + spot.row == 3 && sideValue.col + spot.col == 3) { }   // Created unique for special case  2
                else
                {
                    openLocations[index]  = spot;
                    index++;
                }
        return openLocations;
    }
    private L[] Helper_GetSpecialCase4OpenSpot(L[] mids, L[] corners, PlayerOption type)
    {
        int midCount = 0; L mid = new L();
        int cornerCount = 0; L corner = new L();
        foreach (L spot in mids)
        {
            if (cells[spot.row, spot.col].current == type)
            {
                mid = spot;
                midCount++;
            }
        }
        foreach (L spot in corners)
        {
            if (cells[spot.row, spot.col].current == type)
            {
                corner = spot;
                cornerCount++;
            }
        }

        if (midCount != 1 && cornerCount != 1)
            return null;

        L[] result = new L[3];
        int index = 0;
        foreach (L spot in mids)
            if (cells[spot.row, spot.col].current == PlayerOption.NONE)
                if (corner.row == spot.row || corner.col == spot.col)
                {
                    result[index] = spot;
                    index++;
                }
        foreach (L spot in corners)
            if (cells[spot.row, spot.col].current == PlayerOption.NONE)
                if (mid.row == spot.row || mid.col == spot.col)
                {
                    result[index] = spot;
                    index++;
                }
        return result;
    }
    private L Helper_RegularCases(PlayerOption type)    // Check if can win or loose in Vertical, Horizontal, and Diagonal => return null L if none match
    {
        int diagonalLeftCount = 0; L leftLocation = new L();          // Diagonal
        int diagonalRightCount = 0; L rightLocation = new L();
        for (int i = 0; i < 3; i++)
        {
            if (cells[i, i].current == PlayerOption.NONE)   // From Left 
                leftLocation = new L(i, i);
            else if (cells[i, i].current == type)
                diagonalLeftCount++;

            if (cells[i, 2 - i].current == PlayerOption.NONE)// From Right
                rightLocation = new L(i, 2 - i);
            else if (cells[i, 2 - i].current == type)
                diagonalRightCount++;
        }

        if (diagonalLeftCount == 2 && !leftLocation.isNull()) return leftLocation;
        else if (diagonalRightCount == 2 && !rightLocation.isNull()) return rightLocation;

        int horizontalCount = 0; L horizontalLocation = new L();      // Vertical
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (cells[i, j].current == PlayerOption.NONE)
                    horizontalLocation = new L(i, j);
                else if (cells[i, j].current == type)
                    horizontalCount++;
            }
            if (horizontalCount == 2 && !horizontalLocation.isNull())
                return horizontalLocation;

            horizontalCount = 0; horizontalLocation = new L();
        }

        int verticalCount = 0; L verticalLocation = new L();              // Horizontal
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
                if (cells[i, j].current == PlayerOption.NONE)
                    verticalLocation = new L(i, j);
                else if (cells[i, j].current == type)
                    verticalCount++;
            if (verticalCount == 2 && !verticalLocation.isNull())
                return verticalLocation;

            verticalCount = 0; verticalLocation = new L();
        }

        return new L();
    }
    private PlayerOption getOther()
    { return (currentPlayer == PlayerOption.X ? PlayerOption.O : PlayerOption.X); }
    public void MakeOptimalMove()   /* Optimal move apply only for 3x3 table */
    {
        if (GetWinner() != PlayerOption.NONE || moveCount >= 9) return;

        L[] corners = { new L(0, 0), new L(0, 2), new L(2, 0), new L(2, 2) }; // 4 corners
        L[] mids = { new L(0, 1), new L(1, 0), new L(1, 2), new L(2, 1) };  // 4 mids on the sides

        /* No Special case: Vertical, Horizontal, Diagonal */
        L bestLocation = Helper_RegularCases(currentPlayer);     // ATTACK
        if (bestLocation.row == -1 && bestLocation.col == -1)
        {
            bestLocation = Helper_RegularCases(getOther());         // DEFEND
            if (bestLocation.row != -1 && bestLocation.col != -1)
            {
                print("Turn count: " + moveCount + ". No Special case, defend at: " + bestLocation.toString() + ". Possible moves in total: 1");
                ChooseSpace(bestLocation.row, bestLocation.col);
                return;
            }
        }
        else
        {
            print("Turn count: " + moveCount + ". No Special case, attack at: " + bestLocation.toString() + ". Possible moves in total: 1");
            ChooseSpace(bestLocation.row, bestLocation.col);
            return;
        }

        /*
        Special case 0:    Second Turn: If "x" occupied mid => "o" choose one corner  *(moveCount = 1)
        [* * *] 
        [* x *] 
        [* * *] 
         */
        if (moveCount == 1 && cells[1, 1].current == PlayerOption.X)
        {
            L randomMove = corners[Random.Range(0, 4)];
            print("Turn count: " + moveCount + ". Special case 0, defend at: " + randomMove.toString() + ". Possible moves in total: 4");
            ChooseSpace(randomMove.row, randomMove.col);
            return;
        }


        /*
        Special case 1:    Fourth Turn: if "x" is mid and one corner => "o" choose any open corner *(moveCount = 3)
        [* * x]     
        [* x *]
        [o * *]
         */
        if (moveCount == 3)
            if (cells[1, 1].current == PlayerOption.X)
                if (Helper_CountSpecialCase(corners, PlayerOption.X) > 0)
                {
                    L[] openLocations = Helper_GetSpecialCaseOpenSpot(corners);
                    L randomLocation = openLocations[Random.Range(0, openLocations.Length)];
                    print("Turn count: " + moveCount + ". Special case 1, defend at: " + randomLocation.toString() + ". Possible moves in total: " + openLocations.Length);
                    ChooseSpace(randomLocation.row, randomLocation.col);
                    return;
                }

        /*
        Special case 2:    Fourth Turn or higher: If 1 pair match the description => choose any corner except the opposite corner *(moveCount >= 3)     // Haven't create attack version 
        [* x *] [* o *]
        [x o *] [o x *]
        [* * *] [* x *]
         */
        if (moveCount >= 3 && moveCount < 7)
            if (cells[1, 1].current != PlayerOption.NONE)
                if (Helper_CountSpecialCase(mids, getOther()) == 2)         // DEFEND
                {
                    L[] openLocationCase2 = Helper_GetSpecicalCaseLocations(mids, getOther());
                    L specialCase2 = new L(0, 0);
                    for (int i = 0; i < openLocationCase2.Length; i++)  // Calculate which corner it is
                    {
                        specialCase2.row += openLocationCase2[i].row;
                        specialCase2.col += openLocationCase2[i].col;
                    }
                    if (specialCase2.row % 2 != 0 && specialCase2.col % 2 != 0)
                    {
                        L[] openLocations = Helper_GetSpecialCase2OpenSpot(specialCase2, corners);  // Get available corners except the opposite corner
                        L randomLocation = openLocations[Random.Range(0, openLocations.Length)];
                        print("Turn count: " + moveCount + ". Special case 2, defend at: " + randomLocation.toString() + ". Possible moves in total: " + openLocations.Length);
                        ChooseSpace(randomLocation.row, randomLocation.col);
                        return;
                    }
                }

        /*
        Special case 3:    Fourth Turn or higher: if 1 pair match the description => choose any move at mid x || y *(moveCount >= 3)    // Haven't create attack version 
        [* * x]
        [* o *]
        [x * *]
         */
        if (moveCount >= 3 && moveCount < 7)
            if (cells[1, 1].current != PlayerOption.NONE)
                if (Helper_CountSpecialCase(corners, getOther()) == 2)
                {
                    L[] openLocationCase3 = Helper_GetSpecialCaseOpenSpot(corners);

                    L specialCase3 = new L(0, 0);
                    for (int i = 0; i < openLocationCase3.Length; i++)
                    {
                        specialCase3.row += openLocationCase3[i].row;
                        specialCase3.col += openLocationCase3[i].col;
                    }
                    if (specialCase3.row == 2 && specialCase3.col == 2)
                    {
                        L[] openLocations = Helper_GetSpecialCaseOpenSpot(mids);
                        L randomLocation = openLocations[Random.Range(0, openLocations.Length)];
                        print("Turn count: " + moveCount + ". Special case 3, defend at: " + randomLocation.toString() + ". Possible moves in total: " + openLocations.Length);
                        ChooseSpace(randomLocation.row, randomLocation.col);
                        return;
                    }
                }

        /*
        Special case 4:     A combination of case 2 and 3, still haven't create attack version 
        [* o *]
        [* x *]
        [* x o]  
         */
        if (moveCount == 4)
        {
            L[] specialCase4OpenLocation = Helper_GetSpecialCase4OpenSpot(mids, corners, getOther());
            if (specialCase4OpenLocation != null)
            {
                L randomLocation = specialCase4OpenLocation[Random.Range(0, specialCase4OpenLocation.Length)];
                print(moveCount);
                print(randomLocation.toString());
                print(specialCase4OpenLocation.Length);
                print("Turn count: " + moveCount + ". Special case 4, defend at: " + randomLocation.toString() + ". Possible moves in total: " + specialCase4OpenLocation.Length);
                ChooseSpace(randomLocation.row, randomLocation.col);
                return;
            }
        }

        /* Move at a random open spot */
        if (cells[1, 1].current == PlayerOption.NONE)
        {
            print("Turn count: " + moveCount + ". Middle, move at: (1,1). Possible moves in total: 1");
            ChooseSpace(1, 1);
        }
        else
            Helper_MakeRandomMove();
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public void ChooseSpace(int row, int col)   //////////// ADJUSTED TV
    {
        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[row, col].current != PlayerOption.NONE)//////////// ADJUSTED TV
            return;

        // set the cell to the player's mark
        cells[row, col].current = currentPlayer;//////////// ADJUSTED TV

        // update the visual to display X or O
        board.UpdateCellVisual(row, col, currentPlayer);//////////// ADJUSTED TV

        // if there's no winner, keep playing, otherwise end the game
        if (GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
        }
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        moveCount++;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;
        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;
        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for(int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }
}