using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : LSingleton<EnemyManager> {



    /// <summary>
    /// Gets the square. Return square by row and column
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="col">The column.</param>
    /// <returns></returns>
    Square GetSquare(int row, int col)
    {
        return LevelManager.THIS.GetSquare(row, col);
    }

    //生成敌人
    public void GenerateNewEnemys(bool falling = true)
    {
        int opponentRows = LevelManager.Instance.opponentRows;
        int maxCols = LevelManager.Instance.maxCols;
        int maxRows = LevelManager.Instance.maxRows;
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = 0; row < opponentRows; row++)
            {
                if (GetSquare(col, row) != null)
                {
                    if (!GetSquare(col, row).IsNone() && GetSquare(col, row).CanGoInto() && GetSquare(col, row).item == null)
                    {
                        if ((GetSquare(col, row).item == null && !GetSquare(col, row).IsHaveSolidAbove()) || !falling)
                        {
                            GetSquare(col, row).GenItem(falling);
                        }
                    }
                }
            }
        }
    }


    public void fallingDownEnemy()
    {
        int opponentRows = LevelManager.Instance.opponentRows;
        int maxCols = LevelManager.Instance.maxCols;
        int maxRows = LevelManager.Instance.maxRows;
        //falling down solider
        int beginMineRow = 0;

        for (int i = 0; i < 20; i++)
        {   //just for testing
            for (int col = 0; col < maxCols; col++)
            {
                for (int row = opponentRows - 1; row >= beginMineRow; row--)
                {   //need to enumerate rows from bottom to top
                    if (GetSquare(col, row) != null)
                        GetSquare(col, row).FallOut();
                }
            }
            // yield return new WaitForFixedUpdate();
        }
    }

    public void GenerateEnemySolider()
    {
        int opponentRows = LevelManager.Instance.opponentRows;
        int maxCols = LevelManager.Instance.maxCols;
        int maxRows = LevelManager.Instance.maxRows;
        bool chessColor = false;
        for (int row = 0; row < opponentRows; row++)
        {
            if (maxCols % 2 == 0)
                chessColor = !chessColor;
            for (int col = 0; col < maxCols; col++)
            {
                LevelManager.THIS.CreateSquare(col, row, chessColor);
                chessColor = !chessColor;
            }

        }
    }

        public List<Item> GetEnemyItems()
    {
        int opponentRows = LevelManager.Instance.opponentRows;
        int maxCols = LevelManager.Instance.maxCols;
        int maxRows = LevelManager.Instance.maxRows;
        List<Item> itemsList = new List<Item>();
        for (int row = 0; row < opponentRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                Square square = GetSquare(col, row);
                if (square != null && square.item != null)
                    itemsList.Add(square.item);
            }
        }
        return itemsList;
    }

}
