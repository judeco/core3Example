namespace Interfaces.ExtensionMethods
{
    public static class NullableIntExtensions
    {
        public static bool IsNotPositiveInt(this int? input)
        {
            return input == null || input < 1;
        }
    }
}