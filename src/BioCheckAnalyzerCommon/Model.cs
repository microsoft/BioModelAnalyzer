using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace BioModelAnalyzer
{
    /// <summary>Model definition. This class serializes both to new JSON and old XML formats</summary>
    public class Model
    {
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
            [JsonProperty(PropertyName = "formula")]
            public string Function { get; set; }

            //public int? Number { get; set; }

            //public bool ShouldSerializeNumber()
            //{
            //    return Number != null;
            //}

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

            // [JsonConverter(typeof(StringEnumConverter))]
            // RelationshipType is encoded as number of JSON because attribute is commented
            public RelationshipType Type { get; set; }
        }

        public enum RelationshipType
        {
            Activator, Inhibitor
        }

        [XmlAttribute]
        public string ModelName { get; set; }

        public Cell[] Cells { get; set; }

        public Variable[] Variables { get; set; }

        public Relationship[] Relationships { get; set; }

        /// <summary>Performs preprocessing of Model before passing it to F# code.
        /// Preprocessing takes two steps: replacing avg(pos)-avg(neg) with default value and replacing 
        /// variable names with IDs
        /// </summary>
        public void Preprocess()
        {
            if (Variables == null)
                return;

            foreach (var v in Variables)
                if (v.Function != null)
                    v.Function = v.Function.Trim();

            NullifyDefaultFunction();
            ReplaceVariableNamesWithIDs();
        }

        private void ReplaceVariableNamesWithIDs()
        {
            if (Variables == null)
                return;

            foreach (var v in Variables)
            {
                v.Function = ReplaceVariableNames(v.Function, name =>
                {
                    var fromRelationships = Variables.Where(
                        v1 => Relationships.Where(r => r.FromVariableId == v1.Id && r.ToVariableId == v.Id || r.FromVariableId == v.Id && r.ToVariableId == v1.Id).Count() > 0).FirstOrDefault(vv => vv.Name == name);
                    var found = fromRelationships;
                    return found == null ? name : found.Id.ToString();
                });
            }
        }

        private void NullifyDefaultFunction()
        {
            if (Variables == null)
                return;

            foreach (var v in Variables)
                if (v.Function != null && v.Function.ToLower() == "avg(pos)-avg(neg)")
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
}