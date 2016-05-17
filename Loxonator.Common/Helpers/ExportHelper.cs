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

        private static void ApplyActor(Template templ, Node leaf, ref XElement lastActor)
        {
            if (!leaf.IsActor)
            {
                if (false) // es soll nie gelöscht werden
                    templ.OriginalNodes.Where(node => node.Attribute("EibAddr").Value == leaf.Address && node.Attribute("Type").Value == "EIBactor").Remove();
            }
            else
            {
                List<XElement> existing = templ.OriginalNodes.Where(node => node.Attribute("EibAddr").Value == leaf.Address && node.Attribute("Type").Value == "EIBactor").ToList();
                if (existing.Count > 0)
                {
                    foreach (XElement actor in existing)
                    {
                        actor.SetAttributeValue("Title", leaf.Name);
                        actor.SetAttributeValue("EIBType", ((int)leaf.Type).ToString());
                    }
                }
                else
                {
                    XElement newActor = XElement.Parse(CreateActor(leaf, templ));
                    lastActor.AddAfterSelf(newActor);
                    lastActor = newActor;
                }
            }
        }

        private static void ApplySensor(Template templ, Node leaf, ref XElement lastSensor)
        {
            if (!leaf.IsSensor)
            {
                if (false) // es soll nie gelöscht werden
                    templ.OriginalNodes.Where(node => node.Attribute("EibAddr").Value == leaf.Address && node.Attribute("Type").Value == "EIBsensor").Remove();
            }
            else
            {
                List<XElement> existing = templ.OriginalNodes.Where(node => node.Attribute("EibAddr").Value == leaf.Address && node.Attribute("Type").Value == "EIBsensor").ToList();
                if (existing.Count > 0)
                {
                    foreach (XElement actor in existing)
                    {
                        actor.SetAttributeValue("Title", leaf.Name);
                        actor.SetAttributeValue("EIBType", ((int)leaf.Type).ToString());
                    }
                }
                else
                {
                    XElement newSensor = XElement.Parse(CreateSensor(leaf, templ));
                    lastSensor.AddAfterSelf(newSensor);
                    lastSensor = newSensor;
                }
            }
        }

        private static bool IndexOfSnippet(string line, string startSnippet, string endSnippet, int pos, out int startIndex, out int endIndex)
        {
            startIndex = line.IndexOf(startSnippet, pos, StringComparison.InvariantCultureIgnoreCase);
            endIndex = -1;
            if (startIndex >= pos)
            {
                endIndex = line.IndexOf(endSnippet, startIndex + startSnippet.Length, StringComparison.InvariantCultureIgnoreCase);
                if (endIndex >= startIndex + startSnippet.Length)
                {
                    endIndex = endIndex + endSnippet.Length;
                    return true;
                }
            }
            return false;
        }

        private static string FixDuplicateAttributes(string projectFile)
        {
            StringBuilder fixedXml = new StringBuilder();
            using (StreamReader reader = new StreamReader(projectFile, Encoding.UTF8))
            {
                string line = String.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    int startIndex, endIndex, pos = 0;
                    if (IndexOfSnippet(line, "SpStates=\"", "\"", pos, out startIndex, out endIndex))
                    {
                        pos = endIndex;
                        while (IndexOfSnippet(line, "SpStates=\"", "\"", pos, out startIndex, out endIndex))
                            line = line.Remove(startIndex, endIndex - startIndex);
                    }
                    fixedXml.AppendLine(line);
                }
            }
            return fixedXml.ToString();
        }

        // leider kein wohlgeformtes XML (zB Zeile 7307 Attribut SpStates ist doppelt definiert)
        // http://stackoverflow.com/questions/6609719/how-to-remove-duplicate-attributes-from-xml-with-c-sharp
        public static void ExportProjectFile(Node root, string projectFile)
        {
            string fixedXml = FixDuplicateAttributes(projectFile);
            XDocument project = XDocument.Parse(fixedXml);
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
                ApplyActor(templ, leaf, ref lastActor);
                ApplySensor(templ, leaf, ref lastSensor);
            }
            BackupOriginalFile(projectFile);
            project.Save(projectFile);
        }
    }
}
