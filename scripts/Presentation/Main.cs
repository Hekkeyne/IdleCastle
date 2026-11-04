using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	public override void _Ready()
	{
		var gs = GetNode<GameState>("/root/GameState");
		for (int i =0; i < 20; i++) gs.Click();
		GD.Print("дерева после кликов: " + gs.GetResource("wood"));
		bool ok = gs.Buy("lumber", 1);
		GD.Print("купили"+ok+"дерева после покупки: " + gs.GetResource("wood")+" доход в секунду: "+gs.IncomePerSec("wood"));
	}
}
/*
ДОРОГИЕ ПРОГРАММИСТЫ, ЭТО ТЕСТ СКРИПТОВ, ДАННЫЕ ИЗ JSON ДОШЛИ ДО ЭКОНОМИКИ, 
АВТОЛОАД ПОТЫКАЛ, ПОКУПКА МЕНЯЕТ СОСТОЯИНЕ, ДОХОД МЕНЯЕТСЯ
*/