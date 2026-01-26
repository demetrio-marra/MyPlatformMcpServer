namespace MyPlatformModels.Helpers
{
    public class MyPlatformEnumHelper
    {
        public const string UNKNOWN_VALUE = "UNKNOWN_VALUE";

        public static string EnumToLabel<T>(int value) where T : struct, Enum
        {
            if (Enum.IsDefined(typeof(T), value))
            {
                return ((T)(object)value).ToString();
            }
            return UNKNOWN_VALUE; // Return the int as string if enum value doesn't exist
        }


        public static T? LabelToEnum<T>(string? label) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(label))
                return null;

            if (Enum.TryParse<T>(label, ignoreCase: true, out var result))
                return result;

            return null;
        }
    }
}