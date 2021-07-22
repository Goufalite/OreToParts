using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSP.Localization;

namespace OreToParts
{
    public class OreToParts : PartModule
    {
        [KSPField(guiActive = true, guiActiveUnfocused = true, groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        [UI_ChooseOption(scene = UI_Scene.Flight)]
        public string craftPart;

        public string craftPartName;

        [KSPField(guiActive = true, guiActiveUnfocused = true, guiName = "#oretotanks_costpart", groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public string displayCraftCost;

        public int craftCost;

        [KSPField(guiActive = false)]
        public string partList;

        [KSPField(guiActive = true, guiActiveUnfocused = true, guiName = "#oretotanks_duplicatecost", groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public string duplicateCost;

        [KSPField(guiActive = false)]
        public string resources;

        public Dictionary<string, float> craftResourcesDict;

        public override void OnAwake()
        {
            base.OnAwake();

            // partlist parsing
            var crafts = new List<string>();
            try
            {
                var lPartList = partList.Split(',');
                var craftPartName = new List<String>();
        
                for (int i = 0; i < lPartList.Length; i++)
                {
                    crafts.Add(lPartList[i].Trim());
                    craftPartName.Add(PartHelper.PartDisplayName(lPartList[i].Trim()));
                }
                var uiparts = (UI_ChooseOption)base.Fields["craftPart"].uiControlFlight;
                uiparts.options = crafts.ToArray();
                uiparts.display = craftPartName.ToArray();
                craftPart = uiparts.options[0];

            }
            catch (Exception e)
            {
                print("[OreToParts]Unable to load part list to craft: " + e.Message);
                crafts = new List<string>() { "evaRepairKit" };
                var uiparts = (UI_ChooseOption)base.Fields["craftPart"].uiControlFlight;
                uiparts.options = crafts.ToArray();
                uiparts.display = new string[] { "Repair kit" };
                craftPart = uiparts.options[0];
            }

            // resource parsing
            craftResourcesDict = new Dictionary<string, float>();
            try
            {
                var ressTab = resources.Split(',');
                foreach (var ress in ressTab)
                {
                    var ratioPart = ress.Split('|');
                    craftResourcesDict.Add(ratioPart[0].Trim(), float.Parse(ratioPart[1]));
                }
            }
            catch (Exception e)
            {
                print("[OreToParts]Unable to load resource list: " + e.Message);
                craftResourcesDict.Add("Ore", 1.0f);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
            if (inventory == null)
            {
                return;
            }
            if (inventory.storedParts.ContainsKey(0))
            {
                duplicateCost = PartHelper.DisplayPartCost(inventory.storedParts[0].partName, craftResourcesDict);
            }
            else
            {
                duplicateCost = "?";
            }
            craftPartName = PartHelper.PartDisplayName(craftPart);
            if (craftPartName.StartsWith("??"))
            {
                displayCraftCost = "??";
            }
            else
            {
                displayCraftCost = PartHelper.DisplayPartCost(craftPart, craftResourcesDict);
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }

        [KSPEvent(guiName = "#oretotanks_craftpart", guiActiveEditor = false, guiActive = true, externalToEVAOnly = true, guiActiveUnfocused = true, groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public void CraftPart()
        {
            var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
            if (inventory == null)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_noinventory"), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            // existence
            AvailablePart ap = PartLoader.getPartInfoByName(craftPart);
            if (ap == null)
            {
                print("Unknown part " + craftPart);
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_unknownpart", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if (!ResearchAndDevelopment.PartModelPurchased(ap))
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_researchpart", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //craftable?
            float volPart;
            try
            {
                volPart = PartHelper.PartVolume(craftPart);
                if (volPart == -1.0f)
                {
                    // movable part but cannot be stored
                    ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_noninventorypart", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
            }
            catch
            {
                // construction part, cannot be moved
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_noninventorypart", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            // price
            string canAffordPart = PartHelper.CanAfford(part, craftPart, craftResourcesDict);
            if (!string.IsNullOrEmpty(canAffordPart))
            {
                ScreenMessages.PostScreenMessage(canAffordPart, 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            // volume/mass storage
            string checkMassVolumeInventory = InventoryHelper.CheckMassVolumeInventory(inventory, craftPart);
            if (!string.IsNullOrEmpty(checkMassVolumeInventory))
            {
                ScreenMessages.PostScreenMessage(checkMassVolumeInventory, 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            
            StoreMyPart(inventory, craftPart);
        }

        [KSPEvent(guiName = "#oretotanks_duplicatepart", guiActiveEditor = false, guiActive = true, externalToEVAOnly = true, guiActiveUnfocused = true, groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public void DuplicatePart()
        {
            var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
            if (inventory == null)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_noinventory"), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            
            if (!inventory.storedParts.ContainsKey(0)|| inventory.storedParts[0].IsEmpty)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_missingduplicatepart"), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            StoredPart firstPart = inventory.storedParts[0];

            // price
            string canAffordPart = PartHelper.CanAfford(part, firstPart.partName, craftResourcesDict);
            if (!string.IsNullOrEmpty(canAffordPart))
            {
                ScreenMessages.PostScreenMessage(canAffordPart, 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            // check mass/volume
            string checkMassVolumeInventory = InventoryHelper.CheckMassVolumeInventory(inventory, firstPart.partName);
            if (!string.IsNullOrEmpty(checkMassVolumeInventory))
            {
                ScreenMessages.PostScreenMessage(checkMassVolumeInventory, 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            StoreMyPart(inventory, firstPart.partName);
        }

        private void StoreMyPart(ModuleInventoryPart inventory, string partName)
        {
            try
            {
                bool succes = InventoryHelper.RealStoreCargoPartAtSlot(inventory, partName, -1);
                if (!succes)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_cannotstore", new object[] { PartHelper.PartDisplayName(partName) }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
                PartHelper.ConsumeOre(part, partName, craftResourcesDict);
            }
            catch (Exception e)
            {
                print("[OreToParts]Cannot craft part " + craftPart + " " + e.Message);
            }
        }
        #region debug
#if DEBUG
        [KSPEvent(guiName = "Debug", guiActive = true, groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
#endif
        public void DebugOreToParts()
        {
            var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
            if (inventory == null)
            {
                ScreenMessages.PostScreenMessage("No inventory!", 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            for (int i=0; i< inventory.InventorySlots; i++)
            {
                if (!inventory.storedParts.ContainsKey(i))
                {
                    MyDebug("slot" + i + " - undefined");
                    continue;
                }
                MyDebug("slot " + i + " " + inventory.storedParts[i].ToString());
                MyDebug("slot " + i + " part:" + inventory.storedParts[i].partName);
                MyDebug("slot " + i + " variant:" + inventory.storedParts[i].variantName);
                MyDebug("slot " + i + " full:" + inventory.storedParts[i].IsFull.ToString());
                MyDebug("slot " + i + " canstack:" + inventory.storedParts[i].CanStack.ToString());
                MyDebug("slot " + i + " empty:" + inventory.storedParts[i].IsEmpty.ToString());
                MyDebug("slot " + i + " quantity:" + inventory.storedParts[i].quantity);
                MyDebug("slot " + i + " partpart:" + inventory.storedParts[i].snapshot.partRef.partName);
                MyDebug("slot " + i + " partpart:" + inventory.storedParts[i].snapshot.partRef.Resources["EVA Propellant"].amount);
            }
        }

        private static void MyDebug(string txt)
        {
            print("[OreToParts]DEBUG -- " + txt);
        }

        #endregion
    }
}
