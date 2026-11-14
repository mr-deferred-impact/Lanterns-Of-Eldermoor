using Godot;

public partial class MainMenu : Control {
    #region DeclareNodes
    private Panel menuPanel;
    #endregion

    public override void _Ready() {
        DeclareNodes();
    }

    private void DeclareNodes() {
        menuPanel = GetNode<Panel>("%MenuPanel");
    }

    private void OnMenuButtonPressed() {
        menuPanel.Visible = !menuPanel.Visible;
    }

    private void OnCloseButtonPressed() {
        menuPanel.Hide();
    }
}
