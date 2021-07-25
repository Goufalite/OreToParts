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
        [KSPField(guiActive = true, guiActiveUnfocused = true, guiName = " ", groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public string placeholder1;

        [KSPField(guiActive = true, guiActiveUnfocused = true, groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        [UI_ChooseOption(scene = UI_Scene.Flight)]
        public string craftPart;

        [KSPField(guiActive = true, guiActiveUnfocused = true, guiName = "  ", groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public string placeholder2;

        [KSPField(guiActive = true, guiActiveUnfocused = true, guiName = "#oretotanks_costpart", groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public string displayCraftCost;

        [KSPField(guiActive = true, guiActiveUnfocused = true, guiName = "#oretotanks_duplicatecost", groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public string duplicateCost;

        [KSPField(guiActive = false)]
        public string partList;

        [KSPField(guiActive = false)]
        public string resources;

        // cache for the craft part name when selected by the slider
        public string craftPartName;

        // cache for the resource cost
        public Dictionary<string, float> craftResourcesDict;

        public override void OnAwake()
        {
            base.OnAwake();

            ParsePartList();

            craftResourcesDict = UtilitiesHelper.ParseResources(resources,"Ore",1.0f);
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

        private void ParsePartList()
        {
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
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_unknownpart", new object[] { craftPartName }));
                return;
            }
            if (!ResearchAndDevelopment.PartModelPurchased(ap))
            {
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_researchpart", new object[] { craftPartName }));
                return;
            }

            //craftable?
            try
            {
                if (PartHelper.PartVolume(craftPart) == -1.0f)
                {
                    // movable part but cannot be stored
                    UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_noninventorypart", new object[] { craftPartName }));
                    return;
                }
            }
            catch
            {
                // construction part, cannot be moved
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_noninventorypart", new object[] { craftPartName }));
                return;
            }

            // price
            string canAffordPart = PartHelper.CanAfford(part, craftPart, craftResourcesDict);
            if (!string.IsNullOrEmpty(canAffordPart))
            {
                UtilitiesHelper.PrintMessage(canAffordPart);
                return;
            }

            // volume/mass storage
            string checkMassVolumeInventory = InventoryHelper.CheckMassVolumeInventory(inventory, craftPart);
            if (!string.IsNullOrEmpty(checkMassVolumeInventory))
            {
                UtilitiesHelper.PrintMessage(checkMassVolumeInventory);
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
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_noinventory"));
                return;
            }
            
            if (!inventory.storedParts.ContainsKey(0)|| inventory.storedParts[0].IsEmpty)
            {
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_missingduplicatepart"));
                return;
            }
            StoredPart firstPart = inventory.storedParts[0];

            // price
            string canAffordPart = PartHelper.CanAfford(part, firstPart.partName, craftResourcesDict);
            if (!string.IsNullOrEmpty(canAffordPart))
            {
                UtilitiesHelper.PrintMessage(canAffordPart);
                return;
            }

            // check mass/volume
            string checkMassVolumeInventory = InventoryHelper.CheckMassVolumeInventory(inventory, firstPart.partName);
            if (!string.IsNullOrEmpty(checkMassVolumeInventory))
            {
                UtilitiesHelper.PrintMessage(checkMassVolumeInventory);
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
                    UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_cannotstore", new object[] { PartHelper.PartDisplayName(partName) }));
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
                MyDebug("slot " + i + " partname:" + inventory.storedParts[i].snapshot?.partName ?? "?");
                //MyDebug("slot " + i + " partress:" + inventory.storedParts[i].snapshot?.resources?[0].amount ?? "?");
            }
        }

        private static void MyDebug(string txt)
        {
            print("[OreToParts]DEBUG -- " + txt);
        }

        #endregion
    }
}
