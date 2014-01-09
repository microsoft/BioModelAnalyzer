using System;
using System.Xml;
using System.Xml.Linq;

namespace BioCheck.Web.Analysis.Xml
{
    /// <summary>
    /// Static helper class for common XDocument functionality
    /// </summary>
    public static class XDocumentHelper
    {
        public static string AttributeString(this XElement element, string name)
        {
            return AttributeString(element, name, true);
        }

        public static string AttributeString(this XElement element, string name, bool isRequired)
        {
            var namedAttribute = element.Attribute(name);
            if (namedAttribute == null)
            {
                if (isRequired)
                {
                    throw new XmlException("Missing XML attribute: " + name);
                }
                else
                {
                    return "";
                }
            }

            return namedAttribute.Value;
        }

        public static int AttributeInt(this XElement element, string name)
        {
            var namedAttribute = element.Attribute(name);
            if (namedAttribute == null)
            {
                throw new XmlException("Missing XML attribute: " + name);
            }

            int value;
            if (!int.TryParse(namedAttribute.Value, out value))
            {
                return 0;
            }

            return value;
        }

        public static string ElementString(this XElement element, string name)
        {
            return ElementString(element, name, true);
        }

        public static string ElementString(this XElement element, string name, bool isRequired)
        {
            var namedElement = element.Element(name);
            if (namedElement == null)
            {
                if (isRequired)
                {
                    throw new XmlException("Missing XML element: " + name);
                }
                else
                {
                    return string.Empty;
                }
            }

            return namedElement.Value;
        }

        public static int ElementInt(this XElement element, string name)
        {
            return ElementInt(element, name, true);
        }

        public static double ElementDouble(this XElement element, string name)
        {
            return ElementDouble(element, name, true);
        }

        public static double ElementDouble(this XElement element, string name, bool isRequired)
        {
            var namedElement = element.Element(name);
            if (namedElement == null)
            {
                if (isRequired)
                {
                    throw new XmlException("Missing XML element: " + name);
                }
                else
                {
                    return 0;
                }
            }

            double value;
            if (!double.TryParse(namedElement.Value, out value))
            {
                return default(int);
            }

            return value;
        }

        public static int ElementInt(this XElement element, string name, bool isRequired)
        {
            var namedElement = element.Element(name);
            if (namedElement == null)
            {
                if (isRequired)
                {
                    throw new XmlException("Missing XML element: " + name);
                }
                else
                {
                    return 0;
                }
            }

            int value;
            if (!int.TryParse(namedElement.Value, out value))
            {
                return default(int);
            }

            return value;
        }

        public static int? ElementNullableInt(this XElement element, string name, bool isRequired)
        {
            var namedElement = element.Element(name);
            if (namedElement == null)
            {
                if (isRequired)
                {
                    throw new XmlException("Missing XML element: " + name);
                }
                else
                {
                    return null;
                }
            }

            int value;
            if (!int.TryParse(namedElement.Value, out value))
            {
                return default(int);
            }

            return value;
        }

        public static int ElementInt(this XElement element, string name, int defaultInt)
        {
            var namedElement = element.Element(name);
            if (namedElement == null)
            {
                return defaultInt;
            }

            int value;
            if (!int.TryParse(namedElement.Value, out value))
            {
                return default(int);
            }

            return value;
        }

        public static DateTime ElementDateTime(this XElement element, string name)
        {
            var namedElement = element.Element(name);
            if (namedElement == null)
            {
                throw new XmlException("Missing XML element: " + name);
            }

            DateTime value;
            if (!DateTime.TryParse(namedElement.Value, out value))
            {
                return DateTime.MinValue;
            }

            return value;
        }
    }
}
