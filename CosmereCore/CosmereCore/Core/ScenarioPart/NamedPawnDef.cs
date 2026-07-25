using System.Xml;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class NamedPawnDef {
    public string? adulthood;
    public int age = 20;
    public int chronologicalAge = -1;
    public string? childhood;
    public string? firstName;
    public bool fullFeruchemist;
    public Gender gender = Gender.None;
    public List<string> genes = [];
    public int idealLevel;
    public List<NamedPawnInventoryEntry> inventory = [];
    public string? lastName;
    public bool mistborn;
    public string? nickName;
    public bool noRandomTraits;
    public string? radiantOrder;
    public List<NamedPawnSkillEntry> skills = [];
    public List<NamedPawnTraitEntry> traits = [];
    public string? xenotype;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot) {
        foreach (XmlNode node in xmlRoot.ChildNodes) {
            if (node.NodeType != XmlNodeType.Element) continue;

            switch (node.Name) {
                case "firstName":
                    firstName = node.InnerText;
                    break;
                case "nickName":
                    nickName = node.InnerText;
                    break;
                case "lastName":
                    lastName = node.InnerText;
                    break;
                case "age":
                    if (!int.TryParse(node.InnerText, out age))
                        Logger.Warning($"NamedPawnDef: invalid age value '{node.InnerText}'");
                    break;
                case "chronologicalAge":
                    if (!int.TryParse(node.InnerText, out chronologicalAge))
                        Logger.Warning($"NamedPawnDef: invalid chronologicalAge value '{node.InnerText}'");
                    break;
                case "gender":
                    gender = (Gender)ParseHelper.FromString(node.InnerText, typeof(Gender));
                    break;
                case "xenotype":
                    xenotype = node.InnerText;
                    break;
                case "childhood":
                    childhood = node.InnerText;
                    break;
                case "adulthood":
                    adulthood = node.InnerText;
                    break;
                case "noRandomTraits":
                    if (!bool.TryParse(node.InnerText, out noRandomTraits))
                        Logger.Warning($"NamedPawnDef: invalid noRandomTraits value '{node.InnerText}'");
                    break;
                case "traits":
                    traits = DirectXmlToObject.ObjectFromXml<List<NamedPawnTraitEntry>>(node, false);
                    break;
                case "skills":
                    skills = DirectXmlToObject.ObjectFromXml<List<NamedPawnSkillEntry>>(node, false);
                    break;
                case "genes":
                    genes = DirectXmlToObject.ObjectFromXml<List<string>>(node, false);
                    break;
                case "radiantOrder":
                    radiantOrder = node.InnerText;
                    break;
                case "idealLevel":
                    if (!int.TryParse(node.InnerText, out idealLevel))
                        Logger.Warning($"NamedPawnDef: invalid idealLevel value '{node.InnerText}'");
                    break;
                case "mistborn":
                    if (!bool.TryParse(node.InnerText, out mistborn))
                        Logger.Warning($"NamedPawnDef: invalid mistborn value '{node.InnerText}'");
                    break;
                case "fullFeruchemist":
                    if (!bool.TryParse(node.InnerText, out fullFeruchemist))
                        Logger.Warning($"NamedPawnDef: invalid fullFeruchemist value '{node.InnerText}'");
                    break;
                case "inventory":
                    inventory = DirectXmlToObject.ObjectFromXml<List<NamedPawnInventoryEntry>>(node, false);
                    break;
            }
        }
    }

    public Name? GetName() {
        if (firstName != null && lastName != null) {
            return new NameTriple(firstName, nickName ?? firstName, lastName);
        }

        if (firstName != null) {
            return new NameSingle(nickName != null ? $"{firstName} \"{nickName}\"" : firstName);
        }

        return null;
    }

    public int GetChronologicalAge() {
        return chronologicalAge > 0 ? chronologicalAge : age;
    }
}

public class NamedPawnTraitEntry {
    public string? def;
    public int degree;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot) {
        foreach (XmlNode node in xmlRoot.ChildNodes) {
            if (node.NodeType != XmlNodeType.Element) continue;

            switch (node.Name) {
                case "def":
                    def = node.InnerText;
                    break;
                case "degree":
                    if (!int.TryParse(node.InnerText, out degree))
                        Logger.Warning($"NamedPawnTraitEntry: invalid degree value '{node.InnerText}'");
                    break;
            }
        }
    }
}

public class NamedPawnSkillEntry {
    public string? def;
    public int level;
    public Passion passion = Passion.None;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot) {
        foreach (XmlNode node in xmlRoot.ChildNodes) {
            if (node.NodeType != XmlNodeType.Element) continue;

            switch (node.Name) {
                case "def":
                    def = node.InnerText;
                    break;
                case "level":
                    if (!int.TryParse(node.InnerText, out level))
                        Logger.Warning($"NamedPawnSkillEntry: invalid level value '{node.InnerText}'");
                    break;
                case "passion":
                    passion = (Passion)ParseHelper.FromString(node.InnerText, typeof(Passion));
                    break;
            }
        }
    }
}

public class NamedPawnInventoryEntry {
    public int count = 1;
    public string? stuff;
    public string? thing;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot) {
        foreach (XmlNode node in xmlRoot.ChildNodes) {
            if (node.NodeType != XmlNodeType.Element) continue;

            switch (node.Name) {
                case "thing":
                    thing = node.InnerText;
                    break;
                case "stuff":
                    stuff = node.InnerText;
                    break;
                case "count":
                    if (!int.TryParse(node.InnerText, out count))
                        Logger.Warning($"NamedPawnInventoryEntry: invalid count value '{node.InnerText}'");
                    break;
            }
        }
    }
}