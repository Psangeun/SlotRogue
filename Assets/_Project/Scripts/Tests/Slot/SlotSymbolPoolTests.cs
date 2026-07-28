using NUnit.Framework;
using SlotRogue.Slot.Data;

namespace SlotRogue.Slot.Tests
{
    public sealed class SlotSymbolPoolTests
    {
        [TestCase(SlotSymbolType.Cherry, SlotSymbolPool.DefaultCherryWeight)]
        [TestCase(SlotSymbolType.Lemon, SlotSymbolPool.DefaultLemonWeight)]
        [TestCase(SlotSymbolType.Clover, SlotSymbolPool.DefaultCloverWeight)]
        [TestCase(SlotSymbolType.Bell, SlotSymbolPool.DefaultBellWeight)]
        [TestCase(SlotSymbolType.Diamond, SlotSymbolPool.DefaultDiamondWeight)]
        [TestCase(SlotSymbolType.Seven, SlotSymbolPool.DefaultSevenWeight)]
        public void Reset_Symbols_StartAtDefaultWeight(
            SlotSymbolType symbol,
            float expectedWeight)
        {
            var pool = new SlotSymbolPool();

            Assert.That(pool.GetWeight(symbol), Is.EqualTo(expectedWeight).Within(0.0001f));
        }

        [Test]
        public void Reset_RestoresDefaultWeightsAfterChanges()
        {
            var pool = new SlotSymbolPool();
            pool.AddWeight(SlotSymbolType.Cherry, 5);
            pool.AddWeight(SlotSymbolType.Seven, -2);

            pool.Reset();

            Assert.That(
                pool.GetWeight(SlotSymbolType.Cherry),
                Is.EqualTo(SlotSymbolPool.DefaultWeightFor(SlotSymbolType.Cherry)).Within(0.0001f));
            Assert.That(
                pool.GetWeight(SlotSymbolType.Seven),
                Is.EqualTo(SlotSymbolPool.DefaultWeightFor(SlotSymbolType.Seven)).Within(0.0001f));
        }

        [Test]
        public void DefaultDamage_IsInverseOfDefaultProbabilityCurve()
        {
            Assert.That(
                SlotSymbolAttackValues.DefaultCherryDamage,
                Is.EqualTo(SlotSymbolAttackValues.DefaultLemonDamage));
            Assert.That(
                SlotSymbolAttackValues.DefaultCherryDamage,
                Is.LessThan(SlotSymbolAttackValues.DefaultCloverDamage));
            Assert.That(
                SlotSymbolAttackValues.DefaultLemonDamage,
                Is.LessThan(SlotSymbolAttackValues.DefaultBellDamage));
            Assert.That(
                SlotSymbolAttackValues.DefaultCloverDamage,
                Is.LessThan(SlotSymbolAttackValues.DefaultDiamondDamage));
            Assert.That(
                SlotSymbolAttackValues.DefaultDiamondDamage,
                Is.LessThan(SlotSymbolAttackValues.DefaultSevenDamage));
        }

        [Test]
        public void ResetUniform_SetsSameWeightForAllSymbols()
        {
            var pool = new SlotSymbolPool(4f);

            foreach (SlotSymbolType symbol in SlotSymbolPool.Symbols)
            {
                Assert.That(pool.GetWeight(symbol), Is.EqualTo(4f).Within(0.0001f));
            }
        }

        [Test]
        public void Add_NegativeAmount_ClampsAtZero()
        {
            var pool = new SlotSymbolPool();

            pool.AddWeight(SlotSymbolType.Lemon, -100f);

            Assert.That(pool.GetWeight(SlotSymbolType.Lemon), Is.EqualTo(0f));
        }

        [Test]
        public void IncreaseWeightByProposal_AddsV30DefaultAmount()
        {
            var pool = new SlotSymbolPool();

            pool.IncreaseWeightByProposal(SlotSymbolType.Seven);

            Assert.That(pool.GetWeight(SlotSymbolType.Seven), Is.EqualTo(0.8f).Within(0.0001f));
        }

        [Test]
        public void HalveWeight_MultipliesCurrentWeightByHalf()
        {
            var pool = new SlotSymbolPool();

            pool.HalveWeight(SlotSymbolType.Seven);

            Assert.That(pool.GetWeight(SlotSymbolType.Seven), Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void ProposalWeightOperations_AreOrderSensitive()
        {
            var halveThenIncrease = new SlotSymbolPool();
            halveThenIncrease.HalveWeight(SlotSymbolType.Seven);
            halveThenIncrease.IncreaseWeightByProposal(SlotSymbolType.Seven);

            var increaseThenHalve = new SlotSymbolPool();
            increaseThenHalve.IncreaseWeightByProposal(SlotSymbolType.Seven);
            increaseThenHalve.HalveWeight(SlotSymbolType.Seven);

            Assert.That(
                halveThenIncrease.GetWeight(SlotSymbolType.Seven),
                Is.EqualTo(0.55f).Within(0.0001f));
            Assert.That(
                increaseThenHalve.GetWeight(SlotSymbolType.Seven),
                Is.EqualTo(0.4f).Within(0.0001f));
        }

        [Test]
        public void ProbabilityOf_UsesSymbolWeightOverTotalWeight()
        {
            var pool = new SlotSymbolPool(0);
            pool.SetWeight(SlotSymbolType.Cherry, 3);
            pool.SetWeight(SlotSymbolType.Seven, 1);

            Assert.That(pool.ProbabilityOf(SlotSymbolType.Cherry), Is.EqualTo(0.75d));
            Assert.That(pool.ProbabilityOf(SlotSymbolType.Seven), Is.EqualTo(0.25d));
        }
    }
}
