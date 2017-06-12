using System;
using System.Runtime.InteropServices;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class vButtomAttribute : PropertyAttribute
    {
        public readonly string label;

        public readonly string function;

        public readonly int id;
        /// <summary>
        /// Create a button in Inspector
        /// </summary>
        /// <param name="label">button label</param>
        /// <param name="function">function to call on press</param>
        /// <param name="id">id of button</param>
        public vButtomAttribute(string label, string function, int id)
        {
            this.label = label;
            this.function = function;
            this.id = id;
        }
    }
}

