using KSP.Localization;
using System;

namespace OreToParts
{
    public static class InventoryHelper
    {
        public static bool RealStoreCargoPartAtSlot(ModuleInventoryPart inventory, string partName, int inventorySlot)
        {
            if (string.IsNullOrEmpty(partName))
            {
                return false;
            }
            if (inventory == null)
            {
                return false;
            }
            int availableInventorySlot = -1;

            if (inventorySlot == -1)
            {
                // find first available inventory slot
                for (int i = 0; i < inventory.InventorySlots; i++)
                {
                    if (!inventory.storedParts.ContainsKey(i))
                    {
                        MyDebug("Found undefined empty slot : " + i);

                        availableInventorySlot = i;
                        break;
                    }
                    StoredPart myStoredPart = inventory.storedParts[i];
                    if (myStoredPart != null && myStoredPart.IsEmpty)
                    {
                        MyDebug("Found empty slot : " + i);

                        availableInventorySlot = i;
                        break;
                    }
                    if (myStoredPart != null && myStoredPart.partName.Equals(partName) && myStoredPart.CanStack && myStoredPart.stackCapacity > myStoredPart.quantity)
                    {
                        MyDebug("Found stackable slot : " + i);
                        availableInventorySlot = i;
                        break;
                    }
                }
            }
            else
            {
                if (inventory.storedParts.ContainsKey(inventorySlot) && inventory.storedParts[inventorySlot].IsFull)
                {
                    return false;
                }
                availableInventorySlot = inventorySlot;
            }
            if (availableInventorySlot == -1)
            {
                MyDebug("No slot found!");
                return false;
            }
            try
            {
                if (!inventory.storedParts.ContainsKey(availableInventorySlot) || !inventory.storedParts[availableInventorySlot].CanStack)
                {
                    MyDebug("Full add in " + availableInventorySlot);
                    bool succes = inventory.StoreCargoPartAtSlot(partName, availableInventorySlot);
                    return succes;
                }
                else
                {
                    MyDebug("Quantity add in " + availableInventorySlot);
                    return inventory.UpdateStackAmountAtSlot(availableInventorySlot, inventory.storedParts[availableInventorySlot].quantity + 1);
                }
            }
            catch (Exception e)
            {
                MyDebug("Cannot augment quantity " + partName + " " + e.Message);
                return false;
            }
        }

        private static void MyDebug(string txt)
        {
            UnityEngine.MonoBehaviour.print("[OreToParts]DEBUG -- " + txt);
        }

        public static string CheckMassVolumeInventory(ModuleInventoryPart inventory, string craftPart)
        {
            if (inventory.HasMassLimit && inventory.massCapacity + PartHelper.PartMass(craftPart) > inventory.massLimit + 0.00001f)
            {
                return Localizer.Format("#oretotanks_maxmass", new object[] { PartHelper.PartDisplayName(craftPart) });
            }
            if (inventory.HasPackedVolumeLimit && inventory.volumeCapacity + PartHelper.PartVolume(craftPart) > inventory.packedVolumeLimit + 0.00001f)
            {
                return Localizer.Format("#oretotanks_maxvolume", new object[] { PartHelper.PartDisplayName(craftPart) });
            }
            return "";
        }
    }

}
