// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.AlertClient
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    public class PropertyComparer<T> : IComparer<T>
    {
        #region Public Constructor

        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            this.propertyDescriptor = property;
            Type comparerForPropertyType = typeof(Comparer<>).MakeGenericType(property.PropertyType);
            this.comparer =
                (IComparer)
                    comparerForPropertyType.InvokeMember("Default", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, null, null);
            this.SetListSortDirection(direction);
        }

        #endregion

        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            return this.reverse*this.comparer.Compare(this.propertyDescriptor.GetValue(x), this.propertyDescriptor.GetValue(y));
        }

        #endregion

        #region Public Methods

        public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
        {
            this.SetPropertyDescriptor(descriptor);
            this.SetListSortDirection(direction);
        }

        #endregion

        #region Private Properties

        private readonly IComparer comparer;
        private PropertyDescriptor propertyDescriptor;
        private int reverse;

        #endregion

        #region Private Methods

        private void SetPropertyDescriptor(PropertyDescriptor descriptor)
        {
            this.propertyDescriptor = descriptor;
        }

        private void SetListSortDirection(ListSortDirection direction)
        {
            this.reverse = direction == ListSortDirection.Ascending ? 1 : -1;
        }

        #endregion
    }
}