using System;

namespace ConsoleQuest
{
    class Program
    {
        // ==================================================================================
        // [아키텍처 및 구조 설명]
        // 이 게임은 전형적인 '콘솔 기반의 절차적 진행 게임'입니다.
        // 크게 3가지 단계로 나뉩니다:
        // 1. 초기화 (Initialize): 게임에 필요한 데이터(맵, 플레이어 등)를 준비합니다.
        // 2. 게임 루프 (Game Loop): 게임이 종료될 때까지 [입력 -> 로직 처리 -> 화면 출력]을 반복합니다.
        // 3. 종료 (Exit): 게임을 마무리합니다.
        //
        // 주요 데이터 구조:
        // - gameMapData: 2차원 배열(int[,])을 사용하여 20x20 격자형 맵을 표현합니다.
        //   배열의 인덱스 [y, x]가 곧 맵의 좌표가 됩니다.
        //   값에 따라 0:평지, 1:숲, 2:상점, 3:골인 지점 등으로 구분합니다.
        //
        // - 상태 변수: 플레이어의 위치(x, y), 체력, 돈 등은 static 변수로 관리하여
        //   프로그램 어디서든 접근하고 수정할 수 있게 했습니다. (간단한 구조를 위해)
        // ==================================================================================

        // ==================================================================================
        // [설정 변수] 게임의 규칙을 정하는 변수들
        // ==================================================================================

        // static 키워드: 클래스의 인스턴스(객체)를 만들지 않고도 사용할 수 있는 변수나 함수에 붙입니다.
        // 이 프로그램에서는 Program 클래스 하나만 사용하므로, 편의상 모든 변수를 static으로 선언했습니다.
        static int mapWidth = 20;   // 맵의 가로 크기
        static int mapHeight = 20;  // 맵의 세로 크기

        // 맵 데이터 (0: 안전한 땅, 1: 위험한 숲(몬스터 출몰), 2: 상점, 3: 골인 지점)
        // 2차원 배열은 [세로, 가로] 순서로 접근하는 것이 일반적입니다. (행, 열)
        static int[,] gameMapData = new int[20, 20];

        // 플레이어 상태 변수
        static int playerXPosition = 0; // 플레이어의 가로 위치 (0 ~ 19)
        static int playerYPosition = 0; // 플레이어의 세로 위치 (0 ~ 19)

        static int currentHealth = 100; // 현재 체력
        static int maxHealth = 100;     // 최대 체력
        static int currentGold = 0;     // 현재 가지고 있는 돈
        static int attackPower = 10;    // 공격력 (가위바위보 승리 시 데미지 - 사실 몬스터 잡을땐 한방이라 의미가 적지만 확장성을 위해)

        static bool isGameRunning = true; // 게임이 실행 중인지 확인하는 변수 (while문 조건)

        // ---------------------------------------------------------
        // [과제 4 구현] 난이도 변수
        // ---------------------------------------------------------
        // 0: 쉬움, 1: 보통, 2: 어려움
        static int difficulty = 0; 
        // ---------------------------------------------------------

        // ==================================================================================
        // [메인 함수] 프로그램이 시작되면 가장 먼저 실행되는 진입점(Entry Point)
        // ==================================================================================
        static void Main(string[] args)
        {
            try
            {
                // 1. 게임 초기화 (맵 만들기, 플레이어 위치 잡기, 난이도 설정 등)
                InitializeGame();

                // 2. 게임 루프 (Game Loop)
                // 게임이 끝날 때까지 무한히 반복합니다.
                // 보통 게임은 [입력 처리 -> 게임 로직 업데이트 -> 화면 그리기] 순서로 돕니다.
                // 여기서는 [화면 그리기 -> 입력 대기 및 처리] 순서로 되어 있습니다. (콘솔 특성상 대기가 필요해서)
                while (isGameRunning)
                {
                    // 화면 지우고 다시 그리기 (Render)
                    Render();

                    // 키보드 입력 받아서 처리하기 (Process Input & Update)
                    ProcessInput();
                }
            }
            catch (Exception ex)
            {
                // 예외 처리: 프로그램 실행 중 예상치 못한 에러가 발생했을 때 꺼지지 않고 에러 메시지를 보여줍니다.
                Console.Clear();
                Console.WriteLine("오류가 발생했습니다!");
                Console.WriteLine(ex.Message); // 에러 내용
                Console.WriteLine(ex.StackTrace); // 에러가 발생한 위치 추적
                Console.WriteLine("\n아무 키나 누르면 종료합니다.");
                Console.ReadKey();
            }
        }

