using System.Collections.Generic;
using Godot;

public partial class SegmentedTabs : Control
{
    private Control _indicator;
    private HBoxContainer _row;
    private Button[] _buttons;
    private Dictionary<Button, Color> _col_dict;
    private Tween _tween;
    private bool _waitingForFrame;

    public override void _Ready()
    {
        _indicator = GetNode<Control>("Indicator");
        _row = GetNode<HBoxContainer>("Row");
        _buttons = new[]
        {
            GetNode<Button>("%TabBuildings"),
            GetNode<Button>("%TabUpgrades"),
            GetNode<Button>("%TabPrestige")
        };
        _col_dict = new Dictionary<Button, Color>
        {
            {_buttons[0],new Color("#e18782")},
            {_buttons[1],new Color("#82e187")},
            {_buttons[2],new Color("#b682e1")}
        };
        foreach (var button in _buttons)
            button.Toggled += pressed => { if (pressed) MoveIndicator(true); };
        Resized += () => MoveIndicator(false);
        _waitingForFrame = true;
        GetTree().ProcessFrame += PlaceOnFirstFrame;
    }

    public override void _ExitTree()
    {
        if (_waitingForFrame)
        {
            GetTree().ProcessFrame -= PlaceOnFirstFrame;
            _waitingForFrame = false;
        }
    }

    private void PlaceOnFirstFrame()
    {
        _waitingForFrame = false;
        GetTree().ProcessFrame -= PlaceOnFirstFrame;
        MoveIndicator(false);
    }

    private void MoveIndicator(bool animate)
    {
        Button active = null;
        foreach (var button in _buttons)
        {
            if (button.ButtonPressed)
            {
                active = button;
                break;
            }
        }
        if (active == null) return;
        Rect2 target = active.GetRect();
        target.Position += _row.Position;
        _tween?.Kill();
        if (!animate)
        {
            _indicator.Position = target.Position;
            _indicator.Size = target.Size;
            return;
        }
        _tween = CreateTween().SetParallel(true);
        _tween.TweenProperty(_indicator, "position", target.Position, 0.22)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(_indicator, "size", target.Size, 0.22)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(_indicator, "self_modulate", _col_dict[active], 0.22)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }
}
