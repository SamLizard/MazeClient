using System;
using System.Linq;

public class Point
{
    private bool[] directions;
    private bool connection; 
    private Compartiment compartiment;
    private int remainingDirections;

    public Point(bool connection, int compartiment, int row, int column, int tableLength)
    {
        this.directions = new bool[4] { column != 0, row != 0, column != tableLength - 1, row != tableLength - 1 };
        this.connection = connection;
        this.compartiment = new Compartiment(compartiment);
        this.remainingDirections = this.directions.Count(direction => direction) == 2 ? 2 : 3; ;
    }

    public bool hasRemainingConnections()
    {
        return this.remainingDirections > 0;
    }

    public void setConnection()
    {
        this.connection = true;
    }

    public bool getConnection()
    {
        return this.connection;
    }

    public void setDirections(int place)
    {
        this.directions[place] = false;
        this.remainingDirections--;
    }

    public bool[] getDirections()
    {
        return this.directions;
    }

    public Compartiment getCompartiment()
    {
        return this.compartiment;
    }

    public void setCompartiment(Compartiment compartiment)
    {
        this.compartiment = compartiment;
    }

    public string toString()
    {
        return "[(" + this.directions[0] + ", " + this.directions[1] + ", " + this.directions[2] + ", " + this.directions[3] + ") " + this.connection + ", " + this.remainingDirections + ", " + this.compartiment.toString() + "]";
    }

    public bool canConnect(int direction)
    {
        return this.directions[direction];
    }
}