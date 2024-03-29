﻿namespace SBRW.Nancy.ModelBinding
{
    using System;

    /// <summary>
    /// Represents an exception occurred when binding the properties.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class PropertyBindingException : Exception
    {
        private const string ExceptionMessage = "Unable to bind property: {0}; Attempted value: {1}";

        /// <summary>
        /// Gets the property name for which the bind failed
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets the value which was attempted to be assigned to the property
        /// </summary>
        public string AttemptedValue { get; private set; }

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="propertyName">the name of the property which failed to bind</param>
        /// <param name="attemptedValue">the value attempted to set</param>
        /// <param name="innerException">the underlying exception</param>
        public PropertyBindingException(string propertyName, string attemptedValue, Exception innerException = null)
            : base(String.Format(ExceptionMessage, propertyName, attemptedValue), innerException)
        {
            this.PropertyName = propertyName;
            this.AttemptedValue = attemptedValue;
        }
    }
}