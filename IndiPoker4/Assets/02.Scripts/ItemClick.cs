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
            manager.ShowItemTooltip(itemDescription, mousePos);
        }
        else if (isHovering)
        {
            isHovering = false;
            manager.HideItemTooltip();
        }

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
        if (isHovering && manager != null)
        {
            manager.HideItemTooltip();
        }
    }
}

