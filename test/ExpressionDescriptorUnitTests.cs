using System;
using CronExpressionDescriptor;
using FluentAssertions;
using Quartz;
using Xunit;

namespace SilkierQuartz.Test
{
    //7 is used as Sunday on some systems, and considered valid.
    //Quartz uses this by default, and SilkierQuartz should also

    //Allowed value range in comments;
    //https://github.com/quartznet/quartznet/blob/a22915a9abac1568accb93eb24b4cce5331c8249/src/Quartz/CronExpression.cs#L91

    public class ExpressionDescriptorUnitTests
    {
        [Theory(DisplayName = "Parse Expressions")]
        [InlineData("0 0 2 ? * 7 *", "At 02:00, only on Saturday", false)]
        [InlineData("0 0 7 * * ?", "At 07:00", false)]
        [InlineData("0 0 20 * * ?", "At 20:00", false)]
        [InlineData("0 0 20 6 1/1 ? *", "At 20:00, on day 6 of the month", false)]
        [InlineData("0 0 19 20 11 ?", "At 19:00, on day 20 of the month, only in November", false)]
        [InlineData("0 10,15,20 12 ? * 6,7 *", "At 10, 15, and 20 minutes past the hour, at 12:00, only on Friday and Saturday", false)]
        [InlineData("0 30 10-13 ? * FRI#3", "At 30 minutes past the hour, between 10:00 and 13:59, on the third Friday of the month", false)]
        [InlineData("0 43 9 ? * 5L", "At 09:43, on the last Thursday of the month", false)]
        
        [InlineData("0 0 2 ? * 6 *", "At 02:00, only on Saturday", true)]
        [InlineData("0 0 7 * * ?", "At 07:00", true)]
        [InlineData("0 0 20 * * ?", "At 20:00", true)]
        [InlineData("0 0 20 6 1/1 ? *", "At 20:00, on day 6 of the month", true)]
        [InlineData("0 0 19 20 11 ?", "At 19:00, on day 20 of the month, only in November", true)]
        [InlineData("0 10,15,20 12 ? * 5,6 *", "At 10, 15, and 20 minutes past the hour, at 12:00, only on Friday and Saturday", true)]
        [InlineData("0 30 10-13 ? * FRI#3", "At 30 minutes past the hour, between 10:00 and 13:59, on the third Friday of the month", true)]
        [InlineData("0 43 9 ? * 4L", "At 09:43, on the last Thursday of the month", true)]
        public void ShouldParseExpressions(string cron, string expected, bool isZeroBased)
        {
            var options = new Options() {DayOfWeekStartIndexZero = isZeroBased};
            CronExpression exp = null;
            //Ensure quartz properly parses the cron
            var ex = Record.Exception(() => exp = new CronExpression(cron));

            ex.Should().BeNull("Quartz should correctly parse any expression before we can expect a valid description");

            var result = ExpressionDescriptor.GetDescription(cron, options);
            result.Should()
                  .NotBeNull();

            result.Should().Be(expected);
        }
    }
}