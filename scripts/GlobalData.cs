using Godot;

public abstract partial class GlobalData : Node {
    public static bool IsMouseClicked(InputEvent @event) {
        return (
            @event is InputEventMouseButton mouseButtonEvent &&
            mouseButtonEvent.ButtonIndex == MouseButton.Left &&
            mouseButtonEvent.IsPressed()
        );
    }
}
