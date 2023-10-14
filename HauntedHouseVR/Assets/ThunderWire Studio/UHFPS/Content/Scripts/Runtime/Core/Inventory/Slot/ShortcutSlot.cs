using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UHFPS.Runtime
{
    public class ShortcutSlot : MonoBehaviour
    {
        public GameObject ItemPanel;

        [Header("References")]
        public CanvasGroup FadePanel;
        public Image Background;
        public Image ItemIcon;
        public TMP_Text Quantity;

        [Header("Slot Colors")]
        public Color EmptySlotColor;
        public Color NormalSlotColor;

        private InventoryItem inventoryItem;
        private Inventory inventory;

        public void SetItem(InventoryItem inventoryItem)
        {
            this.inventoryItem = inventoryItem;

            if(inventoryItem != null)
            {
                inventory = inventoryItem.Inventory;
                Item item = inventoryItem.Item;

                // icon orientation and scaling
                Vector2 slotSize = ItemIcon.rectTransform.rect.size;
                slotSize -= new Vector2(10, 10);
                Vector2 iconSize = item.Icon.rect.size;

                Vector2 scaleRatio = slotSize / iconSize;
                float scaleFactor = Mathf.Min(scaleRatio.x, scaleRatio.y);

                ItemIcon.sprite = item.Icon;
                ItemIcon.rectTransform.sizeDelta = iconSize * scaleFactor;
                Quantity.text = inventoryItem.Quantity.ToString();

                Background.color = NormalSlotColor;
                FadePanel.alpha = 1f;
                ItemPanel.SetActive(true);
            }
            else
            {
                ItemIcon.sprite = null;
                Quantity.text = string.Empty;

                Background.color = EmptySlotColor;
                FadePanel.alpha = 0.5f;
                ItemPanel.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateItemQuantity();
        }

        private void UpdateItemQuantity()
        {
            if (inventoryItem == null)
                return;

            int itemQuantity = inventoryItem.Quantity;

            if (!inventoryItem.Item.Settings.alwaysShowQuantity)
            {
                if (itemQuantity > 1) 
                    Quantity.text = inventoryItem.Quantity.ToString();
                else Quantity.text = string.Empty;
            }
            else
            {
                Quantity.text = itemQuantity.ToString();
                Quantity.color = itemQuantity >= 1
                    ? inventory.slotSettings.normalQuantityColor
                    : inventory.slotSettings.zeroQuantityColor;
            }
        }
    }
}