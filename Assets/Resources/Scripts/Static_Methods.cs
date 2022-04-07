using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Static_Methods
{
    public static GameObject[] squares = new GameObject[5] {
        Resources.Load("1_Wall") as GameObject,
        Resources.Load("2_Walls_Adjacent") as GameObject,
        Resources.Load("2_Walls_Opposite") as GameObject,
        Resources.Load("3_Walls") as GameObject,
        Resources.Load("wall") as GameObject
    };
    private static readonly int square_size = 10;
    public static int size;
    private static readonly Tuple<int, int>[] directions = new Tuple<int, int>[4] {
        new Tuple<int, int>(0, -1),
        new Tuple<int, int>(-1, 0),
        new Tuple<int, int>(0, 1),
        new Tuple<int, int>(1, 0)
    };
    private static readonly int[][] RandomDirections = new int[24][] {
        new int[4] {0, 1, 2, 3 }, new int[4] {0, 1, 3, 2 }, new int[4] {0, 2, 1, 3 }, new int[4] {0, 2, 3, 1 }, new int[4] {0, 3, 2, 1 }, new int[4] {0, 3, 1, 2 },
        new int[4] {1, 0, 2, 3 }, new int[4] {1, 0, 3, 2 }, new int[4] {1, 2, 0, 3 }, new int[4] {1, 2, 3, 0 }, new int[4] {1, 3, 2, 0 }, new int[4] {1, 3, 0, 2 },
        new int[4] {2, 0, 1, 3 }, new int[4] {2, 0, 3, 1 }, new int[4] {2, 1, 3, 0 }, new int[4] {2, 1, 0, 3 }, new int[4] {2, 3, 0, 1 }, new int[4] {2, 3, 1, 0 },
        new int[4] {3, 0, 2, 1 }, new int[4] {3, 0, 1, 2 }, new int[4] {3, 1, 2, 0 }, new int[4] {3, 1, 0, 2 }, new int[4] {3, 2, 1, 0 }, new int[4] {3, 2, 0, 1 },
    };

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public static void InstantiateMaze(int[,] table)
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

    public static void InstantiateSquare(int row, int column, int size, int[] walls)
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

    public static void InstantiateMaze(int row, int column, int rotation, int prefab_place, int size)
    {
        int initial_place = ((-size / 2) * square_size) + (square_size / 2) - ((square_size / 2) * (size % 2));
        int place_x = initial_place + row * square_size;  // vertical = x
        int place_z = initial_place + column * square_size;  // horizontale = z
        GameObject square = UnityEngine.Object.Instantiate(squares[prefab_place], new Vector3(place_x, 0, place_z), Quaternion.Euler(0, rotation, 0), GameObject.FindWithTag("Map").transform);
    }

    public static bool IsOutOfBounds(int row, int column)
    {
        return row >= size
            || row < 0
            || column >= size
            || column < 0;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static int[,] CreateMazeList()
    {
        Debug.Log("Size: " + size + ".\nStart164#TIME" + DateTime.Now.ToString("hh:mm:ss.fff"));
        DateTime nowTime = DateTime.Now;
        int[,] table = InitialBoard(size); // integrate this in the Point class
        Point[,] tablePoint = BuildListOfPoint(size);
        int length = tablePoint.GetLength(0);
        int PointRow = UnityEngine.Random.Range(0, length);
        int PointColumn = UnityEngine.Random.Range(0, length);
        int actualDirection;
        Compartiment lastCompartiment = tablePoint[length - 1, length - 1].getCompartiment();

        table = ConnectEveryPoint(table, tablePoint); // connect every point one time
        while (!lastCompartiment.isEqual())
        {
            actualDirection = -1;
            while (actualDirection == -1)
            {
                PointRow = UnityEngine.Random.Range(0, length);
                PointColumn = UnityEngine.Random.Range(0, length);
                if (tablePoint[PointRow, PointColumn].hasRemainingConnections())
                {
                    actualDirection = ChoosePointDirection(tablePoint[PointRow, PointColumn].getDirections(), tablePoint, PointRow, PointColumn);
                }
            }
            UpdatePointTable(tablePoint, actualDirection, PointRow, PointColumn);
            table = UpdateTable(table, actualDirection, PointRow, PointColumn);
        }
        // substract the from the actual time : nowTime - DateTime.Now
        Debug.Log("End189#TIME" + DateTime.Now.ToString("hh:mm:ss.fff") + "\nTime passed: " + (DateTime.Now - nowTime).TotalMilliseconds + " ms.");
        return table;
    }

    public static int[,] InitialBoard(int size)
    {
        int[,] table = new int[size + 1 - size % 2, size + 1 - size % 2];
        for (int i = 0; i < size; i += 2)
        {
            for (int j = 0; j < size; j += 2)
            {
                table[i, j] = 1;
            }
        }
        return table;
    }

    public static int[,] UpdateTable(int[,] table, int actualDirection, int row, int column)
    {
        row *= 2;
        column *= 2;
        table[row, column] = 1;
        (int x, int y) = directions[actualDirection];
        table[row + x, column + y] = 1;
        table[row + 2 * x, column + 2 * y] = 1;
        return table;
    }

    public static Point[,] BuildListOfPoint(int size)
    {
        int tableLength = (int)Math.Ceiling(size / 2.0);
        Point[,] table = new Point[tableLength, tableLength];
        for (int i = 0; i < tableLength; i++)
        {
            for (int j = 0; j < tableLength; j++)
            {
                table[i, j] = new Point(false, i * tableLength + j + 1, i, j, tableLength);
            }
        }
        return table;
    }

    public static int[,] ConnectEveryPoint(int[,] table, Point[,] tablePoint)
    {
        int tableLength = tablePoint.GetLength(0);
        for (int row = 0; row < tableLength; row++)
        {
            for (int column = 0; column < tableLength; column++)
            {
                if (!tablePoint[row, column].getConnection())
                {
                    int actualDirection = ChoosePointDirection(tablePoint[row, column].getDirections(), tablePoint, row, column);
                    UpdatePointTable(tablePoint, actualDirection, row, column);
                    table = UpdateTable(table, actualDirection, row, column);
                }
            }
        }
        return table;
    }

    public static int ChoosePointDirection(bool[] pointDirections, Point[,] tablePoint, int row, int column)
    {
        int directionIndex = UnityEngine.Random.Range(0, 23);
        foreach (int direction in RandomDirections[directionIndex])
        {
            (int x, int y) = directions[direction];
            if (tablePoint[row, column].canConnect(direction) && tablePoint[row + x, column + y].hasRemainingConnections())
            {
                return direction;
            }
        }
        return -1;
    }

    public static void UpdatePointCompartiment(Point actualPoint, Point pointedPoint, Point[,] tablePoint)
    {
        int biggestCompartimentValue = Math.Max(actualPoint.getCompartiment().getValue(), pointedPoint.getCompartiment().getValue());
        if (actualPoint.getCompartiment().getValue() == biggestCompartimentValue)
        {
            Point actualPointToTransfer = actualPoint;
            actualPoint = pointedPoint;
            pointedPoint = actualPointToTransfer;
        }
        if (!actualPoint.getConnection() && !pointedPoint.getConnection())
        {
            actualPoint.setCompartiment(pointedPoint.getCompartiment());
            pointedPoint.getCompartiment().addCount(1);
        }
        else
        {
            Compartiment actualPointLastCompartiment = ConnectToLastCompartimentOf(actualPoint.getCompartiment(), 0);
            Compartiment pointedPointLastCompartiment = ConnectToLastCompartimentOf(pointedPoint.getCompartiment(), 0);
            ConnectCompartiments(actualPointLastCompartiment, pointedPointLastCompartiment);
        }
    }

    public static void UpdatePointTable(Point[,] tablePoint, int actualDirection, int row, int column)
    {
        Point actualPoint = tablePoint[row, column];
        (int x, int y) = directions[actualDirection];
        Point pointedPoint = tablePoint[row + x, column + y];
        UpdatePointCompartiment(actualPoint, pointedPoint, tablePoint);
        actualPoint.setConnection();
        pointedPoint.setConnection();
        actualPoint.setDirections(actualDirection);
        pointedPoint.setDirections(InverseDirection(actualDirection));
    }

    public static int InverseDirection(int actualDirection)
    {
        return (actualDirection + 2) % 4;
    }

    public static Compartiment ConnectToLastCompartimentOf(Compartiment actualCompartiment, int count)
    {
        if (actualCompartiment.getNext() != null)
        {
            Compartiment lastCompartiment = ConnectToLastCompartimentOf(actualCompartiment.getNext(), actualCompartiment.getCount());
            actualCompartiment.setNext(lastCompartiment);
            actualCompartiment.addCount(-count);
            return lastCompartiment;
        }
        return actualCompartiment;
    }

    public static void ConnectCompartiments(Compartiment first, Compartiment second)
    {
        if (first.getValue() > second.getValue())
        {
            first.addCount(second.getCount());
            second.setNext(first);
        }
        else
        {
            if (second.getValue() > first.getValue())
            {
                second.addCount(first.getCount());
                first.setNext(second);
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public static void InstansiateTrophies(int numOfThrophies, int[,] table, GameObject Trophy)
    {
        int tablePointSize = (table.GetLength(0) - 1) / 2;
        if (numOfThrophies < (Math.Pow(tablePointSize, 2) / 3))
        {
            int initial_place = (((-size / 2)) * square_size);
            for (int i = 0; i < numOfThrophies; i++)
            {
                int row = UnityEngine.Random.Range(0, tablePointSize);
                int column = UnityEngine.Random.Range(0, tablePointSize);
                if (table[row, column] == 1)
                {
                    GameObject trophy = UnityEngine.Object.Instantiate(Trophy, new Vector3(initial_place + (row * 2 * square_size), 0.4f, initial_place + (column * 2 * square_size)), Quaternion.identity);
                    trophy.transform.localScale = new Vector3(5, 5, 5);
                    table[row, column] = 2;
                }
                else
                {
                    i--;
                }
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static int[,] stringToTable(string tableString, int tableSize)
    {
        size = tableSize;
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

    public static string TableToString(int[,] table)
    {
        string tableString = "";
        for (int i = 0; i < table.GetLength(0); i++)
        {
            for (int j = 0; j < table.GetLength(0); j++)
            {
                tableString += table[i, j].ToString();
            }
        }
        return tableString;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
