using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loxonator.Common.Data;
using System.Xml.Linq;
using System.IO;

namespace Loxonator.Common.Helpers
{
    public static class ExportHelper
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

        private static void BackupOriginalFile(string originalFile)
        {
            string backupFile = String.Format("{0}.{1:yyyyMMdd.HHmmss}{2}", Path.GetFileNameWithoutExtension(originalFile),
                DateTime.Now, Path.GetExtension(originalFile));
            File.Copy(originalFile, backupFile, true);
        }

        public static void ExportProjectFile(Node root, string projectFile)
        {
            XDocument project = XDocument.Load(projectFile);
            Template templ = new Template();
            templ.Cr = GetDefaultGuid(project.Root, "IoData", "Cr");
            templ.Pr = GetDefaultGuid(project.Root, "IoData", "Pr");
            templ.Ugr = GetDefaultGuid(project.Root, "IoData", "Ugr");
            templ.Ugx = GetDefaultGuid(project.Root, "IoData", "Ugx");
            templ.OriginalNodes.AddRange(project.Descendants("C").Where(node => (node.Attribute("Type").Value == "EIBsensor" ||
                node.Attribute("Type").Value == "EIBactor") && node.Attribute("EibAddr") != null).ToList());
            XElement lastActor = project.Root.Elements().Where(c => c.Attribute("Type").Value == "EIBactorCaption").First();
            XElement lastSensor = project.Root.Elements().Where(c => c.Attribute("Type").Value == "EIBsensorCaption").First();
            foreach (Node leaf in root.AllLeafs)
            {
                templ.OriginalNodes.Where(node => node.Attribute("EibAddr").Value == leaf.Address).Remove(); // so we don't end up with multiple nodes with the same address
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
            BackupOriginalFile(projectFile);
            project.Save(projectFile);
        }
    }
}
