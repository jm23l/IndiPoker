using UnityEngine;
using System.Collections;

public class ButtonAnimation : MonoBehaviour
{
    private Vector3 originalScale;
    public float pressedScale = 0.9f; // 눌렀을 때 크기 (90%)
    public float speed = 10f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    // 버튼의 OnClick 이벤트에 이 함수를 연결해도 됩니다!
    public void OnButtonPressed()
    {
        StopAllCoroutines(); // 애니메이션이 겹치지 않게 초기화
        StartCoroutine(BounceAnimation());
    }

    IEnumerator BounceAnimation()
    {
        // 1. 작아지기
        transform.localScale = originalScale * pressedScale;

        // 2. 아주 짧은 대기
        yield return new WaitForSeconds(0.05f);

        // 3. 다시 원래대로
        transform.localScale = originalScale;
    }
}