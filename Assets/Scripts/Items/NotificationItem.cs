using UnityEngine;
using TMPro;            // Wymagane do tekstów TextMeshPro
using UnityEngine.UI;   // Wymagane do obrazków (Image)
using System.Collections;

public class NotificationItem : MonoBehaviour
{
    [Header("Elementy Prefaba")]
    public TextMeshProUGUI messageText;
    public Image iconImage;

    [Header("Ustawienia")]
    public float lifetime = 3.0f; // Czas w sekundach zanim zniknie

    // Funkcja inicjalizuj¹ca wygl¹d (wywo³ywana przez NotificationManager)
    public void Setup(string itemName, Sprite icon)
    {
        // Ustawiamy tekst
        if (messageText != null)
        {
            messageText.text = "+1 " + itemName;
        }

        // Ustawiamy ikonê (jeœli istnieje)
        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else
            {
                // Jeœli przedmiot nie ma ikony, ukrywamy bia³y kwadrat
                iconImage.enabled = false;
            }
        }

        // Uruchamiamy odliczanie do zniszczenia
        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        // Czekamy okreœlon¹ liczbê sekund
        yield return new WaitForSeconds(lifetime);

        // Niszczymy ten dymek
        Destroy(gameObject);
    }
}