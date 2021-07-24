using System;
using System.Collections.Generic;

namespace OreToParts
{
    public static class UtilitiesHelper
    {
        public static Dictionary<string, float> ParseResources(string res, string defaultResource, float defaultQuantity)
        {
            Dictionary<string, float> resourcesDict = new Dictionary<string, float>();
            try
            {
                var ressTab = res.Split(',');
                foreach (var ress in ressTab)
                {
                    var ratioPart = ress.Split('|');
                    resourcesDict.Add(ratioPart[0].Trim(), float.Parse(ratioPart[1]));
                }
            }
            catch (Exception e)
            {
                UnityEngine.MonoBehaviour.print("[OreToParts]Unable to load resource list: " + e.Message);
                resourcesDict.Add(defaultResource, defaultQuantity);
            }
            return resourcesDict;
        }

        public static void PrintMessage(string message)
        {
            ScreenMessages.PostScreenMessage(message, 2.0f, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
