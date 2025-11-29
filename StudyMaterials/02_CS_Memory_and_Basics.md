# 2. C# 기초 및 메모리 구조 (심화)

## 1. 필수 용어 상세 설명

### ① 클래스 vs 객체 vs 인스턴스 (붕어빵 비유)
이 세 가지는 비슷해 보이지만 명확한 차이가 있습니다.

*   **클래스 (Class)**: **설계도**.
    *   "붕어빵 틀" 그 자체입니다.
    *   실제 먹을 수 있는 붕어빵이 아닙니다. 그냥 쇠로 된 틀입니다.
    *   코드 상에 `class Monster { ... }` 라고 적혀있는 텍스트 덩어리입니다.
*   **객체 (Object)**: **구현할 대상**.
    *   "붕어빵"이라는 개념입니다.
    *   아직 세상에 나오지 않았을 수도 있고, 나올 수도 있는 추상적인 의미가 강합니다.
*   **인스턴스 (Instance)**: **실체**.
    *   틀(Class)에 반죽을 부어 구워내서 **실제 메모리(Heap)에 생성된** 붕어빵입니다.
    *   `new Monster();`를 하는 순간 인스턴스가 됩니다.
    *   우리는 이 인스턴스를 조작해서 게임을 만듭니다.

```csharp
// [클래스] 설계도
public class Monster 
{
    public int hp;
}

void Start() 
{
    // m1은 "참조 변수" (리모컨)
    // new Monster()가 실행될 때 비로소 "인스턴스"가 힙 메모리에 생성됨
    Monster m1 = new Monster(); 
}
```

### ② 델리게이트 (Delegate) - 대리자
함수를 변수처럼 다루는 기능입니다.
*   **기본 개념**: "나 대신 이 함수 좀 실행해줘" 하고 함수를 건네주는 것.
*   **왜 쓰는가?**: 콜백(Callback) 구현, 이벤트 시스템, 유연한 설계.

**예시: 버튼 클릭 시 실행할 행동을 전달**
```csharp
// 1. 델리게이트 선언 (함수 족보: 반환형 void, 매개변수 없음)
public delegate void ButtonClickAction();

public class Button
{
    // 2. 델리게이트 변수 생성 (함수를 담을 빈 그릇)
    public ButtonClickAction OnClick;

    public void Press()
    {
        // 3. 버튼이 눌리면 담겨있는 함수를 실행 (누가 담겨있는지 난 모름)
        if (OnClick != null)
            OnClick();
    }
}

// 사용 예시
void Start()
{
    Button btn = new Button();
    // 함수를 변수처럼 집어넣음! (괄호() 없음 주의)
    btn.OnClick = OpenInventory;

    btn.Press(); // OpenInventory가 실행됨
}

void OpenInventory() { Console.WriteLine("인벤토리 열림"); }
```

### ③ 접근 제한자 (Access Modifiers)
클래스의 멤버를 누가 볼 수 있는지 제어합니다.

| 제한자 | 범위 | 설명 |
|--------|------|------|
| `public` | 모든 곳 | 누구나 접근 가능 |
| `private` | 클래스 내부만 | 외부에서 접근 불가 (기본값) |
| `protected` | 상속받은 자식까지 | 자식 클래스에서 접근 가능 |
| `internal` | 같은 어셈블리(프로젝트) | 다른 프로젝트에서 접근 불가 |

```csharp
public class Player
{
    public int level = 1;          // 어디서든 접근 가능
    private int experience = 0;     // Player 클래스 안에서만
    protected int mana = 100;       // Player와 자식 클래스만
    internal int gold = 500;        // 같은 프로젝트 내에서만
}

// 다른 클래스에서
Player p = new Player();
p.level = 10;      // ✅ OK
p.experience = 50; // ❌ 컴파일 에러! private
```

**실전 팁**:
- 기본적으로 **모든 변수는 private**으로 시작
- 외부에서 접근이 필요하면 **public 프로퍼티**로 노출

---

## 2. 메모리 영역 (Memory Layout) 상세
프로그램이 실행되면 OS는 메모리를 4등분해서 관리합니다.

