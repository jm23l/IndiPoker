using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ActionClick : MonoBehaviour
{
    public string actionType;
    public AudioSource clickSound;
    private static bool isProcessing = false;
    void Update()
    {
        if (isProcessing) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(ClickRoutine());
            }
        }
    }

    IEnumerator ClickRoutine()
    {
        isProcessing = true; 

        if (clickSound != null) clickSound.Play();

        IndianPokerManager manager = Object.FindFirstObjectByType<IndianPokerManager>();
        if (manager != null)
        {
            if (actionType == "Raise") manager.OnClickRaise();
            else if (actionType == "Fold") manager.OnClickFold();
        }

        yield return new WaitForSeconds(3f); 
        isProcessing = false; 
    }
}