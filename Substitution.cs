using System.Data;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

public class Substitution {

   private IDictionary<string, HtmlNode> myComponentProcessedDefinitions = new Dictionary<string, HtmlNode>();    

    public void Process(IDictionary<string, HtmlDocument> htmlDocumentsForProcessing)
    {
        IDictionary<string, HtmlNode> componentDefinitions = new Dictionary<string, HtmlNode>();
        
        NodeSelectionToDictionary(htmlDocumentsForProcessing, "data-cmp-definition", componentDefinitions);

        ProcessComponentsRecursive(componentDefinitions);

        ProcessInjections(GetNodesFromDocuments(htmlDocumentsForProcessing, "data-cmp-inject"));
    }

    private void ProcessComponentsRecursive(IDictionary<string, HtmlNode> componentDefinitionsToProcess)
    {
        IDictionary<string, HtmlNode> componentDefinitionsWithInjectionsRemaining = new Dictionary<string, HtmlNode>();
        
        foreach (KeyValuePair<string, HtmlNode> componentDefinition in componentDefinitionsToProcess)
        {
            HtmlNode componentDefinitionNode = componentDefinition.Value;
            ICollection<HtmlNode> injectionNodes = componentDefinitionNode.SelectNodes($"//*[@data-cmp-inject]");
            if (injectionNodes == null)
            {
                HtmlAttribute definitionAttribute = componentDefinitionNode.GetAttributes().Single(attribute => attribute.Name.Equals("data-cmp-definition"));
                componentDefinitionNode.Attributes.Remove(definitionAttribute);
                myComponentProcessedDefinitions[componentDefinition.Key] = componentDefinitionNode;
                componentDefinitionNode.Remove();
                continue;
            }
            foreach (HtmlNode injectionNode in injectionNodes)
            {
                string componentToInject = injectionNode.GetAttributeValue("data-cmp-inject", string.Empty);
                InjectComponent(injectionNode, componentToInject);
            }
            componentDefinitionsWithInjectionsRemaining.Add(componentDefinition);
        }

        if (componentDefinitionsWithInjectionsRemaining.Any())
        {
            ProcessComponentsRecursive(componentDefinitionsWithInjectionsRemaining);
        }
    }

    private void ProcessInjections(IEnumerable<HtmlNode> injectionsToProcess)
    {
        foreach (HtmlNode injection in injectionsToProcess)
        {
            ICollection<HtmlNode> injectionNodes = injection.SelectNodes($"//*[@data-cmp-inject]");
            if (injectionNodes == null)
            {
                continue;
            }
            foreach (HtmlNode injectionNode in injectionNodes)
            {
                string componentToInject = injectionNode.GetAttributeValue("data-cmp-inject", string.Empty);
                if (!InjectComponent(injectionNode, componentToInject))
                {
                    Console.WriteLine("ERROR: A component named \"" + componentToInject + "\" could not be found. (Node " + injectionNode.ParentNode.OuterHtml + ")");
                }
            }
        }
    }

    private bool InjectComponent(HtmlNode injectionNode, string componentToInject)
    {
        if (!myComponentProcessedDefinitions.ContainsKey(componentToInject))
        {
            return false;
        }
        HtmlNode copyOfCompnent = myComponentProcessedDefinitions[componentToInject].CloneNode(true);
        FillSlots(copyOfCompnent, injectionNode);
        copyOfCompnent = ReplaceParameters(copyOfCompnent, injectionNode);
        injectionNode.ParentNode.ReplaceChild(copyOfCompnent, injectionNode);

        return true;
    }

    private void FillSlots(HtmlNode component, HtmlNode injectionNode)
    {
        ICollection<HtmlNode> slotNodes = component.SelectNodes($"*[@data-cmp-slot]");
        if (slotNodes == null)
        {
            return;
        }
        foreach (HtmlNode slot in slotNodes)
        {
            string slotName = slot.GetAttributeValue("data-cmp-slot", "");
            HtmlNode[]? nodesFromInjectionNodeForSlot = injectionNode.SelectNodes($"//*[@data-cmp-slot='{slotName}']")?.ToArray();
            if (nodesFromInjectionNodeForSlot == null)
            {
                continue;
            }

            HtmlNode nodeFromInjectionNodeForSlot = nodesFromInjectionNodeForSlot.First();
            slot.ParentNode.ReplaceChild(nodeFromInjectionNodeForSlot, slot);
            HtmlAttribute slotAttribute = nodeFromInjectionNodeForSlot.GetAttributes().Single(attribute => attribute.Name.Equals("data-cmp-slot"));
            nodeFromInjectionNodeForSlot.Attributes.Remove(slotAttribute);
            if (nodesFromInjectionNodeForSlot.Length > 1)
            {
                for (int i = 0; i < nodesFromInjectionNodeForSlot.Length - 1; i++)
                {
                    nodeFromInjectionNodeForSlot = nodesFromInjectionNodeForSlot[i+1];
                    nodesFromInjectionNodeForSlot[i].ParentNode.InsertAfter(nodeFromInjectionNodeForSlot,nodesFromInjectionNodeForSlot[i]);
                    slotAttribute = nodeFromInjectionNodeForSlot.GetAttributes().Single(attribute => attribute.Name.Equals("data-cmp-slot"));
                    nodeFromInjectionNodeForSlot.Attributes.Remove(slotAttribute);
                }
            }
        }
    }

    private HtmlNode ReplaceParameters(HtmlNode component, HtmlNode injectionNode)
    {
        string componentHtmlString = component.OuterHtml;
        MatchEvaluator getDataFromInjectionNode = new MatchEvaluator(match => {
            string paramValue = injectionNode.GetAttributeValue(match.Value, match.Value);
            return paramValue;
        });
        string pattern = @"data-cmp-param-[0-9]+";
        componentHtmlString = Regex.Replace(componentHtmlString, pattern, getDataFromInjectionNode);
        HtmlDocument documentWithReplacedParams = new HtmlDocument();
        documentWithReplacedParams.LoadHtml(componentHtmlString);
        return documentWithReplacedParams.DocumentNode;
    }

    private void NodeSelectionToDictionary(IDictionary<string, HtmlDocument> htmlDocumentsForProcessing, string attributeName, IDictionary<string, HtmlNode> targetDictionary)
    {
        foreach (KeyValuePair<string, HtmlDocument> htmlDocument in htmlDocumentsForProcessing)
        {
            ICollection<HtmlNode> componentNodes = htmlDocument.Value.DocumentNode.SelectNodes($"//*[@{attributeName}]");
            if (componentNodes == null)
            {
                continue;
            }
            foreach (HtmlNode component in componentNodes)
            {
                string id = component.GetAttributeValue(attributeName, string.Empty);
                if (targetDictionary.ContainsKey(id))
                {
                    Console.WriteLine("ERROR: A component named \"" + id + "\" already exists.");
                    continue;
                }
                targetDictionary.Add(id, component);
            }
        }
    }
    private IEnumerable<HtmlNode> GetNodesFromDocuments(IDictionary<string, HtmlDocument> htmlDocumentsForProcessing, string attributeName)
    {
        IEnumerable<HtmlNode> result = new List<HtmlNode>();
        foreach (KeyValuePair<string, HtmlDocument> htmlDocument in htmlDocumentsForProcessing)
        {
            ICollection<HtmlNode> nodes = htmlDocument.Value.DocumentNode.SelectNodes($"//*[@{attributeName}]");
            if (nodes == null)
            {
                continue;
            }
            result = result.Concat(nodes);
        }

        return result;
    }
}