using System;
using System.Collections.Generic;
using SlotRogue.Relics.Pool;
using SlotRogue.Slot.Data;

namespace SlotRogue.UI.GameFlow
{
    /// <summary>
    /// 전투 중 유물 상점의 도메인 로직(제안 롤·가격 산정·구매 적용·리롤)을 담는 순수 C# 모델이다.
    /// UI/전투 상태 게이팅과 화면 상태 매핑은 <see cref="BattleScreenController"/>가 담당한다.
    /// </summary>
    internal sealed class RelicShopModel
    {
        public const int OfferCount = 3;
        public const int RerollCost = 1;

        private readonly RelicDefinition[] _offers = new RelicDefinition[OfferCount];
        private readonly bool[] _purchased = new bool[OfferCount];
        private readonly System.Random _random = new();

        public int Count => _offers.Length;

        public RelicDefinition OfferAt(int index)
        {
            return index >= 0 && index < _offers.Length ? _offers[index] : null;
        }

        public bool IsPurchased(int index)
        {
            return index >= 0 && index < _purchased.Length && _purchased[index];
        }

        public int CostOf(int index)
        {
            return CostOfRelic(OfferAt(index));
        }

        public bool CanPurchase(int index)
        {
            RelicDefinition relic = OfferAt(index);
            return relic != null && CanPurchaseRelic(relic);
        }

        public bool HasAnyOffer()
        {
            for (int index = 0; index < _offers.Length; index++)
            {
                if (_offers[index] != null && !_purchased[index])
                {
                    return true;
                }
            }

            return false;
        }

        public void Roll()
        {
            IReadOnlyList<RelicDefinition> pool = RelicCatalog.RewardPool;
            var candidates = new List<RelicDefinition>(pool.Count);
            for (int index = 0; index < pool.Count; index++)
            {
                RelicDefinition relic = pool[index];
                if (relic != null && CanOfferRelic(relic))
                {
                    candidates.Add(relic);
                }
            }

            for (int index = 0; index < _offers.Length; index++)
            {
                _offers[index] = PickAndRemove(candidates);
                _purchased[index] = false;
            }
        }

        public bool TryReroll()
        {
            if (!GameFlowSession.TrySpendRunCoins(RerollCost))
            {
                return false;
            }

            Roll();
            return true;
        }

        public bool TryPurchase(int offerIndex)
        {
            if (offerIndex < 0 ||
                offerIndex >= _offers.Length ||
                _purchased[offerIndex])
            {
                return false;
            }

            RelicDefinition relic = _offers[offerIndex];
            if (relic == null || !CanPurchaseRelic(relic))
            {
                return false;
            }

            int cost = CostOfRelic(relic);
            if (!GameFlowSession.TrySpendRunCoins(cost))
            {
                return false;
            }

            if (!ApplyRelicShopPurchase(relic))
            {
                GameFlowSession.AddRunCoins(cost);
                return false;
            }

            _purchased[offerIndex] = true;
            return true;
        }

        private static bool ApplyRelicShopPurchase(RelicDefinition relic)
        {
            if (relic == null)
            {
                return false;
            }

            // v30 유물 효과는 실행 엔진에서 처리한다 — 구매 시점엔 인벤토리에 넣기만 한다.
            if (relic.OccupiesSlot)
            {
                return GameFlowSession.TryAddRelic(relic);
            }

            return true;
        }

        private static bool CanOfferRelic(RelicDefinition relic)
        {
            if (relic == null)
            {
                return false;
            }

            return relic.MaxCopies <= 0 ||
                GameFlowSession.CountOwnedRelic(relic.Id) < relic.MaxCopies;
        }

        private static bool CanPurchaseRelic(RelicDefinition relic)
        {
            return CanOfferRelic(relic) &&
                (!relic.OccupiesSlot || GameFlowSession.CanAddRelic(relic));
        }

        private RelicDefinition PickAndRemove(List<RelicDefinition> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            int index = _random.Next(candidates.Count);
            RelicDefinition relic = candidates[index];
            candidates.RemoveAt(index);
            return relic;
        }

        private int CostOfRelic(RelicDefinition relic)
        {
            if (relic == null)
            {
                return 0;
            }

            int cost = relic.Price > 0
                ? relic.Price
                : relic.Grade switch
            {
                RelicGrade.Curse => 3,
                RelicGrade.Common => 3,
                RelicGrade.Uncommon => 5,
                RelicGrade.Rare => 8,
                RelicGrade.Legendary => 13,
                _ => 3,
            };

            cost -= GameFlowSession.ShopDiscount;
            return Math.Max(1, cost);
        }
    }

    /// <summary>
    /// 한 전투에서 광고로 받을 수 있는 별조각 횟수와 지급량을 관리한다.
    /// 광고 SDK 상태와 무관한 순수 규칙이므로 전투 시작 시 Reset하고 reward callback에서만 소비한다.
    /// </summary>
    internal sealed class WaveAdRewardModel
    {
        internal const int MaxClaims = 2;
        internal const int RewardPerClaim = 1;

        internal int ClaimedCount { get; private set; }

        internal int RemainingCount => Math.Max(0, MaxClaims - ClaimedCount);

        internal bool CanClaim => RemainingCount > 0;

        internal void Reset()
        {
            ClaimedCount = 0;
        }

        internal bool TryClaim(out int reward)
        {
            if (!CanClaim)
            {
                reward = 0;
                return false;
            }

            ClaimedCount++;
            reward = RewardPerClaim;
            return true;
        }
    }
}
