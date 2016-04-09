using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Loxonator.Common.Data;
using Loxonator.Common.Helpers;

namespace Loxonator
{
    public static class Program
    {
        private static string GetDefaultGuid(XElement parent, string nodeName, string attributeName)
        {
            XElement element = parent.Descendants(nodeName).Where(io => io.Attribute(attributeName) != null).LastOrDefault();
            if (element != null)
                return element.Attribute(attributeName).Value;
            return GuidHelper.NewGuid();
        }

        private static string CreateActor(Node node, Template templ)
        {
            // 0 = 1.0.1
            // 1 = Guid.New
            // 2 = Name
            // 3 = 1/0/1
            // 4 = EIBTyp
            // 5 = Guid.New
            switch (node.Type)
            {
                default:
                // 6 = Ugr
                // 7 = Ugx
                return String.Format(
                    @"<C Type=""EIBactor"" IName=""KGQ{0}"" V=""70"" U=""{1}"" Title=""{2}"" Nio=""1"" EibAddr=""{3}"" EIBType=""{4}"" SourceValHigh=""10"" DestValHigh=""10"">
	                    <Co K=""I"" U=""{5}""/>
	                    <IoData Ugr=""{6}"" Ugx=""{7}""/>
	                    <Display Unit=""&lt;v.1&gt;V""/>
                    </C>",
                    node.DottedAddress, GuidHelper.NewGuid(), node.Name, node.Address, (int)node.Type, GuidHelper.NewGuid(),
                    templ.Ugr, templ.Ugx);
            }
        }

        private static string CreateSensor(Node node, Template templ)
        {
            // 0 = 1.0.1
            // 1 = Guid.New
            // 2 = Name
            // 3 = 1/0/1
            // 4 = EIBTyp
            // 5 = Guid.New
            switch (node.Type)
            {
                default:
                    // 6 = Guid.New
                    // 7 = Cr
                    // 8 = Pr
                    // 9 = Ugr
                    // 10 = Ugx
                    return String.Format(
                        @"<C Type=""EIBsensor"" IName=""KGI{0}"" V=""70"" U=""{1}"" Title=""{2}"" Nio=""2"" EibAddr=""{3}"" EIBType=""{4}"" ForceRemanence=""true"" MaxValue=""100"" SourceValHigh=""100"" DestValHigh=""100"">
	                    <Co K=""AQ"" U=""{5}""/>
	                    <Co K=""Q"" U=""{6}""/>
	                    <IoData Cr=""{7}"" Pr=""{8}"" Ugr=""{9}"" Ugx=""{10}""/>
	                    <Display Unit=""&lt;v&gt;%"" StateOnly=""true"" Step=""5""/>
                    </C>",
                        node.DottedAddress, GuidHelper.NewGuid(), node.Name, node.Address, (int)node.Type, GuidHelper.NewGuid(),
                        GuidHelper.NewGuid(), templ.Cr, templ.Pr, templ.Ugr, templ.Ugx);
            }
        }

        public static void Main(string[] args)
        {
            XDocument import = XDocument.Load(@"TestInitial.xml");
            Node root = new Node();
            int mainIndex = 0;
            foreach (XElement mainGroup in import.Root.Elements().OrderBy(g => Convert.ToInt32(g.Attribute("RangeStart").Value)))
            {
                Node mainNode = new Node(String.Format("{0}", mainIndex), mainGroup.Attribute("Name").Value);
                mainNode.Parent = root;
                int subIndex = 0;
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

            XDocument project = XDocument.Load(@"Empty.loxone");
            Template templ = new Template();
            templ.Cr = GetDefaultGuid(project.Root, "IoData", "Cr");
            templ.Pr= GetDefaultGuid(project.Root, "IoData", "Pr");
            templ.Ugr = GetDefaultGuid(project.Root, "IoData", "Ugr");
            templ.Ugx = GetDefaultGuid(project.Root, "IoData", "Ugx");
            XElement lastActor = project.Root.Elements().Where(c => c.Attribute("Type").Value == "EIBactorCaption").First();
            XElement lastSensor = project.Root.Elements().Where(c => c.Attribute("Type").Value == "EIBsensorCaption").First();
            foreach (Node leaf in root.AllLeafs)
            {
                if (leaf.IsActor)
                {
                    XElement newActor = XElement.Parse(CreateActor(leaf, templ));
                    lastActor.AddAfterSelf(newActor);
                    lastActor = newActor;
                }
                if (leaf.IsSensor)
                {
                    XElement newSensor = XElement.Parse(CreateSensor(leaf, templ));
                    lastSensor.AddAfterSelf(newSensor);
                    lastSensor = newSensor;
                }
            }
            project.Save(@"Finished.loxone");
        }
    }
}
