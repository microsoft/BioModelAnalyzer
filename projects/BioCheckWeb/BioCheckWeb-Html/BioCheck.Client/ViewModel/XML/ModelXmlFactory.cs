using System;
using System.Linq;
using System.Xml.Linq;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.XML.v1;
using BioCheck.ViewModel.XML.v2;
using BioCheck.ViewModel.XML.v3;

namespace BioCheck.ViewModel.XML
{
    public abstract class ModelXmlFactory
    {
        public static ModelViewModel Create(XDocument xdoc)
        {
            var xmlFactory = GetXmlFactory(xdoc);
            var modelVM = xmlFactory.OnCreate(xdoc);
            modelVM.IsLoaded = false;
            return modelVM;
        }

        public static void Load(XDocument xdoc, ModelViewModel modelVM)
        {
            var xmlFactory = GetXmlFactory(xdoc);
            xmlFactory.OnLoad(xdoc, modelVM);
            modelVM.IsLoaded = true;
        }

        protected abstract ModelViewModel OnCreate(XDocument xdoc);

        protected abstract void OnLoad(XDocument xdoc, ModelViewModel modelVM);

        private static ModelXmlFactory GetXmlFactory(XDocument xdoc)
        {
            int version = GetVersion(xdoc);
            switch (version)
            {
                case 1:
                    return new V1Factory();
                case 2:
                    return new V2Factory();
                case 3:
                    return new V3Factory();
                default:
                    throw new InvalidModelXmlException();
            }
        }

        private static int GetVersion(XDocument xdoc)
        {
            var model = xdoc.Descendants("Model").FirstOrDefault();
            if (model != null)
            {
                var versionAttribute = model.Attribute("BioCheckVersion");
                if (versionAttribute == null)
                {
                    return 1;
                }
                var version = Convert.ToInt32(versionAttribute.Value);
                return version;
            }

            throw new InvalidModelXmlException();
        }
    }
}