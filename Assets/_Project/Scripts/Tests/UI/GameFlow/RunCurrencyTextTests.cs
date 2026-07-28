using NUnit.Framework;
using SlotRogue.UI.GameFlow;

namespace SlotRogue.UI.Tests.GameFlow
{
    public sealed class RunCurrencyTextTests
    {
        [TestCase(12, "12")]
        [TestCase(-4, "0")]
        public void FormatPlainAmount_DoesNotAddSpriteTag(int amount, string expected)
        {
            string text = RunCurrencyText.FormatPlainAmount(amount);

            Assert.That(text, Is.EqualTo(expected));
            Assert.That(text, Does.Not.Contain("<sprite"));
        }

        [TestCase(1, "+1")]
        [TestCase(-4, "+0")]
        public void FormatBonusAmount_ShowsPositiveRewardLabel(int amount, string expected)
        {
            string text = RunCurrencyText.FormatBonusAmount(amount);

            Assert.That(text, Is.EqualTo(expected));
            Assert.That(text, Does.Not.Contain("/"));
        }
    }
}
