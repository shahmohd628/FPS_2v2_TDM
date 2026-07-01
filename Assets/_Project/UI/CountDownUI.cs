using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int startNumber = 3;

    private Coroutine _routine;

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
        gameObject.SetActive(true);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PlayCountdown());
    }

    public void Hide()
    {
        if (_routine != null) StopCoroutine(_routine);
        if (panel != null) panel.SetActive(false);
    }

    private IEnumerator PlayCountdown()
    {
        for (int i = startNumber; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        if (panel != null) panel.SetActive(false);
    }
}