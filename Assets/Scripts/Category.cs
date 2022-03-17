using System;
using System.Collections.Generic;

[Serializable]
public class Category
{
    public List<Section> Sections = new List<Section>();
    public string Name;

    public Category()
    {

    }
    public Category (string name)
    {
        this.Name = name;
    }
    public override string ToString()
    {
        string str = "Categoria [" + Name + "]";
        foreach (Section sec in Sections)
        {
            str += "\n\t\t" + sec.ToString();
        }
        return str;
    }
    public Section SearchSection(string name)
    {
        foreach (Section sec in Sections)
        {
            if (name == sec.Name) return sec;
        }
        return null;
    }

    public Section AddSection(string section)
    {
        if (SearchSection(section) == null)
        {
            Section temp = new Section(section);
            this.Sections.Add(temp);
            return temp;
        }
        else
            return SearchSection(section);
    }
    public void RemoveSection(string section)
    {
        if (SearchSection(section) != null)
            this.Sections.Remove(SearchSection(section));
    }
}
