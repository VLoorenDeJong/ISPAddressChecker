using ISPAdressChecker.Helpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
