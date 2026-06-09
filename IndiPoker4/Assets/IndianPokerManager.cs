using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IndianPokerManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI logTxt;
    public TextMeshProUGUI chipsTxt;

    [Header("아이템 떨어지는 연출 연결")]
    public SpriteRenderer itemVisualImage; // 화면에 떨어질 임시 아이템 이미지 오브젝트
    public Transform itemSpawnPos;         // 아이템이 생성되어 출발할 곳 (화면 위쪽 밖)
    public Transform itemTablePos;         // 아이템이 떨어져서 도착할 곳 (테이블 위)
    public GameObject[] itemPrefabs;           // 아이템 이미지들 (0: 돋보기, 1: 시프트 업, 2: 시프트 다운)


    public int myCardNumber;

    [Header("카드 비주얼 시스템")]
    public SpriteRenderer playerCardVisual; // 테이블 위에 배치한 플레이어 카드 오브젝트의 SpriteRenderer
    public Sprite cardBackSprite;
    public Sprite[] cardSprites;            // 1번부터 10번까지의 카드 이미지들 (인스펙터에서 등록)

    [Header("카드 이미지 및 덱 연결")]
    public SpriteRenderer enemyCardImage;
    public SpriteRenderer publicCardImage; // 테이블 중앙의 바닥 패

    public Transform deckPosition;         // 카드를 뽑을 덱의 위치 (출발점)
    public Transform publicCardTargetPos;  // 바닥 패가 놓일 테이블 위치 (도착점)

    [Header("카드 리소스 (Size 14)")]
    public Sprite[] spadeCards;
    public Sprite[] heartCards;
    public Sprite[] diamondCards;
    public Sprite[] cloverCards;

    [Header("설명창 UI 연결 (여기다 드래그하세요!)")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    // 게임 데이터
    private int playerCard, enemyCard, publicCard;
    private int playerSuit, enemySuit, publicSuit;
    private int playerChips = 30;
    private int enemyChips = 30;
    private int currentPot = 0;
    private int roundCounter = 0;
    private bool hasUsedItem = false;

    void Start()
    {
        StartNewRound();
    }

    // ==========================================
    // 1. 라운드 시작 로직
    // ==========================================
    void StartNewRound()
    {
        // 🌟 [추가] 라운드 시작 시 내 카드를 뒷면으로 가립니다.
        myCardNumber = Random.Range(1, 11);
        Debug.Log($"[시작] 내가 뽑은 진짜 카드 번호는: {myCardNumber}");

        // 라운드 시작 시 내 카드를 뒷면으로 가립니다.
        if (playerCardVisual != null && cardBackSprite != null)
        {
            playerCardVisual.sprite = cardBackSprite;
        }

        roundCounter++;

        // 카드 숫자 & 문양 뽑기
        playerCard = Random.Range(1, 14);
        enemyCard = Random.Range(1, 14);
        publicCard = Random.Range(1, 14);

        playerSuit = Random.Range(0, 4);
        enemySuit = Random.Range(0, 4);
        publicSuit = Random.Range(0, 4);

        // 기본 세팅
        playerChips--;
        enemyChips--;
        currentPot = 2;

        // 3턴 아이템 규칙 적용
        if (roundCounter % 3 == 0)
        {
            StartCoroutine(DropItemRoutine());
            hasUsedItem = false;
            logTxt.text = $"[3턴 도래!] 아이템이 충전되었습니다!\n(바닥 패가 깔립니다...)";
        }
        else
        {
            hasUsedItem = true;
            int turnsLeft = 3 - (roundCounter % 3);
            logTxt.text = $"새 라운드 시작. (아이템 충전까지 {turnsLeft}턴 남음)\n(바닥 패가 깔립니다...)";
        }

        UpdateEnemyCardSprite();

        // 바닥 패 애니메이션 시작
        StartCoroutine(DealPublicCardRoutine());
    }

    // ==========================================
    // 2. 바닥 패 애니메이션 코루틴
    // ==========================================
    IEnumerator DealPublicCardRoutine()
    {

        // 덱 위치에서 뒷면 상태로 시작
        publicCardImage.transform.position = deckPosition.position;
        publicCardImage.sprite = spadeCards[0];

        Vector3 startPos = deckPosition.position;
        Vector3 endPos = publicCardTargetPos.position;
        float elapsedTime = 0f;
        float moveDuration = 0.5f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            t = t * t * (3f - 2f * t);

            publicCardImage.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 도착 후 앞면 공개
        publicCardImage.sprite = GetCardSprite(publicSuit, publicCard);


    }

    

    // ==========================================
    // 3. 아이템 사용 로직
    // ==========================================
    public void UseItem_ShiftUp()
    {
        if (hasUsedItem) return;

        playerCard = (playerCard % 13) + 1;
        enemyCard = (enemyCard % 13) + 1;

        logTxt.text = "전체 카드 숫자가 1씩 [올라갑니다]!";
        AfterItemUsed();
    }

    public void UseItem_ShiftDown()
    {
        if (hasUsedItem) return;

        playerCard = playerCard - 1;
        if (playerCard < 1) playerCard = 13;

        enemyCard = enemyCard - 1;
        if (enemyCard < 1) enemyCard = 13;

        logTxt.text = "전체 카드 숫자가 1씩 [내려갑니다]!";
        AfterItemUsed();
    }

    public void UseItem_Magnifier()
    {
        if (hasUsedItem) return;

        int cost = 2; // 돋보기 사용 비용
        if (playerChips < cost)
        {
            logTxt.text = $"칩이 부족합니다! (필요: {cost}칩)";
            return;
        }

        playerChips -= cost;

        int minRange = Mathf.Max(1, playerCard - 4);
        int maxRange = Mathf.Min(13, playerCard + 4);

        logTxt.text = $"[-{cost}칩 지불] 돋보기 발동!\n당신의 카드는 {minRange} ~ {maxRange} 사이에 있습니다.";
        AfterItemUsed();
    }

    void AfterItemUsed()
    {
        hasUsedItem = true;
        UpdateEnemyCardSprite();
    }

    // ==========================================
    // 4. 베팅 로직 (AI 블러핑 포함)
    // ==========================================
    public void OnClickRaise()
    {
        playerChips -= 2;
        currentPot += 2;

        bool aiWillFold = false;
        int rand = Random.Range(0, 100);

        if (playerCard >= 10)
        {
            if (rand < 80) aiWillFold = true;
        }
        else if (playerCard <= 4)
        {
            if (rand < 10) aiWillFold = true;
        }
        else
        {
            if (rand < 50) aiWillFold = true;
        }

        if (aiWillFold)
        {
            logTxt.text = "적이 다이(Fold)했습니다! (판돈 획득)";
            playerChips += currentPot;
            currentPot = 0;
            StartCoroutine(EndRoundRoutine());
        }
        else
        {
            logTxt.text = "적이 당신의 레이즈를 받았습니다! (Call) 결과를 봅니다.";
            enemyChips -= 2;
            currentPot += 2;
            DetermineWinner();
        }
    }

    public void OnClickFold()
    {
        logTxt.text = "당신이 포기했습니다. 적이 판돈을 가져갑니다.";
        enemyChips += currentPot;
        currentPot = 0;
        StartCoroutine(EndRoundRoutine());
    }

    // ==========================================
    // 5. 승패 판정 (바닥 패 시너지 로직)
    // ==========================================
    void DetermineWinner()
    {

        int mySynergy = 0;
        int enemySynergy = 0;

        if (playerSuit == publicSuit) mySynergy += 5;
        if (enemySuit == publicSuit) enemySynergy += 5;

        if (playerCard == publicCard) mySynergy += 10;
        if (enemyCard == publicCard) enemySynergy += 10;

        int finalPlayerScore = playerCard + mySynergy;
        int finalEnemyScore = enemyCard + enemySynergy;

        string resultLog = $"[판정] 나: {playerCard}(+{mySynergy}) vs 적: {enemyCard}(+{enemySynergy})\n";

        if (finalPlayerScore > finalEnemyScore)
        {
            resultLog += $"승리! (+{currentPot} 칩)";
            playerChips += currentPot;
        }
        else if (finalPlayerScore < finalEnemyScore)
        {
            resultLog += $"패배... 적이 판돈을 가져갑니다.";
            enemyChips += currentPot;
        }
        else
        {
            resultLog += $"무승부! 판돈을 나눕니다.";
            playerChips += currentPot / 2;
            enemyChips += currentPot / 2;
        }

        logTxt.text = resultLog;
        currentPot = 0;

        chipsTxt.text = $"나: {playerChips}칩 | 적: {enemyChips}칩";
        StartCoroutine(EndRoundRoutine());
    }

    // ==========================================
    // 유틸리티 함수들
    // ==========================================
    IEnumerator EndRoundRoutine()
    {

        yield return new WaitForSeconds(3.5f);

        Debug.Log($"[종료] 결과창에서 꺼내온 내 카드 번호는: {myCardNumber}");
        if (playerCardVisual != null && myCardNumber >= 1 && myCardNumber < cardSprites.Length)
        {
            playerCardVisual.sprite = cardSprites[myCardNumber];
            Debug.Log($"[연출] 내 카드 공개: {myCardNumber}번 카드");
        }

        if (playerChips <= 0 || enemyChips <= 0)
        {
            logTxt.text = "게임 종료! 누군가 파산했습니다.";
        }
        else
        {
            StartNewRound();
        }
    }

    void UpdateEnemyCardSprite()
    {
        enemyCardImage.sprite = GetCardSprite(enemySuit, enemyCard);
        chipsTxt.text = $"나: {playerChips}칩 | 적: {enemyChips}칩\n현재 판돈: {currentPot}칩";
    }

    Sprite GetCardSprite(int suit, int number)
    {
        switch (suit)
        {
            case 0: return spadeCards[number];
            case 1: return heartCards[number];
            case 2: return diamondCards[number];
            case 3: return cloverCards[number];
            default: return spadeCards[number];
        }
    }


    public void SetPlayerCard(int cardNumber)
    {
        // 카드 숫자에 맞는 이미지를 갈아끼웁니다. 
        // (스프라이트 배열의 인덱스를 카드 숫자와 맞추면 편합니다 예: 1번 카드 이미지 -> element 1)
        if (cardNumber < cardSprites.Length && cardSprites[cardNumber] != null)
        {
            playerCardVisual.sprite = cardSprites[cardNumber];
        }
    }

    // ==========================================
    // 🎬 아이템 프리팹이 위에서 툭 떨어지는 코루틴
    // ==========================================
    IEnumerator DropItemRoutine()
    {
        // 1. 3가지 아이템 중 무작위로 하나 결정
        int randomItemIndex = Random.Range(0, 3);

        // 2. 허공(itemSpawnPos)에 해당 프리팹을 생성
        // 생성되는 순간 유니티 물리 엔진(Rigidbody)에 의해 자동으로 바닥으로 떨어집니다.
        GameObject spawnedItem = Instantiate(itemPrefabs[randomItemIndex], itemSpawnPos.position, Quaternion.identity);

        logTxt.text += $"\n테이블에 아이템이 보급되었습니다!";

        // 🌟 에러 방지: 코루틴 규칙을 맞추기 위해 여기서 함수를 즉시 종료시킵니다.
        // (이제 2초 뒤에 아이템이 마음대로 사라지지 않고 테이블에 영구적으로 남습니다.)
        yield break;
    }
    public void UseClickedItem(int type)
    {
        // 아이템 번호에 맞춰서 기존 아이템 함수를 실행합니다.
        switch (type)
        {
            case 0:
                UseItem_Magnifier();
                break;
            case 1:
                UseItem_ShiftUp();
                break;
            case 2:
                UseItem_ShiftDown();
                break;
        }
    }

    public void HidePlayerCard()
    {
        playerCardVisual.sprite = cardBackSprite;
        playerCardVisual.gameObject.SetActive(true); // 카드는 일단 보여야 함(뒷면으로)
    }

    public void RevealPlayerCard(int myCardNumber)
    {
        if (myCardNumber >= 1 && myCardNumber <= 10)
        {
            playerCardVisual.sprite = cardSprites[myCardNumber];
            Debug.Log($"내 카드 공개: {myCardNumber}번 카드");
        }
    }

    public void ShowItemTooltip(string desc, Vector2 mousePos)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = desc;
            tooltipPanel.SetActive(true);
            // 마우스 커서 살짝 옆에 배치
            tooltipPanel.transform.position = mousePos + new Vector2(30, 30);
        }
    }

    // 아이템이 마우스에서 벗어날 때 실행할 함수
    public void HideItemTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}