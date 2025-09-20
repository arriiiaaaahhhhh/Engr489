using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ObjectiveOverlay : MonoBehaviour
{
    [Header("UI")]
    public Canvas canvas;
    public UnityEngine.UI.Image fullScreenImage;     // species photo (Sprite)
    public UnityEngine.UI.Image textBackdrop;        // text background
    public TMP_Text headerText;       // "NEW SPECIES UNLOCKED"
    public TMP_Text titleText;        // Species Name
    public TMP_Text bodyText;         // species description
    public TMP_Text objectiveText;    // OBJECTIVE
    public TMP_Text hintText;         // Hint
    public TMP_Text clickText;        // "Click anywhere to play"

    bool visible;

    // --- CachePlayer additions ---
    PlayerController playerController;
    void CachePlayer()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>(true);
    }
    // -----------------------------

    void Awake()
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        CachePlayer();        // Cache player on awake (safe if none found)
        HideImmediate();
    }

    public void Show(Sprite sprite, string header, string title, string body, string objective, string hint, bool showClick = true)
    {
        if (fullScreenImage) { fullScreenImage.sprite = sprite; fullScreenImage.enabled = true; } //FitImageToCoverScreen(fullScreenImage);
        if (headerText) headerText.text = header;
        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;
        if (objectiveText) objectiveText.text = objective;
        if (hintText) hintText.text = hint;
        if (clickText) { clickText.gameObject.SetActive(showClick); clickText.text = "Click anywhere to play"; }

        gameObject.SetActive(true);
        visible = true;

        // Pause player input while overlay is up
        CachePlayer();
        if (playerController) playerController.enabled = false;   // <-- disable controls

        UIBaseClass.menuOpen = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowMessage(Sprite sprite, string title, string message)
    {
        Show(sprite, "", title, message, "", "", true);
    }

    public void Hide()
    {
        visible = false;
        gameObject.SetActive(false);

        // Resume player input
        if (playerController) playerController.enabled = true;    // <-- re-enable controls

        UIBaseClass.menuOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void HideImmediate()
    {
        visible = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!visible) return;
        if (Input.GetMouseButtonDown(0))  // click anywhere
        {
            Hide();
        }
    }
}
