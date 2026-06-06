using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IndianPokerManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI enemyCardTxt;
    public TextMeshProUGUI playerCardTxt;
    public TextMeshProUGUI logTxt;
    public TextMeshProUGUI chipsTxt;

    [Header("버튼 연결 (아이템 사용 제어용)")]
    public Button btnShiftUp;
    public Button btnShiftDown;
    public Button btnMagnifier;
    public Button btnRaise;
    public Button btnFold;

    [Header("카드 이미지 세팅")]
    public SpriteRenderer enemyCardImage; // 적 머리 위의 카드 이미지 (UI)
    public Sprite[] spadeCards; 
    public Sprite[] heartCards;
    public Sprite[] diamondCards;
    public Sprite[] cloverCards;

    // 게임 데이터
    private int playerCard;
    private int enemyCard;
    private int playerChips = 30;
    private int enemyChips = 30;
    private int currentPot = 0;
    private bool hasUsedItem = false; // 이번 라운드에 아이템을 썼는지 확인
    private int playerSuit; // 내 문양 (0:스페이드, 1:하트, 2:다이아, 3:클로버)
    private int enemySuit;  // 적 문양

    private int roundCounter = 1;
    private int bonusChips = 5;

    void Start()
    {
        StartNewRound();
    }

    // ==========================================
    // 1. 라운드 시작 로직
    // ==========================================
    void StartNewRound()
    {
        roundCounter++;

        // 1~13 (K) 사이의 숫자 뽑기
        playerCard = Random.Range(1, 14);
        enemyCard = Random.Range(1, 14);

        playerSuit = Random.Range(0, 4);
        enemySuit = Random.Range(0, 4);

        // 참가비(앤티) 1개씩 내기
        playerChips--;
        enemyChips--;
        currentPot = 2;

        // 3턴 조건 검사
        if (roundCounter % 3 == 0)
        {
            hasUsedItem = false;
            SetItemButtonsInteractable(true); // 아이템 버튼 활성화
            logTxt.text = $"[🚨3턴 도래!] 아이템이 충전되었습니다! 베팅하거나 아이템을 쓰세요.";
        }
        else
        {
            hasUsedItem = true; // 이번 턴에는 아이템을 못 쓰게 내부 상태 차단
            SetItemButtonsInteractable(false); // 버튼 비활성화
            int turnsLeft = 3 - (roundCounter % 3);
            logTxt.text = $"새 라운드 시작. (다음 아이템 충전까지 {turnsLeft}턴 남음)";
        }

        playerCardTxt.text = "내 카드: [ ? ]"; // 내 카드는 숨김

        // ❌ [원인 제거] 아래에 있던 강제 활성화 코드와 로그 덮어쓰기 코드를 삭제했습니다.

        UpdateUI();
    }

    // ==========================================
    // 2. 아이템 로직 (UI 버튼에 연결)
    // ==========================================
    public void UseItem_ShiftUp()
    {
        if (hasUsedItem) return;

        // 1을 더하고, 13을 넘어가면 1로 순환 (Wrap-around)
        playerCard = (playerCard % 13) + 1;
        enemyCard = (enemyCard % 13) + 1;

        logTxt.text = "전체 카드 숫자가 1씩 [올라갑니다]!";
        AfterItemUsed();
    }

    public void UseItem_ShiftDown()
    {
        if (hasUsedItem) return;

        // 1을 빼고, 1보다 작아지면 13으로 순환 (Wrap-around)
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

        int cost = 2;

        if (playerChips < cost)
        {
            logTxt.text = $"칩이 부족합니다! (필요: {cost}칩)";
            return;
        }

        playerChips -= cost;

        // 내 카드 범위 계산 (+- 2 오차)
        int minRange = Mathf.Max(1, playerCard - 4);
        int maxRange = Mathf.Min(13, playerCard + 4);

        logTxt.text = $"[돋보기] 당신의 카드는 {minRange} ~ {maxRange} 사이에 있습니다.";
        AfterItemUsed();
    }

    void AfterItemUsed()
    {
        hasUsedItem = true;
        SetItemButtonsInteractable(false); // 아이템 더 이상 못 쓰게 막기
        UpdateUI(); // 바뀐 적의 카드 화면에 반영
    }

    // ==========================================
    // 3. 베팅 로직 (UI 버튼에 연결)
    // ==========================================
    public void OnClickRaise()
    {
        playerChips -= 2;
        currentPot += 2;

        bool aiWillFold = false;
        int rand = Random.Range(0, 100); // 0부터 99까지의 확률 뽑기

        // AI의 확률적 판단 로직
        if (playerCard >= 10)
        {
            // 플레이어 패가 높을 때
            if (rand < 80) aiWillFold = true; // 80%는 합리적으로 포기
            // 나머지 20%는 질 걸 알면서도 들어오는 '블러핑'
        }
        else if (playerCard <= 4)
        {
            // 플레이어 패가 낮을 때
            if (rand < 10) aiWillFold = true; // 10% 확률로 유리한데도 쫄아서 포기
            // 나머지 90%는 이길 줄 알고 콜
        }
        else
        {
            // 애매한 중간 패일 때 (5~9)
            if (rand < 50) aiWillFold = true; // 50:50 반반 승부
        }

        // 판정 결과에 따른 텍스트 및 결과 처리
        if (aiWillFold)
        {
            logTxt.text = "적이 당신의 기세에 쫄아서 다이(Fold)했습니다! (판돈 획득)";
            playerChips += currentPot;
            currentPot = 0;
            StartCoroutine(EndRoundRoutine());
        }
        else
        {
            // AI가 블러핑(구라)을 친 건지, 진짜 자신 있어서 들어온 건지 플레이어는 알 수 없음
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
    // 4. 승패 판정 및 라운드 종료
    // ==========================================
    void DetermineWinner()
    {
        playerCardTxt.text = $"내 카드: [ {playerCard} ]"; // 내 카드 결과 공개

        if (playerCard > enemyCard)
        {
            // 🌟 뇌가 짜릿해지는 문양 일치 잭팟 검사!
            if (playerSuit == enemySuit)
            {
                int totalWin = currentPot + bonusChips;
                logTxt.text = $"🎉대박! 문양이 일치하여 보너스 칩을 획득합니다! (+{totalWin} 칩)";
                playerChips += totalWin;
                enemyChips -= bonusChips; // 적의 주머니에서 보너스 칩을 더 뺏어옵니다.
            }
            else
            {
                logTxt.text = $"승리! 내 패({playerCard}) > 적 패({enemyCard}) (+{currentPot} 칩)";
                playerChips += currentPot;
            }
        }
        else if (playerCard < enemyCard)
        {
            logTxt.text += $"\n패배! 내 패({playerCard}) < 적 패({enemyCard})";
            enemyChips += currentPot;
        }
        else
        {
            logTxt.text += "\n무승부! 판돈을 나눕니다.";
            playerChips += currentPot / 2;
            enemyChips += currentPot / 2;
        }

        currentPot = 0;
        StartCoroutine(EndRoundRoutine());
    }

    IEnumerator EndRoundRoutine()
    {
        playerCardTxt.text = $"내 카드: [ {playerCard} ]"; // 내 카드 결과 공개
        UpdateUI();

        // 버튼 클릭 방지
        btnRaise.interactable = false;
        btnFold.interactable = false;

        yield return new WaitForSeconds(3.0f); // 3초 대기 후 다음 판 시작

        if (playerChips <= 0 || enemyChips <= 0)
        {
            logTxt.text = "게임 종료! 누군가 파산했습니다.";
        }
        else
        {
            btnRaise.interactable = true;
            btnFold.interactable = true;
            StartNewRound();
        }
    }

    // ==========================================
    // 유틸리티 기능
    // ==========================================
    void UpdateUI()
    {
        enemyCardTxt.text = $"적 카드: [ {enemyCard} ]";
        chipsTxt.text = $"나: {playerChips}칩 | 적: {enemyChips}칩\n현재 판돈: {currentPot}칩";

        switch (enemySuit)
        {
            case 0: enemyCardImage.sprite = spadeCards[enemyCard]; break;
            case 1: enemyCardImage.sprite = heartCards[enemyCard]; break;
            case 2: enemyCardImage.sprite = diamondCards[enemyCard]; break;
            case 3: enemyCardImage.sprite = cloverCards[enemyCard]; break;
        }
    }

    void SetItemButtonsInteractable(bool state)
    {
        btnShiftUp.interactable = state;
        btnShiftDown.interactable = state;
        btnMagnifier.interactable = state;
    }
}