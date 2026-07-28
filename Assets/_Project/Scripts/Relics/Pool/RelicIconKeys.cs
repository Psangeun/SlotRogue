namespace SlotRogue.Relics.Pool
{
    public static class RelicIconKeys
    {
        public const string SheetAddress = "Relic Sheet 300";

        public const string Slot00 = SheetAddress + "[icon-Sheet_300_0]";
        public const string Slot01 = SheetAddress + "[icon-Sheet_300_1]";
        public const string Slot02 = SheetAddress + "[icon-Sheet_300_2]";
        public const string Slot03 = SheetAddress + "[icon-Sheet_300_3]";
        public const string Slot04 = SheetAddress + "[icon-Sheet_300_4]";
        public const string Slot05 = SheetAddress + "[icon-Sheet_300_5]";
        public const string Slot06 = SheetAddress + "[icon-Sheet_300_6]";
        public const string Slot07 = SheetAddress + "[icon-Sheet_300_7]";
        public const string Slot08 = SheetAddress + "[icon-Sheet_300_8]";
        public const string Slot09 = SheetAddress + "[icon-Sheet_300_9]";
        public const string Slot10 = SheetAddress + "[icon-Sheet_300_10]";
        public const string Slot11 = SheetAddress + "[icon-Sheet_300_11]";
        public const string Slot12 = SheetAddress + "[icon-Sheet_300_12]";
        public const string Slot13 = SheetAddress + "[icon-Sheet_300_13]";
        public const string Slot14 = SheetAddress + "[icon-Sheet_300_14]";
        public const string Slot15 = SheetAddress + "[icon-Sheet_300_15]";
        public const string Slot16 = SheetAddress + "[icon-Sheet_300_16]";
        public const string Slot17 = SheetAddress + "[icon-Sheet_300_17]";
        public const string Slot18 = SheetAddress + "[icon-Sheet_300_18]";
        public const string Slot19 = SheetAddress + "[icon-Sheet_300_19]";
        public const string Slot20 = SheetAddress + "[icon-Sheet_300_20]";
        public const string Slot21 = SheetAddress + "[icon-Sheet_300_21]";
        public const string Slot22 = SheetAddress + "[icon-Sheet_300_22]";
        public const string Slot23 = SheetAddress + "[icon-Sheet_300_23]";
        public const string Slot24 = SheetAddress + "[icon-Sheet_300_24]";
        public const string Slot25 = SheetAddress + "[icon-Sheet_300_25]";
        public const string Slot26 = SheetAddress + "[icon-Sheet_300_26]";
        public const string Slot27 = SheetAddress + "[icon-Sheet_300_27]";
        public const string Slot28 = SheetAddress + "[icon-Sheet_300_28]";
        public const string Slot29 = SheetAddress + "[icon-Sheet_300_29]";
        public const string Slot30 = SheetAddress + "[icon-Sheet_300_30]";
        public const string Slot31 = SheetAddress + "[icon-Sheet_300_31]";
        public const string Slot32 = SheetAddress + "[icon-Sheet_300_32]";
        public const string Slot33 = SheetAddress + "[icon-Sheet_300_33]";
        public const string Slot34 = SheetAddress + "[icon-Sheet_300_34]";
        public const string Slot35 = SheetAddress + "[icon-Sheet_300_35]";
        public const string Slot36 = SheetAddress + "[icon-Sheet_300_36]";
        public const string Slot37 = SheetAddress + "[icon-Sheet_300_37]";
        public const string Slot38 = SheetAddress + "[icon-Sheet_300_38]";
        public const string Slot39 = SheetAddress + "[icon-Sheet_300_39]";
        public const string Slot40 = SheetAddress + "[icon-Sheet_300_40]";
        public const string Slot41 = SheetAddress + "[icon-Sheet_300_41]";
        public const string Slot42 = SheetAddress + "[icon-Sheet_300_42]";
        public const string Slot43 = SheetAddress + "[icon-Sheet_300_43]";
        public const string Slot44 = SheetAddress + "[icon-Sheet_300_44]";
        public const string Slot45 = SheetAddress + "[icon-Sheet_300_45]";
        public const string Slot46 = SheetAddress + "[icon-Sheet_300_46]";
        public const string Slot47 = SheetAddress + "[icon-Sheet_300_47]";
        public const string Slot48 = SheetAddress + "[icon-Sheet_300_48]";
        public const string Slot49 = SheetAddress + "[icon-Sheet_300_49]";
        public const string Slot50 = SheetAddress + "[icon-Sheet_300_50]";
        public const string Slot51 = SheetAddress + "[icon-Sheet_300_51]";
        public const string Slot52 = SheetAddress + "[icon-Sheet_300_52]";
        public const string Slot53 = SheetAddress + "[icon-Sheet_300_53]";
        public const string Slot54 = SheetAddress + "[icon-Sheet_300_54]";
        public const string Slot55 = SheetAddress + "[icon-Sheet_300_55]";

        public const string Default = Slot00;

        public static readonly string[] All =
        {
            Slot00,
            Slot01,
            Slot02,
            Slot03,
            Slot04,
            Slot05,
            Slot06,
            Slot07,
            Slot08,
            Slot09,
            Slot10,
            Slot11,
            Slot12,
            Slot13,
            Slot14,
            Slot15,
            Slot16,
            Slot17,
            Slot18,
            Slot19,
            Slot20,
            Slot21,
            Slot22,
            Slot23,
            Slot24,
            Slot25,
            Slot26,
            Slot27,
            Slot28,
            Slot29,
            Slot30,
            Slot31,
            Slot32,
            Slot33,
            Slot34,
            Slot35,
            Slot36,
            Slot37,
            Slot38,
            Slot39,
            Slot40,
            Slot41,
            Slot42,
            Slot43,
            Slot44,
            Slot45,
            Slot46,
            Slot47,
            Slot48,
            Slot49,
            Slot50,
            Slot51,
            Slot52,
            Slot53,
            Slot54,
            Slot55,
        };

        public static string ForIndex(int index) =>
            index >= 0 && index < All.Length ? All[index] : Default;

        public static string DefaultFor(RelicRole role)
        {
            switch (role)
            {
                case RelicRole.Defense:
                    return Slot01;
                case RelicRole.Heal:
                    return Slot02;
                case RelicRole.Status:
                    return Slot03;
                case RelicRole.Utility:
                    return Slot06;
                default:
                    return Slot00;
            }
        }
    }
}
