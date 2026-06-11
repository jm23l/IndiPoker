using UnityEngine;
using System.Collections;

public class ButtonAnimation : MonoBehaviour
{
    private Vector3 originalScale;
    public float pressedScale = 0.9f; 
    public float speed = 10f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnButtonPressed()
    {
        StopAllCoroutines(); 
        StartCoroutine(BounceAnimation());
    }

    IEnumerator BounceAnimation()
    {
        transform.localScale = originalScale * pressedScale;

        yield return new WaitForSeconds(0.05f);

        transform.localScale = originalScale;
    }
}