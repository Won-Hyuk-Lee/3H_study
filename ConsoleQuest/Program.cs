using System;

namespace ConsoleQuest
{
    class Program
    {
        // ==================================================================================
        // [설정 변수] 게임의 규칙을 정하는 변수들
        // ==================================================================================

        //static 이란 이 모듈내부에서 전역으로 사용이 가능한 변수. 예를 들면 다른 클래스에서도 사용 가능
        static int mapWidth = 20;   // 맵의 가로 크기
        static int mapHeight = 20;  // 맵의 세로 크기

        // 맵 데이터 (0: 기본땅(몬스터 출몰 가능), 1: 위험한 숲(몬스터 출몰 높음), 2: 상점, 3: 골인 지점)
        static int[,] gameMapData = new int[20, 20];

        // 플레이어 상태 변수
        static int playerXPosition = 0; // 플레이어의 가로 위치 (0 ~ 19)
        static int playerYPosition = 0; // 플레이어의 세로 위치 (0 ~ 19)

        static int currentHealth = 100; // 현재 체력
        static int maxHealth = 100;     // 최대 체력
        static int currentGold = 0;     // 현재 가지고 있는 돈
        static int attackPower = 10;    // 공격력 (가위바위보 승리 시 데미지)

        static bool isGameRunning = true; // 게임이 실행 중인지 확인하는 변수

        // ---------------------------------------------------------
        // [과제 4] 난이도 변수 만들기
        // ---------------------------------------------------------
        // 힌트: 난이도를 저장할 변수가 필요합니다. (예: int difficulty)
        // 0: 쉬움, 1: 보통, 2: 어려움
        static int difficulty = 0; // 기본은 쉬움(0)으로
        // ---------------------------------------------------------

        // ==================================================================================
        // [메인 함수] 프로그램이 시작되면 가장 먼저 실행되는 곳
        // ==================================================================================
        static void Main(string[] args)
        {
            try
            {
                // 1. 게임 초기화 (맵 만들기, 플레이어 위치 잡기 등)
                InitializeGame();

                // 2. 게임 루프 (게임이 끝날 때까지 계속 반복)
                while (isGameRunning)
                {
                    // 화면 지우고 다시 그리기
                    Render();

                    // 키보드 입력 받아서 처리하기
                    ProcessInput();
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("오류가 발생했습니다!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("\n아무 키나 누르면 종료합니다.");
                Console.ReadKey();
            }
        }

        // ==================================================================================
        // [초기화] 게임 시작 전 준비 작업을 하는 함수입니다.
        // ==================================================================================
        static void InitializeGame()
        {
            Console.CursorVisible = false; // 커서 깜빡임 숨기기

            // ---------------------------------------------------------
            // [과제 4] 난이도 선택 함수 호출하기
            // ---------------------------------------------------------
            // TODO: 여기에 SelectDifficulty() 함수를 호출하는 코드를 작성하세요.

            // ---------------------------------------------------------

            // 맵 데이터 채우기
            // 기본은 0(안전)으로 되어있고, 특정 위치에 1(위험)과 2(상점)를 배치합니다.

            // 맵 전체를 안전한 땅(0)으로 초기화
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    gameMapData[y, x] = 0;
                }
            }

            // ---------------------------------------------------------
            // [과제 4] 난이도에 따라 숲과 상점의 개수 바꾸기
            // ---------------------------------------------------------
            // 힌트:
            // difficulty 변수의 값에 따라 forestCount와 shopCount를 다르게 설정해보세요.
            // 쉬움(0): 숲 40개, 상점 3개
            // 보통(1): 숲 60개, 상점 2개
            // 어려움(2): 숲 80개, 상점 1개

            int forestCount = 40; // 기본값
            int shopCount = 3;    // 기본값

            // TODO: 여기에 if문이나 switch문을 사용해서 코드를 작성하세요.

            // ---------------------------------------------------------

            Random random = new Random();

            // 위험한 숲(1) 배치
            for (int i = 0; i < forestCount; i++)
            {
                int randomX = random.Next(0, mapWidth);
                int randomY = random.Next(0, mapHeight);
                // (0,0)은 플레이어 시작 위치, (19,19)는 골인 지점이니 피해서 배치
                if ((randomX != 0 || randomY != 0) && (randomX != 19 || randomY != 19))
                {
                    gameMapData[randomY, randomX] = 1;
                }
            }

            // 상점(2) 배치
            for (int i = 0; i < shopCount; i++)
            {
                while (true)
                {
                    int randomX = random.Next(0, mapWidth);
                    int randomY = random.Next(0, mapHeight);

                    // 빈 땅(0)에만 상점 배치, 시작점과 골인 지점 제외
                    if (gameMapData[randomY, randomX] == 0 &&
                        (randomX != 0 || randomY != 0) &&
                        (randomX != 19 || randomY != 19))
                    {
                        gameMapData[randomY, randomX] = 2;
                        break;
                    }
                }
            }

            // 골인 지점(3) 배치
            gameMapData[19, 19] = 3;
        }

