using System;

[Serializable]
public class Hobby
{
    public string name;

    public Hobby()
    {

    }
    public Hobby (string name)
    {
        this.name = name;
    }
    public override string ToString()
    {
        return this.name;
    }
}
