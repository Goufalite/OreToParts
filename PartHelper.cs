using KSP.Localization;
using System;
using System.Collections.Generic;

namespace OreToParts
{
    public static class PartHelper
    {
        public enum ScrapMode
        {
            always,
            partial,
            block
        }

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

        public static bool CanScrap(Part part, string partName, Dictionary<string, float> resourcesDict, ScrapMode scrapMode)
        {
            if (scrapMode == ScrapMode.always)
            {
                return true;
            }

            float mass = PartMass(partName);
            bool partialResourceFound = false;
            foreach (var ress in resourcesDict)
            {
                double price = Math.Round(ress.Value * mass * 1000, 2);
                if (!part.Resources.Contains(ress.Key) && scrapMode == ScrapMode.block)
                {
                    if (scrapMode == ScrapMode.block)
                    {
                        // no container for resource
                        return false;
                    }
                }
                if (part.Resources[ress.Key].amount + price > part.Resources[ress.Key].maxAmount + 0.0001f)
                {
                    if (scrapMode == ScrapMode.block)
                    {
                        return false;
                    }
                }
                else
                {
                    // there is at least one ressource available
                    partialResourceFound = true;

                }
            }

            // block mode should have detected an inexistant/full resource
            return partialResourceFound;
        }

        public static void ProduceOre(Part part, string partName, Dictionary<string, float> resourcesDict)
        {
            float mass = PartMass(partName);
            foreach (var ress in resourcesDict)
            {
                if (!part.Resources.Contains(ress.Key))
                {
                    continue;
                }
                part.Resources[ress.Key].amount = Math.Min(Math.Round(ress.Value * mass * 1000, 2) + part.Resources[ress.Key].amount, part.Resources[ress.Key].maxAmount);
            }
        }

        public static double MaxAfford(Part part, double requiredAmount, Dictionary<string, float> resourcesDict)
        {
            double returnAmount = requiredAmount;
            foreach(var res in resourcesDict)
            {
                if (!part.Resources.Contains(res.Key))
                {
                    return 0.0f;
                }
                returnAmount = Math.Min(returnAmount, (float)part.Resources[res.Key].amount / res.Value);
            }
            return returnAmount;
        }

        #endregion
    }
}
