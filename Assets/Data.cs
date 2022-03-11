using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Data : MonoBehaviourPun
{
    [SerializeField] public GameObject[] squares;

    private readonly int square_size = 10;
    private int size;

    private readonly Tuple<int, int>[] directions = new Tuple<int, int>[4] {
        new Tuple<int, int>(0, -1),
        new Tuple<int, int>(-1, 0),
        new Tuple<int, int>(0, 1),
        new Tuple<int, int>(1, 0)
    };

    public void setTable(int tableSize, string tableString, bool isMaster)
    {
        Debug.Log("IsMaster: " + isMaster + ". TableString" + tableString);
        this.photonView.RPC("InstantiateMazeInActualClient", RpcTarget.All, tableString, tableSize);
    }

    private bool IsOutOfBounds(int row, int column)
    {
        return row >= size
            || row < 0
            || column >= size
            || column < 0;
    }

    [PunRPC]
    public void InstantiateMazeInActualClient(string tableString, int tableSize)
    {
        Debug.Log("Methode : InstantiateMazeInActualClient started");
        size = tableSize;
        int[,] table = stringToTable(tableString);
        InstantiateMaze(table);
    }

    private int[,] stringToTable(string tableString)
    {
        int[,] table = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                table[i, j] = int.Parse(tableString[i * size + j].ToString());
            }
        }
        return table;
    }

    private void InstantiateMaze(int[,] table)
    {
        for (int row = 0; row < table.GetLength(0); row++)
        {
            for (int column = 0; column < table.GetLength(0); column++)
            {
                if (table[row, column] == 0) { continue; }

                int[] num_of_item = new int[4] { 1, 1, 1, 1 };
                for (int i = 0; i < directions.Length; i++)
                {
                    (int x, int y) = directions[i];
                    if (!IsOutOfBounds(row + x, column + y) && table[row + x, column + y] != 0)
                    {
                        num_of_item[i] = 0;
                    }
                }
                InstantiateSquare(row, column, table.GetLength(0), num_of_item);
            }
        }
    }

    private void InstantiateSquare(int row, int column, int size, int[] walls)
    {
        int num_of_walls = 0;
        int adjacent_walls = 0;
        int prefab_place = 0;
        int rotation = 0;
        for (int i = 0; i < walls.Length; i++)
        {
            num_of_walls += walls[i];
            if ((i == 0 && walls[i] == 1 && walls[3] == 1) || (i > 0 && walls[i] == 1 && walls[i - 1] == 1))
            {
                adjacent_walls++;
            }
        }
        if (num_of_walls == 1)
        {
            prefab_place = 0;
            foreach (int possible_wall in walls)
            {
                if (possible_wall == 1)
                {
                    break;
                }
                rotation += 90;
            }
        }
        else if (num_of_walls == 2)
        {
            if (adjacent_walls == 0)
            {
                prefab_place = 2;
                if (walls[0] == 0)
                {
                    rotation = 90;
                }
            }
            else
            {
                int zeros_before_first_wall = 0;
                prefab_place = 1;
                foreach (int item in walls)
                {
                    if (item == 0)
                    {
                        zeros_before_first_wall += 1;
                    }
                    else
                    {
                        if (zeros_before_first_wall == 0)
                        {
                            rotation = 180;
                            if (walls[3] == 1)
                            {
                                rotation -= 90;
                            }
                            break;
                        }
                        else
                        {
                            rotation = 360 - (2 - zeros_before_first_wall) * 90;
                            break;
                        }
                    }
                }
            }
        }
        else if (num_of_walls == 3)
        {
            prefab_place = 3;
            rotation = -90;
            foreach (int item in walls)
            {
                if (item == 0)
                {
                    break;
                }
                rotation += 90;
            }
        }
        else
        {
            prefab_place = 4;
        }
        InstantiateMaze(row, column, rotation, prefab_place, size);
    }

    public void InstantiateMaze(int row, int column, int rotation, int prefab_place, int size)
    {
        int initial_place = ((-size / 2) * square_size) + (square_size / 2) - ((square_size / 2) * (size % 2));
        int place_x = initial_place + row * square_size;  // vertical = x
        int place_z = initial_place + column * square_size;  // horizontale = z
        GameObject square = Instantiate(squares[prefab_place], new Vector3(place_x, 0, place_z), Quaternion.identity);
        square.transform.SetParent(GameObject.FindWithTag("Map").transform);
        square.transform.Rotate(0, rotation, 0);
    }
}
