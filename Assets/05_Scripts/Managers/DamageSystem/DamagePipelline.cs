using System.Collections.Generic;
// 구현된 Modifier를 통해 데미지 계산

public class DamagePipeline
{
    List<IDamageModifier> modifiers = new();

    public void Add(IDamageModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public DamageResult Calculate(DamageContext context)
    {
        DamageResult res = new()
        {
            finalDamage = context.damage
        };

        for (int i = 0; i < modifiers.Count; ++i)
        {
            modifiers[i].Modify(ref context, ref res);
        }

        return res;
    }
}