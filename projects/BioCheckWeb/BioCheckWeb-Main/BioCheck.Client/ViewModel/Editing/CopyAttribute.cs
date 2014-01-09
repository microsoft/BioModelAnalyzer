using System;

namespace BioCheck.ViewModel.Editing
{
    /// <summary>
    /// Attribute to put on ViewModel Properties to specify that they should be included in copying that object
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class CopyAttribute : Attribute
    {
    }
}