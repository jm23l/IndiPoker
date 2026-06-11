using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IndianPokerManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI logTxt;
    public TextMeshProUGUI chipsTxt;

    [Header("아이템 떨어지는 연출 연결")]
    public SpriteRenderer itemVisualImage; 
    public Transform itemSpawnPos;         
    public Transform itemTablePos;         
    public GameObject[] itemPrefabs;

    [Header("가이드 UI")]
    public GameObject guidePanel; 
    public int myCardNumber;

    [Header("카드 비주얼 시스템")]
    public SpriteRenderer playerCardVisual; 
    public Sprite cardBackSprite;
    public Sprite[] cardSprites;            

    [Header("카드 이미지 및 덱 연결")]
    public SpriteRenderer enemyCardImage;
    public SpriteRenderer publicCardImage; 

    public Transform deckPosition;        
    public Transform publicCardTargetPos;

    public AudioSource casinoBGM;

    [Header("카드 리소스 (Size 14)")]
    public Sprite[] spadeCards;
    public Sprite[] heartCards;
    public Sprite[] diamondCards;
    public Sprite[] cloverCards;

    [Header("설명창 UI 연결 (여기다 드래그하세요!)")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private int playerCard, enemyCard, publicCard;
    private int playerSuit, enemySuit, publicSuit;
    private int playerChips = 30;
    private int enemyChips = 30;
    private int currentPot = 0;
    private int roundCounter = 0;
    private bool hasUsedItem = false;

    void Start()
    {
        if (casinoBGM != null)
        {
            casinoBGM.Stop();
        }
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }
        else
        {
            if (casinoBGM != null) casinoBGM.Play();
            StartNewRound();
        }
    }
    public void CloseGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
            if (casinoBGM != null)
            {
                casinoBGM.Play();
            }
            StartNewRound(); 
        }
    }

    void StartNewRound()
    {
        int mySuit = Random.Range(0, 4);     
        int myNum = Random.Range(1, 14);      
        myCardNumber = (mySuit * 13) + myNum;

        if (playerCardVisual != null && cardBackSprite != null)
        {
            playerCardVisual.sprite = cardBackSprite;
        }

        roundCounter++;

        playerCard = Random.Range(1, 14);
        enemyCard = Random.Range(1, 14);
        publicCard = Random.Range(1, 14);

        playerSuit = Random.Range(0, 4);
        enemySuit = Random.Range(0, 4);
        publicSuit = Random.Range(0, 4);

        playerChips--;
        enemyChips--;
        currentPot = 2;

        if (roundCounter % 3 == 0)
        {
            StartCoroutine(DropItemRoutine());
            hasUsedItem = false;
            logTxt.text = $"아이템이 생성되었습니다!\n(바닥 패가 깔립니다)";
        }
        else
        {
            hasUsedItem = true;
            int turnsLeft = 3 - (roundCounter % 3);
            logTxt.text = $"새 라운드 시작. (아이템 생성까지 {turnsLeft}턴 남음)\n(바닥 패가 깔립니다)";
        }

        UpdateEnemyCardSprite();

        StartCoroutine(DealPublicCardRoutine());
    }

    IEnumerator DealPublicCardRoutine()
    {

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

        publicCardImage.sprite = GetCardSprite(publicSuit, publicCard);


    }

  
    public void UseItem_ShiftUp()
    {
        if (hasUsedItem) return;

        playerCard = (playerCard % 13) + 1;
        enemyCard = (enemyCard % 13) + 1;

        logTxt.text = "전체 카드 숫자가 1씩 올라갑니다!";
        AfterItemUsed();
    }

    public void UseItem_ShiftDown()
    {
        if (hasUsedItem) return;

        playerCard = playerCard - 1;
        if (playerCard < 1) playerCard = 13;

        enemyCard = enemyCard - 1;
        if (enemyCard < 1) enemyCard = 13;

        logTxt.text = "전체 카드 숫자가 1씩 내려갑니다!";
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

        int minRange = Mathf.Max(1, playerCard - 4);
        int maxRange = Mathf.Min(13, playerCard + 4);

        logTxt.text = $"[-{cost}칩 지불] 돋보기 발동!\n당신의 카드는 {minRange} ~ {maxRange} 사이에 있습니다.";
        AfterItemUsed();
    }

    void AfterItemUsed()
    {
        hasUsedItem = true;
        Debug.Log($"아이템 사용 후! 적 카드 숫자: {enemyCard}, 문양: {enemySuit}");
        UpdateEnemyCardSprite();
    }

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
            logTxt.text = "상대가 폴드했습니다! (판돈 획득)";
            playerChips += currentPot;
            currentPot = 0;
            StartCoroutine(EndRoundRoutine());
        }
        else
        {
            logTxt.text = "상대가 당신의 레이즈를 받았습니다! (Call) 결과를 봅니다.";
            enemyChips -= 2;
            currentPot += 2;
            DetermineWinner();
        }
    }

    public void OnClickFold()
    {
        logTxt.text = "당신이 포기했습니다. 상대가 판돈을 가져갑니다.";
        enemyChips += currentPot;
        currentPot = 0;
        StartCoroutine(EndRoundRoutine());
    }

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

        string resultLog = $"[판정] 나: {playerCard}(+{mySynergy}) vs 상대: {enemyCard}(+{enemySynergy})\n";

        if (finalPlayerScore > finalEnemyScore)
        {
            resultLog += $"<color=yellow>승리! (+{currentPot} 칩)";
            playerChips += currentPot;
        }
        else if (finalPlayerScore < finalEnemyScore)
        {
            resultLog += $"<color=red>패배... 상대가 판돈을 가져갑니다.";
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

        chipsTxt.text = $"나: {playerChips}칩 | 상대: {enemyChips}칩";
        StartCoroutine(EndRoundRoutine());
    }

    IEnumerator EndRoundRoutine()
    {

        yield return new WaitForSeconds(1.0f);

        Debug.Log($"[종료] 결과창에서 꺼내온 내 카드 번호는: {myCardNumber}");

        if (playerCardVisual != null && cardSprites != null)
        {
            playerCardVisual.sprite = cardSprites[myCardNumber];
            logTxt.text = "내 카드가 공개됩니다...";
        }
        yield return new WaitForSeconds(2.0f);

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
        chipsTxt.text = $"나: {playerChips}칩 | 상대: {enemyChips}칩\n현재 판돈: {currentPot}칩";
    }

    Sprite GetCardSprite(int suit, int number)
    {
        int safeNum = Mathf.Clamp(number, 1, 13);

        int index = (suit * 13) + number;

        if (index >= 1 && index < cardSprites.Length)
        {
            return cardSprites[index];
        }
        return cardBackSprite;
        
    }


    public void SetPlayerCard(int cardNumber)
    {
        if (cardNumber < cardSprites.Length && cardSprites[cardNumber] != null)
        {
            playerCardVisual.sprite = cardSprites[cardNumber];
        }
    }

    IEnumerator DropItemRoutine()
    {
        int randomItemIndex = Random.Range(0, 3);

        GameObject spawnedItem = Instantiate(itemPrefabs[randomItemIndex], itemSpawnPos.position, Quaternion.identity);

        logTxt.text += $"\n테이블에 아이템이 보급되었습니다!";

        yield break;
    }
    public void UseClickedItem(int type)
    {
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
        playerCardVisual.gameObject.SetActive(true); 
    }

    public void RevealPlayerCard(int myCardNumber)
    {
        if (myCardNumber >= 1 && myCardNumber <= 10)
        {
            playerCardVisual.sprite = cardSprites[myCardNumber];
        }
    }

    public void ShowItemTooltip(string desc, Vector2 mousePos)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = desc;
            tooltipPanel.SetActive(true);
            tooltipPanel.transform.position = mousePos + new Vector2(30, 30);
        }
    }

    public void HideItemTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}