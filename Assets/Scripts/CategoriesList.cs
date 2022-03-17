using System;
using System.Collections.Generic;

[Serializable]
public class CategoriesList
{
    public List<Category> Categories = new List<Category>();
    public override string ToString()
    {
        string str = "Categorias:";
        foreach(Category cat in Categories)
        {
            str += "\n\t" + cat.ToString();
        }
        return str;
    }

    public Category FindCategory(string catName)
    {
        foreach(Category cat in Categories)
        {
            if(catName==cat.Name) return cat;
        }
        return null;
    }

    public Section FindSection(string secName)
    {
        foreach(Category cat in Categories)
        {
            if (cat.SearchSection(secName) != null)
                return cat.SearchSection(secName);
        }
        return null;
    }

    public Hobby FindHobby(string hobName)
    {
        foreach(Category cat in Categories)
        {
            foreach(Section sec in cat.Sections)
            {
                if (sec.SearchHobby(hobName) != null)
                    return sec.SearchHobby(hobName);
            }
        }
        return null;
    }

    public Section FindSectionForHobby(string hobName)
    {
        foreach (Category cat in Categories)
        {
            foreach (Section sec in cat.Sections)
            {
                if (sec.SearchHobby(hobName) != null)
                    return sec;
            }
        }
        return null;
    }

    public Category FindCategoryForSection(string secName)
    {
        foreach (Category cat in Categories)
        {
            if (cat.SearchSection(secName) != null)
                return cat;
        }
        return null;
    }

    public Category AddCategory(string category)
    {
        if (FindCategory(category) == null)
        {
            Category temp = new Category(category);
            this.Categories.Add(temp);
            return temp;
        }
        else
            return FindCategory(category);
    }
    public void RemoveCategory(string category)
    {
        if (FindCategory(category) != null)
            this.Categories.Remove(FindCategory(category));
    }
    public Hobby AddHobby(string category, string section, string hobby)
    {
        Category tempCat = AddCategory(category);

        Section tempSec = tempCat.AddSection(section);

        return tempSec.AddHobby(hobby);
    }
    public void RemoveHobby(string hobby)
    {
        if (FindSectionForHobby(hobby) == null) return;
        if (FindCategoryForSection(FindSectionForHobby(hobby).Name) == null) return;

        Category tempCat = FindCategoryForSection(FindSectionForHobby(hobby).Name);
        Section tempSec = FindSectionForHobby(hobby);

        tempSec.RemoveHobby(hobby);

        // Borrar sección vacía
        if (tempSec.Hobbies.Count == 0)
            tempCat.RemoveSection(tempSec.Name);

        //Borrar categoria vacía
        if (tempCat.Sections.Count == 0)
            RemoveCategory(tempCat.Name);
    }
    public int CountHobbies()
    {
        int temp = 0;
        foreach (Category cat in Categories)
        {
            foreach (Section sec in cat.Sections)
            {
                foreach (Hobby hob in sec.Hobbies) temp++;
            }
        }
        return temp;
    }

    public string[] GetHobbies()
    {
        List<string> results = new List<string>();
        foreach (Category cat in Categories)
        {
            foreach (Section sec in cat.Sections)
            {
                foreach(Hobby hob in sec.Hobbies)
                {
                    results.Add(hob.name);
                }
            }
        }
        return results.ToArray();
    }

    public string[] GetBlankSections()
    {
        List<string> results = new List<string>();
        foreach (Category cat in Categories)
        {
            foreach (Section sec in cat.Sections)
            {
                if(sec.Hobbies.Count == 0)
                    results.Add(sec.Name);
            }
        }
        return results.ToArray();
    }

    public string[] GetBlankCategories()
    {
        List<string> results = new List<string>();
        foreach (Category cat in Categories)
        {
            if (cat.Sections.Count == 0)
                results.Add(cat.Name);
        }
        return results.ToArray();
    }

    public CategoriesList CommonList(CategoriesList other)
    {
        CategoriesList result = new CategoriesList();

        foreach(Category cat in this.Categories)
        {
            if (other.FindCategory(cat.Name) != null)
            {
                Category resCat = result.AddCategory(cat.Name);
                foreach(Section sec in cat.Sections)
                {
                    if(other.FindSection(sec.Name) != null)
                    {
                        Section resSec = resCat.AddSection(sec.Name);
                        foreach(Hobby hob in sec.Hobbies)
                        {
                            if(other.FindHobby(hob.name) != null)
                            {
                                resSec.AddHobby(hob.name);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
}
