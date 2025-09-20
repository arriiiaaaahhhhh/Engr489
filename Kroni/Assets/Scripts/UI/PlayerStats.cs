using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    // Fields for oxygen stat monitoring
    public int maxLungCapacity;
    public int currentLungCapacity;
    Coroutine LungCapacityCo;

    // Check if players head is under the water
    bool swimCheck;

    // Only show Out of Breath pop up once until oxygen > 0 again
    bool outOfBreathShown = false;

    // Store player for respawn
    PlayerController player;

    [Header("UI")]
    public Slider LungCapacitySlider;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>(true);

        // Start at max capacity
        currentLungCapacity = maxLungCapacity;
        LungCapacitySlider.maxValue = maxLungCapacity;
        LungCapacitySlider.value = currentLungCapacity;
    }

    // Update is called once per frame
    void Update()
    {
        if (!UIBaseClass.menuOpen)
        {
            // Refill lungs when above surface
            if (!PlayerController.isSwimming && swimCheck == true)
            {
                swimCheck = false;
                if (LungCapacityCo != null) StopCoroutine(LungCapacityCo);
                ChangeLungCapacity(currentLungCapacity, maxLungCapacity);
            }
            // Decrease oxygen when below water
            if (PlayerController.isSwimming && swimCheck == false)
            {
                swimCheck = true;
                LungCapacityCo = StartCoroutine(DecreaseLungCapacity(currentLungCapacity, 2, 2));
            }

            LungCapacitySlider.value = currentLungCapacity;
        }
    }

    // Slowly decrease remaining oxygen when below surface
    IEnumerator DecreaseLungCapacity(int LungCapacity, int interval, int amount)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            // Ensure lung capacity doesn't drop beneath 0
            if (currentLungCapacity > 0)
            {
                currentLungCapacity = Mathf.Max(currentLungCapacity - amount, 0);
                LungCapacitySlider.value = currentLungCapacity;

                if (currentLungCapacity == 0 && !outOfBreathShown)
                {
                    outOfBreathShown = true;

                    // Teleport to spawn point
                    if (player != null) player.TeleportToSpawn();


                    // Out of breath popup
                    if (ObjectiveManager.Instance != null)
                        ObjectiveManager.Instance.ShowOutOfBreathPopup();
                }
            }
        }
    }

    // Refresh when above water
    public void ChangeLungCapacity(int LungCapacity, int refreshAmount)
    {
        if (refreshAmount > 0)
        {
            currentLungCapacity = Mathf.Min(currentLungCapacity + refreshAmount, maxLungCapacity);
        }
        else
        {
            currentLungCapacity = Mathf.Max(currentLungCapacity + refreshAmount, 0);
        }

        // If we have air again, allow future "Out of Breath" popups
        if (currentLungCapacity > 0) outOfBreathShown = false;

        LungCapacitySlider.value = currentLungCapacity;
    }

    // If player picks up litter, increase lung capacity
    public void BonusOxygen()
    {
        maxLungCapacity += 10;
        currentLungCapacity += 10;

        // Oxygen restored — allow future out-of-breath popups later
        if (currentLungCapacity > 0) outOfBreathShown = false;

        LungCapacitySlider.maxValue = maxLungCapacity;
        LungCapacitySlider.value = currentLungCapacity;
    }
}
