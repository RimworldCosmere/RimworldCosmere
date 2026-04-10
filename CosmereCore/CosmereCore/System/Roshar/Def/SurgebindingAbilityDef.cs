using Cosmere.Core.Def;

namespace Cosmere.System.Roshar.Def;

public class OrderMinIdeal {
    public string order = null!;
    public int minIdeal;
}

public class SurgebindingAbilityDef : AbilityDef {
    public RadiantOrderDef? radiantOrder;
    public int minIdeal;
    public List<OrderMinIdeal>? orderMinIdeal;

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
