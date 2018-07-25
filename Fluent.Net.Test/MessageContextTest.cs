﻿using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Fluent.Net.Test
{
    public class MessageContextTest : MessageContextTestBase
    {
        private MessageContext CreateAddMessagesContext()
        {
            return CreateContext(@"
                foo = Foo
                -bar = Private Bar
            ");
        }

        [Test]
        public void AddMessage_AddsMessages()
        {
            var ctx = CreateAddMessagesContext();
            ctx._messages.Should().ContainKey("foo");
            ctx._terms.Should().NotContainKey("foo");
            ctx._messages.Should().NotContainKey("-bar");
            ctx._terms.Should().ContainKey("-bar");
        }

        [Test]
        public void AddMessage_PreservesExistingMessagesWhenNewAreAdded()
        {
            var ctx = CreateAddMessagesContext();
            ctx.AddMessages(Ftl(@"
                baz = Baz
            "));

            ctx._messages.Should().ContainKey("foo");
            ctx._terms.Should().NotContainKey("foo");
            ctx._messages.Should().NotContainKey("-bar");
            ctx._terms.Should().ContainKey("-bar");

            ctx._messages.Should().ContainKey("baz");
            ctx._terms.Should().NotContainKey("baz");
        }


        [Test]
        public void AddMessage_MessageAndTermNamesCanBeTheSame()
        {
            var ctx = CreateAddMessagesContext();
            ctx.AddMessages(Ftl(@"
                -foo = Private Foo
            "));

            ctx._messages.Should().ContainKey("foo");
            ctx._terms.Should().NotContainKey("foo");
            ctx._messages.Should().NotContainKey("-foo");
            ctx._terms.Should().ContainKey("-foo");
        }

        [Test]
        public void AddMessage_MessagesWithTheSameIdAreNotOverwritten()
        {
            var ctx = CreateAddMessagesContext();
            var errors = ctx.AddMessages(Ftl(@"
                foo = New Foo
            "));

            // Attempt to overwrite error reported
            errors.Count.Should().Be(1);
            ctx._messages.Count.Should().Be(1);

            var msg = ctx.GetMessage("foo");
            var formatErrors = new List<FluentError>();
            var val = ctx.Format(msg, null, formatErrors);
            val.Should().Be("Foo");
            formatErrors.Count.Should().Be(0);
        }

        private MessageContext CreateHasMessageContext()
        {
            return CreateContext(@"
                foo = Foo
                -bar = Bar
            ");
        }


        [Test]
        public void HasMessage_OnlyReturnsTrueForPublicMessages()
        {
            var ctx = CreateHasMessageContext();
            ctx.HasMessage("foo").Should().BeTrue();
        }

        [Test]
        public void HasMessage_ReturnsFalseForTermsAndMissingMessages()
        {
            var ctx = CreateHasMessageContext();
            ctx.HasMessage("-bar").Should().BeFalse();
            ctx.HasMessage("-baz").Should().BeFalse();
            ctx.HasMessage("-baz").Should().BeFalse();
        }

        [Test]
        public void GetMessage_ReturnsPublicMessages()
        {
            var ctx = CreateHasMessageContext();
            var expected = new RuntimeAst.Message()
            {
                Value = new RuntimeAst.StringExpression() { Value = "Foo" }
            };
            ctx.GetMessage("foo").Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetMessageReturnsNullForTermsAndMissingMessages()
        {
            var ctx = CreateHasMessageContext();
            ctx.GetMessage("-bar").Should().BeNull();
            ctx.GetMessage("-baz").Should().BeNull();
            ctx.GetMessage("-baz").Should().BeNull();
        }
    }
}