### ① 코드 (Code)
*   우리가 짠 소스 코드가 기계어로 번역되어 저장됨.
*   **특징**: 읽기 전용 (수정 불가).

### ② 데이터 (Data)
*   **저장 대상**: `static` 변수, 전역 변수.
*   **생명 주기**: 프로그램 시작 시 생성 ~ 프로그램 종료 시 삭제.
*   **주의**: 게임 내내 메모리를 차지하므로, 너무 큰 데이터를 static으로 잡으면 메모리 낭비.
*   **싱글턴**이 바로 이 영역(정확히는 static 변수가 가리키는 참조)을 활용함.

### ③ 힙 (Heap) - "자유 저장소"
*   **저장 대상**: `new`로 생성된 모든 것 (클래스 인스턴스, 배열, 리스트 등).
*   **특징**:
    *   개발자가 원할 때 만들고, (C++에선) 원할 때 지움.
    *   C#은 **Garbage Collector (GC)**가 주기적으로 청소함.
    *   메모리 구조가 복잡해서 스택보다 접근 속도가 약간 느림.
    *   **참조 타입(Reference Type)**이 사는 곳.

### ④ 스택 (Stack) - "작업장"
*   **저장 대상**: 지역 변수, 매개 변수, 리턴 값.
*   **특징**:
    *   함수가 호출될 때 쌓이고(Push), 끝나면 바로 사라짐(Pop).
    *   매우 빠르고 효율적.
    *   **값 타입(Value Type)**이 사는 곳.

---

## 3. 값 타입 vs 참조 타입 (핵심 심화)
메모리에 어떻게 저장되는지가 다릅니다.

### 상황: 변수를 다른 변수에 대입할 때 (`b = a`)

#### 1. 값 타입 (Value Type) : `int`, `float`, `struct`, `bool`
*   **복사 방식**: 내용물(값) 자체를 복사.
*   **결과**: 원본과 사본은 **완전히 남남**.

```csharp
int a = 10;
int b = a; // 10이라는 값을 복사해서 b에 넣음.
b = 20;    // b만 20이 됨.

Console.WriteLine(a); // 10 (원본 유지)
```

#### 2. 참조 타입 (Reference Type) : `class`, `string`, `array`, `delegate`
*   **복사 방식**: 힙 영역의 **주소(참조)**만 복사. (바로가기 아이콘 복사)
*   **결과**: 원본과 사본이 **같은 놈을 가리킴**.

```csharp
class Monster { public int hp; }

Monster m1 = new Monster();
m1.hp = 100;

Monster m2 = m1; // m1이 가리키는 주소를 m2에게 줌. (둘 다 같은 몬스터를 봄)
m2.hp = 50;      // m2를 통해 hp를 깎음.

Console.WriteLine(m1.hp); // 50 (m1도 같이 깎여있음! 중요!)
```

> **비유**:
> *   **값 타입**: 엑셀 파일을 복사해서 친구에게 줌. 친구가 수정해도 내 파일은 그대로.
> *   **참조 타입**: 구글 스프레드시트 링크를 친구에게 줌. 친구가 수정하면 내 화면에서도 바뀜.

---

## 4. 프로퍼티 (Property) vs 필드 (Field)

### 필드 (Field)
변수 그 자체입니다. 직접 값을 저장합니다.

```csharp
public class Player
{
    public int hp = 100; // 필드
}

Player p = new Player();
p.hp = 50; // 직접 접근
```

### 프로퍼티 (Property)
**필드를 보호하면서 접근하는 방법**입니다. `get`/`set` 블록으로 제어합니다.

```csharp
public class Player
{
    private int hp = 100; // private 필드 (외부 접근 차단)

    // 프로퍼티 (겉보기엔 변수처럼 보이지만 함수임)
    public int HP
    {
        get { return hp; } // 읽기
        set
        {
            // 쓰기 (유효성 검증 가능!)
            if (value < 0)
                hp = 0;
            else if (value > 100)
                hp = 100;
            else
                hp = value;
        }
    }
}

// 사용
Player p = new Player();
p.HP = 150;              // set 호출 -> 100으로 제한됨
Console.WriteLine(p.HP); // get 호출 -> 100 출력
```

