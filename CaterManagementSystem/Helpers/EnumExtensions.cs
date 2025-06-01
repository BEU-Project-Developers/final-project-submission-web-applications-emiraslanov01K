// Helpers/EnumExtensions.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CaterManagementSystem.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            // DisplayName atributunu almaq üçün
            var displayName = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();

          
            return displayName ?? enumValue.ToString();
        }
    }
}