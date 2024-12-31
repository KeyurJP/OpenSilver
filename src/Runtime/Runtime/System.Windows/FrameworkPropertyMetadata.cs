﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Data;

namespace System.Windows
{
    /// <summary>
    /// Specifies the types of framework-level property behavior that pertain to a particular
    /// dependency property.
    /// </summary>
    [Flags]
    public enum FrameworkPropertyMetadataOptions : int
    {
        /// <summary>
        /// No options are specified; the dependency property uses the default behavior of the WPF property system.
        /// </summary>
        None = 0x000,

        /// <summary>The measure pass of layout compositions is affected by value changes to this dependency property.</summary>
        AffectsMeasure = 0x001,

        /// <summary>
        /// The arrange pass of layout composition is affected by value changes to this dependency property.
        /// </summary>
        AffectsArrange = 0x002,

        /// <summary>
        /// The measure pass on the parent element is affected by value changes to this dependency property.
        /// </summary>
        AffectsParentMeasure = 0x004,

        /// <summary>
        /// The arrange pass on the parent element is affected by value changes to this dependency property.
        /// </summary>
        AffectsParentArrange = 0x008,

        /// <summary>
        /// Some aspect of rendering or layout composition (other than measure or arrange) is affected by value changes to this dependency property.
        /// </summary>
        AffectsRender = 0x010,

        /// <summary>
        /// The values of this dependency property are inherited by child elements.
        /// </summary>
        Inherits = 0x020,

        /// <summary>
        /// Data binding to this dependency property is not allowed.
        /// </summary>
        NotDataBindable = 0x080,

        /// <summary>
        /// The <see cref="BindingMode"/> for data bindings on this dependency property defaults to <see cref="BindingMode.TwoWay"/>.
        /// </summary>
        BindsTwoWayByDefault = 0x100,
    }