### 자동 프로퍼티 (Auto-Property)
간단한 경우 필드 없이 바로 선언 가능합니다.

```csharp
public class Player
{
    // 컴파일러가 자동으로 private 필드를 만들어줌
    public int Level { get; set; } = 1;

    // 읽기 전용 (초기화 후 변경 불가)
    public string Name { get; private set; }

    // 완전 읽기 전용 (생성자에서만 할당 가능)
    public int MaxHP { get; }

    public Player(string name)
    {
        Name = name;
        MaxHP = 100;
    }
}
```

### 언제 뭘 쓸까?

| 상황 | 선택 | 이유 |
|------|------|------|
| 외부에서 읽기만 필요 | `public int HP { get; private set; }` | 쓰기 방지 |
| 값 검증이 필요함 | get/set 블록 직접 작성 | HP가 음수가 되면 안 됨 |
| 단순 데이터 저장 | `public int Score { get; set; }` | 자동 프로퍼티로 간단히 |
| 유니티 인스펙터 노출 | `public` 필드 또는 `[SerializeField] private` | 프로퍼티는 인스펙터에 안 나옴 |

**실전 예시: HP 시스템**
```csharp
public class Character
{
    private int hp = 100;
    public int MaxHP { get; private set; } = 100;

    public int HP
    {
        get { return hp; }
        set
        {
            hp = Mathf.Clamp(value, 0, MaxHP); // 0~MaxHP 범위로 제한

            // HP가 0이 되면 사망 처리
            if (hp <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        Console.WriteLine("캐릭터 사망!");
    }
}

// 사용
Character player = new Character();
player.HP -= 30;  // set 호출 -> 70
player.HP = 200;  // set 호출 -> 100 (MaxHP 제한)
player.HP = -50;  // set 호출 -> 0 (Die 실행)
```

---

## 5. 객체지향 프로그래밍 (OOP) 핵심

### 5-1. 상속 (Inheritance)
부모 클래스의 기능을 자식이 물려받습니다.

```csharp
// 부모 클래스 (기본 유닛)
public class Unit
{
    public string Name;
    public int HP;

    public void Move()
    {
        Console.WriteLine($"{Name}이(가) 이동합니다.");
    }
}

// 자식 클래스 (마린)
public class Marine : Unit // Unit을 상속
{
    public int Ammo = 50;

    public void Attack()
    {
        if (Ammo > 0)
        {
            Console.WriteLine($"{Name}이(가) 공격! (남은 탄약: {--Ammo})");
        }
    }
}

// 사용
Marine marine = new Marine();
marine.Name = "마린#1";
marine.HP = 40;
marine.Move();   // 부모에게 물려받은 메서드
marine.Attack(); // 자식의 메서드
```

### 5-2. 다형성 (Polymorphism) - 오버라이딩
자식이 부모의 메서드를 **재정의**합니다.

```csharp
public class Unit
{
    public string Name;

    // virtual: "자식아, 너가 필요하면 다시 만들어도 돼"
    public virtual void Attack()
    {
        Console.WriteLine($"{Name}이(가) 기본 공격!");
    }
}

public class Marine : Unit
{
    // override: "아빠 기능 대신 내 방식으로 할게요"
    public override void Attack()
    {
        Console.WriteLine($"{Name}: 소총 발사! 타타타탓!");
    }
}

public class Firebat : Unit
{
    public override void Attack()
    {
        Console.WriteLine($"{Name}: 화염방사기 발사! 활활!");
    }
}

// 사용 (다형성의 힘!)
Unit[] units = new Unit[]
{
    new Marine { Name = "마린" },
    new Firebat { Name = "파이어뱃" }
};

foreach (Unit unit in units)
{
    unit.Attack(); // 각자의 Attack이 실행됨!
}
// 출력:
// 마린: 소총 발사! 타타타탓!
// 파이어뱃: 화염방사기 발사! 활활!
```

