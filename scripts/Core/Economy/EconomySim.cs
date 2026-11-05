using System;
using System.Collections.Generic; // Для словаря
public class EconomySim
{
	private readonly Dictionary<string,double> _wallet = new();//кошелёк название ресурса - кол-во
	private readonly List<BuildingDef> _defs = new();//определения зданий, сколько стоит, где стоит, сколько приносит (из джсона)
	private readonly Dictionary<string, int> _levels = new();//уровни зданий (айди и сколько раз купили)
	public double ClickPower {get; set;} = 1.0;
	public string ClickResource {get; set;} = "wood";

	public double GetResource(string key) => _wallet.TryGetValue(key, out var value) ? value : 0;
	//почитали ресурс, если его нет в кошельке, то возвращаем 0
	public int GetLevel(string key) => _levels.TryGetValue(key, out var value) ? value : 0;
	//почитали сколько раз купили здание, если не покупали, то 0
	public void AddResource(string key, double amount)
	{
		_wallet[key]=GetResource(key)+amount;//читаем текущее кол-во ресов и прибавляем че надо
	}
	public void SetDefinition(IEnumerable<BuildingDef> defs)//IEnumerable - крутая штука (любая коллекция) принимает список и массив
	{
		_defs.Clear();
		_defs.AddRange(defs);//сюда передаём список из JSON, который распарсил ContentService
	}
	public BuildingDef GetDef(string id)
	{
		return _defs.Find(d=>d.Id==id); //ищем по айди опрделенееи ыаа че я несу
	}
	public IReadOnlyList<BuildingDef> GetDefs() => _defs;//возвращаем список определений зданий (чтобы не могли менять)
	public double IncomePerSec(string key)
	{
		double sum = 0;
		foreach (var d in _defs)//перебираем все здания
		{
			int level = GetLevel(d.Id);
			if (level ==0) continue;//если не купили, то пропускаем
			if (d.Income.TryGetValue(key, out var income))
			    sum += income *level;//если есть доход - прибавляем доход * кол-во зданий
		}
		return sum;
	}
	public void Tick(double dt)
	{
		foreach (var d in _defs)
		{
			int level = GetLevel(d.Id);
			if (level == 0) continue;
			foreach (var kv in d.Income)
			    AddResource(kv.Key,kv.Value*level*dt);//прибавляем доход * кол-во зданий * dt	
		}
	}
	public void Click(){AddResource(ClickResource,ClickPower);}
	public bool CanAfford(Dictionary<string,double> cost)
	{
		if (cost == null) return false;
		foreach (var kv in cost)
			if (GetResource(kv.Key)<kv.Value) return false;
		return true;
	}//прозодит по всем ресам в цене и проверяет, хватает ли их в кошельке
	public void Spend(Dictionary<string,double> cost)
	{
		if (cost == null) return;
		foreach (var kv in cost)
			AddResource(kv.Key, -kv.Value);
	}//а этот списывает ресурсы из кошелька
	public Dictionary<string,double> CostFor(string id, int count)
	{
		var result = new Dictionary<string,double>();
		var def = GetDef(id);
		if (def == null || count <=0)return result;
		int level = GetLevel(id);
		double multiplier = Math.Pow(def.Growth,level)*(Math.Pow(def.Growth,count)-1.0)/(def.Growth-1.0);
		foreach (var kv in def.Cost)
			result[kv.Key]=kv.Value*multiplier;
		return result;
	}/*
	цену возвращаем словарём тк здание может стоить несколько ресов
	формула - геометрическая прогрессия
	*/
	public bool TryBuy(string id, int count)
	{
		var cost = CostFor(id,count);
		if (cost.Count == 0 || !CanAfford(cost)) return false;
		Spend(cost);
		_levels[id]=GetLevel(id)+count;
		return true;
	}/*
	сначала считаем цену, цена пустая - здания нет
	не хватает - фолс
	всё ок - списываем и добавляем уровень
	*/

}
