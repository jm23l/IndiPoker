using UnityEngine;
using UnityEngine.InputSystem;

public class ActionClick : MonoBehaviour
{
    [Header("버튼 기능 설정 (Raise 또는 Fold 입력)")]
    public string actionType;

    void Update()
    {
        // 마우스가 연결되어 있고, 왼쪽 버튼을 방금 눌렀다면
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Camera.main == null) return;

            // 마우스 커서 위치를 최신 문법으로 가져옵니다.
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            // 레이저가 관통한 모든 오브젝트 검사
            RaycastHit[] hits = Physics.RaycastAll(ray);

            bool isClicked = false;
            foreach (RaycastHit hit in hits)
            {
                // 부딪힌 녀석 중에 나(이 칩)가 있다면 클릭 성공!
                if (hit.collider.gameObject == gameObject)
                {
                    isClicked = true;
                    break;
                }
            }

            if (isClicked)
            {
                IndianPokerManager manager = Object.FindFirstObjectByType<IndianPokerManager>();
                if (manager != null)
                {
                    // actionType에 적힌 글자에 따라 매니저의 함수를 실행합니다!
                    if (actionType == "Raise")
                    {
                        manager.OnClickRaise(); // (주의) 실제 매니저에 있는 레이즈 함수 이름으로 맞춰주세요!
                        Debug.Log("💰 레이즈 칩 클릭 성공!");
                    }
                    else if (actionType == "Fold")
                    {
                        manager.OnClickFold(); // (주의) 실제 매니저에 있는 폴드 함수 이름으로 맞춰주세요!
                        Debug.Log("🏳️ 폴드 칩 클릭 성공!");
                    }
                }
            }
        }
    }
}