### 5-3. 인터페이스 (Interface)
**"이 기능들을 반드시 구현하세요"** 라는 계약서입니다.

```csharp
// 인터페이스 정의 (이름은 I로 시작하는 게 관례)
public interface IDamageable
{
    void TakeDamage(int damage); // 구현부 없음 (세미콜론만)
}

public interface IHealable
{
    void Heal(int amount);
}

// 구현 (여러 개 가능!)
public class Player : IDamageable, IHealable
{
    public int HP = 100;

    public void TakeDamage(int damage)
    {
        HP -= damage;
        Console.WriteLine($"플레이어가 {damage} 데미지를 받았습니다. (HP: {HP})");
    }

    public void Heal(int amount)
    {
        HP += amount;
        Console.WriteLine($"플레이어가 {amount} 회복했습니다. (HP: {HP})");
    }
}

public class Building : IDamageable
{
    public int HP = 500;

    public void TakeDamage(int damage)
    {
        HP -= damage;
        Console.WriteLine($"건물이 {damage} 데미지를 받았습니다. (HP: {HP})");
    }
    // Heal은 구현 안 함 (건물은 회복 불가)
}

// 사용
void DealDamage(IDamageable target, int damage)
{
    target.TakeDamage(damage); // 플레이어든 건물이든 상관없음!
}

Player player = new Player();
Building building = new Building();

DealDamage(player, 30);     // 플레이어 공격
DealDamage(building, 100);  // 건물 공격
```

**인터페이스 vs 상속**:
- **상속**: "A는 B이다" (Marine **은** Unit이다) - 하나만 가능
- **인터페이스**: "A는 B를 할 수 있다" (Player는 데미지를 **받을 수 있다**) - 여러 개 가능

### 5-4. 추상 클래스 (Abstract Class)
인터페이스와 일반 클래스의 중간입니다.

```csharp
// 추상 클래스 (new로 생성 불가)
public abstract class Weapon
{
    public string Name;
    public int Damage;

    // 일반 메서드 (모든 자식이 공유)
    public void ShowInfo()
    {
        Console.WriteLine($"무기: {Name}, 공격력: {Damage}");
    }

    // 추상 메서드 (자식이 반드시 구현해야 함)
    public abstract void Attack();
}

public class Gun : Weapon
{
    public override void Attack()
    {
        Console.WriteLine($"{Name}으로 총 쏘기!");
    }
}

public class Sword : Weapon
{
    public override void Attack()
    {
        Console.WriteLine($"{Name}으로 베기!");
    }
}

// 사용
Gun gun = new Gun { Name = "권총", Damage = 15 };
gun.ShowInfo();  // 부모의 메서드
gun.Attack();    // 자식의 구현

// Weapon weapon = new Weapon(); // ❌ 컴파일 에러! 추상 클래스는 생성 불가
```

**언제 뭘 쓸까?**:
- **추상 클래스**: 공통 기능 + 강제 구현이 필요할 때
- **인터페이스**: 서로 다른 클래스들이 같은 기능을 한다는 것만 보장할 때

---

## 6. 제네릭 (Generic)

### 문제 상황
같은 기능인데 타입만 다르다면?

```csharp
// int용 리스트
public class IntList
{
    private int[] items = new int[10];
    public void Add(int item) { /* ... */ }
}

// string용 리스트
public class StringList
{
    private string[] items = new string[10];
    public void Add(string item) { /* ... */ }
}
// 타입마다 클래스를 만들어야 함... 너무 비효율적!
```

### 해결: 제네릭 (타입을 변수처럼!)
```csharp
// T는 "Type"을 의미하는 플레이스홀더
public class MyList<T>
{
    private T[] items = new T[10];
    private int count = 0;

    public void Add(T item)
    {
        items[count++] = item;
    }

    public T Get(int index)
    {
        return items[index];
    }
}

// 사용
MyList<int> numbers = new MyList<int>();      // T를 int로 교체
numbers.Add(10);
numbers.Add(20);

MyList<string> names = new MyList<string>();  // T를 string으로 교체
names.Add("홍길동");
names.Add("김철수");

MyList<Monster> monsters = new MyList<Monster>(); // 커스텀 클래스도 가능!
```

