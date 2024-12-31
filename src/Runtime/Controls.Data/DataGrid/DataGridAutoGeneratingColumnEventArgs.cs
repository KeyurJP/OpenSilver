// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace System.Windows.Controls
{
    /// <summary>
    /// Provides data for the <see cref="DataGrid.AutoGeneratingColumn" /> event. 
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    public class DataGridAutoGeneratingColumnEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridAutoGeneratingColumnEventArgs" /> class.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property bound to the generated column.
        /// </param>
        /// <param name="propertyType">
        /// The <see cref="Type" /> of the property bound to the generated column.
        /// </param>
        /// <param name="column">
        /// The generated column.
        /// </param>
        public DataGridAutoGeneratingColumnEventArgs(string propertyName, Type propertyType, DataGridColumn column)
            : this(column, propertyName, propertyType, null)
        {
        }

        internal DataGridAutoGeneratingColumnEventArgs(DataGridColumn column, string propertyName, Type propertyType, object propertyDescriptor)
        {
            Debug.Assert(propertyDescriptor is null || typeof(PropertyInfo).IsAssignableFrom(propertyDescriptor.GetType()));

            this.Column = column;
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
            this.PropertyDescriptor = propertyDescriptor;
        }

        /// <summary>
        /// Gets the generated column.
        /// </summary>
        public DataGridColumn Column { get; set; }

        /// <summary>
        /// Gets the name of the property bound to the generated column.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the <see cref="Type" /> of the property bound to the generated column.
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// Descriptor of the property for which the column is gettign generated
        /// </summary>
        public object PropertyDescriptor { get; }
    }
}