    /// <summary>
    /// Reports or applies metadata for a dependency property, specifically adding framework-specific
    /// property system characteristics.
    /// </summary>
    public class FrameworkPropertyMetadata : PropertyMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class.
        /// </summary>
        public FrameworkPropertyMetadata() 
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the specified default value.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value of the dependency property, usually provided as a value of
        /// a specific type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// defaultValue is set to <see cref="DependencyProperty.UnsetValue"/>.
        /// </exception>
        public FrameworkPropertyMetadata(object defaultValue)
            : base(defaultValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the specified <see cref="PropertyChangedCallback"/> callback.
        /// </summary>
        /// <param name="propertyChangedCallback">
        /// A reference to a handler implementation that the property system will call whenever
        /// the effective value of the property changes.
        /// </param>
        public FrameworkPropertyMetadata(PropertyChangedCallback propertyChangedCallback)
            : base(propertyChangedCallback)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the specified callbacks.
        /// </summary>
        /// <param name="propertyChangedCallback">
        /// A reference to a handler implementation that the property system will call whenever
        /// the effective value of the property changes.
        /// </param>
        /// <param name="coerceValueCallback">
        /// A reference to a handler implementation will be called whenever the property
        /// system calls <see cref="DependencyObject.CoerceValue(DependencyProperty)"/>
        /// for this dependency property.
        /// </param>
        public FrameworkPropertyMetadata(PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
            : base(propertyChangedCallback)
        {
            CoerceValueCallback = coerceValueCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the provided default value and specified <see cref="PropertyChangedCallback"/>
        /// callback.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value of the dependency property, usually provided as a value of
        /// a specific type.
        /// </param>
        /// <param name="propertyChangedCallback">
        /// A reference to a handler implementation that the property system will call whenever
        /// the effective value of the property changes.
        /// </param>
        /// <exception cref="ArgumentException">
        /// defaultValue is set to <see cref="DependencyProperty.UnsetValue"/>.
        /// </exception>
        public FrameworkPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback)
            : base(defaultValue, propertyChangedCallback)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the provided default value and specified callbacks.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value of the dependency property, usually provided as a specific type.
        /// </param>
        /// <param name="propertyChangedCallback">
        /// A reference to a handler implementation that the property system will call whenever
        /// the effective value of the property changes.
        /// </param>
        /// <param name="coerceValueCallback">
        /// A reference to a handler implementation that will be called whenever the property
        /// system calls <see cref="DependencyObject.CoerceValue(DependencyProperty)"/>
        /// for this dependency property.
        /// </param>
        /// <exception cref="ArgumentException">
        /// defaultValue is set to <see cref="DependencyProperty.UnsetValue"/>.
        /// </exception>
        public FrameworkPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
            : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the provided default value and framework-level metadata options.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value of the dependency property, usually provided as a value of
        /// a specific type.
        /// </param>
        /// <param name="flags">
        /// The metadata option flags (a combination of <see cref="FrameworkPropertyMetadataOptions"/>
        /// values). These options specify characteristics of the dependency property that
        /// interact with systems such as layout or data binding.
        /// </param>
        /// <exception cref="ArgumentException">
        /// defaultValue is set to <see cref="DependencyProperty.UnsetValue"/>.
        /// </exception>
        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags)
            : base(defaultValue)
        {
            TranslateFlags(flags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the provided default value and framework metadata options, and specified
        /// <see cref="PropertyChangedCallback"/> callback.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value of the dependency property, usually provided as a value of
        /// a specific type.
        /// </param>
        /// <param name="flags">
        /// The metadata option flags (a combination of <see cref="FrameworkPropertyMetadataOptions"/>
        /// values). These options specify characteristics of the dependency property that 
        /// interact with systems such as layout or data binding.
        /// </param>
        /// <param name="propertyChangedCallback">
        /// A reference to a handler implementation that the property system will call whenever
        /// the effective value of the property changes.
        /// </param>
        /// <exception cref="ArgumentException">
        /// defaultValue is set to <see cref="DependencyProperty.UnsetValue"/>.
        /// </exception>
        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback)
            : base(defaultValue, propertyChangedCallback)
        {
            TranslateFlags(flags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkPropertyMetadata"/> class
        /// with the provided default value and framework metadata options, and specified
        /// callbacks.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value of the dependency property, usually provided as a specific type.
        /// </param>
        /// <param name="flags">
        /// The metadata option flags (a combination of <see cref="FrameworkPropertyMetadataOptions"/>
        /// values). These options specify characteristics of the dependency property that
        /// interact with systems such as layout or data binding.
        /// </param>
        /// <param name="propertyChangedCallback">
        /// A reference to a handler implementation that the property system will call whenever
        /// the effective value of the property changes.
        /// </param>
        /// <param name="coerceValueCallback">
        /// A reference to a handler implementation that will be called whenever the property
        /// system calls <see cref="DependencyObject.CoerceValue(DependencyProperty)"/>
        /// against this property.
        /// </param>
        /// <exception cref="ArgumentException">
        /// defaultValue is set to <see cref="DependencyProperty.UnsetValue"/>.
        /// </exception>
        public FrameworkPropertyMetadata(
            object defaultValue,
            FrameworkPropertyMetadataOptions flags,
            PropertyChangedCallback propertyChangedCallback,
            CoerceValueCallback coerceValueCallback)
            : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
            TranslateFlags(flags);
        }
        
        /// <summary>
        /// Gets or sets a value that indicates whether a dependency property potentially
        /// affects the measure pass during layout engine operations.
        /// </summary>
        /// <returns>
        /// true if the dependency property on which this metadata exists potentially affects
        /// the measure pass; otherwise, false. The default is false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The metadata has already been applied to a dependency property operation, so
        /// that metadata is sealed and properties of the metadata cannot be set.
        /// </exception>
        public bool AffectsMeasure
        {
            get { return ReadFlag(MetadataFlags.FW_AffectsMeasureID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_AffectsMeasureID, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a dependency property potentially
        /// affects the arrange pass during layout engine operations.
        /// </summary>
        /// <returns>
        /// true if the dependency property on which this metadata exists potentially affects
        /// the arrange pass; otherwise, false. The default is false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The metadata has already been applied to a dependency property operation, so
        /// that metadata is sealed and properties of the metadata cannot be set.
        /// </exception>
        public bool AffectsArrange
        {
            get { return ReadFlag(MetadataFlags.FW_AffectsArrangeID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_AffectsArrangeID, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a dependency property potentially
        /// affects the measure pass of its parent element's layout during layout engine
        /// operations.
        /// </summary>
        /// <returns>
        /// true if the dependency property on which this metadata exists potentially affects
        /// the measure pass specifically on its parent element; otherwise, false.The default
        /// is false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The metadata has already been applied to a dependency property operation, so
        /// that metadata is sealed and properties of the metadata cannot be set.
        /// </exception>
        public bool AffectsParentMeasure
        {
            get { return ReadFlag(MetadataFlags.FW_AffectsParentMeasureID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_AffectsParentMeasureID, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a dependency property potentially
        /// affects the arrange pass of its parent element's layout during layout engine
        /// operations.
        /// </summary>
        /// <returns>
        /// true if the dependency property on which this metadata exists potentially affects
        /// the arrange pass specifically on its parent element; otherwise, false. The default
        /// is false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The metadata has already been applied to a dependency property operation, so
        /// that metadata is sealed and properties of the metadata cannot be set.
        /// </exception>
        public bool AffectsParentArrange
        {
            get { return ReadFlag(MetadataFlags.FW_AffectsParentArrangeID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_AffectsParentArrangeID, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a dependency property potentially
        /// affects the general layout in some way that does not specifically influence arrangement
        /// or measurement, but would require a redraw.
        /// </summary>
        /// <returns>
        /// true if the dependency property on which this metadata exists affects rendering;
        /// otherwise, false. The default is false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The metadata has already been applied to a dependency property operation, so
        /// that metadata is sealed and properties of the metadata cannot be set.
        /// </exception>
        public bool AffectsRender
        {
            get { return ReadFlag(MetadataFlags.FW_AffectsRenderID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_AffectsRenderID, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the dependency property supports data binding.
        /// </summary>
        /// <returns>
        /// true if the property does not support data binding; otherwise, false. The default is false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The metadata has already been applied to a dependency property operation, so that metadata 
        /// is sealed and properties of the metadata cannot be set.
        /// </exception>
        public bool IsNotDataBindable
        {
            get { return ReadFlag(MetadataFlags.FW_IsNotDataBindableID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_IsNotDataBindableID, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the property binds two-way by default.
        /// </summary>
        /// <returns>
        /// true if the dependency property on which this metadata exists binds two-way by default;
        /// otherwise, false. The default is false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The metadata has already been applied to a dependency property operation, so that metadata
        /// is sealed and properties of the metadata cannot be set.
        /// </exception>
        public bool BindsTwoWayByDefault
        {
            get { return ReadFlag(MetadataFlags.FW_BindsTwoWayByDefaultID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_BindsTwoWayByDefaultID, value); }
        }

        /// <summary>
        /// Gets a value that indicates whether data binding is supported for the dependency property.
        /// </summary>
        /// <returns>
        /// true if data binding is supported on the dependency property to which this metadata applies;
        /// otherwise, false. The default is true.
        /// </returns>
        public bool IsDataBindingAllowed
        {
            get { return !ReadFlag(MetadataFlags.FW_IsNotDataBindableID) && !ReadOnly; }
        }

        /// <summary>
        /// Does the represent the metadata for a ReadOnly property
        /// </summary>
        private bool ReadOnly
        {
            get { return ReadFlag(MetadataFlags.FW_ReadOnlyID); }
            set { CheckSealed(); WriteFlag(MetadataFlags.FW_ReadOnlyID, value); }
        }

        /// <summary>
        /// Creates a new instance of this property metadata.  This method is used
        /// when metadata needs to be cloned.  After CreateInstance is called the
        /// framework will call Merge to merge metadata into the new instance.
        /// Deriving classes must override this and return a new instance of
        /// themselves.
        /// </summary>
        internal override PropertyMetadata CreateInstance()
        {
            return new FrameworkPropertyMetadata();
        }

        /// <summary>
        /// Enables a merge of the source metadata with base metadata.
        /// </summary>
        /// <param name="baseMetadata">
        /// The base metadata to merge.
        /// </param>
        /// <param name="dp">
        /// The dependency property this metadata is being applied to.
        /// </param>
        protected override void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
        {
            // Does parameter validation
            base.Merge(baseMetadata, dp);

            // Source type is guaranteed to be the same type or base type
            if (baseMetadata is FrameworkPropertyMetadata fbaseMetadata)
            {
                // Merge source metadata into this

                // Modify metadata merge state fields directly (not through accessors
                // so that "modified" bits remain intact

                // Merge state
                // Defaults to false, derived classes can only enable
                WriteFlag(MetadataFlags.FW_AffectsMeasureID, ReadFlag(MetadataFlags.FW_AffectsMeasureID) | fbaseMetadata.AffectsMeasure);
                WriteFlag(MetadataFlags.FW_AffectsArrangeID, ReadFlag(MetadataFlags.FW_AffectsArrangeID) | fbaseMetadata.AffectsArrange);
                WriteFlag(MetadataFlags.FW_AffectsParentMeasureID, ReadFlag(MetadataFlags.FW_AffectsParentMeasureID) | fbaseMetadata.AffectsParentMeasure);
                WriteFlag(MetadataFlags.FW_AffectsParentArrangeID, ReadFlag(MetadataFlags.FW_AffectsParentArrangeID) | fbaseMetadata.AffectsParentArrange);
                WriteFlag(MetadataFlags.FW_AffectsRenderID, ReadFlag(MetadataFlags.FW_AffectsRenderID) | fbaseMetadata.AffectsRender);
                WriteFlag(MetadataFlags.FW_BindsTwoWayByDefaultID, ReadFlag(MetadataFlags.FW_BindsTwoWayByDefaultID) | fbaseMetadata.BindsTwoWayByDefault);
                WriteFlag(MetadataFlags.FW_IsNotDataBindableID, ReadFlag(MetadataFlags.FW_IsNotDataBindableID) | fbaseMetadata.IsNotDataBindable);

                // Override state
                if (!IsModified(MetadataFlags.FW_InheritsModifiedID))
                {
                    IsInherited = fbaseMetadata.Inherits;
                }
            }
        }

        /// <summary>
        /// Called when this metadata has been applied to a property, which indicates that
        /// the metadata is being sealed.
        /// </summary>
        /// <param name="dp">
        /// The dependency property to which the metadata has been applied.
        /// </param>
        /// <param name="targetType">
        /// The type associated with this metadata if this is type-specific metadata. If
        /// this is default metadata, this value can be null.
        /// </param>
        protected override void OnApply(DependencyProperty dp, Type targetType)
        {
            // Remember if this is the metadata for a ReadOnly property
            ReadOnly = dp.ReadOnly;

            base.OnApply(dp, targetType);
        }

        private static bool IsFlagSet(FrameworkPropertyMetadataOptions flag, FrameworkPropertyMetadataOptions flags)
        {
            return (flags & flag) != 0;
        }

        private void TranslateFlags(FrameworkPropertyMetadataOptions flags)
        {
            // Convert flags to state sets. If a flag is set, then,
            // the value is set on the respective property. Otherwise,
            // the state remains unset

            // This means that state is cumulative across base classes
            // on a merge where appropriate

            if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsMeasure, flags))
            {
                AffectsMeasure = true;
            }

            if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsArrange, flags))
            {
                AffectsArrange = true;
            }

            if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsParentMeasure, flags))
            {
                AffectsParentMeasure = true;
            }

            if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsParentArrange, flags))
            {
                AffectsParentArrange = true;
            }

            if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsRender, flags))
            {
                AffectsRender = true;
            }

            if (IsFlagSet(FrameworkPropertyMetadataOptions.Inherits, flags))
            {
                IsInherited = true;
            }

            if (IsFlagSet(FrameworkPropertyMetadataOptions.NotDataBindable, flags))
            {
                IsNotDataBindable = true;
            }

            if (IsFlagSet(FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, flags))
            {
                BindsTwoWayByDefault = true;
            }
        }

        internal void SetModified(MetadataFlags id) { WriteFlag(id, true); }
        internal bool IsModified(MetadataFlags id) { return ReadFlag(id); }
    }
}
