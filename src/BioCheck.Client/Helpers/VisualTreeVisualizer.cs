using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace BioCheck.Helpers
{
    /// <summary>
    /// Helper class to visualize the Silverlight Visual Tree as a textual-tree in the debugger. 
    /// The class provides three main methods:
    ///     -VisualTreeVisualizer.WriteDownwards( DependencyObject )
    ///     -VisualTreeVisualizer.UpDownwards( DependencyObject )
    ///     -VisualTreeVisualizer.GetElementByHashCode( int )
    /// Call the methods direct in the "immediate window" during the debug session in Visual Studio.
    /// </summary>
    /// <remarks>This class was implemented, because of the missing feature to write a visualizer for Visual Studio compatible with Silverlight CLR.</remarks>
    /// <see cref="http://blog.thekieners.com/2009/11/07/silverlight-visual-tree-visualizer/"/>
    internal static class VisualTreeVisualizer
    {
        #region Public methods

        /// <summary>
        /// Searchs an element in the visual tree matching with the given hash code. 
        /// </summary>
        /// <param name="hashCode">The hash code to search for.</param>
        /// <returns>The element in the visual three matching the given hash code, otherwise null</returns>
        /// <remarks>Optain the hash code from the output of WriteDownwards/WriteUpwards.</remarks>
        public static DependencyObject GetElementByHashCode(int hashCode)
        {
            // start search at the root of the visual tree
            DependencyObject root = Application.Current.RootVisual as DependencyObject;

            return GetElementByHashCode(root, hashCode);
        }

        /// <summary>
        /// Searchs an element in the visual tree matching with the given hash code. 
        /// </summary>
        /// <param name="obj">A element in the visual tree as root element to search downwards the tree.</param>
        /// <param name="hashCode">The hash code to search for.</param>
        /// <returns>The element in the visual three matching the given hash code, otherwise null</returns>
        /// <remarks>
        /// This method is especially used in case where the visual tree is not part of the 
        /// main visual tree of the Silverlight application, as it happen with popup controls.
        /// Optain the hash code from the output of WriteDownwards/WriteUpwards.
        /// </remarks>
        public static DependencyObject GetElementByHashCode(DependencyObject obj, int hashCode)
        {
            if (obj == null)
                return null;

            // check if hash code matches
            if (obj.GetHashCode() == hashCode)
                return obj;

            // store child count local
            int childCount = VisualTreeHelper.GetChildrenCount(obj);

            // check if one of the child match to the hash code
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child.GetHashCode() == hashCode)
                    return child;
            }

            // go down the tree
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                DependencyObject returnObj = GetElementByHashCode(child, hashCode);

                // if object was found, return
                if (returnObj != null)
                    return returnObj;
            }

            // nothing found
            return null;
        }

        /// <summary>
        /// Writes the full visual tree from a given dependencyobject down to the leafs into the output.      
        /// The output is like the following:    
        ///  VisualTreeVisualizer.WriteDownwards (this)
        ///  -DataGridDevWithPagging.MainPage  23692278
        ///   |-Grid Name=LayoutRoot 41429416
        ///     |-StackPanel  21454193
        ///       |-Button  26765710
        ///       | |-Grid  5894079
        ///       |   |-Border Name=Background 11903911
        ///       |   | |-Grid  40026340
        ///       |   |   |-Border    Name=BackgroundAnimation 24692740
        ///       |   |   |-Rectangle Name=BackgroundGradient 20908074
        ///       |   |-ContentPresenter Name=contentPresenter 654897
        ///       |   | |-Grid  1816341
        ///       |   |   |-TextBlock  7658356
        ///       |   |-Rectangle Name=DisabledVisualElement 53954942
        ///       |   |-Rectangle Name=FocusVisualElement 15832433
        ///       |-Button Name=btnSetValue 50632145
        ///         |-Grid  12905972
        ///           |-Border Name=Background 8274172
        ///           | |-Grid  7358688
        ///           |   |-Border    Name=BackgroundAnimation 66228199
        ///           |   |-Rectangle Name=BackgroundGradient 59182880
        ///           |-ContentPresenter Name=contentPresenter 16347077
        ///           | |-Grid  38750844
        ///           |   |-TextBlock  49044892
        ///           |-Rectangle Name=DisabledVisualElement 62883878
        ///           |-Rectangle Name=FocusVisualElement 29083993
        /// </summary>
        /// <param name="obj">A element in the visual tree to start with the down walk.</param>
        public static void WriteDownwards(DependencyObject obj)
        {
            if (obj == null)
                return;

            // use a string builder, because of better string performance
            StringBuilder output = new StringBuilder();

            // variable to count the total elements
            int visualElementCount = 0;

            // collect the tree informations
            CollectTreeDown(obj, output, 0, new Stack<int>(), 0, ref visualElementCount);

            // write it out
            Debug.WriteLine(output);
            Debug.WriteLine("Total Visual Elements: " + visualElementCount);
        }

        /// <summary>
        /// Writes the visual tree from a given dependencyobject up to the application root into the output.
        /// The output is like the following:    
        /// VisualTreeVisualizer.WriteUpwards ( this.btnDemo )
        /// |-DataGridDevWithPagging.MainPage  23692278
        ///   |-Grid Name=LayoutRoot 41429416
        ///    |-StackPanel  21454193
        ///     |-Button Name=btnDemo 26239245
        /// </summary>
        /// <param name="obj">A element in the visual tree to start with the up-walk.</param>
        public static void WriteUpwards(DependencyObject obj)
        {
            if (obj == null)
                return;

            // list to collect the elements
            List<string> levels = new List<string>();
         
            // collect the tree informations
            CollectTreeUp(obj, levels);

            // reverse the tree information to get the application as root
            levels.Reverse();

            // collect the string
            string indent = "";
            StringBuilder output = new StringBuilder();
            foreach (string level in levels)
            {
                output.Append(indent + " |-" + level + Environment.NewLine);
                indent += " ";
            }

            // write it out
            Debug.WriteLine(output);
            Debug.WriteLine("Total Levels to Root: " + levels.Count);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the type name from a DependencyObject. 
        /// If the instance is part of the System.Windows namespace it only return the class name and not the full name.
        /// </summary>
        private static string GetTypeName(DependencyObject obj)
        {
            string typeName = obj.GetType().FullName;
            // only display the pure name for System.Windows types
            if (typeName.StartsWith("System.Windows"))
                typeName = obj.GetType().Name;

            return typeName;
        }

        /// <summary>
        /// Walks up the tree from the given object and collects tree information.
        /// </summary>
        private static void CollectTreeUp(DependencyObject obj, List<string> levels)
        {
            if (obj == null)
                return;

            // name of type
            string node = GetTypeName(obj);

            // XAML name
            node += " " + GetElementName(obj);

            // hashcode
            node += " " + obj.GetHashCode();

            // add level
            levels.Add(node);

            // follow parent
            CollectTreeUp(VisualTreeHelper.GetParent(obj), levels);

        }

        /// <summary>
        /// Walks down the tree from the given object and collects tree information.
        /// </summary>
        private static void CollectTreeDown(DependencyObject obj, StringBuilder output, int level, Stack<int> treeLineIndexes, int propertyIntend, ref int visualElementCount)
        {
            if (obj == null)
                return;

            // count visual elements
            visualElementCount++;

            // prepare intend/treelines
            string intend = "";
            for (int i = 0; i < level; i++)
            {
                if (treeLineIndexes.Contains(i) || i + 1 >= level)
                    intend += " |";
                else
                    intend += "  ";
            }

            // type name
            string typeName = GetTypeName(obj);
            output.Append(intend + "-" + typeName);

            output.Append(GetIntendString(propertyIntend - typeName.Length));

            // XAML name
            output.Append(" " + GetElementName(obj));

            // hashcode
            output.Append(" " + obj.GetHashCode());

            output.Append(" " + GetProperties(obj));

            output.Append(Environment.NewLine);

            // get child count
            int childCount = VisualTreeHelper.GetChildrenCount(obj);

            // remember treeline if more than one child
            if (childCount >= 2)
                treeLineIndexes.Push(level);

            int levelForChildTree = ++level;
            int lastChildIncludedInIntend = -1;
            int intendForChild = 0;
            for (int i = 0; i < childCount; i++)
            {
                // remove the treeline for last element
                if (childCount >= 2 && i + 1 == childCount)
                    treeLineIndexes.Pop();

                if (lastChildIncludedInIntend < i)
                    lastChildIncludedInIntend = GetPropertyValueIntend(obj, i, out intendForChild);

                // follow the child
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                CollectTreeDown(child, output, levelForChildTree, treeLineIndexes, intendForChild, ref visualElementCount);
            }
        }



        private static string GetProperties(DependencyObject obj)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            string propertyValues = "";

            if (parent != null && obj is FrameworkElement && parent.GetType().IsAssignableFrom(typeof(Grid)))
            {
                FrameworkElement frmkElement = obj as FrameworkElement;
                int row = Grid.GetRow(frmkElement);
                int col = Grid.GetColumn(frmkElement);

                if (row > 0)
                    propertyValues += " Grid.Row=\"" + row + "\"";

                if (col > 0)
                    propertyValues += " Grid.Column=\"" + col + "\"";
            }

            if (obj is UIElement)
            {
                UIElement uiElement = obj as UIElement;

                Visibility visibility = uiElement.Visibility;

                if (uiElement.Visibility == Visibility.Collapsed)
                    propertyValues += " Visibility=Collapsed";
            }

            return propertyValues.Trim();

        }



        /// <summary>
        /// Gets the xaml name of the given element
        /// </summary>
        private static string GetElementName(DependencyObject obj)
        {
            if (obj is FrameworkElement)
            {
                string name = (obj as FrameworkElement).Name;

                if (!string.IsNullOrEmpty(name))
                    return "Name=" + name;
            }

            return "";
        }


        /// <summary>
        /// Helper method to calculate the property intend.
        /// </summary>
        private static int GetPropertyValueIntend(DependencyObject obj, int currentIndex, out int intend)
        {
            // get child count
            int childCount = VisualTreeHelper.GetChildrenCount(obj);

            int propertyIntendForChild = 0;
            int i = currentIndex;
            for (; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                int childChildrenCount = VisualTreeHelper.GetChildrenCount(child);
                if (childChildrenCount > 0)
                    break;

                int typeNameLength = GetTypeName(child).Length;
                propertyIntendForChild = Math.Max(propertyIntendForChild, typeNameLength);
            }

            intend = propertyIntendForChild;

            return i;
        }

        private static string GetIntendString(int intend)
        {
            return "".PadLeft(Math.Max(0, intend), ' ');
        }

        #endregion
    }
}
