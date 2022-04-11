namespace ReviewIt.Web.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    public class RequiredIfAttribute : RequiredAttribute
    {
        public string PropertyName { get; set; }
        public object DesiredValue { get; set; }

        public RequiredIfAttribute(string propertyName = null, object desiredvalue = null)
        {
            PropertyName = propertyName;
            DesiredValue = desiredvalue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if(context != null)
            {
                object instance = context.ObjectInstance;
                Type type = instance.GetType();
                bool submit = (bool?)type.GetProperty("Submit")?.GetValue(instance, null) ?? true;

                if (submit)
                {
                    PropertyInfo prop = type.GetProperty(
                        string.IsNullOrWhiteSpace(this.PropertyName) ? string.Empty : this.PropertyName);

                    if (string.IsNullOrWhiteSpace(this.PropertyName)
                        || (prop?.GetValue(instance, null) ?? null)?.ToString() == this.DesiredValue.ToString()
                        || this.EnumCompare(prop, instance))
                    {
                        return base.IsValid(value, context);
                    }
                }
            }


            var result = ValidationResult.Success;
            return result;
        }

        private bool EnumCompare(PropertyInfo enumProp, object instance)
        {
            if (!(enumProp.PropertyType.IsGenericType && enumProp.PropertyType.GenericTypeArguments.Any(x => x.IsEnum)))
            {
                return false;
            }

            var enumType = enumProp.PropertyType.GenericTypeArguments.First(x => x.IsEnum);

            var actualValue = Enum.Parse(enumType, enumProp.GetValue(instance, null).ToString());
            var desiredValue = Enum.Parse(enumType, this.DesiredValue.ToString());

            return actualValue.Equals(desiredValue);
        }
    }

}