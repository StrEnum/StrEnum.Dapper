using System;
using System.Data;
using FluentAssertions;
using Moq;
using Xunit;

namespace StrEnum.Dapper.UnitTests
{
    public class StrEnumTypeHandlerTests
    {
        public class Sport : StringEnum<Sport>
        {
            public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
        }

        [Fact]
        public void SetValue_GivenAnEnumMember_ShouldSetParameterTypeToStringAndValueToMemberValue()
        {
            var handler = new StrEnumTypeHandler<Sport>();

            var mockedParameter = new Mock<IDbDataParameter>();
            mockedParameter.SetupAllProperties();

            var parameter = mockedParameter.Object;

            handler.SetValue(parameter, Sport.TrailRunning);

            parameter.DbType.Should().Be(DbType.String);
            parameter.Value.Should().Be("TRAIL_RUNNING");
        }

        [Fact]
        public void Parse_GivenValidEnumValue_ShouldReturnEnumMember()
        {
            var handler = new StrEnumTypeHandler<Sport>();

            var member = handler.Parse("TRAIL_RUNNING");

            member.Should().Be(Sport.TrailRunning);
        }

        [Fact]
        public void Parse_GivenNull_ShouldReturnNull()
        {
            var handler = new StrEnumTypeHandler<Sport>();

            var member = handler.Parse(null);

            member.Should().BeNull();
        }

        [Fact]
        public void Parse_GivenMemberName_ShouldThrowAnException()
        {
            var handler = new StrEnumTypeHandler<Sport>();

            var parse = () => handler.Parse("TrailRunning");

            parse.Should().Throw<ArgumentException>();
        }
    }
}