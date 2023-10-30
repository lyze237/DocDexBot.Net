using System.Web;

namespace DocDexBot.Net.Interactions;

public class WikiLink
{
    public string Name { get; }
    public string? Link { get; }

    private List<WikiLink> Children { get; } = new();
    
    public WikiLink? Parent { get; private set; }

    public WikiLink(string name, string? link = null) =>
        (Name, Link) = (name, link);

    public void AddChildren(IEnumerable<WikiLink> children)
    {
        foreach (var child in children)
        {
            Children.Add(child);
            child.Parent = this;
        }
    }

    public string GetFullName()
    {
        var name = HttpUtility.HtmlDecode(Name);

        var parent = Parent;
        while (parent != null)
        {
            name = parent.Name + " / " + name;
            parent = parent.Parent;
        }

        return name;
    }

    public List<WikiLink> GetAllChildren()
    {
        var allChildren = GetAllChildren(this);
        allChildren.Add(this);
        
        return allChildren;
    }

    private static List<WikiLink> GetAllChildren(WikiLink currentNode)
    {
        var list = new List<WikiLink>();
        
        foreach (var child in currentNode.Children)
        {
            list.Add(child);
            list.AddRange(GetAllChildren(child));
        }
        
        return list;
    }
}