### 제네릭 메서드
```csharp
public class Utils
{
    // T 타입을 매개변수로 받아서 출력
    public static void Print<T>(T value)
    {
        Console.WriteLine($"값: {value}, 타입: {typeof(T)}");
    }
}

// 사용
Utils.Print<int>(100);           // 값: 100, 타입: System.Int32
Utils.Print<string>("안녕");     // 값: 안녕, 타입: System.String
Utils.Print(3.14);               // 타입 추론 가능 (float로 자동 인식)
```

### 제네릭 제약 (Constraints)
T의 종류를 제한합니다.

```csharp
// where T : 조건
public class DamageSystem<T> where T : IDamageable
{
    public void Attack(T target, int damage)
    {
        target.TakeDamage(damage); // IDamageable을 구현했다는 게 보장됨!
    }
}

// 사용
DamageSystem<Player> playerDmg = new DamageSystem<Player>(); // ✅ OK
DamageSystem<int> intDmg = new DamageSystem<int>();         // ❌ 에러! int는 IDamageable 아님
```

**주요 제약 종류**:
```csharp
where T : class           // T는 참조 타입이어야 함 (클래스)
where T : struct          // T는 값 타입이어야 함 (int, float 등)
where T : new()           // T는 기본 생성자가 있어야 함
where T : IComparable     // T는 IComparable 인터페이스를 구현해야 함
where T : Monster         // T는 Monster 클래스이거나 상속받아야 함
```

### 실전 예시: 오브젝트 풀
```csharp
public class ObjectPool<T> where T : new()
{
    private Queue<T> pool = new Queue<T>();

    public T Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();
        else
            return new T(); // new() 제약 덕분에 생성 가능
    }

    public void Return(T obj)
    {
        pool.Enqueue(obj);
    }
}

// 사용
ObjectPool<Bullet> bulletPool = new ObjectPool<Bullet>();
Bullet b1 = bulletPool.Get();
bulletPool.Return(b1);
```

---

## 7. LINQ (Language Integrated Query)

LINQ는 **컬렉션(List, Array 등)을 SQL처럼 쿼리**하는 기능입니다.

### 기본 예시: 리스트 필터링

**LINQ 없이:**
```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
List<int> evenNumbers = new List<int>();

foreach (int num in numbers)
{
    if (num % 2 == 0)
    {
        evenNumbers.Add(num);
    }
}
// evenNumbers = { 2, 4, 6, 8, 10 }
```

**LINQ 사용:**
```csharp
using System.Linq; // 필수!

List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
List<int> evenNumbers = numbers.Where(num => num % 2 == 0).ToList();
// 한 줄 끝!
```

### 람다 표현식 (=>)
`=>` 는 "이 조건일 때"라는 뜻입니다.

```csharp
// 읽는 법: "num이라는 매개변수를 받아서, num % 2 == 0 조건을 확인"
num => num % 2 == 0

// 풀어쓰면:
bool IsEven(int num)
{
    return num % 2 == 0;
}
```

### 주요 LINQ 메서드

#### 1. Where (조건 필터링)
```csharp
List<int> scores = new List<int> { 45, 89, 72, 55, 91, 38 };

// 60점 이상만
var passedScores = scores.Where(score => score >= 60).ToList();
// { 89, 72, 91 }
```

#### 2. Select (변환/투영)
```csharp
List<string> names = new List<string> { "kim", "lee", "park" };

// 모두 대문자로 변환
var upperNames = names.Select(name => name.ToUpper()).ToList();
// { "KIM", "LEE", "PARK" }

// 이름의 길이로 변환
var nameLengths = names.Select(name => name.Length).ToList();
// { 3, 3, 4 }
```

#### 3. OrderBy / OrderByDescending (정렬)
```csharp
List<int> numbers = new List<int> { 5, 2, 8, 1, 9 };

var ascending = numbers.OrderBy(n => n).ToList();
// { 1, 2, 5, 8, 9 }

var descending = numbers.OrderByDescending(n => n).ToList();
// { 9, 8, 5, 2, 1 }
```

