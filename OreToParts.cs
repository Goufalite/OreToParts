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

        public Dictionary<string, float> resourcesDict;

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
                    craftPartName.Add(PartDisplayName(lPartList[i].Trim()));
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
            resourcesDict = new Dictionary<string, float>();
            try
            {
                var ressTab = resources.Split(',');
                foreach (var ress in ressTab)
                {
                    var ratioPart = ress.Split('|');
                    resourcesDict.Add(ratioPart[0].Trim(), float.Parse(ratioPart[1]));
                }
            }
            catch (Exception e)
            {
                print("[OreToParts]Unable to load resource list: " + e.Message);
                resourcesDict.Add("Ore", 1.0f);
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
                duplicateCost = DisplayPartCost(PartMass(inventory.storedParts[0].partName));
            }
            else
            {
                duplicateCost = "?";
            }
            craftPartName = PartDisplayName(craftPart);
            displayCraftCost = DisplayPartCost(PartMass(craftPart));
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
                volPart = PartVolume(craftPart);
                if (volPart==-1.0f)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_noninventorypart", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
            }
            catch
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_noninventorypart", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            // price
            string canAffordPart = CanAfford(craftPart);
            if (!string.IsNullOrEmpty(canAffordPart))
            {
                ScreenMessages.PostScreenMessage(canAffordPart, 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            if (inventory.HasMassLimit && inventory.massCapacity + PartMass(craftPart) > inventory.massLimit + 0.00001f)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_maxmass", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if (inventory.HasPackedVolumeLimit && inventory.volumeCapacity + volPart > inventory.packedVolumeLimit + 0.00001f)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_maxvolume", new object[] { craftPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
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
            var myPartName = PartDisplayName(firstPart.partName);
            
            // price
            string canAffordPart = CanAfford(firstPart.partName);
            if (!string.IsNullOrEmpty(canAffordPart))
            {
                ScreenMessages.PostScreenMessage(canAffordPart, 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //check volume, since the part is duplicated it has a volume
            if (inventory.HasMassLimit && inventory.massCapacity+PartMass(firstPart.partName) > inventory.massLimit + 0.00001f)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_maxmass", new object[] { myPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if (inventory.HasPackedVolumeLimit && inventory.volumeCapacity + PartVolume(firstPart.partName) > inventory.packedVolumeLimit + 0.00001f)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_maxvolume", new object[] { myPartName }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            StoreMyPart(inventory, firstPart.partName);
        }

        private void StoreMyPart(ModuleInventoryPart inventory, string partName)
        {
            try
            {
                bool succes = RealStoreCargoPartAtSlot(inventory, partName, -1);
                if (!succes)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#oretotanks_cannotstore", new object[] { PartDisplayName(partName) }), 2.0f, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
                ConsumeOre(partName);
            }
            catch (Exception e)
            {
                print("[OreToParts]Cannot craft part " + craftPart + " " + e.Message);
            }
        }
        #region debug

        private static void myDebug(string txt)
        {
            print("[OreToParts]DEBUG -- " + txt);
        }

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
                    myDebug("slot" + i + " - undefined");
                    continue;
                }
                myDebug("slot " + i + " " + inventory.storedParts[i].ToString());
                myDebug("slot " + i + " part:" + inventory.storedParts[i].partName);
                myDebug("slot " + i + " variant:" + inventory.storedParts[i].variantName);
                myDebug("slot " + i + " full:" + inventory.storedParts[i].IsFull.ToString());
                myDebug("slot " + i + " canstack:" + inventory.storedParts[i].CanStack.ToString());
                myDebug("slot " + i + " empty:" + inventory.storedParts[i].IsEmpty.ToString());
                myDebug("slot " + i + " quantity:" + inventory.storedParts[i].quantity);
                
            }
        }
        #endregion
        #region helpers
        private static float PartMass(string partName)
        {
            AvailablePart availablePart = PartLoader.getPartInfoByName(partName);
            if (availablePart == null)
            {
                throw new Exception("Unknown part " + partName);
            }
            float mass = 0.0f;
            bool getvalue = availablePart.partConfig.TryGetValue("mass", ref mass);
            if (!getvalue)
            {
                throw new Exception("No mass found " + partName);
            }
            return mass;
        }

        private static string PartDisplayName(string partName)
        {
            AvailablePart availablePart = PartLoader.getPartInfoByName(partName);
            if (availablePart == null)
            {
                print("Unknown part " + partName);
                return "??" + partName + "??";
            }
            return availablePart.title;
        }

        private static float PartVolume(string partName)
        {
            AvailablePart availablePart = PartLoader.getPartInfoByName(partName);
            if (availablePart == null)
            {
                throw new Exception("Unknown part " + partName);
            }
            float volume = 0.0f;
            bool getvalue = false;
            foreach (ConfigNode module in availablePart.partConfig.GetNodes())
            {
                string name = "";
                module.TryGetValue("name", ref name);
                if (!name.Equals("ModuleCargoPart"))
                {
                    continue;
                }
                getvalue = module.TryGetValue("packedVolume", ref volume);
                break;

            }

            if (!getvalue)
            {
                throw new Exception("No volume found " + partName);
            }
            return volume;
        }

        private string CanAfford(string partName)
        {
            float mass = PartMass(partName);
            foreach (var ress in resourcesDict)
            {
                double price = Math.Round(ress.Value * mass * 1000, 2);
                if (!part.Resources.Contains(ress.Key))
                {
                    return Localizer.Format("#oretotanks_cannotafford", new object[] { ress.Key, price });
                }
                if (part.Resources[ress.Key].amount < price)
                {
                    return Localizer.Format("#oretotanks_cannotafford", new object[] { ress.Key, price });
                }
            }
            return "";
        }

        private void ConsumeOre(string partName)
        {
            float mass = PartMass(partName);
            foreach (var ress in resourcesDict)
            { 
                part.Resources[ress.Key].amount -= Math.Round(ress.Value * mass * 1000, 2);
            }
        }

        private string DisplayPartCost(float mass)
        {
            string price = "";
            foreach(var ress in this.resourcesDict)
            {
                price += ress.Key + " (" + Math.Round(ress.Value * mass * 1000,2) + ") ";
            }
            return price;
        }

        private static bool RealStoreCargoPartAtSlot(ModuleInventoryPart inventory, string partName, int inventorySlot)
        {
            if (String.IsNullOrEmpty(partName))
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
                        myDebug("Found undefined empty slot : " + i);

                        availableInventorySlot = i;
                        break;
                    }
                    StoredPart myStoredPart = inventory.storedParts[i];
                    if (myStoredPart != null && myStoredPart.IsEmpty)
                    {
                        myDebug("Found empty slot : " + i);

                        availableInventorySlot = i;
                        break;
                    }
                    if (myStoredPart != null && myStoredPart.partName.Equals(partName) && myStoredPart.CanStack && myStoredPart.stackCapacity > myStoredPart.quantity)
                    {
                        myDebug("Found stackable slot : " + i);
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
                myDebug("No slot found!");
                return false;
            }
            try
            {
                if (!inventory.storedParts.ContainsKey(availableInventorySlot) || !inventory.storedParts[availableInventorySlot].CanStack)
                {
                    myDebug("Full add in " + availableInventorySlot);
                    bool succes = inventory.StoreCargoPartAtSlot(partName, availableInventorySlot);
                    return succes;
                }
                else
                {
                    myDebug("Quantity add in " + availableInventorySlot);
                    return inventory.UpdateStackAmountAtSlot(availableInventorySlot, inventory.storedParts[availableInventorySlot].quantity+1);
                }

            }
            catch (Exception e)
            {
                print("[OreToParts]Cannot augment quantity " + partName + " " + e.Message);
                return false;
            }

        }
        #endregion
    }
}
