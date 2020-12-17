using System;
using FluentAssertions;
using OwnID.Extensions;
using Xunit;

namespace OwnID.Tests.Extensions
{
    public class GuidExtensions
    {
        [Fact]
        public void ToShortString_Test()
        {
            var guid = Guid.Parse("2a615a9d-27fb-466d-82a9-6ab3a5d1da12");
            guid.ToShortString().Should().Be("nVphKvsnbUaCqWqzpdHaEg");
        }

        [Fact]
        public void FromShortStringToGuid_Test()
        {
            "nVphKvsnbUaCqWqzpdHaEg".FromShortStringToGuid().Should()
                .Be(Guid.Parse("2a615a9d-27fb-466d-82a9-6ab3a5d1da12"));
        }
    }
}