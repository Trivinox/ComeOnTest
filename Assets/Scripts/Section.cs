using System;
using System.Collections.Generic;

[Serializable]
public class Section
{
    public List<Hobby> Hobbies = new List<Hobby>();
    public string Name;

    public Section()
    {
        
    }
    public Section (string name)
    {
        this.Name = name;
    }
    public override string ToString()
    {
        string str = "Sección [" + Name + "]";
        foreach (Hobby hob in Hobbies)
        {
            str += "\n\t\t\t" + hob.ToString();
        }
        return str;
    }
    public Hobby SearchHobby(string name)
    {
        foreach (Hobby hob in Hobbies)
        {
            if (name == hob.name) return hob;
        }
        return null;
    }

    public Hobby AddHobby(string hobby)
    {
        if (SearchHobby(hobby) == null)
        {
            Hobby temp = new Hobby(hobby);
            this.Hobbies.Add(temp);
            return temp;
        }
        else
            return SearchHobby(hobby);
    }

    public void RemoveHobby(string hobby)
    {
        if (SearchHobby(hobby) != null)
            this.Hobbies.Remove(SearchHobby(hobby));
    }
}
