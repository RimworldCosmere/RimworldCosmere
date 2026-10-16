using Cosmere.Core.Def;

namespace Cosmere.System.Roshar.Def;

public class OrderMinIdeal {
    public int minIdeal;
    public string order = null!;
}

public class SurgebindingAbilityDef : AbilityDef {
    public int minIdeal;
    public List<OrderMinIdeal>? orderMinIdeal;
    public RadiantOrderDef? radiantOrder;
    public bool showInRadial = true;

    public int GetMinIdealForOrder(string orderDefName) {
        if (orderMinIdeal == null) return minIdeal;

        for (int i = 0; i < orderMinIdeal.Count; i++) {
            if (orderMinIdeal[i].order == orderDefName) {
                return orderMinIdeal[i].minIdeal;
            }
        }

        return minIdeal;
    }
}
