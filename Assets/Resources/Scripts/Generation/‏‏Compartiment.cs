using System;

public class Compartiment
{
    private int value;
    private int count;
    private Compartiment next;

    public Compartiment(int value)
    {
        this.value = value;
        this.count = 1;
        this.next = null;
    }

    public void addCount(int count)
    {
        this.count += count;
    }

    public int getCount()
    {
        return this.count;
    }

    public bool isEqual()
    {
        return this.value == this.count;
    }

    public int getValue()
    {
        return this.value;
    }

    public Compartiment getNext()
    {
        return this.next;
    }

    public void setNext(Compartiment compartiment)
    {
        this.next = compartiment;
    }

    public string toString()
    {
        if (this.next != null)
        {
            return "(" + this.value + ", " + this.count + "), " + this.next.toString();
        }
        return "(" + this.value + ", " + this.count + "), null";
    }
}
