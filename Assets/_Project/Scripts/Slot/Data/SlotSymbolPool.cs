using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SlotRogue.Slot.Data
{
    /// <summary>
    /// 런(run)별 심볼 출현 확률 테이블. 심볼 <b>종류</b>는 6종 고정이고,
    /// 각 슬롯 칸은 심볼별 <b>가중치 / 전체 가중치</b> 확률로 독립 추첨합니다.
    ///
    /// 인스턴스는 런 내내 유지(식별자 불변)되며, 새 런 시작 시 <see cref="Reset"/>로
    /// 가중치만 초기화합니다.
    /// </summary>
    public sealed class SlotSymbolPool
    {
        // 기획상 "심볼 풀"이라는 물리적 가방은 쓰지 않는다. 이 값들은 한 칸에 어떤 심볼이
        // 나올지 정하는 상대 가중치이며, 최종 확률은 항상 현재 가중치 합으로 다시 계산한다.

        /// <summary>심볼 가중치 제안의 고정소수 1단위.</summary>
        public const float ProposalWeightUnit = 0.1f;

        /// <summary>심볼 가중치 증가 제안 기본 1회분(체리/레몬 v30 값).</summary>
        public const float ProposalWeightIncrease = 0.3f;

        /// <summary>심볼 가중치 절반 제안 배율.</summary>
        public const float ProposalWeightHalfMultiplier = 0.5f;

        /// <summary>체리 시작 가중치.</summary>
        public const float DefaultCherryWeight = 1.3f;

        /// <summary>레몬 시작 가중치.</summary>
        public const float DefaultLemonWeight = 1.3f;

        /// <summary>클로버 시작 가중치.</summary>
        public const float DefaultCloverWeight = 1.0f;

        /// <summary>종 시작 가중치.</summary>
        public const float DefaultBellWeight = 1.0f;

        /// <summary>다이아 시작 가중치.</summary>
        public const float DefaultDiamondWeight = 0.8f;

        /// <summary>7 시작 가중치.</summary>
        public const float DefaultSevenWeight = 0.5f;

        private static readonly SlotSymbolType[] AllSymbols =
        {
            SlotSymbolType.Cherry,
            SlotSymbolType.Seven,
            SlotSymbolType.Diamond,
            SlotSymbolType.Bell,
            SlotSymbolType.Clover,
            SlotSymbolType.Lemon,
        };

        private static readonly SlotSymbolType[] ProbabilityDisplaySymbols =
        {
            SlotSymbolType.Cherry,
            SlotSymbolType.Lemon,
            SlotSymbolType.Clover,
            SlotSymbolType.Bell,
            SlotSymbolType.Diamond,
            SlotSymbolType.Seven,
        };

        private readonly Dictionary<SlotSymbolType, float> _weights = new();

        public SlotSymbolPool()
        {
            Reset();
        }

        /// <summary>모든 심볼을 같은 가중치로 시작하는 테이블(테스트/디버그용).</summary>
        public SlotSymbolPool(float initialWeightPerSymbol)
        {
            ResetUniform(initialWeightPerSymbol);
        }

        /// <summary>추첨 대상이 되는 모든 심볼 종류(고정).</summary>
        public static IReadOnlyList<SlotSymbolType> Symbols => AllSymbols;

        /// <summary>UI 확률 표시용 고정 순서.</summary>
        public static IReadOnlyList<SlotSymbolType> ProbabilityDisplayOrder =>
            ProbabilityDisplaySymbols;

        /// <summary>현재 테이블의 총 가중치.</summary>
        public float TotalWeight
        {
            get
            {
                float total = 0f;
                foreach (SlotSymbolType symbol in AllSymbols) total += GetWeight(symbol);
                return total;
            }
        }

        /// <summary>기존 저장/테스트 호환용 별칭. 새 코드는 <see cref="TotalWeight"/>를 사용한다.</summary>
        public float Total => TotalWeight;

        public float GetWeight(SlotSymbolType symbol) =>
            _weights.TryGetValue(symbol, out float weight) ? weight : 0f;

        /// <summary>기존 저장/테스트 호환용 별칭. 새 코드는 <see cref="GetWeight"/>를 사용한다.</summary>
        public float GetCount(SlotSymbolType symbol) => GetWeight(symbol);

        public static float DefaultWeightFor(SlotSymbolType symbol) =>
            symbol switch
            {
                SlotSymbolType.Cherry => DefaultCherryWeight,
                SlotSymbolType.Lemon => DefaultLemonWeight,
                SlotSymbolType.Clover => DefaultCloverWeight,
                SlotSymbolType.Bell => DefaultBellWeight,
                SlotSymbolType.Diamond => DefaultDiamondWeight,
                SlotSymbolType.Seven => DefaultSevenWeight,
                _ => 0f,
            };

        /// <summary>기존 저장/테스트 호환용 별칭. 새 코드는 <see cref="DefaultWeightFor"/>를 사용한다.</summary>
        public static float DefaultCountFor(SlotSymbolType symbol) => DefaultWeightFor(symbol);

        public double ProbabilityOf(SlotSymbolType symbol)
        {
            float total = TotalWeight;
            return total > 0f
                ? GetWeight(symbol) / (double)total
                : 1d / AllSymbols.Length;
        }

        /// <summary>새 런 시작 시 호출. 심볼 가중치를 기본 시작값으로 되돌립니다.</summary>
        public void Reset()
        {
            foreach (SlotSymbolType symbol in AllSymbols)
            {
                _weights[symbol] = DefaultWeightFor(symbol);
            }
        }

        /// <summary>모든 심볼을 같은 가중치로 되돌립니다(테스트/디버그용).</summary>
        public void ResetUniform(float initialWeightPerSymbol)
        {
            float start = Math.Max(0f, initialWeightPerSymbol);
            foreach (SlotSymbolType symbol in AllSymbols) _weights[symbol] = start;
        }

        /// <summary>보상 등으로 특정 심볼 가중치를 늘립니다(음수면 감소, 0 미만 방지).</summary>
        public void AddWeight(SlotSymbolType symbol, float amount)
        {
            if (Math.Abs(amount) <= float.Epsilon) return;
            _weights[symbol] = Math.Max(0f, GetWeight(symbol) + amount);
        }

        /// <summary>기존 저장/테스트 호환용 별칭. 새 코드는 <see cref="AddWeight"/>를 사용한다.</summary>
        public void Add(SlotSymbolType symbol, float amount)
        {
            AddWeight(symbol, amount);
        }

        /// <summary>심볼 가중치 증가 제안 1회분을 적용합니다.</summary>
        public void IncreaseWeightByProposal(SlotSymbolType symbol)
        {
            AddWeight(symbol, ProposalWeightIncrease);
        }

        /// <summary>
        /// 심볼 가중치를 절반으로 줄입니다("덜 나온다" 계열 보상용).
        /// </summary>
        public void HalveWeight(SlotSymbolType symbol)
        {
            _weights[symbol] = Math.Max(0f, GetWeight(symbol) * ProposalWeightHalfMultiplier);
        }

        /// <summary>심볼 가중치를 지정값으로 설정합니다(0 미만 방지). 저장된 런 복원에 사용합니다.</summary>
        public void SetWeight(SlotSymbolType symbol, float weight)
        {
            _weights[symbol] = Math.Max(0f, weight);
        }

        /// <summary>기존 저장/테스트 호환용 별칭. 새 코드는 <see cref="SetWeight"/>를 사용한다.</summary>
        public void SetCount(SlotSymbolType symbol, float count)
        {
            SetWeight(symbol, count);
        }

        /// <summary>
        /// 한 칸에 들어갈 심볼 하나를 가중치 기반으로 뽑습니다. <paramref name="exclude"/>는 제외합니다.
        /// 전체 가중치가 0이거나 전부 제외되면 균등 폴백합니다.
        /// </summary>
        public SlotSymbolType Draw(Random random, ISet<SlotSymbolType> exclude = null)
        {
            if (random == null) random = new Random();

            float total = 0f;
            SlotSymbolType fallback = AllSymbols[0];
            bool hasAllowedSymbol = false;
            foreach (SlotSymbolType symbol in AllSymbols)
            {
                if (exclude != null && exclude.Contains(symbol)) continue;
                fallback = symbol;
                hasAllowedSymbol = true;
                total += GetWeight(symbol);
            }

            if (total <= 0f || !hasAllowedSymbol)
            {
                return AllSymbols[random.Next(AllSymbols.Length)];
            }

            double roll = random.NextDouble() * total;
            foreach (SlotSymbolType symbol in AllSymbols)
            {
                if (exclude != null && exclude.Contains(symbol)) continue;
                roll -= GetWeight(symbol);
                if (roll < 0d) return symbol;
            }

            return fallback;
        }

        public string BuildSummary()
        {
            var builder = new StringBuilder();
            foreach (SlotSymbolType symbol in AllSymbols)
            {
                builder
                    .Append(symbol)
                    .Append(' ')
                    .Append((ProbabilityOf(symbol) * 100d).ToString("0.#", CultureInfo.InvariantCulture))
                    .Append("% (p")
                    .Append(GetWeight(symbol).ToString("0.###", CultureInfo.InvariantCulture))
                    .Append(")   ");
            }
            return builder.ToString().TrimEnd();
        }
    }
}
