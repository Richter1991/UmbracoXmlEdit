using System;

namespace UmbracoXmlEdit.Helpers
{
    class ParseHelper
    {
        internal static object GetTypedValue<T>(string value)
        {
            var currentType = typeof(T);
            object typedValue = null;

            bool validParsed = true;
            if (currentType == typeof(int))
            {
                int intValue;
                if (int.TryParse(value, out intValue))
                {
                    typedValue = intValue;
                }
                else
                {
                    validParsed = false;
                }
            }
            else if (currentType == typeof(Guid))
            {
                Guid guidValue;
                if (Guid.TryParse(value, out guidValue))
                {
                    typedValue = guidValue;
                }
                else
                {
                    validParsed = false;
                }
            }
            else if (currentType == typeof(DateTime))
            {
                DateTime dateTimeValue;
                if (DateTime.TryParse(value, out dateTimeValue))
                {
                    typedValue = dateTimeValue;
                }
                else
                {
                    validParsed = false;
                }
            }
            else if (currentType == typeof(string))
            {
                // Set value as string
                typedValue = value;
            }
            else
            {
                throw new NotImplementedException(string.Format("Type '{0}' is not implemented", currentType.Name));
            }

            if (!validParsed)
            {
                throw new InvalidCastException(string.Format("Couldn't parse value '{0}' to type '{1}'", value, currentType.Name));
            }

            return typedValue;
        }
    }
}