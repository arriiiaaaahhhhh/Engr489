using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }
    public PlayerStats PSInstance;

    public List<ItemScriptableObject> inventoryList = new List<ItemScriptableObject>();

    public GameObject inventory;        // UI panel container (the strip)
    public GameObject InventoryItem;    // Icon prefab
    public bool inventoryOpen;
    public float playerReach = 20f;
    // Item Collection
    public GameObject cam;


    // Inventory display
    public int capacity = 6;
    public Vector2 iconSize = new Vector2(100, 100);
    public float iconSpacing = 8f;


    void Awake()
    {
        Instance = this;
        PSInstance = FindObjectOfType<PlayerStats>();
        RefreshUI();
        inventoryOpen = true;
        inventory.SetActive(true);
    }

    void Update()
    {
        if (UIBaseClass.menuOpen) return;

        InteractableObject i = HoverObject();

        if (i != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PickUpItem(i);
            }
        }
    }

    InteractableObject HoverObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, playerReach))
        {
            var io = hit.collider.GetComponent<InteractableObject>();
            return io != null ? io : hit.collider.GetComponentInParent<InteractableObject>();
        }
        return null;
    }

    // Allow player to pick up items, if item is a fish then add it to inventory
    void PickUpItem(InteractableObject itemPicked)
    {
        if (inventoryList.Count >= capacity)
        {
            UnityEngine.Debug.Log("inventory full!");
            return;
        }

        if (itemPicked != null)
        {   // Picking up litter increases lung capacity
            if (itemPicked.isLitter)
            {
                // Show litter popup
                if (ObjectiveManager.Instance != null)
                    ObjectiveManager.Instance.ShowLitterPopup();
                Destroy(itemPicked.gameObject);
                PSInstance.BonusOxygen();
                RefreshUI();
                return;
            }
            // Only pick up item if it's the current objective
            ObjectiveManager.Instance.NotifyCollected(itemPicked.gameObject.tag);
            if (ObjectiveManager.Instance.isObjective)
            {
                inventoryList.Add(itemPicked.item);
                Destroy(itemPicked.gameObject);
                RefreshUI();
                // If objective is reached, clear the inventory for the next set of objectives
                if (ObjectiveManager.Instance.clearInventory)
                {
                    inventoryList.Clear();
                    ObjectiveManager.Instance.clearInventory = false;
                }
            }

        }
        RefreshUI();
        return;
    }

    void RefreshUI()
    {
        for (int i = inventory.transform.childCount - 1; i >= 0; i--)
            Destroy(inventory.transform.GetChild(i).gameObject);

        int count = Mathf.Min(inventoryList.Count, capacity);
        for (int i = 0; i < count; i++)
        {
            var it = inventoryList[i];
            var go = Instantiate(InventoryItem, inventory.transform);
            var rt = go.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = iconSize;
            rt.anchoredPosition = new Vector2(i * (iconSize.x + iconSpacing), 0f);

            var img = go.GetComponentInChildren<UnityEngine.UI.Image>();
            if (img != null) img.sprite = it.sprite;
        }
    }

}
