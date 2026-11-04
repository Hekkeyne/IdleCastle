using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	private GameState _gs;
	private Label _wood;
	private Button _bLumber, _bQuarry, _bFarm, _bMarket; //кнопки покупки зданий
	public override void _Ready()
	{
		_gs = GetNode<GameState>("/root/GameState");
		_wood = GetNode<Label>("UI/Root/VBox/Wood");
		_bLumber = GetNode<Button>("UI/Root/VBox/BuyLumber");
		_bQuarry = GetNode<Button>("UI/Root/VBox/BuyQuarry");
		_bFarm = GetNode<Button>("UI/Root/VBox/BuyFarm");
		_bMarket = GetNode<Button>("UI/Root/VBox/BuyMarket");
		GetNode<Button>("UI/Root/VBox/Click").Pressed += _gs.Click;
		//подписываемся на кнопки покупки зданий
		_bLumber.Pressed += () => _gs.Buy("lumber", 1);
		_bQuarry.Pressed += () => _gs.Buy("quarry", 1);
		_bFarm.Pressed += () => _gs.Buy("farm", 1);
		_bMarket.Pressed += () => _gs.Buy("market", 1);
		_gs.Changed += Refresh;
		Refresh();
		//подписываемся на сигнал Changed из GameState, чтобы обновлять UI
	}
    public override void _ExitTree()
    {
        if(_gs!=null) _gs.Changed -= Refresh;	//отписываемся от сигнала Changed из GameState, чтобы не было утечки памяти
    }
	private void Refresh()
	{
		_wood.Text="Дерево "+F(_gs.GetResource("wood"))+" +"+ F(_gs.IncomePerSec("wood"))+"/сек)";
		UpdateButton(_bLumber, "lumber", "Лесопилка");
		UpdateButton(_bQuarry, "quarry", "Каменоломня");
		UpdateButton(_bFarm, "farm", "Ферма");
		UpdateButton(_bMarket, "market", "Рынок");
		//подписываемся на сигнал Changed из GameState, чтобы обновлять UI
	}
	private void UpdateButton(Button b, string id, string title)
	{
		var cost = _gs.CostFor(id, 1);
		string text = title + " ур." + _gs.GetLevel(id);
		foreach(var kv in cost)
		{
			text += " " + kv.Key + " " + F(kv.Value);
		}
		b.Text = text;
		b.Disabled = !_gs.CanBuy(id, 1);
	}//обновляем кнопку покупки здания
	private static string F(double v)
	{
		return v.ToString("0.#");
	}//форматируем число, чтобы не было много нулей после запятой	

}