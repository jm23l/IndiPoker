using UnityEngine;
using UnityEngine.InputSystem;

public class ItemClick : MonoBehaviour
{
    [Header("아이템 종류 (0:돋보기, 1:시프트업, 2:시프트다운)")]
    public int itemType;

    void Update()
    {
        // 마우스가 연결되어 있고, 왼쪽 버튼을 방금 눌렀다면
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Camera.main == null) return;

            // 마우스 커서 위치를 최신 문법으로 가져옵니다.
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            RaycastHit[] hits = Physics.RaycastAll(ray);

            string hitLogs = "[엑스레이 스캔 결과] 마우스가 관통한 오브젝트들: ";
            bool hitThisItem = false;

            foreach (RaycastHit hit in hits)
            {
                hitLogs += $"[{hit.collider.gameObject.name}] ";
                if (hit.collider.gameObject == gameObject)
                {
                    hitThisItem = true;
                }
            }

            if (hits.Length > 0)
            {
                Debug.Log(hitLogs);
            }

            if (hitThisItem)
            {
                IndianPokerManager manager = Object.FindFirstObjectByType<IndianPokerManager>();
                if (manager != null)
                {
                    if (itemType == 0) manager.UseItem_Magnifier();
                    else if (itemType == 1) manager.UseItem_ShiftUp();
                    else if (itemType == 2) manager.UseItem_ShiftDown();

                    Debug.Log($"🎉 {gameObject.name} 아이템 사용 성공!");
                    Destroy(gameObject);
                }
            }
        }
    }
}

