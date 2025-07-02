using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A data class for storing format informations
    /// </summary>
    public sealed class FormatData
    {
        /// <summary>
        /// The format argument
        /// </summary>
        public object Argument;

        /// <summary>
        /// Checks that Argument is a UnityEngine.Color
        /// </summary>
        public bool IsColorData => Argument is Color;

        /// <summary>
        /// The index of the first character of the format placeholder in the format string
        /// </summary>
        public int Position;

        /// <summary>
        /// The number of valid chars to apply color formatting
        /// </summary>
        public int Range;
    }

    internal static class FormatDataAccess
    {
        private static readonly ConditionalWeakTable<IColorFormatProvider, Data> colorFormatCWT = new();

        internal static Data GetData(this IColorFormatProvider self) => colorFormatCWT.GetValue(self, _ => new());

        internal sealed class Data
        {
            /// <summary>
            /// Collection of active format data being processed
            /// </summary>
            public LinkedList<NodeData> Entries = new LinkedList<NodeData>();

            public void AddNodeEntry(StringBuilder builder)
            {
                NodeData data = new NodeData()
                {
                    Builder = builder
                };
                Entries.AddLast(new LinkedListNode<NodeData>(data));
            }

            public void RemoveLastNodeEntry()
            {
                Entries.RemoveLast();
            }
        }

        internal class NodeData
        {
            /// <summary>
            /// The builder in control of building the formatted string
            /// </summary>
            public StringBuilder Builder;

            /// <summary>
            /// The format argument currently being processed
            /// </summary>
            public FormatData Current;
        }
    }
}
