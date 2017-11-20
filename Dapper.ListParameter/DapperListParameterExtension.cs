using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Dapper.ListParameter
{
    public static class DapperListParameterExtension
    {
        /// <summary>
        /// Add new dynamic parameter from list of object
        /// </summary>
        /// <typeparam name="T">type of enumerable</typeparam>
        /// <param name="source">dapper dynamic parameters</param>
        /// <param name="name">parameter name</param>
        /// <param name="dataTableType">sql user-defined table type name</param>
        /// <param name="enumerable">list of values</param>
        /// <param name="orderedColumnNames">if more than one column in a TVP, columns order must match order of columns in TVP</param>
        public static void AddList<T>(this DynamicParameters source, string name, string dataTableType, IEnumerable<T> enumerable, IEnumerable<string> orderedColumnNames = null)
        {
            var dataTable = new DataTable();
            var isValueTypeOrString = typeof(T).IsValueType || typeof(T).FullName.Equals("System.String");
            if (isValueTypeOrString)
            {
                var colName = orderedColumnNames == null
                    ? "ColWithoutName"
                    : orderedColumnNames.First();

                var col = GetDataColumn(typeof(T), colName);
                dataTable.Columns.Add(colName);
                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(obj);
                }
            }
            else
            {
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var readableProperties = properties.Where(w => w.CanRead).ToArray();
                if (readableProperties.Length > 1 && orderedColumnNames == null)
                {
                    throw new ArgumentException("Ordered list of column names must be provided when TVP contains more than one column");
                }

                var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name)).ToArray();
                foreach (string colName in columnNames)
                {
                    var propertyType = readableProperties.Single(s => s.Name.Equals(colName)).PropertyType;
                    var col = GetDataColumn(propertyType, colName);
                    dataTable.Columns.Add(col);
                }

                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(columnNames.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj, null)).ToArray());
                }
            }

            source.Add(name, dataTable.AsTableValuedParameter(dataTableType));
        }

        private static DataColumn GetDataColumn(Type type, string name)
        {
            var col = new DataColumn(name);

            if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
            {
                col.AllowDBNull = true;
            }

            if (type.IsEnum)
            {
                col.DataType = typeof(int);
            }
            else
            {
                col.DataType = type.ConvertNullbleTypeToNonNullableType();
            }

            return col;
        }

        private static Type ConvertNullbleTypeToNonNullableType(this Type type)
        {
            if ((!type.IsValueType || Nullable.GetUnderlyingType(type) != null) &&
                type != typeof(string))
            {
                var genericArguments = type.GetGenericArguments().ToList();
                if (genericArguments.Count > 0)
                {
                    var nonNullableType = genericArguments[0];
                    if (nonNullableType.IsEnum)
                    {
                        return typeof(int);
                    }
                    return nonNullableType;
                }
            }

            return type;
        }
    }
}