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
        [KSPField(guiActive = false)]
        public string sourceResource;

        // cache for the resource cost
        public Dictionary<string, float> refuelResourcesDict;

        public override void OnAwake()
        {
            base.OnAwake();

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
            var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
            if (inventory == null)
            {
                return;
            }

            RefillInventory(inventory);

            if (part.protoModuleCrew != null)
            {
                foreach (var kerbal in part.protoModuleCrew)
                {
                    if (kerbal.KerbalInventoryModule == null)
                    {
                        continue;
                    }
                    RefillInventory(kerbal.KerbalInventoryModule);
                }
            }
        }

        private void RefillInventory(ModuleInventoryPart inventory)
        {
            for (int i = 0; i < inventory.InventorySlots; i++)
            {
                if (!inventory.storedParts.ContainsKey(i))
                {
                    // empty slot
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
                            continue;
                        }
                        if (resIndex.amount == resIndex.maxAmount)
                        {
                            continue;
                        }

                        // store
                        double fuelNeeded = resIndex.maxAmount - resIndex.amount;
                        double realFuelNeeded = PartHelper.MaxAfford(part, fuelNeeded, refuelResourcesDict);
                        
                        if (fuelNeeded != realFuelNeeded)
                        {
                           UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_refuelevaempty"));
                        }

                        foreach (var res in refuelResourcesDict)
                        {
                            double resourceNeeded = realFuelNeeded * res.Value;
                            part.Resources[res.Key].amount -= resourceNeeded;
                        }
                        resIndex.amount += realFuelNeeded;
                        resIndex.UpdateConfigNodeAmounts();
                        GameEvents.onModuleInventoryChanged.Fire(inventory);

                    }
                    catch (Exception e)
                    {
                        print("[OreToParts] Cannot refuel part at slot " + i + " : " + e.Message);
                    }
                }
            }
        }
    }
}