        // ==================================================================================
        // [초기화] 게임 시작 전 준비 작업을 하는 함수입니다.
        // ==================================================================================
        static void InitializeGame()
        {
            Console.CursorVisible = false; // 커서 깜빡임 숨기기 (깔끔한 화면을 위해)

            // ---------------------------------------------------------
            // [과제 4 구현] 난이도 선택 함수 호출
            // ---------------------------------------------------------
            SelectDifficulty();
            // ---------------------------------------------------------

            // 맵 데이터 초기화
            // 먼저 맵 전체를 0(안전한 땅)으로 채웁니다.
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    gameMapData[y, x] = 0;
                }
            }

            // ---------------------------------------------------------
            // [과제 4 구현] 난이도에 따라 숲과 상점의 개수 설정
            // ---------------------------------------------------------
            int forestCount = 0;
            int shopCount = 0;

            switch (difficulty)
            {
                case 0: // 쉬움
                    forestCount = 40; // 숲 40개
                    shopCount = 3;
                    break;
                case 1: // 보통
                    forestCount = 60; // 숲 60개
                    shopCount = 2;
                    break;
                case 2: // 어려움
                    forestCount = 80; // 숲 80개
                    shopCount = 1;
                    break;
                default: // 예외 상황 방지
                    forestCount = 60;
                    shopCount = 2;
                    break;
            }
            // ---------------------------------------------------------

            Random random = new Random();

            // 1. 위험한 숲(1) 배치
            // [중복 방지 로직]
            // 랜덤으로 좌표를 뽑았는데 이미 숲이거나, 시작점/도착점이라면 다시 뽑아야 합니다.
            int currentForests = 0;
            while (currentForests < forestCount)
            {
                int randomX = random.Next(0, mapWidth);
                int randomY = random.Next(0, mapHeight);

                // 조건: 빈 땅(0)이어야 하고, 시작점(0,0)과 도착점(19,19)이 아니어야 함
                if (gameMapData[randomY, randomX] == 0 &&
                    !(randomX == 0 && randomY == 0) &&
                    !(randomX == 19 && randomY == 19))
                {
                    gameMapData[randomY, randomX] = 1; // 숲 배치
                    currentForests++; // 배치 성공했으므로 카운트 증가
                }
                // 만약 조건을 만족하지 못하면(이미 숲이거나 시작/끝점이면) 아무것도 하지 않고 다시 while문 처음으로 돌아감
            }

            // 2. 상점(2) 배치
            // 숲 배치와 동일한 방식으로 중복을 피해서 배치합니다.
            int currentShops = 0;
            while (currentShops < shopCount)
            {
                int randomX = random.Next(0, mapWidth);
                int randomY = random.Next(0, mapHeight);

                // 빈 땅(0)에만 상점 배치 (숲 위에 덮어쓰지 않도록 gameMapData[y,x] == 0 체크)
                if (gameMapData[randomY, randomX] == 0 &&
                    !(randomX == 0 && randomY == 0) &&
                    !(randomX == 19 && randomY == 19))
                {
                    gameMapData[randomY, randomX] = 2; // 상점 배치
                    currentShops++;
                }
            }

