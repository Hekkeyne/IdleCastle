using System;
using Godot;
using System.Collections.Generic;
public partial class GameState : Node
{
    [Signal] public delegate void ChangedEventHandler();
    private readonly EconomySim _sim = new();
    private double _tickAcc;//копит время
    private double _uiAcc;//копит время но для юай
    private const double Step = 0.1;//шаг в секундАХ, чтоб менять экономику
    public override void _Ready()
    {
        var defs = ContentService.LoadBuildingDefs();//читаем из джсона определения зданий (сколько стоит, сколько приносит, где стоит)
        _sim.SetDefinition(defs);//передаём в симулятор экономики
    }
    public override void _Process(double delta)
    {
        _tickAcc += delta;//копим время, чтоб потом раз в 0.1 сек обновлять экономику
        int guard = 0;
        while (_tickAcc>=Step && guard < 20)//защита от зацикливания, если что-то пошло не так и игра подвисла, то не будем бесконечно тикать экономику
        {
            _sim.Tick(Step);
            _tickAcc -= Step;
            guard++;
        }
        _uiAcc += delta;
        if (_uiAcc>=0.1)//раз в 0.1 сек обновляем юай
        {
            _uiAcc = 0;
            EmitSignal(SignalName.Changed);//changed тк changedEventHandler
        }
    }
    public double GetResource(string key) => _sim.GetResource(key);//читаем ресурс из симулятора экономики
    public int GetLevel(string id) => _sim.GetLevel(id);//читаем уровень здания из симулятора экономики
    public double IncomePerSec(string key)=>_sim.IncomePerSec(key);//читаем доход в секунду из симулятора экономики
    public Dictionary<string,double> CostFor(string id, int count)=>_sim.CostFor(id,count);//читаем цену за здание из симулятора экономики
    public void Click()//кликнули по экрану, передаём в симулятор экономики
    {
        _sim.Click();
        EmitSignal(SignalName.Changed);
    }
    public bool Buy(string id, int count)//покупаем здание, передаём в симулятор экономики
    {
        bool ok = _sim.TryBuy(id,count);
        if (ok) EmitSignal(SignalName.Changed);
        return ok;
    }
}