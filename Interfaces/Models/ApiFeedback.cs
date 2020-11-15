using static Common.Constants.HttpConstants;

namespace Common.Models
{
    public class ApiFeedback
    {
        public int HttpCode { get; }
        public string? Explanation { get; }

        public ApiFeedback()
        {
            HttpCode = HttpOk;
        }

        public ApiFeedback(int httpCode, string? explanation)
        {
            HttpCode = httpCode;
            Explanation = explanation;
        }

        public ApiFeedback(int httpCode)
        {
            HttpCode = httpCode;
        }

        public bool Equals(ApiFeedback? input)
        {
            if (input == null)
            {
                return false;
            }
            return HttpCode == input.HttpCode && Explanation == input.Explanation;
        }

        public override int GetHashCode()
        {
            var hash = 19;
            unchecked
            { // allow "wrap around" in the int
                hash = hash * 31 + HttpCode;
                hash = hash * 31 + (Explanation == null ? 0 : Explanation.GetHashCode());
            }
            return hash;
        }
    }
}