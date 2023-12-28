using ISPAddressChecker.Helpers;

namespace ISPAddressCheckerTests
{
    public class StringHelpersTests
    {
        [Fact]
        public void MakeISPAddressLogReady_ValidISPAddress_ReturnsMaskedIPAddress()
        {
            // Arrange
            string ISPAddress = "192.168.1.100";

            // Act
            string result = StringHelpers.MakeISPAddressLogReady(ISPAddress);

            // Assert
            Assert.Equal("192.1**.1.1**", result);
        }

        [Fact]
        public void MakeISPAddressLogReady_NullISPAddress_ReturnsEmptyString()
        {
            // Arrange
            string ISPAddress = null;

            // Act
            string result = StringHelpers.MakeISPAddressLogReady(ISPAddress);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void MakeISPAddressLogReady_EmptyISPAddress_ReturnsEmptyString()
        {
            // Arrange
            string ISPAddress = string.Empty;

            // Act
            string result = StringHelpers.MakeISPAddressLogReady(ISPAddress);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void MakeISPAddressLogReady_WhitespaceISPAddress_ReturnsEmptyString()
        {
            // Arrange
            string ISPAddress = "   ";

            // Act
            string result = StringHelpers.MakeISPAddressLogReady(ISPAddress);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void MakeEmailAddressLogReady_ShouldMaskEmailAddress()
        {
            // Arrange
            string emailAddress = "example@example.com";
            string expected = "ex***@example.com";

            // Act
            string actual = StringHelpers.MakeEmailAddressLogReady(emailAddress);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestMakeHttpRequestHostDashboardReady()
        {
            // Arrange
            string host = null;
            string expected = "NoHostFound";

            // Act
            string result1 = StringHelpers.MakeHttpRequestHostDashboardReady(host);

            // Assert
            Assert.Equal(expected, result1);
        }

        [Fact]
        public void TestMakeHttpRequestHostDashboardReady_EmptyHost_ReturnsNoHostFound()
        {
            // Arrange
            string host = "";
            string expected = "NoHostFound";

            // Act
            string result2 = StringHelpers.MakeHttpRequestHostDashboardReady(host);

            // Assert
            Assert.Equal(expected, result2);
        }

        [Fact]
        public void TestMakeHttpRequestHostDashboardReady_ShortHost_ReturnsMaskedHost()
        {
            // Arrange
            string host = "abc";
            string expected = "***";

            // Act
            string result3 = StringHelpers.MakeHttpRequestHostDashboardReady(host);

            // Assert
            Assert.Equal(expected, result3);
        }

        [Fact]
        public void TestMakeHttpRequestHostDashboardReady_LongHost_ReturnsMaskedHost()
        {
            // Arrange
            string host = "example.com";
            string expected = "example****";

            // Act
            string result4 = StringHelpers.MakeHttpRequestHostDashboardReady(host);

            // Assert
            Assert.Equal(expected, result4);
        }
    }
}
