using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemClick : MonoBehaviour
{
    [Header("아이템 설정")]
    public int itemType;
    [TextArea] public string itemDescription;

    private IndianPokerManager manager;
    private bool isHovering = false;

    void Start()
    {
        // 씬에 배치되어 있는 게임 매니저를 자동으로 연동합니다.
        manager = Object.FindFirstObjectByType<IndianPokerManager>();
    }

    void Update()
    {
        if (Camera.main == null || manager == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            isHovering = true;
            // 🌟 내 설명을 매니저에게 전달하여 띄우도록 대리 요청합니다.
            manager.ShowItemTooltip(itemDescription, mousePos);
        }
        else if (isHovering)
        {
            isHovering = false;
            manager.HideItemTooltip();
        }

        // 클릭 로직
        if (Mouse.current.leftButton.wasPressedThisFrame && isHovering)
        {
            if (itemType == 0) manager.UseItem_Magnifier();
            else if (itemType == 1) manager.UseItem_ShiftUp();
            else if (itemType == 2) manager.UseItem_ShiftDown();

            manager.HideItemTooltip();
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // 아이템이 클릭되어 파괴될 때 설명창도 함께 닫아줍니다.
        if (isHovering && manager != null)
        {
            manager.HideItemTooltip();
        }
    }
}

