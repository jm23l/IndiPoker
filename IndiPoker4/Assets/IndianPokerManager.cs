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

    // 게임 데이터
    private int playerCard;
    private int enemyCard;
    private int playerChips = 30;
    private int enemyChips = 30;
    private int currentPot = 0;
    private bool hasUsedItem = false; // 이번 라운드에 아이템을 썼는지 확인

    void Start()
    {
        StartNewRound();
    }

    // ==========================================
    // 1. 라운드 시작 로직
    // ==========================================
    void StartNewRound()
    {
        // 1~13 (K) 사이의 숫자 뽑기
        playerCard = Random.Range(1, 14);
        enemyCard = Random.Range(1, 14);

        // 참가비(앤티) 1개씩 내기
        playerChips--;
        enemyChips--;
        currentPot = 2;

        hasUsedItem = false;
        SetItemButtonsInteractable(true); // 아이템 버튼 활성화

        UpdateUI();
        logTxt.text = "새 라운드가 시작되었습니다. [아이템]을 쓰거나 [베팅]하세요.";
        playerCardTxt.text = "내 카드: [ ? ]"; // 내 카드는 숨김
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

        // 내 카드 범위 계산 (+- 2 오차)
        int minRange = Mathf.Max(1, playerCard - 2);
        int maxRange = Mathf.Min(13, playerCard + 2);

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

        // 간단한 몬스터 AI 반응 (내 카드가 10 이상이면 몬스터가 쫄아서 포기할 확률 발생)
        if (playerCard >= 10 && Random.Range(0, 100) < 50)
        {
            logTxt.text = "적이 당신의 카드를 보고 쫄아서 다이(Fold)했습니다!";
            playerChips += currentPot;
            currentPot = 0;
            StartCoroutine(EndRoundRoutine());
        }
        else
        {
            logTxt.text = "적이 당신의 레이즈를 받았습니다! (Call)";
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
        if (playerCard > enemyCard)
        {
            logTxt.text += $"\n승리! 내 패({playerCard}) > 적 패({enemyCard})";
            playerChips += currentPot;
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
    }

    void SetItemButtonsInteractable(bool state)
    {
        btnShiftUp.interactable = state;
        btnShiftDown.interactable = state;
        btnMagnifier.interactable = state;
    }
}