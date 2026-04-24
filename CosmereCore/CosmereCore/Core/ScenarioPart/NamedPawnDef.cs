using System.Xml;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class NamedPawnDef {
    public int age = 20;
    public int chronologicalAge = -1;
    public string? firstName;
    public bool fullFeruchemist;
    public Gender gender = Gender.None;
    public List<string> genes = [];
    public int idealLevel;
    public List<NamedPawnInventoryEntry> inventory = [];
    public string? lastName;
    public bool mistborn;
    public string? nickName;
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
                    age = int.Parse(node.InnerText);
                    break;
                case "chronologicalAge":
                    chronologicalAge = int.Parse(node.InnerText);
                    break;
                case "gender":
                    gender = (Gender)ParseHelper.FromString(node.InnerText, typeof(Gender));
                    break;
                case "xenotype":
                    xenotype = node.InnerText;
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
                    idealLevel = int.Parse(node.InnerText);
                    break;
                case "mistborn":
                    mistborn = bool.Parse(node.InnerText);
                    break;
                case "fullFeruchemist":
                    fullFeruchemist = bool.Parse(node.InnerText);
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
                    degree = int.Parse(node.InnerText);
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
                    level = int.Parse(node.InnerText);
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
                    count = int.Parse(node.InnerText);
                    break;
            }
        }
    }
}