#### 4. First / FirstOrDefault
```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

int first = numbers.First(); // 1
int firstEven = numbers.First(n => n % 2 == 0); // 2

// FirstOrDefault: 없으면 기본값 (0, null 등) 반환
List<int> empty = new List<int>();
int result = empty.FirstOrDefault(); // 0 (에러 안 남!)
```

#### 5. Any / All (조건 검사)
```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

bool hasEven = numbers.Any(n => n % 2 == 0); // true (짝수 있음?)
bool allPositive = numbers.All(n => n > 0);  // true (모두 양수?)
bool allEven = numbers.All(n => n % 2 == 0); // false
```

#### 6. Count / Sum / Average / Max / Min
```csharp
List<int> scores = new List<int> { 85, 92, 78, 88, 95 };

int count = scores.Count();              // 5
int sum = scores.Sum();                  // 438
double average = scores.Average();       // 87.6
int max = scores.Max();                  // 95
int min = scores.Min();                  // 78

// 조건부 Count
int highScores = scores.Count(s => s >= 90); // 2
```

### 실전 예시: 유닛 관리

```csharp
public class Unit
{
    public string Name;
    public int HP;
    public int MaxHP;
    public bool IsAlive => HP > 0;
}

List<Unit> units = new List<Unit>
{
    new Unit { Name = "마린#1", HP = 40, MaxHP = 40 },
    new Unit { Name = "마린#2", HP = 15, MaxHP = 40 },
    new Unit { Name = "메딕#1", HP = 60, MaxHP = 60 },
    new Unit { Name = "마린#3", HP = 0, MaxHP = 40 },
};

// 살아있는 유닛만
var aliveUnits = units.Where(u => u.IsAlive).ToList();

// 부상당한 유닛만 (HP < MaxHP)
var injuredUnits = units.Where(u => u.HP < u.MaxHP && u.IsAlive).ToList();

// HP가 가장 낮은 유닛
var weakest = units.OrderBy(u => u.HP).First();

// 마린만 필터링
var marines = units.Where(u => u.Name.Contains("마린")).ToList();

// 평균 HP
double avgHP = units.Average(u => u.HP); // 28.75

// 모두 살아있는지 확인
bool allAlive = units.All(u => u.IsAlive); // false
```

### LINQ 체이닝 (여러 메서드 연결)
```csharp
// 살아있는 유닛 중에서 HP가 30 이하인 유닛의 이름만 가져오기
var criticalUnitNames = units
    .Where(u => u.IsAlive)           // 살아있는 유닛만
    .Where(u => u.HP <= 30)          // HP 30 이하만
    .OrderBy(u => u.HP)              // HP 오름차순 정렬
    .Select(u => u.Name)             // 이름만 추출
    .ToList();

Console.WriteLine(string.Join(", ", criticalUnitNames));
// 출력: 마린#2
```

### 쿼리 문법 (Query Syntax) - 선택사항
SQL과 비슷한 문법도 지원합니다.

```csharp
// 메서드 문법
var result = units.Where(u => u.HP > 50).Select(u => u.Name).ToList();

// 쿼리 문법 (같은 결과)
var result2 = (from u in units
               where u.HP > 50
               select u.Name).ToList();
```

**팁**: 실무에서는 **메서드 문법**을 더 많이 씁니다.

---

## 8. Null 참조와 안전한 처리

### Null이란?
**"아무것도 가리키지 않는 참조"**입니다. 포인터가 없는 상태입니다.

```csharp
Monster m = null; // "나는 아직 몬스터를 안 만들었어"
Console.WriteLine(m.HP); // ❌ NullReferenceException! (게임 크래시)
```

### Null 체크 방법

#### 1. if문으로 체크 (전통적 방법)
```csharp
Monster monster = GetMonster(); // 몬스터를 가져옴 (없을 수도 있음)

if (monster != null)
{
    monster.Attack();
}
else
{
    Console.WriteLine("몬스터가 없습니다.");
}
```

