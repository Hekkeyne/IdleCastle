# IDLE CASTLE

Карта проекта.

```
IdleCastle/
  data/
  scenes/
    GameState.tscn
    screens/
    components/
  scripts/
    Core/
    Services/
    Presentation/
  tests/
  docs/
```

## Записи по коду

```
scripts/Core/Economy/BuildingDef.cs
Описание одного здания. Чистая модель данных, логики тут нет. Поля публичные, чтобы ContentService мог заполнить их из JSON.

Id string, короткий id здания, например lumber. Стабильный, используется как ключ везде.
NameKey string, ключ перевода, например b_lumber. По нему берётся название из translations.
Cost Dictionary<string,double>, цена по ресурсам, например wood 15. Словарь, потому что цена может быть из нескольких ресурсов.
Growth double, множитель цены за каждый купленный уровень, обычно 1.15.
Income Dictionary<string,double>, доход по ресурсам в секунду за одну штуку, например wood 0.5.
Plot int, номер участка на земле, куда встаёт здание.


scripts/Core/Economy/EconomySim.cs
Логика экономики. Обычный класс, не Node, без using Godot. Хранит два изменяемых состояния: ресурсы игрока и уровни зданий. Определения зданий приходят снаружи и внутри класса не меняются.

Поля.
_wallet Dictionary<string,double>, сколько какого ресурса у игрока. Ключ это имя ресурса, wood, stone и так далее.
_defs List<BuildingDef>, определения зданий, загружаются один раз из buildings.json.
_levels Dictionary<string,int>, сколько раз куплено каждое здание. Ключ это id здания.

Свойства.
ClickPower double, сколько ресурса даёт один клик. Сейчас 1.0.
ClickResource string, какой ресурс даёт клик. Сейчас wood.

Параметры и локальные переменные.
key string, имя ресурса или id здания, по нему ищем в словарях.
value double или int, то что вернул TryGetValue.
amount double, сколько прибавить или отнять в AddResource.
defs IEnumerable<BuildingDef>, любая коллекция зданий, принимаем и список и массив.
id string, id здания.
count int, сколько штук покупаем.
d BuildingDef, текущее здание в цикле.
level int, сколько раз здание куплено.
sum double, накопитель дохода в IncomePerSec.
kv KeyValuePair<string,double>, пара ключ и значение при переборе словаря, у неё есть Key и Value.
cost Dictionary<string,double>, цена покупки.
result Dictionary<string,double>, собранная цена в CostFor.
multiplier double, во сколько раз дороже стоит эта покупка.
def BuildingDef, найденное определение, может быть null.

Методы кратко.
GetResource, GetLevel, AddResource, чтение и изменение кошелька и уровней.
SetDefinition, GetDef, загрузка определений и поиск по id.
IncomePerSec, Tick, Click, доход в секунду, начисление за время, клик.
CanAfford, Spend, проверка и списание цены.
CostFor, TryBuy, расчёт цены и сама покупка.


scripts/Services/ContentService.cs
Читает data/buildings.json и превращает в список BuildingDef. Лежит в Services, потому что читает файл через Godot.

BuildingData это вспомогательный класс, повторяет корень JSON.
Version int, версия формата данных, пока не используется.
Buildings List<BuildingDef>, список зданий из файла.

Options JsonSerializerOptions, настройки разбора, одни на все вызовы.
IncludeFields bool, читать публичные поля, иначе BuildingDef останется пустым.
PropertyNameCaseInsensitive bool, не сверять регистр ключей.

LoadBuildingDefs это статический метод, возвращает List<BuildingDef>.
path const string, путь res://data/buildings.json.
json string, содержимое файла.
data BuildingData, результат разбора, может быть null.
e JsonException, ошибка разбора, ловится в catch.


scripts/Services/GameState.cs
Автолоад. Держит EconomySim, грузит контент, гоняет тик и сообщает интерфейсу об изменениях. Лежит в Services, потому что живёт в дереве сцены.

Changed сигнал, говорит подписчикам что цифры поменялись.
_sim EconomySim, сама экономика.
_tickAcc double, копит время до следующего шага.
_uiAcc double, копит время до обновления интерфейса.
Step const double, размер шага симуляции, 0.1 секунды.
guard int, локальная защита от зацикливания, максимум 20 шагов за кадр.

_Ready, читает контент через ContentService и отдаёт в SetDefinition.
_Process, режет время на шаги по Step, тикает экономику, раз в 0.1 секунды шлёт Changed.
Click, добавляет ресурс за клик и сразу шлёт Changed.
Buy, покупает, шлёт Changed только при успехе, возвращает результат.
CanBuy, проверяет, хватает ли ресурсов на покупку, но не покупает.
GetResource, GetLevel, IncomePerSec, CostFor, тонкие обёртки над _sim.


scripts/Presentation/Main.cs
Экран игры. Показывает HUD, обновляет лейблы и кнопки по сигналу Changed. Позже сюда же добавятся 3D-мир и кубы зданий.

_gs GameState, ссылка на автолоад.
_wood Label, лейбл с деревом и доходом.
_bLumber Button, кнопка лесопилки.
_bQuarry Button, кнопка каменоломни.
_bFarm Button, кнопка фермы.
_bMarket Button, кнопка рынка.

_Ready, находит узлы по путям, подписывает кнопки и сигнал Changed, обновляет экран.
_ExitTree, отписывается от Changed, чтобы закрытый экран не ловил сигналы.
Refresh, пишет дерево и доход в лейбл, обновляет все четыре кнопки.
UpdateButton, считает цену, собирает текст с уровнем и ценой, гасит кнопку если ресурсов не хватает.
F, форматирует число, оставляет один знак после запятой.


data/buildings.json
Данные зданий. Ключи совпадают с полями BuildingDef.

version int, версия формата, пригодится для миграций.
buildings массив, список зданий.
id string, id здания, должен совпадать с BuildingDef.Id.
nameKey string, ключ перевода.
cost объект, ресурс и его количество, например wood 15.
growth число, множитель цены.
income объект, ресурс и его количество в секунду.
plot int, номер участка.

Ресурсы сейчас: wood, stone, food, gold.
Здания сейчас: lumber, quarry, farm, market.
Лестница такая: дерево кормит каменоломню, камень нужен ферме, еда нужна рынку, рынок даёт золото.
```
