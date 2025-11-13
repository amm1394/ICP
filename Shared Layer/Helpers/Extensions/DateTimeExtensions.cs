namespace Shared.Icp.Helpers.Extensions
{
    /// <summary>
    /// Extension methods that provide common helpers for working with <see cref="DateTime"/> values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These helpers cover Persian (Jalali) date formatting, day/month boundaries, range checks, and a simple age
    /// computation. They do not mutate the input and are implemented as pure extension methods.
    /// </para>
    /// <para>
    /// Time zone guidance: these methods do not change the <see cref="DateTime.Kind"/> or convert time zones unless
    /// explicitly documented. Prefer UTC for storage/transfer (ISO-8601 in JSON) and apply conversions at the
    /// application boundaries as needed.
    /// </para>
    /// </remarks>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Formats a <see cref="DateTime"/> as a Persian (Jalali) date string in the form yyyy/MM/dd.
        /// </summary>
        /// <param name="dateTime">The date/time value to render using the Persian calendar.</param>
        /// <returns>
        /// A string in the format <c>yyyy/MM/dd</c> representing the Persian calendar date corresponding to <paramref name="dateTime"/>.
        /// </returns>
        /// <remarks>
        /// Uses <see cref="System.Globalization.PersianCalendar"/> to compute the year, month, and day. Digits are rendered
        /// using Latin numerals by string interpolation; replace characters if Persian numerals are desired in the UI.
        /// This does not perform any time zone conversion.
        /// </remarks>
        public static string ToPersianDate(this DateTime dateTime)
        {
            var persianCalendar = new System.Globalization.PersianCalendar();
            var year = persianCalendar.GetYear(dateTime);
            var month = persianCalendar.GetMonth(dateTime);
            var day = persianCalendar.GetDayOfMonth(dateTime);

            return $"{year:0000}/{month:00}/{day:00}";
        }

        /// <summary>
        /// Formats a <see cref="DateTime"/> as a Persian (Jalali) date plus a 24-hour time component (HH:mm:ss).
        /// </summary>
        /// <param name="dateTime">The date/time value to format.</param>
        /// <returns>
        /// A string combining <see cref="ToPersianDate(System.DateTime)"/> with the time portion of <paramref name="dateTime"/>.
        /// </returns>
        /// <remarks>
        /// The date portion is based on the Persian calendar; the time portion is taken directly from the provided
        /// <paramref name="dateTime"/> without time zone conversion.
        /// </remarks>
        public static string ToPersianDateTime(this DateTime dateTime)
        {
            return $"{dateTime.ToPersianDate()} {dateTime:HH:mm:ss}";
        }

        /// <summary>
        /// Returns the start of the day (00:00:00.0000000) for the specified date.
        /// </summary>
        /// <param name="dateTime">The date/time value whose day start is desired.</param>
        /// <returns>
        /// A <see cref="DateTime"/> at midnight for the given date, preserving the original <see cref="DateTime.Kind"/>.
        /// </returns>
        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// Returns the end of the day (23:59:59.9999999) for the specified date.
        /// </summary>
        /// <param name="dateTime">The date/time value whose day end is desired.</param>
        /// <returns>
        /// A <see cref="DateTime"/> representing the last tick of the given day (inclusive upper bound).
        /// </returns>
        /// <remarks>
        /// Implemented as <c>date.Date.AddDays(1).AddTicks(-1)</c>, leveraging .NET's 100-nanosecond tick resolution.
        /// </remarks>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// Returns the first instant (00:00:00.0000000) of the month for the specified date.
        /// </summary>
        /// <param name="dateTime">The date/time value whose month start is desired.</param>
        /// <returns>
        /// A <see cref="DateTime"/> set to the first day of the month at midnight.
        /// </returns>
        /// <remarks>
        /// Note: This constructor does not preserve <see cref="DateTime.Kind"/> and creates an unspecified kind result.
        /// </remarks>
        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        /// <summary>
        /// Returns the last instant (23:59:59.9999999) of the month for the specified date.
        /// </summary>
        /// <param name="dateTime">The date/time value whose month end is desired.</param>
        /// <returns>
        /// A <see cref="DateTime"/> representing the last tick of the month (inclusive upper bound).
        /// </returns>
        /// <remarks>
        /// Implemented as <c>StartOfMonth().AddMonths(1).AddTicks(-1)</c>. As with <see cref="StartOfMonth"/>, the result
        /// has an unspecified <see cref="DateTime.Kind"/>.
        /// </remarks>
        public static DateTime EndOfMonth(this DateTime dateTime)
        {
            return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
        }

        /// <summary>
        /// Calculates the age in whole years as of today based on a birth date.
        /// </summary>
        /// <param name="birthDate">The birth date to evaluate.</param>
        /// <returns>
        /// The age in full years. If <paramref name="birthDate"/> is in the future, the result may be negative.
        /// </returns>
        /// <remarks>
        /// Uses the Gregorian calendar and today's date (local time) for computation. The result is decremented by
        /// one year if the birthday has not occurred yet in the current year.
        /// </remarks>
        public static int CalculateAge(this DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// Determines whether a <see cref="DateTime"/> lies within a closed interval [start, end].
        /// </summary>
        /// <param name="dateTime">The value to test.</param>
        /// <param name="start">The inclusive start boundary.</param>
        /// <param name="end">The inclusive end boundary.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="dateTime"/> is greater than or equal to <paramref name="start"/> and less than
        /// or equal to <paramref name="end"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// No validation is performed to ensure <paramref name="start"/> is less than or equal to <paramref name="end"/>;
        /// if the bounds are reversed, the result will be <c>false</c> for all inputs.
        /// </remarks>
        public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
        {
            return dateTime >= start && dateTime <= end;
        }
    }
}