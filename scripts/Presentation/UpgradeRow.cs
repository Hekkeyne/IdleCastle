using System;
using System.ComponentModel;
using Godot;
public partial class UpgradeRow : Button
{
    private GameState _gs;
    private string _id;
    public override void _Ready()
    {
        _gs = GetNode<GameState>("/root/GameState");
        _gs.Changed += Refresh;
        Pressed+=OnPressed;//подписка и сбор gamestate
    }
    public override void _ExitTree()
    {
        if (_gs!=null) _gs.Changed-=Refresh;//отписка если выходишь из дерева
    }
    public void Setup(string id)
    {
        _id = id;
        Refresh();
    }
    private void OnPressed()
    {
        if (_id!=null) _gs.Buy(_id,1);//покупка, если нажатое не нул
    }
    private void Refresh()
    {
        if (_id == null) return;
        var def = _gs.GetDef(_id);
        string title = def !=null?def.Title:_id;
        string text = title + " ур." + _gs.GetLevel(_id);
        foreach (var kv in _gs.CostFor(_id,1))
            text+= " " + kv.Key + " " + F(kv.Value);
        Text = text;
        Disabled = !_gs.CanBuy(_id,1);
    }//если айди не пуст - берём билду, берём титл, в тексте создаем титл + уровень + цена
    private static string F(double v) => v.ToString("0.#");
}