#### 2. Null 조건 연산자 (?.)
```csharp
Monster monster = GetMonster();

// monster가 null이 아니면 Attack() 실행, null이면 아무것도 안 함
monster?.Attack();

// 값을 가져올 때도 사용
int? hp = monster?.HP; // monster가 null이면 hp도 null
```

#### 3. Null 병합 연산자 (??)
```csharp
Monster monster = GetMonster();

// monster가 null이면 기본값 사용
Monster safeMonster = monster ?? new Monster(); // null이면 새로 생성

// 또는
int hp = monster?.HP ?? 0; // monster가 null이면 0 반환
```

#### 4. Nullable 타입 (값 타입에 null 허용)
```csharp
int normalInt = 10;         // null 불가
int? nullableInt = null;    // null 가능! (물음표 주목)

if (nullableInt.HasValue)
{
    Console.WriteLine(nullableInt.Value); // 값 접근
}

// 또는
int result = nullableInt ?? 0; // null이면 0 사용
```

### 실전 예시: 유닛 찾기

```csharp
public class GameManager
{
    private List<Unit> units = new List<Unit>();

    // 유닛 찾기 (없을 수 있음)
    public Unit FindUnit(string name)
    {
        return units.FirstOrDefault(u => u.Name == name); // 없으면 null 반환
    }

    // 안전한 공격 명령
    public void AttackTarget(string targetName)
    {
        Unit target = FindUnit(targetName);

        if (target != null)
        {
            target.TakeDamage(10);
        }
        else
        {
            Console.WriteLine($"{targetName}을(를) 찾을 수 없습니다.");
        }
    }

    // Null 조건 연산자 활용
    public void HealTarget(string targetName, int amount)
    {
        FindUnit(targetName)?.Heal(amount); // null이면 아무 일도 안 일어남
    }

    // Null 병합 연산자로 기본값 제공
    public int GetTargetHP(string targetName)
    {
        return FindUnit(targetName)?.HP ?? 0; // 없으면 0
    }
}
```

### 유니티에서의 주의사항

유니티의 `Object`는 특별합니다. `== null` 체크를 오버라이드합니다.

```csharp
public class Player : MonoBehaviour
{
    public GameObject target;

    void Update()
    {
        // ✅ 올바른 방법
        if (target == null)
        {
            Debug.Log("타겟 없음");
        }

        // ⚠️ 유니티에서는 이렇게 하지 마세요
        if (target is null) // C# 패턴 매칭 - 유니티 오브젝트와 맞지 않음
        {
            // Destroy된 오브젝트를 null로 인식 못할 수 있음
        }
    }
}
```

**핵심 규칙**:
1. 참조 타입 변수는 항상 **null일 수 있다**고 가정
2. 사용 전에 **null 체크** 필수
3. `?.` 와 `??` 연산자를 적극 활용
4. 유니티 오브젝트는 `== null` 사용

---

## 9. 종합 실습: RPG 캐릭터 시스템

배운 내용을 모두 활용한 예제입니다.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// 인터페이스
public interface IDamageable
{
    void TakeDamage(int damage);
}

public interface IHealable
{
    void Heal(int amount);
}

// 추상 클래스
public abstract class Character : IDamageable
{
    // 프로퍼티
    public string Name { get; set; }
    public int Level { get; set; } = 1;

    private int hp;
    public int HP
    {
        get => hp;
        set => hp = Math.Clamp(value, 0, MaxHP); // 0~MaxHP 범위
    }

    public int MaxHP { get; protected set; }

    // 추상 메서드 (자식이 구현)
    public abstract void Attack();

    // 인터페이스 구현
    public virtual void TakeDamage(int damage)
    {
        HP -= damage;
        Console.WriteLine($"{Name}이(가) {damage} 데미지를 받았습니다. (HP: {HP}/{MaxHP})");

        if (HP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Console.WriteLine($"{Name}이(가) 쓰러졌습니다...");
    }
}

// 구체 클래스들
public class Warrior : Character
{
    public Warrior(string name)
    {
        Name = name;
        MaxHP = 150;
        HP = MaxHP;
    }

