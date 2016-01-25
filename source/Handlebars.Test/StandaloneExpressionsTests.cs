using System.IO;
using NUnit.Framework;

namespace HandlebarsDotNet.Test
{
    [TestFixture]
    public class StandaloneExpressionsTests
    {
        [TestCase("a\r\n{{#with block}}\r\nb\r\n{{/with}}\r\nc")]
        [TestCase("a\n{{#with block}}\nb\n{{/with}}\nc")]
        [TestCase("a\r\n  {{#with block}}  \r\nb\r\n  {{/with}}  \r\nc")]
        public void WhenHelperExpressionsIsOnOwnLine(string source)
        {
            var template = Handlebars.Compile(source);

            var actual = template(new { block = "block" });
            Assert.That(actual, Is.EqualTo("a\r\nb\r\nc"));
        }

        [TestCase("a\r\n{{#with block}}\r\nb{{/with}}  \r\nc", "a\r\nb  \r\nc")] 
        [TestCase("a  {{#with block}}  \r\nb\r\n{{/ with}}  c", "a    \r\nb\r\n  c")] 
        public void WhenHelperExpressionsIsNotOnOwnLine(string source, string expected)
        {
            var template = Handlebars.Compile(source);

            var actual = template(new {block = "block"});
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void NonBlockHelpersAreIgnored()
        {
            var source = "a\n{{this}}\nc";
            var template = Handlebars.Compile(source);

            var actual = template("b");
            Assert.That(actual, Is.EqualTo("a\nb\nc"));
        }

        [Test]
        public void NestedBlocks()
        {
            var source = "{{#with 'block'}}\r\n{{#with 'another block'}}\r\nsome content\r\n{{/with}}\r\n{{/with}}";
            var template = Handlebars.Compile(source);

            var actual = template(null);
            Assert.That(actual, Is.EqualTo("some content\r\n"));
        }

        [Test]
        public void DoubleNewLines()
        {
            var source = "a\r\n{{#with block}}\r\nb\r\n\r\n{{/with}}\r\nc";

            var template = Handlebars.Compile(source);

            var actual = template(new { block = "block"});
            Assert.That(actual, Is.EqualTo("a\r\nb\r\n\r\nc"));
        }
    }
}
