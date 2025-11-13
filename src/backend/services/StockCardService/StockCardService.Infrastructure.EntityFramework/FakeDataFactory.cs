using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Infrastructure.EntityFramework
{
    public static class FakeDataFactory
    {
        public static List<ShareCard> ShareCards => new List<ShareCard>()
        {
            new ShareCard()
            {
                Id = Guid.Parse("eb980257-db33-4fe0-80dd-3ddf5277d791"),
                Ticker = "GAZP",
                Name = "Газпром (ПАО) - обыкн.",
                Description = "ПАО «Газпром» — транснациональная энергетическая компания",
                CurrentPrice = 0m,
                Currency = "RUB"
            }            ,
            new ShareCard()
            {
                Id = Guid.Parse("778e0b49-d17b-4c34-a67a-1a3e6dfd998f"),
                Ticker = "SBER",
                Name = "Сбербанк России ПАО - обыкн.",
                Description = "ПАО «СберБанк России» занимается оказанием банковских и финансовых услуг",
                CurrentPrice = 0m,
                Currency = "RUB"
            },
            new ShareCard()
            {
                Id = Guid.Parse("3f95da41-6ce4-48e8-8ab1-7b42895b549e"),
                Ticker = "NVTK",
                Name = "ПАО НОВАТЭК - обыкн.",
                Description = "ПАО «НОВАТЭК» занимается исследованием, добычей, переработкой и сбытом природного газа и жидких углеводородов",
                CurrentPrice = 0m,
                Currency = "RUB"
            },
            new ShareCard()
            {
                Id = Guid.Parse("36612108-8dc5-4095-a926-73f4ec6f8ab2"),
                Ticker = "ROSN",
                Name = "ПАО НК Роснефть",
                Description = "ПАО НК «Роснефть» занимается исследованием, освоением, добычей и продажей нефти и газа",
                CurrentPrice = 0m,
                Currency = "RUB"
            },
            new ShareCard()
            {
                Id = Guid.Parse("3fa8c26a-cfb5-47db-91a9-0d1ee4224c22"),
                Ticker = "T",
                Name = "Т-Технологии МКПАО - обыкн.",
                Description = "ПАО «Т-Технологии» оказывает банковские и финансовые услуги",
                CurrentPrice = 0m,
                Currency = "RUB"
            },
            new ShareCard()
            {
                Id = Guid.Parse("7d7bc5fd-6f44-4c25-baa1-17c7196a1ff6"),
                Ticker = "LKOH",
                Name = "ЛУКОЙЛ",
                Description = "ПАО «ЛУКОЙЛ» занимается исследованием, добычей, переработкой и сбытом нефти",
                CurrentPrice = 0m,
                Currency = "RUB"
            },
            new ShareCard()
            {
                Id = Guid.Parse("b18fc5a1-5787-4f20-86c8-24a84bb84d5b"),
                Ticker = "VTBR",
                Name = "ПАО Банк ВТБ - обыкн.",
                Description = "ПАО «Банк ВТБ» — финансовое учреждение, предоставляющее целый спектр банковских услуг и продуктов",
                CurrentPrice = 0m,
                Currency = "RUB"
            },
            new ShareCard()
            {
                Id = Guid.Parse("e907de0d-4d35-4e00-99af-74dfa2956852"),
                Ticker = "AFLT",
                Name = "ПАО Аэрофлот",
                Description = "ПАО «Аэрофлот — Российские авиалинии» осуществляет полёты и коммерческую деятельность на воздушных линиях",
                CurrentPrice = 0m,
                Currency = "RUB"
            },
            new ShareCard()
            {
                Id = Guid.Parse("4fb1b866-cc54-477e-96ac-63ea39c58507"),
                Ticker = "GMKN",
                Name = "ГМК Нор.Никель ПАО - обыкн.",
                Description = "ПАО ГМК Норильский Никель занимается исследованием, добычей и переработкой руды и неметаллических минерало",
                CurrentPrice = 0m,
                Currency = "RUB"
            }
        };

        public static List<BondCard> BondCards => new List<BondCard>()
        {
            new BondCard()
            {
                Id = Guid.Parse("01075a5d-e01f-4ab8-a639-ea2534a7b045"),
                Ticker = "RU000A107E81",
                Name = "БИННОФАРМ ГРУПП-001Р-03",
                Board = "TQCB",
                Description = "Российская фармацевтическая компания",
                Currency = "RUB",
                CurrentPrice = 0m,
                FaceValue = 1000m,
                Rating = string.Empty,
                MaturityPeriod = DateTime.SpecifyKind(DateTime.Parse("2026-12-14T00:00:00Z"), DateTimeKind.Utc)
            },
            new BondCard()
            {
                Id = Guid.Parse("ac7f1af8-4556-4535-bba9-a121c55babcd"),
                Ticker = "RU000A10AU73",
                Name = "ГТЛК 002P-07",
                Board = "TQCB",
                Description = "Российская лизинговая компания, принадлежащая государству",
                Currency = "RUB",
                CurrentPrice = 0m,
                FaceValue = 1000m,
                Rating = string.Empty,
                MaturityPeriod = DateTime.SpecifyKind(DateTime.Parse("2026-08-04T00:00:00Z"), DateTimeKind.Utc)
            },
            new BondCard()
            {
                Id = Guid.Parse("fd74ff80-8cb1-4327-80c8-1c4cafc978c4"),
                Ticker = "RU000A103760",
                Name = "Совкомбанк ООО БО-04",
                Board = "TQCB",
                Description = "Российский частный коммерческий банк",
                Currency = "RUB",
                CurrentPrice = 0m,
                FaceValue = 1000m,
                Rating = string.Empty,
                MaturityPeriod = DateTime.SpecifyKind(DateTime.Parse("2026-06-01T00:00:00Z"), DateTimeKind.Utc)
            },
            new BondCard()
            {
                Id = Guid.Parse("5b3c1ed3-7ade-4bde-abfa-4200b48882a8"),
                Ticker = "RU000A103943",
                Name = "Аэрофлот-росс.авиалин.ПАО БО-1",
                Board = "TQCB",
                Description = "ПАО «Аэрофлот — Российские авиалинии» осуществляет полёты и коммерческую деятельность на воздушных линиях",
                Currency = "RUB",
                CurrentPrice = 0m,
                FaceValue = 1000m,
                Rating = string.Empty,
                MaturityPeriod = DateTime.SpecifyKind(DateTime.Parse("2026-06-11T00:00:00Z"), DateTimeKind.Utc)
            },
            new BondCard()
            {
                Id = Guid.Parse("0e3af20f-1ddd-4f2b-8265-f70c5fd92938"),
                Ticker = "RU000A10BK17",
                Name = "Газпром нефть БО 003P-15R",
                Board = "TQCB",
                Description = "Российская вертикально-интегрированная нефтяная компания",
                Currency = "RUB",
                CurrentPrice = 0m,
                FaceValue = 1000m,
                Rating = string.Empty,
                MaturityPeriod = DateTime.SpecifyKind(DateTime.Parse("2030-04-11T00:00:00Z"), DateTimeKind.Utc)
            },
            new BondCard()
            {
                Id = Guid.Parse("f4adc1ee-ee24-409a-9d80-58d584a6c3d8"),
                Ticker = "RU000A10B8R9",
                Name = "ИНАРКТИКА 002Р-02",
                Board = "TQCB",
                Description = "Крупнейший российский производитель аквакультурного лосося и форели",
                Currency = "RUB",
                CurrentPrice = 0m,
                FaceValue = 1000m,
                Rating = string.Empty,
                MaturityPeriod = DateTime.SpecifyKind(DateTime.Parse("2028-04-03T00:00:00Z"), DateTimeKind.Utc)
            },
            new BondCard()
            {
                Id = Guid.Parse("e0c94b2d-a727-411f-bbdd-fbedd171885a"),
                Ticker = "SU26248RMFS3",
                Name = "ОФЗ-ПД 26248 16/05/40",
                Board = "TQOB",
                Description = "Государственная облигация с постоянным купоном",
                Currency = "RUB",
                CurrentPrice = 0m,
                FaceValue = 1000m,
                Rating = string.Empty,
                MaturityPeriod = DateTime.SpecifyKind(DateTime.Parse("2040-05-16T00:00:00Z"), DateTimeKind.Utc)
            }
        };

        public static List<Coupon> Coupons => new List<Coupon>()
        {
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("01075a5d-e01f-4ab8-a639-ea2534a7b045"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-09-15T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 47.32m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("01075a5d-e01f-4ab8-a639-ea2534a7b045"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-12-15T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 47.32m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("01075a5d-e01f-4ab8-a639-ea2534a7b045"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-03-15T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 47.32m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("ac7f1af8-4556-4535-bba9-a121c55babcd"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-09-08T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 19.73m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("ac7f1af8-4556-4535-bba9-a121c55babcd"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-10-08T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 19.73m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("ac7f1af8-4556-4535-bba9-a121c55babcd"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-11-07T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 19.73m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("ac7f1af8-4556-4535-bba9-a121c55babcd"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-12-07T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 19.73m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("fd74ff80-8cb1-4327-80c8-1c4cafc978c4"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-12-01T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 20m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("fd74ff80-8cb1-4327-80c8-1c4cafc978c4"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-03-02T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 20m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("fd74ff80-8cb1-4327-80c8-1c4cafc978c4"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-06-01T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 20m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("5b3c1ed3-7ade-4bde-abfa-4200b48882a8"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-12-11T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 20.82m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("5b3c1ed3-7ade-4bde-abfa-4200b48882a8"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-03-12T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 20.82m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("5b3c1ed3-7ade-4bde-abfa-4200b48882a8"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-06-11T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 20.82m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("0e3af20f-1ddd-4f2b-8265-f70c5fd92938"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-08-21T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 14.93m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("0e3af20f-1ddd-4f2b-8265-f70c5fd92938"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-09-20T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 14.93m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("0e3af20f-1ddd-4f2b-8265-f70c5fd92938"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-10-20T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 14.93m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("0e3af20f-1ddd-4f2b-8265-f70c5fd92938"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-11-19T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 14.93m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("f4adc1ee-ee24-409a-9d80-58d584a6c3d8"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-10-06T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 47.37m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("f4adc1ee-ee24-409a-9d80-58d584a6c3d8"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-01-05T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 47.37m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("f4adc1ee-ee24-409a-9d80-58d584a6c3d8"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-04-06T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 47.37m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("e0c94b2d-a727-411f-bbdd-fbedd171885a"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-12-03T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 61.08m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("e0c94b2d-a727-411f-bbdd-fbedd171885a"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-06-03T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 61.08m
            },
            new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("e0c94b2d-a727-411f-bbdd-fbedd171885a"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2026-12-02T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Value = 61.08m
            }
        };

        public static List<Dividend> Dividends => new List<Dividend>()
        {
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("eb980257-db33-4fe0-80dd-3ddf5277d791"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2022-10-07T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2022",
                Value = 51.03m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("eb980257-db33-4fe0-80dd-3ddf5277d791"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2021-07-15T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2020",
                Value = 12.55m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("eb980257-db33-4fe0-80dd-3ddf5277d791"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2020-07-16T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2019",
                Value = 15.24m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("778e0b49-d17b-4c34-a67a-1a3e6dfd998f"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-07-18T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024",
                Value = 34.84m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("778e0b49-d17b-4c34-a67a-1a3e6dfd998f"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2024-07-11T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2023",
                Value = 33.3m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("778e0b49-d17b-4c34-a67a-1a3e6dfd998f"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2023-05-11T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2022",
                Value = 25m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("3f95da41-6ce4-48e8-8ab1-7b42895b549e"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-04-28T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024",
                Value = 82.15m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("3f95da41-6ce4-48e8-8ab1-7b42895b549e"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2024-03-26T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2023",
                Value = 78.59m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("3f95da41-6ce4-48e8-8ab1-7b42895b549e"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2023-05-03T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2022",
                Value = 105.58m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("36612108-8dc5-4095-a926-73f4ec6f8ab2"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-07-20T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024",
                Value = 51.15m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("36612108-8dc5-4095-a926-73f4ec6f8ab2"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2024-07-18T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2023",
                Value = 59.78m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("36612108-8dc5-4095-a926-73f4ec6f8ab2"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2023-07-11T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2022",
                Value = 38.36m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("3fa8c26a-cfb5-47db-91a9-0d1ee4224c22"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-07-17T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2025Q1",
                Value = 33m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("3fa8c26a-cfb5-47db-91a9-0d1ee4224c22"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-05-15T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024",
                Value = 32m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("3fa8c26a-cfb5-47db-91a9-0d1ee4224c22"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2024-11-25T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024Q3",
                Value = 92.5m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("7d7bc5fd-6f44-4c25-baa1-17c7196a1ff6"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-06-03T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024",
                Value = 541m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("7d7bc5fd-6f44-4c25-baa1-17c7196a1ff6"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2024-12-24T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024Q3",
                Value = 514m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("7d7bc5fd-6f44-4c25-baa1-17c7196a1ff6"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2024-07-05T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2023",
                Value = 498m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("b18fc5a1-5787-4f20-86c8-24a84bb84d5b"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-07-11T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024",
                Value = 25.58m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("b18fc5a1-5787-4f20-86c8-24a84bb84d5b"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2021-07-17T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2020",
                Value = 4m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("b18fc5a1-5787-4f20-86c8-24a84bb84d5b"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2020-10-05T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2019",
                Value = 7m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("e907de0d-4d35-4e00-99af-74dfa2956852"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2025-07-18T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2024",
                Value = 5.27m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("e907de0d-4d35-4e00-99af-74dfa2956852"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2019-07-05T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2018",
                Value = 2.68m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("e907de0d-4d35-4e00-99af-74dfa2956852"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2018-07-06T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2017",
                Value = 12.8m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("4fb1b866-cc54-477e-96ac-63ea39c58507"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2023-12-26T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2023Q3",
                Value = 915.33m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("4fb1b866-cc54-477e-96ac-63ea39c58507"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2022-06-14T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2021",
                Value = 11666.22m
            },
            new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.Parse("4fb1b866-cc54-477e-96ac-63ea39c58507"),
                CutOffDate = DateTime.SpecifyKind(DateTime.Parse("2022-01-14T00:00:00Z"), DateTimeKind.Utc),
                Currency = "RUB",
                Period = "2021Q3",
                Value = 1523.17m
            }
        };
    }
}
