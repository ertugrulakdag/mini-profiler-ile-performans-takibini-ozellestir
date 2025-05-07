namespace WebAppForMiniProfiler.Extensions
{
    public static class Extension
    {
        public static DateTime? ExtToDateTime(this string? dateTime)
        {
            if (string.IsNullOrWhiteSpace(dateTime))
            {
                return null;
            }
            return DateTime.TryParse(dateTime, out var result) ? result : throw new ArgumentNullException(nameof(dateTime), $"Girdiğiniz tarih değeri '{dateTime}' geçersiz.");
        }
    }
}
