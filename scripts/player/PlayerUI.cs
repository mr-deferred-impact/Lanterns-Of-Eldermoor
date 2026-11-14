using Godot;

public partial class PlayerUI : Control {
    #region DeclareNodes
    private Panel inventoryPanel;
    #endregion

    public override void _Ready() {
        InitializeNodes();
    }

    public override void _UnhandledInput(InputEvent @event) {
        if (GlobalData.IsMouseClicked(@event)) {
            if (inventoryPanel.Visible) {
                inventoryPanel.Hide();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void InitializeNodes() {
        inventoryPanel = GetNode<Panel>("%InventoryPanel");
    }

    private void OnInventoryButtonPressed() {
        inventoryPanel.Visible = !inventoryPanel.Visible;
    }

    private void OnCloseButtonPressed() {
        inventoryPanel.Hide();
    }
}
