using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BrokerIQ.Dto.Dto.Import;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class Import
    {
        public static string GetSampleHeader(IEnumerable<ImportRecordDefinitionDto> definitions, string delimitier)
        {
            var header = string.Empty;
            foreach (var field in definitions.Where(d => d.Active).OrderBy(d => d.ColumnOrder))
            {
                if (!string.IsNullOrWhiteSpace(header)) header += delimitier;
                header += field.FieldName;
            }

            return header;
        }

        public static string GetPropertyValue<T>(this T theObject, PropertyInfo property, byte maxLength = 18)
        {
            var typeName = property.PropertyType.FullName;

            if (typeName.Contains("DateTime"))
            {
                var dateValue = property.GetValue(theObject);

                return dateValue == null ? string.Empty : ((DateTime)dateValue).ToString("yyyy-MM-dd");
            }

            var propertyValue = property.GetValue(theObject);
            var value = propertyValue is not null ? property.GetValue(theObject).ToString() : string.Empty;
            return value.Length > maxLength ? string.Concat(value.AsSpan(0, 15), "...") : value;
        }
    }
}