        // ==================================================================================
        // [난이도 선택] 게임 시작 시 난이도를 고르는 함수입니다.
        // ==================================================================================
        static void SelectDifficulty()
        {
            // ---------------------------------------------------------
            // [과제 4] 난이도 선택 기능 구현하기
            // ---------------------------------------------------------
            // 힌트:
            // 1. 화면에 난이도 메뉴를 보여줍니다. (1. 쉬움, 2. 보통, 3. 어려움)
            // 2. 사용자에게 입력을 받습니다. (Console.ReadLine)
            // 3. 입력받은 값에 따라 difficulty 변수를 설정합니다.
            //    - "1"이면 difficulty = 0
            //    - "2"이면 difficulty = 1
            //    - "3"이면 difficulty = 2

            // TODO: 여기에 코드를 작성하세요.
            Console.WriteLine("난이도 선택 기능이 아직 구현되지 않았습니다. (기본값: 쉬움)");
            System.Threading.Thread.Sleep(1000);

            // ---------------------------------------------------------
        }

        // ==================================================================================
        // [화면 출력] 맵, 플레이어, 상태창을 화면에 그리는 함수입니다.
        // ==================================================================================
        static void Render()
        {
            Console.SetCursorPosition(0, 0); // 커서를 맨 위로 이동

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
            // 난이도 표시 (과제 4를 하면 바뀝니다)
            string diffStr = difficulty == 0 ? "쉬움" : (difficulty == 1 ? "보통" : "어려움");
            Console.WriteLine($"[상태] 체력: {currentHealth}/{maxHealth} | 돈: {currentGold}G | 공격력: {attackPower} | 난이도: {diffStr}");
            Console.WriteLine("========================================");
            Console.WriteLine("[조작] 방향키: 이동 | Q: 종료");
            Console.WriteLine("----------------------------------------");

            // 현재 위치에 대한 설명 출력
            int currentTile = gameMapData[playerYPosition, playerXPosition];
            if (currentTile == 0) Console.WriteLine("평원입니다. 방심하면 몬스터가 튀어나옵니다.     ");
            else if (currentTile == 1) Console.WriteLine("으스스한 숲입니다... 몬스터가 나올 것 같습니다! ");
            else if (currentTile == 2) Console.WriteLine("상점입니다. 물건을 살 수 있습니다.              ");
            else if (currentTile == 3) Console.WriteLine("목적지에 도착했습니다! ★                       ");
        }

        // ==================================================================================
        // [입력 처리] 키보드 입력을 받아서 이동하거나 행동하는 함수입니다.
        // ==================================================================================
        static void ProcessInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true); // 키 입력 대기

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
                // [과제 1] 위쪽, 아래쪽 이동 구현하기
                // ---------------------------------------------------------

                // TODO: 여기에 코드를 작성하세요 (과제 1)

                // ---------------------------------------------------------

                case ConsoleKey.Q:
                    isGameRunning = false;
                    return;
            }

            // 이동하려는 곳이 맵 밖인지 확인 (맵 밖으로 나가면 안되니까!)
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
                // 30% 확률로 몬스터 만남
                if (random.Next(0, 100) < 40)
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
            // [과제 3] 다양한 몬스터 만들기
            // ---------------------------------------------------------
            // 힌트:
            // 1. 랜덤 숫자를 뽑습니다. (예: 0~2)
            // 2. 숫자에 따라 몬스터의 이름, 공격력, 보상을 다르게 설정합니다.
            //    (난이도에 따라 강한 몬스터가 나올 확률을 다르게 해보세요!)
            //    - 0: 슬라임 (약함)
            //    - 1: 고블린 (보통)
            //    - 2: 오크 (강함)

            string monsterName = "슬라임";
            int monsterDamage = random.Next(5, 10);
            int monsterReward = random.Next(5, 15);

            // TODO: 여기에 코드를 작성


            // ---------------------------------------------------------

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("           / \\__");
            Console.WriteLine("          (    @\\___");
            Console.WriteLine("          /         O");
            Console.WriteLine("         /   (_____/");
            Console.WriteLine("        /_____/   U");
            Console.WriteLine("");
            Console.WriteLine($"   야생의 {monsterName}이(가) 나타났다!");
            Console.ResetColor();
            Console.WriteLine("========================================");

            // 가위바위보 로직
            Console.WriteLine("가위바위보 대결! (1: 가위, 2: 바위, 3: 보)");
            Console.Write("입력: ");

            FlushInputBuffer(); // 입력 버퍼 비우기
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
            FlushInputBuffer();
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

            // 화면 잔상 제거
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

                FlushInputBuffer(); // 입력 버퍼 비우기
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
                    // [과제 2] 구매 로직 구현하기
                    // ---------------------------------------------------------

                    // TODO: 여기에 코드를 작성하세요 (과제 2 - 물약)
                    Console.WriteLine("아직 구매 기능이 완성되지 않았습니다! (과제)");
                    System.Threading.Thread.Sleep(1000);

                    // ---------------------------------------------------------
                }
                else if (input == "2") // 갑옷
                {
                    int price = 100;

                    // TODO: 여기에 코드를 작성하세요 (과제 2 - 갑옷)
                    // 효과: maxHealth를 20 증가, currentHealth도 20 증가
                    Console.WriteLine("아직 구매 기능이 완성되지 않았습니다! (과제)");
                    System.Threading.Thread.Sleep(1000);
                }
                else if (input == "3") // 검
                {
                    int price = 150;

                    // TODO: 여기에 코드를 작성하세요 (과제 2 - 검)
                    // 효과: attackPower를 5 증가
                    Console.WriteLine("아직 구매 기능이 완성되지 않았습니다! (과제)");
                    System.Threading.Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    System.Threading.Thread.Sleep(500);
                }
            }
            // 화면 잔상 제거
            Console.Clear();
        }

        // ==================================================================================
        // [유틸리티] 입력 버퍼를 비우는 함수
        // ==================================================================================
        static void FlushInputBuffer()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }
    }
}
