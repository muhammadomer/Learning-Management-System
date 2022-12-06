using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace HUC.Web.App.Shared
{
    public class CommaSeparatedValuesModelBinder : DefaultModelBinder
    {
        private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetMethod("ToArray");

        protected override object GetPropertyValue(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, IModelBinder propertyBinder)
        {
            try
            {
                if (propertyDescriptor.PropertyType.GetInterface(typeof(IEnumerable).Name) != null)
                {
                    var actualValue = bindingContext.ValueProvider.GetValue(propertyDescriptor.Name);

                    if (actualValue != null && !String.IsNullOrWhiteSpace(actualValue.AttemptedValue) &&
                        actualValue.AttemptedValue.Contains(","))
                    {
                        var valueType = propertyDescriptor.PropertyType.GetElementType() ??
                                        propertyDescriptor.PropertyType.GetGenericArguments().FirstOrDefault();
                        bool isCommaSeparated = propertyDescriptor.Attributes.OfType<CommaSeparatedAttribute>().Any();

                        if (isCommaSeparated && valueType != null &&
                            valueType.GetInterface(typeof(IConvertible).Name) != null)
                        {
                            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(valueType));

                            foreach (var splitValue in actualValue.AttemptedValue.Split(new[] { ',' }))
                            {
                                list.Add(Convert.ChangeType(splitValue, valueType));
                            }

                            if (propertyDescriptor.PropertyType.IsArray)
                            {
                                return ToArrayMethod.MakeGenericMethod(valueType).Invoke(this, new[] { list });
                            }
                            else
                            {
                                return list;
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CommaSeparatedAttribute : Attribute
    {

    }
}