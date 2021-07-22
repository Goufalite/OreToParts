using KSP.Localization;
using System;
using System.Collections.Generic;

namespace OreToParts
{
    public static class PartHelper
    {
        #region Part info
        public static float PartMass(string partName)
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

        public static float PartVolume(string partName)
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

        public static string PartDisplayName(string partName)
        {
            AvailablePart availablePart = PartLoader.getPartInfoByName(partName);
            if (availablePart == null)
            {
                return "??" + partName + "??";
            }
            return availablePart.title;
        }
        #endregion

        #region Resources
        public static string DisplayPartCost(string partName, Dictionary<string, float> resourcesDict)
        {
            float mass = PartHelper.PartMass(partName);
            string price = "";
            foreach (var ress in resourcesDict)
            {
                price += ress.Key + " (" + Math.Round(ress.Value * mass * 1000, 2) + ") ";
            }
            return price;
        }

        public static void ConsumeOre(Part part, string partName, Dictionary<string, float> resourcesDict)
        {
            float mass = PartMass(partName);
            foreach (var ress in resourcesDict)
            {
                part.Resources[ress.Key].amount -= Math.Round(ress.Value * mass * 1000, 2);
            }
        }

        public static string CanAfford(Part part, string partName, Dictionary<string, float> resourcesDict)
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
        #endregion
    }
}
