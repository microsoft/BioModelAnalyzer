using System;
using System.Collections.Generic;
using System.Reflection;
using MvvmFx.Common.ViewModels;

namespace BioCheck.ViewModel.Editing
{
    /// <summary>
    /// Static manager class for copying and pasting
    /// </summary>
    public static class CopyPasteManager
    {
        private static ViewModelBase clipboard;

        public static Dictionary<string, object> clipboardData = new Dictionary<string, object>();

        public static Dictionary<string, object> ClipboardData
        {
            get { return clipboardData; }
        }

        public static ViewModelBase Clipboard
        {
            get { return clipboard; }
        }

        public static void Clear()
        {
            clipboard = null;
            clipboardData.Clear();
        }

        public static void Copy(ICopyable source)
        {
            clipboard = source.Copy();
        }

        public static void Paste(ICopyable target)
        {
            target.Paste(clipboard);
        }

        public static bool CanPaste(ICopyable target)
        {
            if (clipboard == null)
                return false;

            if (target == null)
                return true;

            if (target.CanPaste(clipboard))
                return true;
            
            return false;
        }

        public static void Paste(object source, object target)
        {
            foreach (PropertyInfo propInfo in source.GetType().GetProperties())
            {
                if (Attribute.GetCustomAttributes(propInfo, typeof(CopyAttribute)).Length > 0)
                {
                    var value = propInfo.GetValue(source, null);
                    propInfo.SetValue(target, value, null);
                }
            }
        }
    }
}