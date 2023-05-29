
using System.Diagnostics;

namespace Roguelike;

internal class Weapon
{
    public string Name { get; private set; }
    public string Description { get; set; } = string.Empty;

    public Dice Damage { get; set; }

    public Effect[] Effects { get; set; } = Array.Empty<Effect>();

    public Weapon(string name, string dice)
    {
        Name = name;
        Damage = Dice.Create(dice);
    }

    public Weapon(string name, string description, string dice)
    {
        Name = name;
        Description = description;
        Damage = Dice.Create(dice);
    }

    [DebuggerDisplay("{Name} {Dice}")]
    internal record Effect
    {
        public string Name { get; init; }
        public Dice Dice { get; set; }

        public Effect(string name, string dice)
        {
            Name = name;
            Dice = Dice.Create(dice);
        }

        public override string ToString()
        {
            return $"{Name} {Dice}";
        }
    }
}