            // 3. 골인 지점(3) 배치
            gameMapData[19, 19] = 3;
        }

        // ==================================================================================
        // [난이도 선택] 게임 시작 시 난이도를 고르는 함수입니다.
        // ==================================================================================
        static void SelectDifficulty()
        {
            // ---------------------------------------------------------
            // [과제 4 구현] 난이도 선택 기능
            // ---------------------------------------------------------
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("           난이도를 선택하세요");
                Console.WriteLine("========================================");
                Console.WriteLine("1. 쉬움   (숲 조금, 상점 많음)");
                Console.WriteLine("2. 보통   (적절한 밸런스)");
                Console.WriteLine("3. 어려움 (숲 많음, 상점 적음)");
                Console.WriteLine("========================================");
                Console.Write("번호 입력: ");

                string? input = Console.ReadLine();

                if (input == "1")
                {
                    difficulty = 0;
                    break; // 올바른 입력이 들어오면 반복문 탈출
                }
                else if (input == "2")
                {
                    difficulty = 1;
                    break;
                }
                else if (input == "3")
                {
                    difficulty = 2;
                    break;
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다. 다시 선택해주세요.");
                    System.Threading.Thread.Sleep(500); // 0.5초 대기 후 다시 표시
                }
            }
            Console.WriteLine($"난이도가 설정되었습니다. 게임을 시작합니다!");
            System.Threading.Thread.Sleep(1000);
            // ---------------------------------------------------------
        }

        // ==================================================================================
        // [화면 출력] 맵, 플레이어, 상태창을 화면에 그리는 함수입니다.
        // ==================================================================================
        static void Render()
        {
            Console.SetCursorPosition(0, 0); // 커서를 맨 위로 이동 (화면을 지우는 것보다 깜빡임이 덜함)

            // 1. 맵 그리기
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    // 플레이어 위치라면 플레이어 문자(@) 출력
                    if (x == playerXPosition && y == playerYPosition)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("웃"); // 플레이어 캐릭터
                        Console.ResetColor();
                    }
                    else
                    {
                        // 맵의 종류에 따라 다른 글자 출력
                        int tileType = gameMapData[y, x];
                        if (tileType == 0) // 안전
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write(". ");
                        }
                        else if (tileType == 1) // 위험
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("숲");
                        }
                        else if (tileType == 2) // 상점
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write("집");
                        }
                        else if (tileType == 3) // 골인 지점
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("★");
                        }
                        Console.ResetColor();
                    }
                }
                Console.WriteLine(); // 줄바꿈
            }

            // 2. 상태창 그리기 (맵 아래에 표시)
            Console.WriteLine("========================================");
            string diffStr = difficulty == 0 ? "쉬움" : (difficulty == 1 ? "보통" : "어려움");
            Console.WriteLine($"[상태] 체력: {currentHealth}/{maxHealth} | 돈: {currentGold}G | 공격력: {attackPower} | 난이도: {diffStr}");
            Console.WriteLine("========================================");
            Console.WriteLine("[조작] 방향키: 이동 | Q: 종료");
            Console.WriteLine("----------------------------------------");

            // 현재 위치에 대한 설명 출력
            int currentTile = gameMapData[playerYPosition, playerXPosition];
            if (currentTile == 0) Console.WriteLine("평원입니다. 방심하면 몬스터가 튀어나옵니다.     "); // 텍스트 변경
            else if (currentTile == 1) Console.WriteLine("으스스한 숲입니다... 몬스터가 나올 것 같습니다! ");
            else if (currentTile == 2) Console.WriteLine("상점입니다. 물건을 살 수 있습니다.              ");
            else if (currentTile == 3) Console.WriteLine("목적지에 도착했습니다! ★                       ");
        }

        // ==================================================================================
        // [입력 처리] 키보드 입력을 받아서 이동하거나 행동하는 함수입니다.
        // ==================================================================================
        static void ProcessInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true); // 키 입력 대기 (화면에 글자 안나오게 true)

            // 임시로 이동할 위치를 계산하기 위한 변수
            int nextX = playerXPosition;
            int nextY = playerYPosition;

            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    nextX = playerXPosition - 1;
                    break;
                case ConsoleKey.RightArrow:
                    nextX = playerXPosition + 1;
                    break;
                // ---------------------------------------------------------
                // [과제 1 구현] 위쪽, 아래쪽 이동
                // ---------------------------------------------------------
                case ConsoleKey.UpArrow:
                    nextY = playerYPosition - 1; // 위로 가면 Y좌표 감소
                    break;
                case ConsoleKey.DownArrow:
                    nextY = playerYPosition + 1; // 아래로 가면 Y좌표 증가
                    break;
                // ---------------------------------------------------------

                case ConsoleKey.Q:
                    isGameRunning = false;
                    return;
            }

            // 이동하려는 곳이 맵 밖인지 확인 (유효성 검사)
            if (nextX >= 0 && nextX < mapWidth && nextY >= 0 && nextY < mapHeight)
            {
                // 맵 안이라면 실제로 플레이어 위치를 바꿉니다.
                playerXPosition = nextX;
                playerYPosition = nextY;

                // 이동한 자리에서 무슨 일이 일어날지 체크합니다.
                CheckTileEvent();
            }
        }

        // ==================================================================================
        // [이벤트 체크] 이동한 타일에서 몬스터를 만나거나 상점에 들어가는 로직입니다.
        // ==================================================================================
        static void CheckTileEvent()
        {
            int tileType = gameMapData[playerYPosition, playerXPosition];
            Random random = new Random();

            if (tileType == 1) // 위험한 숲
            {
                // 숲에서는 30% 확률로 몬스터 만남 (기존 유지)
                if (random.Next(0, 100) < 30)
                {
                    StartBattle();
                }
            }
            else if (tileType == 0) // 일반 평지 (난이도별 확률 추가)
            {
                // 난이도에 따라 평지에서도 몬스터를 만날 확률 설정
                int encounterChance = 0;
                
                if (difficulty == 0) encounterChance = 5;       // 쉬움: 5% (매우 낮음)
                else if (difficulty == 1) encounterChance = 15; // 보통: 15%
                else encounterChance = 30;                      // 어려움: 30% (꽤 높음)

                if (random.Next(0, 100) < encounterChance)
                {
                    StartBattle();
                }
            }
            else if (tileType == 2) // 상점
            {
                EnterShop();
            }
            else if (tileType == 3) // 골인 지점
            {
                GameClear();
            }
        }

        // ==================================================================================
        // [게임 클리어] 목적지에 도착했을 때 실행됩니다.
        // ==================================================================================
        static void GameClear()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("****************************************");
            Console.WriteLine("*                                      *");
            Console.WriteLine("*           G A M E   C L E A R        *");
            Console.WriteLine("*                                      *");
            Console.WriteLine("****************************************");
            Console.WriteLine("\n축하합니다! 무사히 목적지에 도착했습니다!");
            Console.WriteLine($"최종 체력: {currentHealth}");
            Console.WriteLine($"최종 소지금: {currentGold} G");
            Console.ResetColor();
            Console.WriteLine("\n아무 키나 누르면 종료합니다.");
            Console.ReadKey();
            isGameRunning = false;
        }

        // ==================================================================================
        // [전투] 몬스터와 가위바위보 대결을 합니다.
        // ==================================================================================
        static void StartBattle()
        {
            Console.Clear();
            Random random = new Random();

            // ---------------------------------------------------------
            // [과제 3 구현] 다양한 몬스터 만들기 (난이도별 확률 조정)
            // ---------------------------------------------------------
            string monsterName = "";
            int monsterDamage = 0;
            int monsterReward = 0;

            int monsterType = 0;
            int roll = random.Next(0, 100); // 0~99 랜덤 값

            if (difficulty == 0) // 쉬움
            {
                // 약한 몬스터가 나올 확률을 높게 설정
                if (roll < 70) monsterType = 0;      // 70% 확률로 슬라임
                else if (roll < 90) monsterType = 1; // 20% 확률로 고블린
                else monsterType = 2;                // 10% 확률로 오크
            }
            else if (difficulty == 1) // 보통
            {
                // 중간 밸런스
                if (roll < 50) monsterType = 0;      // 50% 확률로 슬라임
                else if (roll < 80) monsterType = 1; // 30% 확률로 고블린
                else monsterType = 2;                // 20% 확률로 오크
            }
            else // 어려움
            {
                // 완전 랜덤 (33.3% 씩)
                monsterType = random.Next(0, 3);
            }

            if (monsterType == 0)
            {
                monsterName = "슬라임";
                monsterDamage = random.Next(5, 10);  // 약한 데미지
                monsterReward = random.Next(5, 15);  // 적은 보상
            }
            else if (monsterType == 1)
            {
                monsterName = "고블린";
                monsterDamage = random.Next(10, 20); // 보통 데미지
                monsterReward = random.Next(15, 30); // 보통 보상
            }
            else
            {
                monsterName = "오크";
                monsterDamage = random.Next(20, 35); // 강한 데미지
                monsterReward = random.Next(30, 50); // 많은 보상
            }
            // ---------------------------------------------------------

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("           / \\__");
            Console.WriteLine("          (    @\\___");
            Console.WriteLine("          /         O");
            Console.WriteLine("         /   (_____/");
            Console.WriteLine("        /_____/   U");
            Console.WriteLine("");
            Console.WriteLine($"   야생의 {monsterName}이(가) 나타났다!");
            Console.WriteLine($"   (예상 피해: {monsterDamage}, 현상금: {monsterReward} G)");
            Console.ResetColor();
            Console.WriteLine("========================================");

            // 가위바위보 로직
            Console.WriteLine("가위바위보 대결! (1: 가위, 2: 바위, 3: 보)");
            Console.Write("입력: ");

            FlushInputBuffer(); // 입력 받기 전 버퍼 비우기
            string? input = Console.ReadLine();
            int playerChoice = 0;

            // 입력값이 숫자인지 확인
            if (int.TryParse(input, out playerChoice) && (playerChoice >= 1 && playerChoice <= 3))
            {
                int monsterChoice = random.Next(1, 4); // 1~3 랜덤

                string[] handNames = { "없음", "가위", "바위", "보" };

                Console.WriteLine($"\n플레이어: {handNames[playerChoice]} vs {monsterName}: {handNames[monsterChoice]}");

                bool isWin = false;
                bool isDraw = (playerChoice == monsterChoice);

                // 승리 조건: (나:가위, 적:보) or (나:바위, 적:가위) or (나:보, 적:바위)
                if (!isDraw)
                {
                    if ((playerChoice == 1 && monsterChoice == 3) ||
                        (playerChoice == 2 && monsterChoice == 1) ||
                        (playerChoice == 3 && monsterChoice == 2))
                    {
                        isWin = true;
                    }
                }

                if (isDraw)
                {
                    Console.WriteLine("비겼습니다! 아무 일도 일어나지 않습니다.");
                }
                else if (isWin)
                {
                    currentGold += monsterReward;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"이겼습니다! {monsterName}을(를) 물리쳤습니다.");
                    Console.WriteLine($"{monsterReward} 골드를 얻었습니다!");
                    Console.ResetColor();
                }
                else
                {
                    currentHealth -= monsterDamage;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"졌습니다... {monsterName}에게 맞았습니다.");
                    Console.WriteLine($"체력이 {monsterDamage} 깎였습니다.");
                    Console.ResetColor();
                }
            }
            else
            {
                // 잘못된 입력 시 패널티 적용 (패배 처리)
                currentHealth -= monsterDamage;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("잘못된 입력입니다. 당황해서 도망쳤습니다! (패배 처리)");
                Console.WriteLine($"도망치다가 {monsterName}에게 등을 맞았습니다.");
                Console.WriteLine($"체력이 {monsterDamage} 깎였습니다.");
                Console.ResetColor();
            }

            Console.WriteLine("\n아무 키나 누르면 맵으로 돌아갑니다.");
            FlushInputBuffer(); // 키 입력 대기 전 버퍼 비우기
            Console.ReadKey();

            // 체력이 0 이하면 게임 오버 처리
            if (currentHealth <= 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("체력이 모두 소진되었습니다... GAME OVER");
                Console.ResetColor();
                isGameRunning = false;
            }
            
            // 전투 종료 후 맵으로 돌아갈 때 화면을 깨끗이 지워줍니다.
            // (전투 화면의 글자가 맵 화면에 남는 것을 방지)
            Console.Clear();
        }

        // ==================================================================================
        // [상점] 아이템을 구매하는 곳입니다.
        // ==================================================================================
        static void EnterShop()
        {
            bool inShop = true;
            while (inShop)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("============== [ 상 점 ] ==============");
                Console.ResetColor();
                Console.WriteLine($"현재 가진 돈: {currentGold} G");
                Console.WriteLine($"현재 체력: {currentHealth}/{maxHealth}");
                Console.WriteLine($"현재 공격력: {attackPower}");
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("1. 빨간 물약 (체력 20 회복) - 가격: 30 G");
                Console.WriteLine("2. 튼튼한 갑옷 (최대 체력 +20 증가) - 가격: 100 G");
                Console.WriteLine("3. 날카로운 검 (공격력 +5 증가) - 가격: 150 G");
                Console.WriteLine("0. 나가기");
                Console.WriteLine("=======================================");
                Console.Write("무엇을 구매하시겠습니까? (번호 입력): ");

                FlushInputBuffer(); // 입력 받기 전 버퍼 비우기
                string? input = Console.ReadLine();

                if (input == "0")
                {
                    inShop = false; // 상점 루프 종료
                    Console.WriteLine("상점을 나갑니다.");
                    System.Threading.Thread.Sleep(1000); // 1초 대기
                }
                else if (input == "1") // 빨간 물약
                {
                    int price = 30;
                    // ---------------------------------------------------------
                    // [과제 2 구현] 물약 구매
                    // ---------------------------------------------------------
                    if (currentGold >= price)
                    {
                        currentGold -= price;
                        currentHealth += 20;
                        // 최대 체력을 넘지 않도록 보정
                        if (currentHealth > maxHealth) currentHealth = maxHealth;

                        Console.WriteLine("빨간 물약을 구매했습니다! 체력이 회복됩니다.");
                    }
                    else
                    {
                        Console.WriteLine("돈이 부족합니다!");
                    }
                    System.Threading.Thread.Sleep(1000);
                    // ---------------------------------------------------------
                }
                else if (input == "2") // 갑옷
                {
                    int price = 100;
                    // ---------------------------------------------------------
                    // [과제 2 구현] 갑옷 구매
                    // ---------------------------------------------------------
                    if (currentGold >= price)
                    {
                        currentGold -= price;
                        maxHealth += 20;
                        currentHealth += 20; // 최대 체력이 늘어난 만큼 현재 체력도 채워줌
                        Console.WriteLine("튼튼한 갑옷을 구매했습니다! 최대 체력이 증가했습니다.");
                    }
                    else
                    {
                        Console.WriteLine("돈이 부족합니다!");
                    }
                    System.Threading.Thread.Sleep(1000);
                    // ---------------------------------------------------------
                }
                else if (input == "3") // 검
                {
                    int price = 150;
                    // ---------------------------------------------------------
                    // [과제 2 구현] 검 구매
                    // ---------------------------------------------------------
                    if (currentGold >= price)
                    {
                        currentGold -= price;
                        attackPower += 5;
                        Console.WriteLine("날카로운 검을 구매했습니다! 공격력이 증가했습니다.");
                    }
                    else
                    {
                        Console.WriteLine("돈이 부족합니다!");
                    }
                    System.Threading.Thread.Sleep(1000);
                    // ---------------------------------------------------------
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    System.Threading.Thread.Sleep(500);
                }
            }
            // 상점 종료 후 맵으로 돌아갈 때 화면 정리
            Console.Clear();
        }

        // ==================================================================================
        // [유틸리티] 입력 버퍼를 비우는 함수
        // ==================================================================================
        static void FlushInputBuffer()
        {
            // 키보드 버퍼에 남아있는 키 입력들을 모두 읽어서 없앱니다.
            // 이동 중에 연타한 키가 전투나 상점 메뉴 선택에 영향을 주지 않게 하기 위함입니다.
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }
    }
}
