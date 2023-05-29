
namespace Roguelike;

internal class Equipment
{
    public bool TwoHanded { get; private set; }

    public Weapon? LeftHand { get; set; } = null;
    public Weapon? RightHand { get; set; } = null;
}
