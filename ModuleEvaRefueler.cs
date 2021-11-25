using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OreToParts
{
    public class ModuleEvaRefueler : PartModule
    {
        [KSPField(isPersistant = false, guiActive = false)]
        public string sourceResource;

        /// <summary>
        /// This field is a little tricky: since a bug  affects the fuel of jetpacks in inventories, sometimes players need to roleplay a full fuel usage even if the jetpack is half full.
        /// If true, the total amount of the EVA part will be taken from the RESOURCE_NEEDED, not the needed fuel
        /// Bug : https://forum.kerbalspaceprogram.com/index.php?/topic/203705-11121121ore-to-parts-craft-parts-with-ore-duplicate-parts/&do=findComment&comment=4055256
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false)]
        public bool forceRefuel;

        // cache for the resource cost
        public Dictionary<string, float> refuelResourcesDict;

        public override void OnAwake()
        {
            base.OnAwake();

            //resetting properties
            var info = UtilitiesHelper.GetPartInfo(this.part, this.ClassName);
            if (info != null)
            {
                sourceResource = info.GetValue("sourceResource");
            }

            refuelResourcesDict = UtilitiesHelper.ParseResources(part, this.GetType().Name, "MonoPropellant", 1.0f);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
            if (inventory == null)
            {
                return;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }

        [KSPEvent(guiName = "#oretotanks_refueleva", guiActiveEditor = false, guiActive = true, externalToEVAOnly = true, guiActiveUnfocused = true, groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public void Refuel()
        {
            try
            {
                var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
                if (inventory == null)
                {
                    return;
                }

                MyDebug("Refueling inventory");
                RefillInventory(inventory);

                if (part.protoModuleCrew != null)
                {
                    foreach (var kerbal in part.protoModuleCrew)
                    {
                        MyDebug("Refueling " + kerbal.displayName);
                        if (kerbal.KerbalInventoryModule != null)
                        {
                            RefillInventory(kerbal.KerbalInventoryModule);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                print("[OreToParts] Cannot refuel EVA part : " + ex.Message);
            }
        }

        private void RefillInventory(ModuleInventoryPart inventory)
        {
            for (int i = 0; i < inventory.InventorySlots; i++)
            {
                MyDebug("Slot "+i);
                if (!inventory.storedParts.ContainsKey(i))
                {
                    MyDebug("Empty slot");
                    continue;
                }
                if (inventory.storedParts[i].snapshot?.resources != null)
                {
                    var myPart = inventory.storedParts[i].snapshot;

                    try
                    {
                        var resIndex = myPart.resources.FirstOrDefault(a => a.resourceName.Equals(sourceResource));

                        if (resIndex == null)
                        {
                            MyDebug("No source resource "+sourceResource+" found to refuel EVA");
                            continue;
                        }
                        if (resIndex.amount == resIndex.maxAmount && !forceRefuel)
                        {
                            MyDebug("Full EVA part, no need to refuel.");
                            continue;
                        }

                        // store
                        double fuelNeeded = forceRefuel ? resIndex.maxAmount : resIndex.maxAmount - resIndex.amount;
                        double realFuelNeeded = PartHelper.MaxAfford(part, fuelNeeded, refuelResourcesDict);
                        
                        MyDebug("Fuel needed = " + fuelNeeded + " Affordable fuel = " + realFuelNeeded);
                        if (fuelNeeded != realFuelNeeded)
                        {
                            UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_refuelevaempty"));
                            if (forceRefuel)
                            {
                                // partial refuel is forbidden in this case
                                continue;
                            }
                        }

                        foreach (var res in refuelResourcesDict)
                        {
                            double resourceNeeded = realFuelNeeded * res.Value;
                            MyDebug("Removing "+resourceNeeded+" with "+res.Key);
                            part.Resources[res.Key].amount -= resourceNeeded;
                        }
                        
                        resIndex.amount = forceRefuel ? resIndex.maxAmount : resIndex.amount + realFuelNeeded;
                        MyDebug("EVA Part is now "+resIndex.amount);
                        resIndex.UpdateConfigNodeAmounts();
                        GameEvents.onModuleInventoryChanged.Fire(inventory);

                    }
                    catch (Exception e)
                    {
                        print("[OreToParts] Cannot refuel part at slot " + i + " : " + e.Message);
                    }
                }
                else
                {
                    MyDebug("Part doesn't have resources");
                }
            }
        }
        private static void MyDebug(string txt)
        {
#if DEBUG
            print("[OreToParts] DEBUG -- " + txt);
#endif
        }
    }
}
