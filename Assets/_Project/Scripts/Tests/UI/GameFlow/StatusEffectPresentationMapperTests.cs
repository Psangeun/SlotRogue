using System;
using NUnit.Framework;
using SlotRogue.Core.Combat;
using SlotRogue.UI.Combat.Presentation;
using SlotRogue.UI.GameFlow;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SlotRogue.UI.Tests.GameFlow
{
    public sealed class StatusEffectPresentationMapperTests
    {
        [TestCase(StatusEffectKind.Burn, 3, 8, 9, 3)]
        [TestCase(StatusEffectKind.Infection, 7, 2, 9, 2)]
        [TestCase(StatusEffectKind.Vulnerable, 7, 1, 9, 1)]
        [TestCase(StatusEffectKind.Weaken, 7, 4, 9, 4)]
        [TestCase(StatusEffectKind.Lifesteal, 7, 5, 9, 5)]
        [TestCase(StatusEffectKind.Thorns, 6, 8, 9, 6)]
        [TestCase(StatusEffectKind.Freeze, 7, 8, 2, 2)]
        public void Map_UsesStatusSpecificDisplayValue(
            StatusEffectKind kind,
            int magnitude,
            int stackCount,
            int remainingTurns,
            int expectedDisplayValue)
        {
            StatusEffectViewData result = StatusEffectPresentationMapper.Map(
                kind,
                magnitude,
                stackCount,
                remainingTurns);

            Assert.That(result.Kind, Is.EqualTo(kind));
            Assert.That(result.DisplayValue, Is.EqualTo(expectedDisplayValue));
            Assert.That(result.ShowValue, Is.True);
        }

        [Test]
        public void Map_DisplayValueOne_RemainsVisible()
        {
            StatusEffectViewData result = StatusEffectPresentationMapper.Map(
                StatusEffectKind.Vulnerable,
                magnitude: 0,
                stackCount: 1,
                remainingTurns: 0);

            Assert.That(result.DisplayValue, Is.EqualTo(1));
            Assert.That(result.ShowValue, Is.True);
        }

        [Test]
        public void Map_NonPositiveDisplayValue_HidesValue()
        {
            StatusEffectViewData result = StatusEffectPresentationMapper.Map(
                StatusEffectKind.Burn,
                magnitude: 0,
                stackCount: 5,
                remainingTurns: 5);

            Assert.That(result.DisplayValue, Is.Zero);
            Assert.That(result.ShowValue, Is.False);
        }

        [Test]
        public void Map_UnsupportedKind_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                StatusEffectPresentationMapper.Map(
                    (StatusEffectKind)999,
                    magnitude: 1,
                    stackCount: 1,
                    remainingTurns: 1));
        }

        [TestCase(StatusEffectKind.Lifesteal, StatusEffectDisplayGroup.Buff)]
        [TestCase(StatusEffectKind.Thorns, StatusEffectDisplayGroup.Buff)]
        [TestCase(StatusEffectKind.Burn, StatusEffectDisplayGroup.Debuff)]
        [TestCase(StatusEffectKind.Freeze, StatusEffectDisplayGroup.Debuff)]
        [TestCase(StatusEffectKind.Infection, StatusEffectDisplayGroup.Debuff)]
        [TestCase(StatusEffectKind.Vulnerable, StatusEffectDisplayGroup.Debuff)]
        [TestCase(StatusEffectKind.Weaken, StatusEffectDisplayGroup.Debuff)]
        public void GetDisplayGroup_ClassifiesBuffsAndDebuffs(
            StatusEffectKind kind,
            StatusEffectDisplayGroup expected)
        {
            Assert.That(
                StatusEffectPresentationMapper.GetDisplayGroup(kind),
                Is.EqualTo(expected));
        }

        [Test]
        public void PresentationState_AddOrReplace_DoesNotDuplicateSameKind()
        {
            var state = new CombatStatusPresentationState();
            var participantId = new CombatParticipantId(100);

            state.AddOrReplace(
                participantId,
                new StatusEffectViewData(StatusEffectKind.Infection, 2, showValue: true));
            state.AddOrReplace(
                participantId,
                new StatusEffectViewData(StatusEffectKind.Infection, 5, showValue: true));

            StatusEffectViewData[] statuses = state.GetAll(participantId);
            Assert.That(statuses, Has.Length.EqualTo(1));
            Assert.That(statuses[0].DisplayValue, Is.EqualTo(5));
        }

        [Test]
        public void PresentationState_Remove_DoesNotAffectOtherParticipant()
        {
            var state = new CombatStatusPresentationState();
            var firstParticipantId = new CombatParticipantId(100);
            var secondParticipantId = new CombatParticipantId(101);
            var status = new StatusEffectViewData(
                StatusEffectKind.Vulnerable,
                displayValue: 2,
                showValue: true);
            state.AddOrReplace(firstParticipantId, status);
            state.AddOrReplace(secondParticipantId, status);

            state.Remove(firstParticipantId, StatusEffectKind.Vulnerable);

            Assert.That(state.GetAll(firstParticipantId), Is.Empty);
            Assert.That(state.GetAll(secondParticipantId), Has.Length.EqualTo(1));
        }

        [Test]
        public void PlayerStatusPanel_RenderPositionsBuffsFromTopAndDebuffsFromBottom()
        {
            const string PrefabPath = "Assets/_Project/Prefabs/UI/RunGame/Bottom Panel.prefab";

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                RectTransform[] rectTransforms =
                    prefabRoot.GetComponentsInChildren<RectTransform>(includeInactive: true);
                RectTransform panelRoot = Array.Find(
                    rectTransforms,
                    rectTransform => rectTransform.name == "Player Stat Panel");

                Assert.That(panelRoot, Is.Not.Null);

                PlayerStatusPanelView view = panelRoot.GetComponent<PlayerStatusPanelView>();
                Assert.That(view, Is.Not.Null);
                Assert.That(view.EnsureReferences(), Is.True);

                view.Render(new[]
                {
                    new StatusEffectViewData(StatusEffectKind.Thorns, 3, showValue: true),
                    new StatusEffectViewData(StatusEffectKind.Lifesteal, 1, showValue: true),
                    new StatusEffectViewData(StatusEffectKind.Infection, 4, showValue: true),
                    new StatusEffectViewData(StatusEffectKind.Weaken, 2, showValue: true),
                });

                RectTransform firstBuff = FindSlotByText(panelRoot, "가시 3");
                RectTransform secondBuff = FindSlotByText(panelRoot, "흡혈 1");
                RectTransform firstDebuff = FindSlotByText(panelRoot, "감염 4");
                RectTransform secondDebuff = FindSlotByText(panelRoot, "약화 2");

                Assert.That(firstBuff, Is.Not.Null);
                Assert.That(secondBuff, Is.Not.Null);
                Assert.That(firstDebuff, Is.Not.Null);
                Assert.That(secondDebuff, Is.Not.Null);
                Assert.That(firstBuff.anchorMin.y, Is.EqualTo(1f));
                Assert.That(firstBuff.anchoredPosition.y, Is.EqualTo(0f));
                Assert.That(secondBuff.anchorMin.y, Is.EqualTo(1f));
                Assert.That(secondBuff.anchoredPosition.y, Is.LessThan(0f));
                Assert.That(firstDebuff.anchorMin.y, Is.EqualTo(0f));
                Assert.That(firstDebuff.anchoredPosition.y, Is.EqualTo(0f));
                Assert.That(secondDebuff.anchorMin.y, Is.EqualTo(0f));
                Assert.That(secondDebuff.anchoredPosition.y, Is.GreaterThan(0f));
                Assert.That(firstBuff.GetComponentsInChildren<Image>(includeInactive: false), Has.Length.EqualTo(1));
                Assert.That(firstDebuff.GetComponentsInChildren<Image>(includeInactive: false), Has.Length.EqualTo(1));
                Assert.That(firstBuff.GetComponent<Image>().sprite.name, Is.EqualTo("ingame_statback_0"));
                Assert.That(firstDebuff.GetComponent<Image>().sprite.name, Is.EqualTo("ingame_statback_1"));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static RectTransform FindSlotByText(RectTransform panelRoot, string text)
        {
            for (int index = 0; index < panelRoot.childCount; index++)
            {
                var child = (RectTransform)panelRoot.GetChild(index);
                if (child.gameObject.activeSelf && GetFirstTmpText(child) == text)
                {
                    return child;
                }
            }

            return null;
        }

        private static string GetFirstTmpText(RectTransform root)
        {
            Component[] components = root.GetComponentsInChildren<Component>(includeInactive: true);
            for (int index = 0; index < components.Length; index++)
            {
                Component component = components[index];
                if (component != null &&
                    component.GetType().FullName == "TMPro.TextMeshProUGUI")
                {
                    return component.GetType()
                        .GetProperty("text")
                        ?.GetValue(component) as string;
                }
            }

            return null;
        }

    }
}
