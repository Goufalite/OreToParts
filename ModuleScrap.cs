using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OreToParts
{
    public class ModuleScrap : PartModule
    {
        [KSPField(guiActive = false)]
        public string partList;

        [KSPField(guiActive = false)]
        public string resources;

        [KSPField(guiActive = false)]
        public float scrapRatio;

        [KSPField(guiActive = false)]
        public string scrapMode;

        public PartHelper.ScrapMode scrapModeEnum;

        // cache for the scrap cost
        public Dictionary<string, float> scrapResourcesDict;

        public override void OnAwake()
        {
            base.OnAwake();

            scrapResourcesDict = UtilitiesHelper.ParseResources(resources,"Ore",0.5f);

            // scrap mode
            switch (scrapMode)
            {
                case "always":
                    scrapModeEnum = PartHelper.ScrapMode.always;
                    break;
                case "block":
                    scrapModeEnum = PartHelper.ScrapMode.block;
                    break;
                case "partial":
                default:
                    scrapModeEnum = PartHelper.ScrapMode.partial;
                    break;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }

        [KSPEvent(guiName = "#oretotanks_scrappart", guiActiveEditor = false, guiActive = true, externalToEVAOnly = true, guiActiveUnfocused = true, groupName = "craftParts", groupDisplayName = "#oretotanks_groupname")]
        public void ScrapPart()
        {
            var inventory = part.Modules.OfType<ModuleInventoryPart>().SingleOrDefault();
            if (inventory == null)
            {
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_noinventory"));
                return;
            }

            if (!inventory.storedParts.ContainsKey(0) || inventory.storedParts[0].IsEmpty)
            {
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_missingduplicatepart"));
                return;
            }
            StoredPart firstPart = inventory.storedParts[0];

            // price
            if (!PartHelper.CanScrap(part, firstPart.partName, scrapResourcesDict, scrapModeEnum))
            {
                UtilitiesHelper.PrintMessage(Localizer.Format("#oretotanks_cannotscrap", new object[] { PartHelper.PartDisplayName(firstPart.partName) }));
                return;
            }

            // produce ore
            PartHelper.ProduceOre(part, firstPart.partName, scrapResourcesDict);

            // remove part
            inventory.RemoveNPartsFromInventory(firstPart.partName, 1);
            GameEvents.onModuleInventoryChanged.Fire(inventory);
        }
    }
}
