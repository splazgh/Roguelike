
namespace Roguelike;

internal static class DiceExt
{
    public static int Throw(this Dice[] dices)
    {
        return dices.Sum(dice => dice.Throw());
    }

    public static int Throw(this List<Dice> dices)
    {
        return dices.Sum(dice => dice.Throw());
    }
}
