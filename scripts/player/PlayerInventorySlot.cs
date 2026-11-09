using Godot;
using System;

[GlobalClass]
public partial class PlayerInventorySlot : Button {
    public event EventHandler<OnItemSelectedEventArgs> OnItemSelected;

    public class OnItemSelectedEventArgs : EventArgs {
        public Item item;
    }

    public enum Item {
        Sword,
        Lantern,
    }

    [Export] private Item item;

    public override void _Ready() {
        Pressed += OnPressed;
    }

    private void OnPressed() {
        OnItemSelected?.Invoke(this, new OnItemSelectedEventArgs { item = item });
    }
}
