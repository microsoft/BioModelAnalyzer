using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace bma.client
{

    public class Model
    {
        public Cell[] Cells { get; set; }

        public Variable[] Variables { get; set; }

        public Relationship[] Relationships { get; set; }

        public void ReplaceVariableNamesWithIDs()
        {
            if (Variables == null)
                return;

            foreach (var v in Variables)
            {
                v.Function = ReplaceVariableNames(v.Function, name =>
                {
                    var fromRelationships = Variables.Where(v1 => Relationships.Where(r => r.FromVariableId == v1.Id && r.ToVariableId == v.Id || r.FromVariableId == v.Id && r.ToVariableId == v1.Id).Count() > 0).FirstOrDefault(vv => vv.Name == name);
                    var found = fromRelationships;
                    return found == null ? name : found.Id.ToString();
                });
            }
        }

        public void NullifyDefaultFunction()
        {
            if (Variables == null)
                return;

            foreach (var v in Variables)
                if (v.Function != null && v.Function.Replace(" ", "").ToLower() == "avg(pos)-avg(neg)")
                    v.Function = null;
        }

        private static string ReplaceVariableNames(string s, Func<string, string> f)
        {
            const string var = "var(";
            int startPos = 0;
            int index;
            while ((index = s.IndexOf(var, startPos)) >= 0)
            {
                int endIndex = s.IndexOf(")", index);
                if (endIndex < 0)
                    return s;
                var varName = s.Substring(index + var.Length, endIndex - index - var.Length);
                s = s.Remove(index + var.Length, endIndex - index - var.Length).Insert(index + var.Length, f(varName));
                startPos = index + 1;
            }
            return s;
        }
    }

    public class Cell
    {
        [XmlAttribute]
        public string Name { get; set; }
    }

    public class Variable
    {
        [XmlAttribute]
        public int Id { get; set; }

        [DefaultValue((string)null)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        public double RangeFrom { get; set; }

        public double RangeTo { get; set; }

        [DefaultValue((string)null)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Function { get; set; }

        public int? Number { get; set; }

        public bool ShouldSerializeNumber()
        {
            return Number != null;
        }

        public Tag[] Tags { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Relationship
    {
        [XmlAttribute]
        public int Id { get; set; }

        public int FromVariableId { get; set; }

        public int ToVariableId { get; set; }

        public RelationshipType Type { get; set; }
    }

    public enum RelationshipType
    {
        Activator, Inhibitor
    }
}