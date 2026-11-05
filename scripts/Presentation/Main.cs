using Godot;

public partial class Main : Node2D
{
    private GameState _gs;

    private Label _wood, _stone, _food, _gold;
    private Control _buildPanel, _upgradePanel, _prestigePanel;
    private Node3D _castle;
    private Tween _punchTween;
	private Control _buildList;

    public override void _Ready()
    {
        _gs = GetNode<GameState>("/root/GameState");
		_buildList = GetNode<Control>("%BuildList");
        _wood = GetNode<Label>("%Wood");
        _stone = GetNode<Label>("%Stone");
        _food = GetNode<Label>("%Food");
        _gold = GetNode<Label>("%Gold");
		//лейблы ресурсов
        _buildPanel = GetNode<Control>("%BuildPanel");
        _upgradePanel = GetNode<Control>("%UpgradePanel");
        _prestigePanel = GetNode<Control>("%PrestigePanel");
		//панели (строительство, улучшения, престиж)
        _castle = GetNode<Node3D>("World/Castle");
		//замок
        GetNode<Button>("%TabBuildings").Pressed += () => ShowTab(0);
        GetNode<Button>("%TabUpgrades").Pressed += () => ShowTab(1);
        GetNode<Button>("%TabPrestige").Pressed += () => ShowTab(2);
		//кнопки переключения панелей
        _gs.Changed += Refresh;
        ShowTab(0);//вызываю сразу показ панели с билдами
        Refresh();
		var rowScene = GD.Load<PackedScene>("res://scenes/components/UpgradeRow.tscn");
		foreach(var def in _gs.GetDefs())
		{
			var row = rowScene.Instantiate<UpgradeRow>();
			_buildList.AddChild(row);
			row.Setup(def.Id);
		}
    }

    public override void _ExitTree()
    {
        if (_gs != null) _gs.Changed -= Refresh;//отписка от события при выходе из дерева
    }

    private void ShowTab(int index)
    {
        _buildPanel.Visible = index == 0;
        _upgradePanel.Visible = index == 1;
        _prestigePanel.Visible = index == 2;
    }
//метод для отображения выбранной панели
    private void Refresh()
    {
        SetRes(_wood, "wood", "Дерево");
        SetRes(_stone, "stone", "Камень");
        SetRes(_food, "food", "Еда");
        SetRes(_gold, "gold", "Золото");
    }
//метод для обновления информации о ресурсах и кнопках строительства
    private void SetRes(Label label, string key, string title)
    {
        label.Text = title + " " + F(_gs.GetResource(key))
                   + "\n+" + F(_gs.IncomePerSec(key)) + "/с";
    }
//метод для обновления информации о ресурсах (количество и доход в секунду)
    private static string F(double v) => v.ToString("0.#");
//метод для форматирования чисел (убирает лишние нули после запятой)
    public override void _UnhandledInput(InputEvent @event)
    {
        Vector2 pos;
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            pos = mb.Position;//если событие - нажатие левой кнопки мыши, то берем позицию курсора
        else if (@event is InputEventScreenTouch st && st.Pressed)
            pos = st.Position;//если событие - касание экрана, то берем позицию касания
        else
            return;
        var cam = GetNode<Camera3D>("World/CameraRig/Camera3D");
        Vector3 from = cam.ProjectRayOrigin(pos);//from - точка начала луча (камера)
        Vector3 dir = cam.ProjectRayNormal(pos);//dir - направление луча (нормализованный вектор)
        if (Mathf.Abs(dir.Y) < 0.0001f) return;//если луч почти параллелен плоскости Y=0, то пересечения не будет
        float t = -from.Y / dir.Y;//t - расстояние до пересечения луча с плоскостью Y=0 (земля)
        if (t < 0) return;
        Vector3 hit = from + dir * t;//hit - точка пересечения луча с плоскостью Y=0
        if (Mathf.Abs(hit.X) > 4.5f || Mathf.Abs(hit.Z) > 4.5f) return;
        _gs.Click();
        PunchCastle();
    }
	/*здесь метод _UnhandledInput, который обрабатывает нажатия мыши и касания экрана (которые не забрал интерфейс).
Он определяет позицию нажатия, проецирует луч из камеры в 3D пространство и проверяет, попадает ли он в область замка (площадка 9 на 9, 
половина - 4.5 на 4.5, по модулю нажатие не должно превышать 4.5 на 4.5 (х и z)). 
Если да, то вызывается метод _gs.Click() для обработки клика и метод PunchCastle() для анимации замка.
	 */

    private void PunchCastle()
    {
        _punchTween?.Kill();
        _castle.Scale = Vector3.One * 1.1f;
        _punchTween = CreateTween();
        _punchTween.TweenProperty(_castle, "scale", Vector3.One, 0.18f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }//метод для анимации "панча" замка (увеличение масштаба на 10% и плавное возвращение к исходному размеру)
}