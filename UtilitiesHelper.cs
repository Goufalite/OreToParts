using System;
using System.Collections.Generic;
using System.Linq;

namespace OreToParts
{
    public static class UtilitiesHelper
    {
        public static ConfigNode GetPartInfo(Part part, string moduleName)
        {
            if (part?.partInfo != null)
            {
                return GameDatabase.Instance.GetConfigs("PART")
                                .Single(c => part.partInfo.name == c.name.Replace('_', '.'))
                                .config.GetNodes("MODULE")
                                .Single(n => n.GetValue("name") == moduleName);
            }
            else
            {
                return null;
            }
        }
        
        public static Dictionary<string, float> ParseResources(Part part, string moduleName, string defaultResource, float defaultRatio)
        {
            Dictionary<string, float> resourcesDict = new Dictionary<string, float>();
            ConfigNode node;
            try
            {
                node = null;
                if (part.partInfo != null)
                {
                    node =
                        GameDatabase.Instance.GetConfigs("PART")
                                    .Single(c => part.partInfo.name == c.name.Replace('_', '.'))
                                    .config.GetNodes("MODULE")
                                    .Single(n => n.GetValue("name") == moduleName);
                }
                if (node == null)
                {
                    throw new Exception("node is null !?");
                }
                var nodesResource = node.GetNode("RESOURCESLIST");
                if (nodesResource == null)
                {
                    throw new Exception("No RESOURCESLIST node");
                }
                var resources = nodesResource.GetNodes("RESOURCE_NEEDED");
                UnityEngine.MonoBehaviour.print("[OreToParts] Found resources : " + resources.Length);

                for (int i = 0; i < resources.Length; i++)
                {
                    resourcesDict.Add(resources[i].GetValue("name"), float.Parse(resources[i].GetValue("ratio")));
                }
            }
            catch (Exception e)
            {
                UnityEngine.MonoBehaviour.print("[OreToParts] Unable to load resources : " + e.Message);
                resourcesDict.Add(defaultResource, defaultRatio);
            }
            return resourcesDict;
        }

        public static void PrintMessage(string message)
        {
            ScreenMessages.PostScreenMessage(message, 2.0f, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
