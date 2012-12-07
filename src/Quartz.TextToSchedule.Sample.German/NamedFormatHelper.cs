using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quartz.TextToSchedule.Util
{
    /// <summary>
    /// Helper methods to have a named format string.
    /// </summary>
    internal static class NamedFormatHelper
    {
        private static string GetObjectValue(string fieldName, object formatObj, bool throwExceptionIfNotFound)
        {
            Type targetType = null;
            MemberInfo[] members = null;

            if (formatObj is Type)
            {
                targetType = (Type)formatObj;
                members = targetType.GetMember(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            }
            else
            {
                targetType = formatObj.GetType();
                members = targetType.GetMember(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            }

            if (members == null || members.Length == 0)
            {
                if (throwExceptionIfNotFound)
                    throw new Exception(string.Format("fieldName of {0} is not found on type of {1}", fieldName, targetType));
                else
                    return null;
            }

            var member = members[0];
            object returnValue = null;

            if (member is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)member;

                if (property.GetGetMethod().IsStatic)
                    returnValue = property.GetValue(null, null);
                else
                    returnValue = property.GetValue(formatObj, null);
            }
            else if (member is FieldInfo)
            {
                FieldInfo field = (FieldInfo)member;

                if (field.IsStatic)
                    returnValue = field.GetValue(null);
                else
                    returnValue = field.GetValue(formatObj);
            }

            return returnValue == null ? null : returnValue.ToString();
        }

        public static string NamedFormat(string format, object formatObject)
        {
            return NamedFormat(format, null, formatObject);
        }
        public static string NamedFormat(string format, IFormatProvider provider, object formatObject)
        {
            return NamedFormat(format, provider, formatObject, true);
        }
        public static string NamedFormat(string format, IFormatProvider provider, object formatObject, bool throwExceptionIfNotFound)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            List<object> formatValues = new List<object>();
            string rewrittenFormat = r.Replace(format, delegate(Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                string propertyName = propertyGroup.Value;

                formatValues.Add(GetObjectValue(propertyName, formatObject, throwExceptionIfNotFound));

                return new string('{', startGroup.Captures.Count) + (formatValues.Count - 1) + formatGroup.Value
                  + new string('}', endGroup.Captures.Count);
            });

            return string.Format(provider, rewrittenFormat, formatValues.ToArray());
        }
    }
}

