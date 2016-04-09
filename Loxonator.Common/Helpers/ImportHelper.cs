using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Loxonator.Common.Data;
using System.IO;

namespace Loxonator.Common.Helpers
{
    public static class ImportHelper
    {
        public static void LoadImportFile(Node root, string fileName)
        {
            XDocument import = XDocument.Load(fileName);
            root.IsSelected = true;
            root.IsExpanded = true;
            foreach (Node child in root.Children.ToList())
                child.Parent = null; // clears the Children of root
            root.Name = String.Empty;
            int mainIndex = 0; // Adresse für Übergruppe egal, wird einfach aufsteigend generiert
            // exportiert werden sowieso nur Leafs
            foreach (XElement mainGroup in import.Root.Elements().OrderBy(g => Convert.ToInt32(g.Attribute("RangeStart").Value)))
            {
                Node mainNode = new Node(String.Format("{0}", mainIndex), mainGroup.Attribute("Name").Value);
                mainNode.Parent = root;
                int subIndex = 0; // auch die Adresse der Untergruppe ist egal
                foreach (XElement subGroup in mainGroup.Elements().OrderBy(g => Convert.ToInt32(g.Attribute("RangeStart").Value)))
                {
                    Node subNode = new Node(String.Format("{0}/{1}", mainIndex, subIndex++), subGroup.Attribute("Name").Value);
                    subNode.Parent = mainNode;
                    foreach (XElement address in subGroup.Elements())
                    {
                        Node node = new Node(address.Attribute("Address").Value, address.Attribute("Name").Value);
                        node.Parent = subNode;
                    }
                }
                mainIndex++;
            }
            root.Name = Path.GetFileName(fileName);
        }
    }
}
