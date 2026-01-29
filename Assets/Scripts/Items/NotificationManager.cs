using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    [Header("Konfiguracja")]
    public GameObject notificationPrefab;
    public Transform notificationContainer;

    public void ShowNotification(string itemName, Sprite icon)
    {
        if (notificationPrefab == null || notificationContainer == null)
        {
            Debug.LogError("NotificationManager: Brakuje przypisanego Prefaba lub Kontenera w Inspectorze!");
            return;
        }

        // Tworzymy dymek
        GameObject newNotif = Instantiate(notificationPrefab, notificationContainer);

        // Konfigurujemy go
        NotificationItem itemScript = newNotif.GetComponent<NotificationItem>();
        if (itemScript != null)
        {
            itemScript.Setup(itemName, icon);
        }
    }
}