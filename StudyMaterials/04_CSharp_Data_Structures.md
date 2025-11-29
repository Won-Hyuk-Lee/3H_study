# 4강: C# 자료구조 (Data Structures)

## 목차
1. [자료구조란?](#자료구조란)
2. [배열 (Array)](#배열-array)
3. [리스트 (List)](#리스트-list)
4. [딕셔너리 (Dictionary)](#딕셔너리-dictionary)
5. [큐 (Queue)](#큐-queue)
6. [스택 (Stack)](#스택-stack)
7. [해시셋 (HashSet)](#해시셋-hashset)
8. [링크드 리스트 (LinkedList)](#링크드-리스트-linkedlist)
9. [자료구조 성능 비교](#자료구조-성능-비교)
10. [실전 프로젝트: 스타크래프트 유닛 관리 시스템](#실전-프로젝트-스타크래프트-유닛-관리-시스템)

---

## 자료구조란?

자료구조는 데이터를 효율적으로 저장하고 관리하기 위한 방법입니다. 게임 개발에서는 수많은 유닛, 아이템, 이벤트를 관리해야 하므로 적절한 자료구조 선택이 매우 중요합니다.

### 왜 자료구조가 중요한가?

스타크래프트를 예로 들면:
- 맵에 존재하는 **200개의 유닛**을 어떻게 관리할 것인가?
- **선택된 유닛들**을 어떻게 추적할 것인가?
- **생산 대기열**에 있는 유닛들을 어떻게 순서대로 처리할 것인가?
- **미니맵의 유닛 위치**를 어떻게 빠르게 찾을 것인가?

이런 문제들을 해결하기 위해 다양한 자료구조가 필요합니다.

---

## 배열 (Array)

### 개념
배열은 **고정된 크기**의 연속된 메모리 공간에 같은 타입의 데이터를 저장하는 자료구조입니다.

### 특징
- **크기 고정**: 선언 시 크기가 결정되며 변경 불가
- **인덱스 접근**: O(1) - 매우 빠름
- **메모리 연속**: 캐시 효율성이 좋음

### 스타크래프트 예시: 인구수 제한

```csharp
// 테란의 커맨드 센터별 인구수 제공량
public class SupplySystem
{
    // 커맨드 센터는 최대 5개까지만 지을 수 있다고 가정
    private int[] commandCenterSupply = new int[5];

    public void BuildCommandCenter(int index)
    {
        if (index >= 0 && index < commandCenterSupply.Length)
        {
            commandCenterSupply[index] = 10; // 커맨드 센터 하나당 +10 인구
            Console.WriteLine($"커맨드 센터 {index + 1} 건설 완료. 인구 +10");
        }
    }

    public int GetTotalSupply()
    {
        int total = 0;
        for (int i = 0; i < commandCenterSupply.Length; i++)
        {
            total += commandCenterSupply[i];
        }
        return total;
    }

    public void DestroyCommandCenter(int index)
    {
        if (index >= 0 && index < commandCenterSupply.Length)
        {
            Console.WriteLine($"커맨드 센터 {index + 1} 파괴됨. 인구 -{commandCenterSupply[index]}");
            commandCenterSupply[index] = 0;
        }
    }
}

// 사용 예시
SupplySystem supply = new SupplySystem();
supply.BuildCommandCenter(0); // 첫 번째 커맨드 센터
supply.BuildCommandCenter(1); // 두 번째 커맨드 센터
Console.WriteLine($"총 인구 한계: {supply.GetTotalSupply()}"); // 출력: 총 인구 한계: 20
supply.DestroyCommandCenter(0); // 적의 공격으로 파괴!
Console.WriteLine($"총 인구 한계: {supply.GetTotalSupply()}"); // 출력: 총 인구 한계: 10
```

### 다차원 배열: 미니맵 시야

```csharp
// 64x64 타일 맵의 시야 정보
public class VisionMap
{
    private bool[,] visibleTiles = new bool[64, 64]; // 2차원 배열

    public void RevealArea(int x, int y, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < 64 && ny >= 0 && ny < 64)
                {
                    // 원형 시야 계산
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        visibleTiles[nx, ny] = true;
                    }
                }
            }
        }
    }

    public bool IsVisible(int x, int y)
    {
        if (x >= 0 && x < 64 && y >= 0 && y < 64)
        {
            return visibleTiles[x, y];
        }
        return false;
    }

    public void PrintVisionAroundPoint(int x, int y, int range)
    {
        Console.WriteLine($"[{x}, {y}] 주변 시야:");
        for (int dy = -range; dy <= range; dy++)
        {
            for (int dx = -range; dx <= range; dx++)
            {
                int nx = x + dx;
                int ny = y + dy;
                Console.Write(IsVisible(nx, ny) ? "O " : "X ");
            }
            Console.WriteLine();
        }
    }
}

// 사용 예시
VisionMap visionMap = new VisionMap();
visionMap.RevealArea(32, 32, 5); // 맵 중앙에 반경 5의 시야 확보
visionMap.PrintVisionAroundPoint(32, 32, 7);
```

---

## 리스트 (List<T>)

### 개념
List는 **동적 크기**를 가진 배열입니다. 필요에 따라 자동으로 크기가 조절됩니다.

### 특징
- **동적 크기**: 요소 추가/제거 시 자동으로 크기 조절
- **인덱스 접근**: O(1)
- **끝에 추가**: O(1) (평균)
- **중간 삽입/삭제**: O(n) - 느림

### 스타크래프트 예시: 선택된 유닛 관리

```csharp
public enum UnitType
{
    Marine,
    Medic,
    Firebat,
    Ghost,
    SiegeTank
}

public class Unit
{
    public string Name { get; set; }
    public UnitType Type { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int AttackPower { get; set; }
    public bool IsAlive => HP > 0;

    public Unit(string name, UnitType type, int maxHP, int attackPower)
    {
        Name = name;
        Type = type;
        HP = maxHP;
        MaxHP = maxHP;
        AttackPower = attackPower;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP < 0) HP = 0;
        Console.WriteLine($"{Name}이(가) {damage} 데미지를 받았습니다. (남은 HP: {HP}/{MaxHP})");
    }

    public void Heal(int amount)
    {
        HP += amount;
        if (HP > MaxHP) HP = MaxHP;
        Console.WriteLine($"{Name}이(가) {amount} 회복했습니다. (현재 HP: {HP}/{MaxHP})");
    }
}

public class UnitSelectionManager
{
    private List<Unit> selectedUnits = new List<Unit>();
    private const int MAX_SELECTION = 12; // 스타크래프트 선택 제한

    // 유닛 선택
    public bool SelectUnit(Unit unit)
    {
        if (selectedUnits.Count >= MAX_SELECTION)
        {
            Console.WriteLine("최대 12개의 유닛만 선택할 수 있습니다!");
            return false;
        }

        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            Console.WriteLine($"{unit.Name} 선택됨. (선택된 유닛: {selectedUnits.Count}/12)");
            return true;
        }
        return false;
    }

    // 유닛 선택 해제
    public void DeselectUnit(Unit unit)
    {
        if (selectedUnits.Remove(unit))
        {
            Console.WriteLine($"{unit.Name} 선택 해제됨.");
        }
    }

    // 모든 유닛 선택 해제
    public void DeselectAll()
    {
        selectedUnits.Clear();
        Console.WriteLine("모든 유닛 선택 해제됨.");
    }

    // 선택된 모든 유닛 이동 명령
    public void MoveSelectedUnits(int x, int y)
    {
        Console.WriteLine($"\n선택된 {selectedUnits.Count}개 유닛이 ({x}, {y})로 이동합니다.");
        foreach (var unit in selectedUnits)
        {
            Console.WriteLine($"  - {unit.Name} 이동 중...");
        }
    }

    // 선택된 유닛 중 특정 타입만 필터링
    public List<Unit> GetUnitsByType(UnitType type)
    {
        List<Unit> filtered = new List<Unit>();
        foreach (var unit in selectedUnits)
        {
            if (unit.Type == type)
            {
                filtered.Add(unit);
            }
        }
        return filtered;

        // LINQ를 사용한 방법 (더 간결):
        // return selectedUnits.Where(u => u.Type == type).ToList();
    }

    // 부상당한 유닛만 필터링 (메딕의 치료 대상)
    public List<Unit> GetInjuredUnits()
    {
        List<Unit> injured = new List<Unit>();
        foreach (var unit in selectedUnits)
        {
            if (unit.HP < unit.MaxHP && unit.IsAlive)
            {
                injured.Add(unit);
            }
        }
        return injured;
    }

    // 선택된 유닛 정보 출력
    public void PrintSelectedUnits()
    {
        Console.WriteLine($"\n=== 선택된 유닛 ({selectedUnits.Count}/12) ===");
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            Unit unit = selectedUnits[i];
            Console.WriteLine($"{i + 1}. {unit.Name} ({unit.Type}) - HP: {unit.HP}/{unit.MaxHP}");
        }
    }

    // 죽은 유닛 제거
    public void RemoveDeadUnits()
    {
        int removed = selectedUnits.RemoveAll(u => !u.IsAlive);
        if (removed > 0)
        {
            Console.WriteLine($"{removed}개의 유닛이 전사했습니다...");
        }
    }
}

// 사용 예시
var selectionManager = new UnitSelectionManager();

// 유닛 생성
var marine1 = new Unit("마린#1", UnitType.Marine, 40, 6);
var marine2 = new Unit("마린#2", UnitType.Marine, 40, 6);
var medic1 = new Unit("메딕#1", UnitType.Medic, 60, 0);
var firebat1 = new Unit("파이어뱃#1", UnitType.Firebat, 50, 16);

// 유닛 선택
selectionManager.SelectUnit(marine1);
selectionManager.SelectUnit(marine2);
selectionManager.SelectUnit(medic1);
selectionManager.SelectUnit(firebat1);

selectionManager.PrintSelectedUnits();

// 전투 발생!
marine1.TakeDamage(25);
marine2.TakeDamage(30);
firebat1.TakeDamage(50); // 전사

selectionManager.RemoveDeadUnits();

// 부상당한 유닛 치료
var injuredUnits = selectionManager.GetInjuredUnits();
Console.WriteLine($"\n치료가 필요한 유닛: {injuredUnits.Count}개");
foreach (var unit in injuredUnits)
{
    medic1.Heal(unit.HP); // 메딕이 치료 (실제로는 메딕에 Heal 기능 필요)
}

// 이동 명령
selectionManager.MoveSelectedUnits(100, 200);
```

### List의 주요 메서드

```csharp
List<Unit> units = new List<Unit>();

// 추가
units.Add(marine1);                          // 끝에 추가
units.Insert(0, medic1);                     // 특정 위치에 삽입
units.AddRange(new[] { marine2, firebat1 }); // 여러 개 추가

// 제거
units.Remove(marine1);                       // 특정 요소 제거
units.RemoveAt(0);                          // 인덱스로 제거
units.RemoveAll(u => u.HP <= 0);            // 조건에 맞는 모든 요소 제거
units.Clear();                              // 전체 제거

// 검색
bool hasMarine = units.Contains(marine1);    // 포함 여부
int index = units.IndexOf(marine1);         // 인덱스 찾기
Unit found = units.Find(u => u.Type == UnitType.Medic); // 조건 검색

// 정렬
units.Sort((a, b) => a.HP.CompareTo(b.HP)); // HP 오름차순 정렬

// 기타
int count = units.Count;                    // 요소 개수
Unit firstUnit = units[0];                  // 인덱스 접근
```

---

## 딕셔너리 (Dictionary<TKey, TValue>)

### 개념
Dictionary는 **키(Key)-값(Value) 쌍**으로 데이터를 저장하는 자료구조입니다. 해시테이블을 기반으로 하여 매우 빠른 검색이 가능합니다.

### 특징
- **키로 접근**: O(1) - 매우 빠름
- **키 중복 불가**: 같은 키는 하나만 존재
- **순서 보장 안됨**: 입력 순서와 저장 순서가 다를 수 있음

### 스타크래프트 예시: 유닛 업그레이드 시스템

```csharp
public enum UpgradeType
{
    InfantryWeapons,    // 보병 무기
    InfantryArmor,      // 보병 방어력
    VehicleWeapons,     // 차량 무기
    VehicleArmor,       // 차량 방어력
    ShipWeapons,        // 함선 무기
    ShipArmor,          // 함선 방어력
    StimPack,           // 스팀팩
    U238Shells          // U-238 탄환 (마린 사거리 +1)
}

public class Upgrade
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int MaxLevel { get; set; }
    public int MineralCost { get; set; }
    public int GasCost { get; set; }
    public int UpgradeTime { get; set; } // 초 단위

    public Upgrade(string name, int maxLevel, int mineralCost, int gasCost, int time)
    {
        Name = name;
        Level = 0;
        MaxLevel = maxLevel;
        MineralCost = mineralCost;
        GasCost = gasCost;
        UpgradeTime = time;
    }

    public bool CanUpgrade()
    {
        return Level < MaxLevel;
    }

    public void LevelUp()
    {
        if (CanUpgrade())
        {
            Level++;
            Console.WriteLine($"{Name} 업그레이드 완료! (레벨: {Level}/{MaxLevel})");
        }
    }
}

public class UpgradeManager
{
    private Dictionary<UpgradeType, Upgrade> upgrades = new Dictionary<UpgradeType, Upgrade>();
    private int minerals;
    private int gas;

    public UpgradeManager(int startMinerals, int startGas)
    {
        minerals = startMinerals;
        gas = startGas;
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        // 업그레이드 정보 초기화
        upgrades[UpgradeType.InfantryWeapons] = new Upgrade("보병 무기", 3, 100, 100, 160);
        upgrades[UpgradeType.InfantryArmor] = new Upgrade("보병 방어력", 3, 100, 100, 160);
        upgrades[UpgradeType.VehicleWeapons] = new Upgrade("차량 무기", 3, 100, 100, 160);
        upgrades[UpgradeType.VehicleArmor] = new Upgrade("차량 방어력", 3, 100, 100, 160);
        upgrades[UpgradeType.StimPack] = new Upgrade("스팀팩", 1, 100, 100, 80);
        upgrades[UpgradeType.U238Shells] = new Upgrade("U-238 탄환", 1, 150, 150, 60);
    }

    // 업그레이드 시도
    public bool TryUpgrade(UpgradeType type)
    {
        if (!upgrades.ContainsKey(type))
        {
            Console.WriteLine("존재하지 않는 업그레이드입니다.");
            return false;
        }

        Upgrade upgrade = upgrades[type];

        if (!upgrade.CanUpgrade())
        {
            Console.WriteLine($"{upgrade.Name}은(는) 이미 최대 레벨입니다.");
            return false;
        }

        // 자원 계산 (레벨이 올라갈수록 비용 증가)
        int cost = upgrade.MineralCost * (upgrade.Level + 1);
        int gasCost = upgrade.GasCost * (upgrade.Level + 1);

        if (minerals < cost || gas < gasCost)
        {
            Console.WriteLine($"자원이 부족합니다. 필요: {cost} 미네랄, {gasCost} 가스 | 보유: {minerals} 미네랄, {gas} 가스");
            return false;
        }

        // 자원 소모 및 업그레이드
        minerals -= cost;
        gas -= gasCost;
        upgrade.LevelUp();

        Console.WriteLine($"소모된 자원: {cost} 미네랄, {gasCost} 가스");
        Console.WriteLine($"남은 자원: {minerals} 미네랄, {gas} 가스");

        return true;
    }

    // 업그레이드 레벨 조회
    public int GetUpgradeLevel(UpgradeType type)
    {
        if (upgrades.TryGetValue(type, out Upgrade upgrade))
        {
            return upgrade.Level;
        }
        return 0;
    }

    // 모든 업그레이드 현황 출력
    public void PrintAllUpgrades()
    {
        Console.WriteLine("\n=== 업그레이드 현황 ===");
        Console.WriteLine($"자원: {minerals} 미네랄, {gas} 가스\n");

        foreach (var kvp in upgrades)
        {
            UpgradeType type = kvp.Key;
            Upgrade upgrade = kvp.Value;
            string progress = new string('■', upgrade.Level) + new string('□', upgrade.MaxLevel - upgrade.Level);
            Console.WriteLine($"{upgrade.Name,-15} [{progress}] {upgrade.Level}/{upgrade.MaxLevel}");
        }
    }

    // 자원 추가
    public void AddResources(int mineralAmount, int gasAmount)
    {
        minerals += mineralAmount;
        gas += gasAmount;
        Console.WriteLine($"자원 획득: +{mineralAmount} 미네랄, +{gasAmount} 가스");
    }
}

// 사용 예시
UpgradeManager upgradeManager = new UpgradeManager(500, 500);

upgradeManager.PrintAllUpgrades();

Console.WriteLine("\n--- 업그레이드 시작 ---");
upgradeManager.TryUpgrade(UpgradeType.InfantryWeapons);  // 성공
upgradeManager.TryUpgrade(UpgradeType.InfantryWeapons);  // 성공 (레벨 2)
upgradeManager.TryUpgrade(UpgradeType.StimPack);         // 성공
upgradeManager.TryUpgrade(UpgradeType.InfantryArmor);    // 성공

upgradeManager.PrintAllUpgrades();

upgradeManager.AddResources(1000, 1000);
upgradeManager.TryUpgrade(UpgradeType.InfantryWeapons);  // 레벨 3
upgradeManager.TryUpgrade(UpgradeType.InfantryWeapons);  // 실패 (최대 레벨)

upgradeManager.PrintAllUpgrades();
```

### 딕셔너리 활용: 유닛 생산 비용 관리

```csharp
public class UnitCostDatabase
{
    private Dictionary<UnitType, (int minerals, int gas, int supply)> unitCosts;

    public UnitCostDatabase()
    {
        unitCosts = new Dictionary<UnitType, (int, int, int)>
        {
            { UnitType.Marine, (50, 0, 1) },
            { UnitType.Medic, (50, 25, 1) },
            { UnitType.Firebat, (50, 25, 1) },
            { UnitType.Ghost, (25, 75, 1) },
            { UnitType.SiegeTank, (150, 100, 2) }
        };
    }

    public bool GetUnitCost(UnitType type, out int minerals, out int gas, out int supply)
    {
        if (unitCosts.TryGetValue(type, out var cost))
        {
            minerals = cost.minerals;
            gas = cost.gas;
            supply = cost.supply;
            return true;
        }

        minerals = gas = supply = 0;
        return false;
    }

    public void PrintUnitCosts()
    {
        Console.WriteLine("=== 유닛 생산 비용 ===");
        foreach (var kvp in unitCosts)
        {
            Console.WriteLine($"{kvp.Key,-12} - 미네랄: {kvp.Value.minerals,3}, 가스: {kvp.Value.gas,3}, 인구수: {kvp.Value.supply}");
        }
    }
}

// 사용 예시
UnitCostDatabase costDB = new UnitCostDatabase();
costDB.PrintUnitCosts();

if (costDB.GetUnitCost(UnitType.SiegeTank, out int m, out int g, out int s))
{
    Console.WriteLine($"\n시즈 탱크 생산 비용: {m} 미네랄, {g} 가스, {s} 인구수");
}
```

---

## 큐 (Queue<T>)

### 개념
Queue는 **선입선출(FIFO: First In First Out)** 방식의 자료구조입니다. 먼저 들어간 데이터가 먼저 나옵니다.

### 특징
- **Enqueue**: 끝에 추가 - O(1)
- **Dequeue**: 앞에서 제거 - O(1)
- **Peek**: 앞의 요소 확인 (제거 안함) - O(1)

### 스타크래프트 예시: 유닛 생산 대기열

```csharp
public class ProductionOrder
{
    public UnitType UnitType { get; set; }
    public int ProductionTime { get; set; } // 생산에 걸리는 시간 (초)
    public int RemainingTime { get; set; }

    public ProductionOrder(UnitType type, int time)
    {
        UnitType = type;
        ProductionTime = time;
        RemainingTime = time;
    }
}

public class Barracks
{
    public string Name { get; set; }
    private Queue<ProductionOrder> productionQueue = new Queue<ProductionOrder>();
    private ProductionOrder currentProduction = null;
    private const int MAX_QUEUE_SIZE = 5; // 최대 5개까지 대기열 가능

    public Barracks(string name)
    {
        Name = name;
    }

    // 유닛 생산 추가
    public bool QueueUnit(UnitType type, int productionTime)
    {
        if (productionQueue.Count >= MAX_QUEUE_SIZE)
        {
            Console.WriteLine($"{Name}: 생산 대기열이 가득 찼습니다! (최대 {MAX_QUEUE_SIZE}개)");
            return false;
        }

        ProductionOrder order = new ProductionOrder(type, productionTime);
        productionQueue.Enqueue(order);

        Console.WriteLine($"{Name}: {type} 생산 대기열에 추가됨. (대기: {productionQueue.Count}개)");

        // 현재 생산 중인 유닛이 없으면 즉시 시작
        if (currentProduction == null)
        {
            StartNextProduction();
        }

        return true;
    }

    // 다음 생산 시작
    private void StartNextProduction()
    {
        if (productionQueue.Count > 0)
        {
            currentProduction = productionQueue.Dequeue();
            Console.WriteLine($"{Name}: {currentProduction.UnitType} 생산 시작! (남은 대기: {productionQueue.Count}개)");
        }
    }

    // 시간 경과 처리 (매 초마다 호출된다고 가정)
    public Unit Update(int deltaTime = 1)
    {
        if (currentProduction == null)
            return null;

        currentProduction.RemainingTime -= deltaTime;

        Console.WriteLine($"{Name}: {currentProduction.UnitType} 생산 중... ({currentProduction.RemainingTime}초 남음)");

        // 생산 완료
        if (currentProduction.RemainingTime <= 0)
        {
            UnitType completedType = currentProduction.UnitType;
            Console.WriteLine($"{Name}: {completedType} 생산 완료!");

            Unit newUnit = CreateUnit(completedType);
            currentProduction = null;

            // 다음 유닛 생산 시작
            StartNextProduction();

            return newUnit;
        }

        return null;
    }

    private Unit CreateUnit(UnitType type)
    {
        return type switch
        {
            UnitType.Marine => new Unit("마린", type, 40, 6),
            UnitType.Medic => new Unit("메딕", type, 60, 0),
            UnitType.Firebat => new Unit("파이어뱃", type, 50, 16),
            _ => new Unit("알 수 없는 유닛", type, 1, 1)
        };
    }

    // 생산 취소 (맨 앞 항목)
    public bool CancelProduction()
    {
        if (currentProduction != null)
        {
            Console.WriteLine($"{Name}: {currentProduction.UnitType} 생산 취소됨.");
            currentProduction = null;
            StartNextProduction();
            return true;
        }
        return false;
    }

    // 대기열 확인
    public void ShowQueue()
    {
        Console.WriteLine($"\n=== {Name} 생산 대기열 ===");

        if (currentProduction != null)
        {
            Console.WriteLine($"[생산 중] {currentProduction.UnitType} - {currentProduction.RemainingTime}초 남음");
        }
        else
        {
            Console.WriteLine("[생산 중] 없음");
        }

        if (productionQueue.Count > 0)
        {
            Console.WriteLine($"\n대기 중인 유닛: {productionQueue.Count}개");
            int position = 1;
            foreach (var order in productionQueue)
            {
                Console.WriteLine($"  {position}. {order.UnitType} ({order.ProductionTime}초)");
                position++;
            }
        }
        else
        {
            Console.WriteLine("대기 중인 유닛: 없음");
        }
    }

    // 대기열의 다음 유닛 확인 (제거하지 않음)
    public UnitType? PeekNext()
    {
        if (productionQueue.Count > 0)
        {
            return productionQueue.Peek().UnitType;
        }
        return null;
    }
}

// 사용 예시: 배럭스 시뮬레이션
Barracks barracks = new Barracks("배럭스 #1");

// 유닛들을 대기열에 추가
barracks.QueueUnit(UnitType.Marine, 5);   // 마린 생산 시간: 5초
barracks.QueueUnit(UnitType.Marine, 5);
barracks.QueueUnit(UnitType.Medic, 6);    // 메딕 생산 시간: 6초
barracks.QueueUnit(UnitType.Firebat, 5);

barracks.ShowQueue();

// 시간 경과 시뮬레이션
Console.WriteLine("\n=== 시간 경과 시뮬레이션 ===");
List<Unit> producedUnits = new List<Unit>();

for (int time = 1; time <= 25; time++)
{
    Console.WriteLine($"\n--- {time}초 경과 ---");
    Unit completed = barracks.Update(1);

    if (completed != null)
    {
        producedUnits.Add(completed);
        Console.WriteLine($"★ {completed.Name} 생산 완료! (총 생산: {producedUnits.Count}개)");
    }
}

barracks.ShowQueue();

Console.WriteLine($"\n총 생산된 유닛: {producedUnits.Count}개");
foreach (var unit in producedUnits)
{
    Console.WriteLine($"  - {unit.Name} ({unit.Type})");
}
```

### 큐 활용: 명령 대기열

```csharp
public enum CommandType
{
    Move,
    Attack,
    Patrol,
    Hold
}

public class UnitCommand
{
    public CommandType Type { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public string Description { get; set; }

    public UnitCommand(CommandType type, int x, int y, string desc)
    {
        Type = type;
        TargetX = x;
        TargetY = y;
        Description = desc;
    }
}

public class CommandQueue
{
    private Queue<UnitCommand> commands = new Queue<UnitCommand>();

    public void AddCommand(UnitCommand command)
    {
        commands.Enqueue(command);
        Console.WriteLine($"명령 추가: {command.Description} ({commands.Count}개 대기 중)");
    }

    public UnitCommand ExecuteNextCommand()
    {
        if (commands.Count > 0)
        {
            UnitCommand cmd = commands.Dequeue();
            Console.WriteLine($"명령 실행: {cmd.Description}");
            return cmd;
        }
        return null;
    }

    public void ShowCommands()
    {
        Console.WriteLine($"대기 중인 명령: {commands.Count}개");
        int i = 1;
        foreach (var cmd in commands)
        {
            Console.WriteLine($"  {i}. {cmd.Description}");
            i++;
        }
    }
}

// Shift 키를 누른 상태에서 여러 명령 내리기
CommandQueue cmdQueue = new CommandQueue();
cmdQueue.AddCommand(new UnitCommand(CommandType.Move, 100, 100, "위치 (100, 100)로 이동"));
cmdQueue.AddCommand(new UnitCommand(CommandType.Attack, 150, 150, "위치 (150, 150) 공격"));
cmdQueue.AddCommand(new UnitCommand(CommandType.Move, 50, 50, "위치 (50, 50)로 복귀"));
cmdQueue.ShowCommands();
```

---

## 스택 (Stack<T>)

### 개념
Stack은 **후입선출(LIFO: Last In First Out)** 방식의 자료구조입니다. 나중에 들어간 데이터가 먼저 나옵니다.

### 특징
- **Push**: 위에 추가 - O(1)
- **Pop**: 위에서 제거 - O(1)
- **Peek**: 맨 위 요소 확인 (제거 안함) - O(1)

### 스타크래프트 예시: 행동 히스토리 (Undo 기능)

```csharp
public class GameAction
{
    public string ActionType { get; set; }
    public string Description { get; set; }
    public object Data { get; set; }
    public DateTime Timestamp { get; set; }

    public GameAction(string type, string desc, object data = null)
    {
        ActionType = type;
        Description = desc;
        Data = data;
        Timestamp = DateTime.Now;
    }
}

public class ActionHistory
{
    private Stack<GameAction> actionHistory = new Stack<GameAction>();
    private Stack<GameAction> redoStack = new Stack<GameAction>();
    private const int MAX_HISTORY = 20; // 최대 20개 행동 기록

    // 새로운 행동 기록
    public void RecordAction(GameAction action)
    {
        // 새 행동을 하면 redo 스택은 비워짐
        redoStack.Clear();

        actionHistory.Push(action);
        Console.WriteLine($"[기록] {action.Description}");

        // 최대 개수 초과 시 가장 오래된 항목 제거
        if (actionHistory.Count > MAX_HISTORY)
        {
            // Stack을 임시 리스트로 변환하여 가장 아래 항목 제거
            var temp = actionHistory.ToList();
            temp.RemoveAt(temp.Count - 1);
            actionHistory = new Stack<GameAction>(temp.Reverse<GameAction>());
        }
    }

    // 행동 취소 (Ctrl+Z)
    public bool Undo()
    {
        if (actionHistory.Count == 0)
        {
            Console.WriteLine("취소할 행동이 없습니다.");
            return false;
        }

        GameAction action = actionHistory.Pop();
        redoStack.Push(action);

        Console.WriteLine($"[취소] {action.Description}");
        return true;
    }

    // 행동 다시 실행 (Ctrl+Y)
    public bool Redo()
    {
        if (redoStack.Count == 0)
        {
            Console.WriteLine("다시 실행할 행동이 없습니다.");
            return false;
        }

        GameAction action = redoStack.Pop();
        actionHistory.Push(action);

        Console.WriteLine($"[다시 실행] {action.Description}");
        return true;
    }

    // 최근 행동 확인 (제거 안함)
    public GameAction PeekLastAction()
    {
        if (actionHistory.Count > 0)
        {
            return actionHistory.Peek();
        }
        return null;
    }

    // 히스토리 출력
    public void ShowHistory()
    {
        Console.WriteLine("\n=== 행동 히스토리 ===");
        if (actionHistory.Count == 0)
        {
            Console.WriteLine("기록된 행동이 없습니다.");
            return;
        }

        int i = 1;
        foreach (var action in actionHistory)
        {
            Console.WriteLine($"{i}. {action.Description} ({action.Timestamp:HH:mm:ss})");
            i++;
        }
    }
}

// 사용 예시
ActionHistory history = new ActionHistory();

// 게임 플레이 중 행동 기록
history.RecordAction(new GameAction("BuildUnit", "SCV 생산"));
history.RecordAction(new GameAction("BuildBuilding", "배럭스 건설 (위치: 32, 45)"));
history.RecordAction(new GameAction("BuildUnit", "마린 생산"));
history.RecordAction(new GameAction("Move", "유닛 8개를 (120, 80)으로 이동"));
history.RecordAction(new GameAction("Attack", "적 본진 공격"));

history.ShowHistory();

Console.WriteLine("\n--- 실수로 공격 명령을 내렸습니다! ---");
history.Undo(); // 공격 취소

Console.WriteLine("\n--- 이동도 취소 ---");
history.Undo(); // 이동 취소

Console.WriteLine("\n--- 아니다, 이동은 해야겠다 ---");
history.Redo(); // 이동 다시 실행

history.ShowHistory();
```

### 스택 활용: 건물 선택 히스토리

```csharp
public class Building
{
    public string Name { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public Building(string name, int x, int y)
    {
        Name = name;
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"{Name} ({X}, {Y})";
    }
}

public class BuildingSelectionHistory
{
    private Stack<Building> selectionHistory = new Stack<Building>();

    // 건물 선택 (백스페이스로 이전 건물로 돌아가기)
    public void SelectBuilding(Building building)
    {
        selectionHistory.Push(building);
        Console.WriteLine($"선택: {building}");
    }

    // 이전 건물로 돌아가기 (백스페이스 키)
    public Building GoToPreviousBuilding()
    {
        if (selectionHistory.Count > 1)
        {
            selectionHistory.Pop(); // 현재 건물 제거
            Building previous = selectionHistory.Peek();
            Console.WriteLine($"이전 건물로 이동: {previous}");
            return previous;
        }

        Console.WriteLine("이전 건물이 없습니다.");
        return null;
    }

    // 현재 선택된 건물
    public Building GetCurrentBuilding()
    {
        if (selectionHistory.Count > 0)
        {
            return selectionHistory.Peek();
        }
        return null;
    }
}

// 사용 예시
BuildingSelectionHistory buildingHistory = new BuildingSelectionHistory();

// 건물들을 순서대로 선택
buildingHistory.SelectBuilding(new Building("커맨드 센터", 30, 30));
buildingHistory.SelectBuilding(new Building("배럭스", 45, 50));
buildingHistory.SelectBuilding(new Building("팩토리", 60, 50));
buildingHistory.SelectBuilding(new Building("스타포트", 75, 50));

Console.WriteLine("\n--- 백스페이스로 이전 건물 선택 ---");
buildingHistory.GoToPreviousBuilding(); // 팩토리로
buildingHistory.GoToPreviousBuilding(); // 배럭스로

var current = buildingHistory.GetCurrentBuilding();
Console.WriteLine($"현재 선택: {current}");
```

---

## 해시셋 (HashSet<T>)

### 개념
HashSet은 **중복을 허용하지 않는** 집합 자료구조입니다. 순서가 없으며 매우 빠른 검색이 가능합니다.

### 특징
- **중복 불가**: 같은 값을 여러 번 추가해도 하나만 저장
- **추가/제거/검색**: O(1) - 매우 빠름
- **순서 없음**: 입력 순서와 상관없이 저장

### 스타크래프트 예시: 감지된 적 유닛 추적

```csharp
public class EnemyUnit
{
    public int Id { get; set; }
    public UnitType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public EnemyUnit(int id, UnitType type, int x, int y)
    {
        Id = id;
        Type = type;
        X = x;
        Y = y;
    }

    // HashSet에서 중복 판단을 위한 메서드
    public override bool Equals(object obj)
    {
        if (obj is EnemyUnit other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Type} #{Id} at ({X}, {Y})";
    }
}

public class RadarSystem
{
    private HashSet<EnemyUnit> detectedEnemies = new HashSet<EnemyUnit>();
    private HashSet<int> destroyedEnemyIds = new HashSet<int>();

    // 적 유닛 감지
    public void DetectEnemy(EnemyUnit enemy)
    {
        // HashSet은 자동으로 중복을 제거
        if (detectedEnemies.Add(enemy))
        {
            Console.WriteLine($"[새로운 적 감지!] {enemy}");
        }
        else
        {
            Console.WriteLine($"[이미 감지된 적] {enemy}");
        }
    }

    // 적 유닛 파괴
    public void EnemyDestroyed(int enemyId)
    {
        var enemy = detectedEnemies.FirstOrDefault(e => e.Id == enemyId);
        if (enemy != null)
        {
            detectedEnemies.Remove(enemy);
            destroyedEnemyIds.Add(enemyId);
            Console.WriteLine($"[적 제거됨] {enemy}");
        }
    }

    // 특정 적이 감지되었는지 확인
    public bool IsEnemyDetected(int enemyId)
    {
        return detectedEnemies.Any(e => e.Id == enemyId);
    }

    // 특정 타입의 적이 있는지 확인
    public bool HasEnemyType(UnitType type)
    {
        return detectedEnemies.Any(e => e.Type == type);
    }

    // 모든 감지된 적 출력
    public void ShowDetectedEnemies()
    {
        Console.WriteLine($"\n=== 감지된 적 유닛 ({detectedEnemies.Count}개) ===");
        foreach (var enemy in detectedEnemies)
        {
            Console.WriteLine($"  - {enemy}");
        }
    }

    // 통계
    public void ShowStatistics()
    {
        Console.WriteLine("\n=== 레이더 통계 ===");
        Console.WriteLine($"현재 감지된 적: {detectedEnemies.Count}개");
        Console.WriteLine($"파괴한 적: {destroyedEnemyIds.Count}개");

        // 타입별 집계
        var typeGroups = detectedEnemies.GroupBy(e => e.Type);
        Console.WriteLine("\n타입별 감지 현황:");
        foreach (var group in typeGroups)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()}개");
        }
    }
}

// 사용 예시
RadarSystem radar = new RadarSystem();

// 적 유닛 감지 (중복 포함)
radar.DetectEnemy(new EnemyUnit(1, UnitType.Marine, 100, 100));
radar.DetectEnemy(new EnemyUnit(2, UnitType.Marine, 105, 100));
radar.DetectEnemy(new EnemyUnit(3, UnitType.SiegeTank, 110, 100));
radar.DetectEnemy(new EnemyUnit(1, UnitType.Marine, 102, 98)); // 중복! (ID가 같음)

radar.ShowDetectedEnemies();

Console.WriteLine("\n--- 전투 발생 ---");
radar.EnemyDestroyed(1);
radar.EnemyDestroyed(2);

radar.ShowDetectedEnemies();
radar.ShowStatistics();
```

### 해시셋 활용: 방문한 타일 추적

```csharp
public struct Tile
{
    public int X { get; set; }
    public int Y { get; set; }

    public Tile(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}

public class PathfindingSystem
{
    private HashSet<Tile> visitedTiles = new HashSet<Tile>();
    private HashSet<Tile> obstacles = new HashSet<Tile>();

    public void AddObstacle(int x, int y)
    {
        obstacles.Add(new Tile(x, y));
    }

    public void VisitTile(int x, int y)
    {
        Tile tile = new Tile(x, y);

        if (obstacles.Contains(tile))
        {
            Console.WriteLine($"{tile} - 장애물이 있어 이동 불가!");
            return;
        }

        if (visitedTiles.Add(tile))
        {
            Console.WriteLine($"{tile} - 새로운 타일 방문");
        }
        else
        {
            Console.WriteLine($"{tile} - 이미 방문한 타일");
        }
    }

    public bool HasVisited(int x, int y)
    {
        return visitedTiles.Contains(new Tile(x, y));
    }

    public void ShowExplorationProgress()
    {
        Console.WriteLine($"\n탐색 진행도: {visitedTiles.Count}개 타일 방문");
    }
}

// 사용 예시
PathfindingSystem pathfinding = new PathfindingSystem();

// 장애물 설정 (미네랄, 가스)
pathfinding.AddObstacle(5, 5);
pathfinding.AddObstacle(5, 6);

// 유닛 이동
pathfinding.VisitTile(1, 1);
pathfinding.VisitTile(2, 1);
pathfinding.VisitTile(3, 1);
pathfinding.VisitTile(3, 1); // 중복 방문
pathfinding.VisitTile(5, 5); // 장애물!

pathfinding.ShowExplorationProgress();
```

---

## 링크드 리스트 (LinkedList<T>)

### 개념
LinkedList는 각 요소가 다음(또는 이전) 요소를 가리키는 노드로 연결된 자료구조입니다.

### 특징
- **중간 삽입/삭제**: O(1) - 매우 빠름 (노드 위치를 알고 있을 때)
- **인덱스 접근**: O(n) - 느림
- **메모리**: 포인터 저장으로 인한 오버헤드

### 스타크래프트 예시: 순찰 경로 시스템

```csharp
public class PatrolPoint
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Description { get; set; }
    public int WaitTime { get; set; } // 해당 지점에서 대기 시간 (초)

    public PatrolPoint(int x, int y, string desc = "", int waitTime = 0)
    {
        X = x;
        Y = y;
        Description = desc;
        WaitTime = waitTime;
    }

    public override string ToString()
    {
        return $"({X}, {Y}) {Description}";
    }
}

public class PatrolRoute
{
    private LinkedList<PatrolPoint> route = new LinkedList<PatrolPoint>();
    private LinkedListNode<PatrolPoint> currentNode = null;

    // 순찰 경로 끝에 추가
    public void AddPatrolPoint(PatrolPoint point)
    {
        route.AddLast(point);
        Console.WriteLine($"순찰 지점 추가: {point}");

        // 첫 번째 지점이면 현재 위치로 설정
        if (currentNode == null)
        {
            currentNode = route.First;
        }
    }

    // 특정 위치에 순찰 지점 삽입
    public void InsertAfterCurrent(PatrolPoint point)
    {
        if (currentNode != null)
        {
            route.AddAfter(currentNode, point);
            Console.WriteLine($"현재 위치 다음에 순찰 지점 추가: {point}");
        }
    }

    // 다음 순찰 지점으로 이동
    public PatrolPoint MoveToNext()
    {
        if (currentNode == null)
        {
            Console.WriteLine("순찰 경로가 비어있습니다.");
            return null;
        }

        // 다음 노드로 이동 (순환)
        currentNode = currentNode.Next ?? route.First;

        Console.WriteLine($"다음 순찰 지점으로 이동: {currentNode.Value}");
        return currentNode.Value;
    }

    // 이전 순찰 지점으로 이동
    public PatrolPoint MoveToPrevious()
    {
        if (currentNode == null)
        {
            Console.WriteLine("순찰 경로가 비어있습니다.");
            return null;
        }

        // 이전 노드로 이동 (순환)
        currentNode = currentNode.Previous ?? route.Last;

        Console.WriteLine($"이전 순찰 지점으로 이동: {currentNode.Value}");
        return currentNode.Value;
    }

    // 현재 지점 제거
    public void RemoveCurrentPoint()
    {
        if (currentNode == null)
        {
            Console.WriteLine("제거할 순찰 지점이 없습니다.");
            return;
        }

        var nodeToRemove = currentNode;

        // 다음 노드로 이동 (없으면 이전, 그것도 없으면 null)
        currentNode = currentNode.Next ?? currentNode.Previous;

        route.Remove(nodeToRemove);
        Console.WriteLine($"순찰 지점 제거됨: {nodeToRemove.Value}");

        if (currentNode != null)
        {
            Console.WriteLine($"현재 위치: {currentNode.Value}");
        }
    }

    // 전체 순찰 경로 출력
    public void ShowRoute()
    {
        Console.WriteLine($"\n=== 순찰 경로 ({route.Count}개 지점) ===");

        if (route.Count == 0)
        {
            Console.WriteLine("순찰 지점이 없습니다.");
            return;
        }

        int index = 1;
        var node = route.First;
        while (node != null)
        {
            string marker = (node == currentNode) ? " <- 현재 위치" : "";
            Console.WriteLine($"{index}. {node.Value}{marker}");
            node = node.Next;
            index++;
        }
    }

    // 순찰 경로 역순으로 출력
    public void ShowRouteReverse()
    {
        Console.WriteLine($"\n=== 순찰 경로 (역순) ===");

        var node = route.Last;
        int index = route.Count;
        while (node != null)
        {
            string marker = (node == currentNode) ? " <- 현재 위치" : "";
            Console.WriteLine($"{index}. {node.Value}{marker}");
            node = node.Previous;
            index--;
        }
    }

    // 순찰 시뮬레이션
    public void SimulatePatrol(int cycles)
    {
        Console.WriteLine($"\n=== {cycles}사이클 순찰 시뮬레이션 ===");

        int totalPoints = route.Count * cycles;
        for (int i = 0; i < totalPoints; i++)
        {
            var point = MoveToNext();
            if (point != null && point.WaitTime > 0)
            {
                Console.WriteLine($"  -> 이 지점에서 {point.WaitTime}초 대기");
            }
        }
    }
}

// 사용 예시
PatrolRoute patrol = new PatrolRoute();

// 순찰 경로 설정
patrol.AddPatrolPoint(new PatrolPoint(50, 50, "본진 앞", 3));
patrol.AddPatrolPoint(new PatrolPoint(100, 50, "오른쪽 언덕", 5));
patrol.AddPatrolPoint(new PatrolPoint(100, 100, "적 진영 근처", 10));
patrol.AddPatrolPoint(new PatrolPoint(50, 100, "왼쪽 확장 기지", 5));

patrol.ShowRoute();

Console.WriteLine("\n--- 순찰 시작 ---");
patrol.MoveToNext();
patrol.MoveToNext();

Console.WriteLine("\n--- 긴급 상황! 중간에 새로운 감시 지점 추가 ---");
patrol.InsertAfterCurrent(new PatrolPoint(120, 80, "미네랄 라인", 3));

patrol.ShowRoute();

Console.WriteLine("\n--- 계속 순찰 ---");
patrol.MoveToNext();
patrol.MoveToNext();
patrol.MoveToNext(); // 처음으로 돌아감 (순환)

patrol.ShowRoute();

Console.WriteLine("\n--- 3사이클 순찰 시뮬레이션 ---");
patrol.SimulatePatrol(3);
```

### LinkedList 활용: 버프/디버프 효과 관리

```csharp
public class Effect
{
    public string Name { get; set; }
    public int Duration { get; set; } // 남은 지속 시간
    public int Power { get; set; }
    public bool IsBuff { get; set; }

    public Effect(string name, int duration, int power, bool isBuff)
    {
        Name = name;
        Duration = duration;
        Power = power;
        IsBuff = isBuff;
    }

    public override string ToString()
    {
        string type = IsBuff ? "[버프]" : "[디버프]";
        return $"{type} {Name} (효과: {Power}, 남은 시간: {Duration}초)";
    }
}

public class EffectManager
{
    private LinkedList<Effect> activeEffects = new LinkedList<Effect>();

    // 효과 추가
    public void AddEffect(Effect effect)
    {
        activeEffects.AddLast(effect);
        Console.WriteLine($"효과 추가: {effect}");
    }

    // 시간 경과 업데이트 (매 초마다 호출)
    public void Update(int deltaTime = 1)
    {
        var node = activeEffects.First;

        while (node != null)
        {
            var nextNode = node.Next; // 미리 저장 (Remove 시 필요)

            node.Value.Duration -= deltaTime;

            if (node.Value.Duration <= 0)
            {
                Console.WriteLine($"효과 만료: {node.Value.Name}");
                activeEffects.Remove(node);
            }

            node = nextNode;
        }
    }

    // 총 공격력 버프 계산
    public int GetTotalAttackBonus()
    {
        int total = 0;
        foreach (var effect in activeEffects)
        {
            if (effect.IsBuff && effect.Name.Contains("공격"))
            {
                total += effect.Power;
            }
        }
        return total;
    }

    // 현재 활성 효과 출력
    public void ShowActiveEffects()
    {
        Console.WriteLine($"\n=== 활성 효과 ({activeEffects.Count}개) ===");
        foreach (var effect in activeEffects)
        {
            Console.WriteLine($"  {effect}");
        }
    }
}

// 사용 예시
EffectManager effectMgr = new EffectManager();

// 다양한 효과 적용
effectMgr.AddEffect(new Effect("스팀팩", 10, 50, true)); // 공격속도 +50%
effectMgr.AddEffect(new Effect("옵티컬 플레어", 8, -100, false)); // 시야 -100%
effectMgr.AddEffect(new Effect("공격력 업그레이드", 999, 1, true)); // 영구 효과

effectMgr.ShowActiveEffects();

Console.WriteLine("\n--- 5초 경과 ---");
for (int i = 0; i < 5; i++)
{
    effectMgr.Update(1);
}

effectMgr.ShowActiveEffects();
```

---

## 자료구조 성능 비교

| 작업 | Array | List | Dictionary | Queue | Stack | HashSet | LinkedList |
|------|-------|------|------------|-------|-------|---------|------------|
| **끝에 추가** | N/A | O(1)* | O(1) | O(1) | O(1) | O(1) | O(1) |
| **시작에 추가** | N/A | O(n) | N/A | N/A | N/A | O(1) | O(1) |
| **중간에 삽입** | N/A | O(n) | N/A | N/A | N/A | O(1) | O(1)** |
| **인덱스 접근** | O(1) | O(1) | N/A | N/A | N/A | N/A | O(n) |
| **키/값 검색** | O(n) | O(n) | O(1) | N/A | N/A | O(1) | O(n) |
| **제거** | N/A | O(n) | O(1) | O(1)*** | O(1)*** | O(1) | O(1)** |
| **메모리 효율** | 최고 | 높음 | 중간 | 높음 | 높음 | 중간 | 낮음 |

\* 용량 재할당이 필요 없는 경우
\** 노드 위치를 알고 있는 경우
\*** 앞/위에서만 제거 가능

### 자료구조 선택 가이드

```csharp
// ✅ 이럴 때 Array를 사용하세요
// - 크기가 고정되어 있을 때
// - 빠른 인덱스 접근이 필요할 때
int[] playerScores = new int[4]; // 4인 게임

// ✅ 이럴 때 List를 사용하세요
// - 크기가 동적으로 변할 때
// - 순서가 중요할 때
// - 인덱스 접근이 필요할 때
List<Unit> selectedUnits = new List<Unit>();

// ✅ 이럴 때 Dictionary를 사용하세요
// - 키로 빠르게 값을 찾아야 할 때
// - 유일한 식별자로 데이터를 관리할 때
Dictionary<int, Player> playerById = new Dictionary<int, Player>();

// ✅ 이럴 때 Queue를 사용하세요
// - 선입선출 순서가 필요할 때
// - 대기열, 작업 순서 관리
Queue<ProductionOrder> buildQueue = new Queue<ProductionOrder>();

// ✅ 이럴 때 Stack을 사용하세요
// - 후입선출 순서가 필요할 때
// - Undo/Redo 기능
// - 역순 처리
Stack<GameAction> actionHistory = new Stack<GameAction>();

// ✅ 이럴 때 HashSet을 사용하세요
// - 중복을 제거해야 할 때
// - 포함 여부만 확인하면 될 때
// - 집합 연산이 필요할 때
HashSet<int> visitedTileIds = new HashSet<int>();

// ✅ 이럴 때 LinkedList를 사용하세요
// - 중간 삽입/삭제가 빈번할 때
// - 순환 구조가 필요할 때
LinkedList<PatrolPoint> patrolRoute = new LinkedList<PatrolPoint>();
```

---

## 실전 프로젝트: 스타크래프트 유닛 관리 시스템

모든 자료구조를 활용한 종합 프로젝트입니다.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarcraftUnitManager
{
    // ==================== 유닛 정의 ====================

    public enum UnitType
    {
        SCV, Marine, Medic, Firebat, Ghost, Vulture, SiegeTank, Goliath, Wraith, Dropship
    }

    public enum UnitState
    {
        Idle, Moving, Attacking, Gathering, Repairing, Dead
    }

    public class Unit
    {
        private static int _nextId = 1;

        public int Id { get; private set; }
        public string Name { get; set; }
        public UnitType Type { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int AttackPower { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public UnitState State { get; set; }
        public bool IsAlive => HP > 0;

        public Unit(UnitType type, int x, int y)
        {
            Id = _nextId++;
            Type = type;
            X = x;
            Y = y;
            State = UnitState.Idle;

            // 타입별 스탯 설정
            SetStatsByType(type);
            Name = $"{type} #{Id}";
        }

        private void SetStatsByType(UnitType type)
        {
            switch (type)
            {
                case UnitType.SCV:
                    MaxHP = HP = 60;
                    AttackPower = 5;
                    break;
                case UnitType.Marine:
                    MaxHP = HP = 40;
                    AttackPower = 6;
                    break;
                case UnitType.Medic:
                    MaxHP = HP = 60;
                    AttackPower = 0;
                    break;
                case UnitType.Firebat:
                    MaxHP = HP = 50;
                    AttackPower = 16;
                    break;
                case UnitType.SiegeTank:
                    MaxHP = HP = 150;
                    AttackPower = 30;
                    break;
                default:
                    MaxHP = HP = 100;
                    AttackPower = 10;
                    break;
            }
        }

        public void TakeDamage(int damage)
        {
            HP -= damage;
            if (HP <= 0)
            {
                HP = 0;
                State = UnitState.Dead;
            }
        }

        public void Heal(int amount)
        {
            if (IsAlive)
            {
                HP = Math.Min(HP + amount, MaxHP);
            }
        }

        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
            State = UnitState.Moving;
        }

        public override string ToString()
        {
            return $"{Name} [{Type}] HP:{HP}/{MaxHP} 위치:({X},{Y}) 상태:{State}";
        }
    }

    // ==================== 게임 매니저 ====================

    public class GameManager
    {
        // Dictionary: ID로 유닛 빠르게 찾기
        private Dictionary<int, Unit> allUnitsById = new Dictionary<int, Unit>();

        // List: 선택된 유닛 관리 (순서 중요)
        private List<Unit> selectedUnits = new List<Unit>();

        // Queue: 유닛 생산 대기열
        private Queue<UnitType> productionQueue = new Queue<UnitType>();

        // Stack: 행동 히스토리 (Undo)
        private Stack<string> actionHistory = new Stack<string>();

        // HashSet: 시야 확보된 타일
        private HashSet<(int x, int y)> exploredTiles = new HashSet<(int, int)>();

        // LinkedList: 순찰 경로
        private LinkedList<(int x, int y)> patrolRoute = new LinkedList<(int, int)>();

        // 게임 자원
        private int minerals = 500;
        private int gas = 200;

        private const int MAX_SELECTION = 12;

        // ==================== 유닛 생성 ====================

        public Unit CreateUnit(UnitType type, int x, int y)
        {
            Unit unit = new Unit(type, x, y);
            allUnitsById[unit.Id] = unit;

            RecordAction($"유닛 생성: {unit.Name}");
            RevealArea(x, y, 5); // 시야 확보

            Console.WriteLine($"[생성] {unit}");
            return unit;
        }

        public Unit GetUnitById(int id)
        {
            allUnitsById.TryGetValue(id, out Unit unit);
            return unit;
        }

        // ==================== 유닛 선택 (List) ====================

        public bool SelectUnit(int unitId)
        {
            Unit unit = GetUnitById(unitId);
            if (unit == null || !unit.IsAlive)
            {
                Console.WriteLine("유닛을 찾을 수 없거나 죽은 유닛입니다.");
                return false;
            }

            if (selectedUnits.Count >= MAX_SELECTION)
            {
                Console.WriteLine($"최대 {MAX_SELECTION}개까지만 선택 가능합니다!");
                return false;
            }

            if (!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                RecordAction($"유닛 선택: {unit.Name}");
                Console.WriteLine($"[선택] {unit.Name} (선택된 유닛: {selectedUnits.Count}/{MAX_SELECTION})");
                return true;
            }

            return false;
        }

        public void DeselectAll()
        {
            int count = selectedUnits.Count;
            selectedUnits.Clear();
            RecordAction($"{count}개 유닛 선택 해제");
            Console.WriteLine($"[선택 해제] 모든 유닛 선택 해제");
        }

        public void ShowSelectedUnits()
        {
            Console.WriteLine($"\n=== 선택된 유닛 ({selectedUnits.Count}/{MAX_SELECTION}) ===");
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {selectedUnits[i]}");
            }
        }

        // ==================== 유닛 생산 (Queue) ====================

        public void QueueProduction(UnitType type)
        {
            productionQueue.Enqueue(type);
            RecordAction($"생산 대기열 추가: {type}");
            Console.WriteLine($"[대기열] {type} 추가 (대기: {productionQueue.Count}개)");
        }

        public Unit ProduceNextUnit(int x, int y)
        {
            if (productionQueue.Count == 0)
            {
                Console.WriteLine("생산 대기열이 비어있습니다.");
                return null;
            }

            UnitType type = productionQueue.Dequeue();
            Console.WriteLine($"[생산 완료] {type} (남은 대기: {productionQueue.Count}개)");

            return CreateUnit(type, x, y);
        }

        public void ShowProductionQueue()
        {
            Console.WriteLine($"\n=== 생산 대기열 ({productionQueue.Count}개) ===");
            int i = 1;
            foreach (var type in productionQueue)
            {
                Console.WriteLine($"{i}. {type}");
                i++;
            }
        }

        // ==================== 행동 히스토리 (Stack) ====================

        private void RecordAction(string action)
        {
            actionHistory.Push($"[{DateTime.Now:HH:mm:ss}] {action}");
        }

        public void Undo()
        {
            if (actionHistory.Count > 0)
            {
                string action = actionHistory.Pop();
                Console.WriteLine($"[Undo] {action}");
            }
            else
            {
                Console.WriteLine("되돌릴 행동이 없습니다.");
            }
        }

        public void ShowHistory(int count = 10)
        {
            Console.WriteLine($"\n=== 최근 행동 (최대 {count}개) ===");
            int i = 1;
            foreach (var action in actionHistory)
            {
                Console.WriteLine($"{i}. {action}");
                if (i >= count) break;
                i++;
            }
        }

        // ==================== 시야 탐색 (HashSet) ====================

        private void RevealArea(int centerX, int centerY, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        exploredTiles.Add((centerX + dx, centerY + dy));
                    }
                }
            }
        }

        public bool IsTileExplored(int x, int y)
        {
            return exploredTiles.Contains((x, y));
        }

        public void ShowExplorationStats()
        {
            Console.WriteLine($"\n=== 탐색 통계 ===");
            Console.WriteLine($"탐색된 타일: {exploredTiles.Count}개");
        }

        // ==================== 순찰 경로 (LinkedList) ====================

        public void AddPatrolPoint(int x, int y)
        {
            patrolRoute.AddLast((x, y));
            RecordAction($"순찰 지점 추가: ({x}, {y})");
            Console.WriteLine($"[순찰 경로] 지점 추가: ({x}, {y}) - 총 {patrolRoute.Count}개");
        }

        public void ShowPatrolRoute()
        {
            Console.WriteLine($"\n=== 순찰 경로 ({patrolRoute.Count}개 지점) ===");
            int i = 1;
            foreach (var point in patrolRoute)
            {
                Console.WriteLine($"{i}. ({point.x}, {point.y})");
                i++;
            }
        }

        public void ExecutePatrol()
        {
            if (selectedUnits.Count == 0)
            {
                Console.WriteLine("선택된 유닛이 없습니다.");
                return;
            }

            if (patrolRoute.Count == 0)
            {
                Console.WriteLine("순찰 경로가 설정되지 않았습니다.");
                return;
            }

            Console.WriteLine($"\n=== {selectedUnits.Count}개 유닛 순찰 시작 ===");
            foreach (var point in patrolRoute)
            {
                foreach (var unit in selectedUnits)
                {
                    unit.MoveTo(point.x, point.y);
                }
                Console.WriteLine($"전체 유닛이 ({point.x}, {point.y})로 이동");
                RevealArea(point.x, point.y, 5);
            }

            RecordAction($"{selectedUnits.Count}개 유닛 순찰 완료");
        }

        // ==================== 전투 시스템 ====================

        public void SimulateBattle()
        {
            if (selectedUnits.Count == 0)
            {
                Console.WriteLine("선택된 유닛이 없습니다.");
                return;
            }

            Console.WriteLine($"\n=== 전투 시작! ===");
            Random rand = new Random();

            foreach (var unit in selectedUnits.ToList())
            {
                int damage = rand.Next(10, 30);
                unit.TakeDamage(damage);
                Console.WriteLine($"{unit.Name}이(가) {damage} 데미지를 받았습니다. (남은 HP: {unit.HP})");

                if (!unit.IsAlive)
                {
                    Console.WriteLine($"💀 {unit.Name}이(가) 전사했습니다...");
                }
            }

            // 죽은 유닛 제거
            int deadCount = selectedUnits.RemoveAll(u => !u.IsAlive);
            if (deadCount > 0)
            {
                RecordAction($"{deadCount}개 유닛 전사");
            }

            // 메딕으로 치료
            var medics = selectedUnits.Where(u => u.Type == UnitType.Medic).ToList();
            var injured = selectedUnits.Where(u => u.HP < u.MaxHP && u.Type != UnitType.Medic).ToList();

            if (medics.Count > 0 && injured.Count > 0)
            {
                Console.WriteLine($"\n=== 메딕 {medics.Count}명이 치료 시작 ===");
                foreach (var patient in injured)
                {
                    int healAmount = 10;
                    patient.Heal(healAmount);
                    Console.WriteLine($"💊 {patient.Name} 치료 완료 (현재 HP: {patient.HP}/{patient.MaxHP})");
                }
            }
        }

        // ==================== 통계 및 정보 ====================

        public void ShowAllUnits()
        {
            Console.WriteLine($"\n=== 전체 유닛 ({allUnitsById.Count}개) ===");

            var aliveUnits = allUnitsById.Values.Where(u => u.IsAlive).ToList();
            var deadUnits = allUnitsById.Values.Where(u => !u.IsAlive).ToList();

            Console.WriteLine($"\n생존: {aliveUnits.Count}개");
            foreach (var unit in aliveUnits)
            {
                Console.WriteLine($"  {unit}");
            }

            if (deadUnits.Count > 0)
            {
                Console.WriteLine($"\n전사: {deadUnits.Count}개");
                foreach (var unit in deadUnits)
                {
                    Console.WriteLine($"  {unit}");
                }
            }
        }

        public void ShowGameStats()
        {
            Console.WriteLine($"\n========== 게임 통계 ==========");
            Console.WriteLine($"자원: {minerals} 미네랄, {gas} 가스");
            Console.WriteLine($"전체 유닛: {allUnitsById.Count}개");
            Console.WriteLine($"  - 생존: {allUnitsById.Values.Count(u => u.IsAlive)}개");
            Console.WriteLine($"  - 전사: {allUnitsById.Values.Count(u => !u.IsAlive)}개");
            Console.WriteLine($"선택된 유닛: {selectedUnits.Count}/{MAX_SELECTION}");
            Console.WriteLine($"생산 대기열: {productionQueue.Count}개");
            Console.WriteLine($"순찰 경로: {patrolRoute.Count}개 지점");
            Console.WriteLine($"탐색된 지역: {exploredTiles.Count}개 타일");
            Console.WriteLine($"행동 기록: {actionHistory.Count}개");
            Console.WriteLine($"==============================");
        }
    }

    // ==================== 메인 프로그램 ====================

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("   스타크래프트 유닛 관리 시스템");
            Console.WriteLine("========================================\n");

            GameManager game = new GameManager();

            // 1. 유닛 생성 (Dictionary)
            Console.WriteLine("[ 1단계: 유닛 생성 ]\n");
            var unit1 = game.CreateUnit(UnitType.Marine, 10, 10);
            var unit2 = game.CreateUnit(UnitType.Marine, 12, 10);
            var unit3 = game.CreateUnit(UnitType.Medic, 11, 12);
            var unit4 = game.CreateUnit(UnitType.Firebat, 13, 11);
            var unit5 = game.CreateUnit(UnitType.SiegeTank, 15, 10);

            // 2. 유닛 선택 (List)
            Console.WriteLine("\n[ 2단계: 유닛 선택 ]\n");
            game.SelectUnit(unit1.Id);
            game.SelectUnit(unit2.Id);
            game.SelectUnit(unit3.Id);
            game.SelectUnit(unit4.Id);
            game.ShowSelectedUnits();

            // 3. 생산 대기열 (Queue)
            Console.WriteLine("\n[ 3단계: 추가 유닛 생산 대기열 ]\n");
            game.QueueProduction(UnitType.Marine);
            game.QueueProduction(UnitType.Marine);
            game.QueueProduction(UnitType.Medic);
            game.ShowProductionQueue();

            Console.WriteLine("\n생산 시작...\n");
            game.ProduceNextUnit(20, 20);
            game.ProduceNextUnit(22, 20);
            game.ShowProductionQueue();

            // 4. 순찰 경로 설정 (LinkedList)
            Console.WriteLine("\n[ 4단계: 순찰 경로 설정 ]\n");
            game.AddPatrolPoint(30, 30);
            game.AddPatrolPoint(50, 30);
            game.AddPatrolPoint(50, 50);
            game.AddPatrolPoint(30, 50);
            game.ShowPatrolRoute();

            // 5. 순찰 실행
            Console.WriteLine("\n[ 5단계: 순찰 실행 ]\n");
            game.ExecutePatrol();

            // 6. 전투 발생!
            Console.WriteLine("\n[ 6단계: 적과 조우! 전투 발생 ]\n");
            game.SimulateBattle();

            // 7. 상태 확인
            Console.WriteLine("\n[ 7단계: 전투 후 상황 ]\n");
            game.ShowSelectedUnits();

            // 8. 탐색 통계 (HashSet)
            Console.WriteLine("\n[ 8단계: 탐색 통계 ]\n");
            game.ShowExplorationStats();

            // 9. 행동 히스토리 (Stack)
            Console.WriteLine("\n[ 9단계: 행동 히스토리 ]\n");
            game.ShowHistory(15);

            // 10. 최종 통계
            Console.WriteLine("\n[ 최종 통계 ]\n");
            game.ShowGameStats();
            game.ShowAllUnits();

            Console.WriteLine("\n========================================");
            Console.WriteLine("         시뮬레이션 완료!");
            Console.WriteLine("========================================");
        }
    }
}
```

---

## 마치며

### 핵심 정리

1. **Array**: 고정 크기, 빠른 접근 → 크기가 변하지 않는 데이터
2. **List**: 동적 크기, 순서 중요 → 가장 많이 사용하는 자료구조
3. **Dictionary**: 키-값 쌍, 빠른 검색 → ID로 데이터 찾기
4. **Queue**: 선입선출 → 대기열, 작업 순서
5. **Stack**: 후입선출 → Undo/Redo, 역순 처리
6. **HashSet**: 중복 제거, 빠른 검색 → 집합 연산
7. **LinkedList**: 중간 삽입/삭제 → 순환 구조

### 실전 팁

```csharp
// ❌ 나쁜 예: 잘못된 자료구조 선택
List<int> visitedTileIds = new List<int>();
if (visitedTileIds.Contains(tileId)) // O(n) - 느림!
{
    // ...
}

// ✅ 좋은 예: 적절한 자료구조 선택
HashSet<int> visitedTileIds = new HashSet<int>();
if (visitedTileIds.Contains(tileId)) // O(1) - 빠름!
{
    // ...
}
```

### 다음 단계

5강에서는 **디자인 패턴**을 배울 예정입니다. 오늘 배운 자료구조들이 디자인 패턴에서 어떻게 활용되는지 알아보겠습니다!

---

## 연습 문제

### 문제 1: 자원 관리 시스템
미네랄과 가스를 관리하는 `ResourceManager` 클래스를 작성하세요. 자원 획득/소비 내역을 Stack으로 관리하여 Undo 기능을 구현하세요.

### 문제 2: 멀티플레이어 게임
여러 플레이어의 정보를 Dictionary로 관리하는 `MultiplayerManager`를 작성하세요. 플레이어 ID를 키로 사용하세요.

### 문제 3: 스킬 쿨다운 시스템
스킬 이름과 남은 쿨다운 시간을 관리하는 시스템을 작성하세요. 매 프레임 업데이트 시 쿨다운을 감소시키세요.

좋은 공부 되세요! 🎮