    public override void Attack()
    {
        Console.WriteLine($"{Name}이(가) 칼로 베기! (데미지: 30)");
    }
}

public class Mage : Character
{
    public int Mana { get; set; } = 100;

    public Mage(string name)
    {
        Name = name;
        MaxHP = 80;
        HP = MaxHP;
    }

    public override void Attack()
    {
        if (Mana >= 20)
        {
            Mana -= 20;
            Console.WriteLine($"{Name}이(가) 파이어볼 시전! (데미지: 50, 마나: {Mana})");
        }
        else
        {
            Console.WriteLine($"{Name}: 마나 부족!");
        }
    }
}

public class Healer : Character, IHealable
{
    public Healer(string name)
    {
        Name = name;
        MaxHP = 100;
        HP = MaxHP;
    }

    public override void Attack()
    {
        Console.WriteLine($"{Name}이(가) 지팡이로 후려치기! (데미지: 10)");
    }

    public void Heal(int amount)
    {
        HP += amount;
        Console.WriteLine($"{Name}이(가) {amount} 회복했습니다. (HP: {HP}/{MaxHP})");
    }

    public void HealOther(IHealable target, int amount)
    {
        target.Heal(amount);
        Console.WriteLine($"{Name}이(가) 힐 시전!");
    }
}

// 제네릭 파티 시스템
public class Party<T> where T : Character
{
    private List<T> members = new List<T>();

    public void AddMember(T member)
    {
        members.Add(member);
        Console.WriteLine($"{member.Name}이(가) 파티에 합류했습니다.");
    }

    public void ShowPartyStatus()
    {
        Console.WriteLine("\n=== 파티 현황 ===");
        foreach (var member in members)
        {
            Console.WriteLine($"{member.Name} (Lv.{member.Level}) - HP: {member.HP}/{member.MaxHP}");
        }
    }

    // LINQ 활용
    public T GetWeakestMember()
    {
        return members.OrderBy(m => m.HP).FirstOrDefault();
    }

    public List<T> GetInjuredMembers()
    {
        return members.Where(m => m.HP < m.MaxHP).ToList();
    }

    public bool IsAnyoneDead()
    {
        return members.Any(m => m.HP <= 0);
    }
}

// 메인 실행
class Program
{
    static void Main(string[] args)
    {
        // 캐릭터 생성
        Warrior warrior = new Warrior("전사");
        Mage mage = new Mage("마법사");
        Healer healer = new Healer("힐러");

        // 파티 구성
        Party<Character> party = new Party<Character>();
        party.AddMember(warrior);
        party.AddMember(mage);
        party.AddMember(healer);

        party.ShowPartyStatus();

        Console.WriteLine("\n=== 전투 시작! ===");

        // 다형성 활용
        warrior.Attack();
        mage.Attack();
        healer.Attack();

        Console.WriteLine("\n=== 적의 공격! ===");
        warrior.TakeDamage(50);
        mage.TakeDamage(70);

        party.ShowPartyStatus();

        Console.WriteLine("\n=== 힐러의 치료! ===");
        // LINQ로 부상자 찾기
        var injured = party.GetInjuredMembers();
        foreach (var member in injured)
        {
            if (member is IHealable healable)
            {
                healer.HealOther(healable, 30);
            }
        }

        party.ShowPartyStatus();

        // Null 안전 처리
        Character weakest = party.GetWeakestMember();
        Console.WriteLine($"\n가장 약한 멤버: {weakest?.Name ?? "없음"}");
    }
}
```

---

## 10. 정리 및 다음 단계

### 이번 강의에서 배운 것
✅ 접근 제한자 (public, private, protected, internal)
✅ 프로퍼티 vs 필드 (get/set, 자동 프로퍼티)
✅ 상속, 다형성, 인터페이스, 추상 클래스 (OOP 4대 원칙)
✅ 제네릭 (타입을 변수처럼 사용)
✅ LINQ (컬렉션 쿼리)
✅ Null 참조 처리 (?., ??, Nullable)

### 다음 강의 예고: 3강 - 디자인 패턴
실무에서 필수적인 디자인 패턴들을 스타크래프트 예시로 배웁니다!
