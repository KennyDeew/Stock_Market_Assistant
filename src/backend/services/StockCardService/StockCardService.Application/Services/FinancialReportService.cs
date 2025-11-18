using Microsoft.Extensions.Logging;
using StockCardService.Abstractions.Repositories;
using StockCardService.Infrastructure.Messaging.Kafka;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_FinancialReport;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly IMongoRepository<FinancialReport, Guid> _financialReportRepository;
        private readonly ILogger<FinancialReportService> _logger;
        private readonly IKafkaProducer<string, FinancialReportCreatedMessage> _producer;


        public FinancialReportService(IMongoRepository<FinancialReport, Guid> financialReportRepository, ILogger<FinancialReportService> logger, IKafkaProducer<string, FinancialReportCreatedMessage> producer)
        {
            _financialReportRepository = financialReportRepository;
            _logger = logger;
            _producer = producer;
        }

        /// <summary>
        /// Добавить финансовый отчет
        /// </summary>
        /// <param name="сreatingFinancialReportDto">Dto создаваемого финансового отчета</param>
        /// <returns></returns>
        public async Task<Guid> CreateAsync(CreatingFinancialReportDto сreatingFinancialReportDto)
        {
            var financialReport = new FinancialReport()
            {
                Id = Guid.NewGuid(),
                ParentId = сreatingFinancialReportDto.ParentId,
                Name = сreatingFinancialReportDto.Name,
                Description = сreatingFinancialReportDto.Description,
                Period = сreatingFinancialReportDto.Period,
                Revenue = сreatingFinancialReportDto.Revenue,
                EBITDA = сreatingFinancialReportDto.EBITDA,
                NetProfit = сreatingFinancialReportDto.NetProfit,
                CAPEX = сreatingFinancialReportDto.CAPEX,
                FCF = сreatingFinancialReportDto.FCF,
                Debt = сreatingFinancialReportDto.Debt,
                TotalAssets = сreatingFinancialReportDto.TotalAssets,
                NonCurrentAssets = сreatingFinancialReportDto.NonCurrentAssets,
                CurrentAssets = сreatingFinancialReportDto.CurrentAssets,
                Inventories = сreatingFinancialReportDto.Inventories,
                AccountsReceivable = сreatingFinancialReportDto.AccountsReceivable,
                CashAndEquivalents = сreatingFinancialReportDto.CashAndEquivalents,
                NonCurrentLiabilities = сreatingFinancialReportDto.NonCurrentLiabilities,
                CurrentLiabilities = сreatingFinancialReportDto.CurrentLiabilities
            };

            await _financialReportRepository.AddAsync(financialReport);

            // Формируем Kafka-сообщение
            var kafkaMessage = new FinancialReportCreatedMessage
            {
                ShareCardId = financialReport.ParentId,
                Name = financialReport.Name,
                Period = financialReport.Period
            };

            // Отправляем в Kafka
            await _producer.ProduceAsync(kafkaMessage, CancellationToken.None);

            _logger.LogInformation($"Financial report '{financialReport.Name}' for ShareCard {financialReport.ParentId} created and message sent.");

            return financialReport.Id;
        }

        /// <summary>
        /// Удалить финансовый отчет
        /// </summary>
        /// <param name="id">Id удаляемого отчета</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            await _financialReportRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Получить все финансовые отчеты
        /// </summary>
        /// <returns>Коллекция отчетов</returns>
        public async Task<IEnumerable<FinancialReportDto>> GetAllAsync()
        {
            var finReports = await _financialReportRepository.GetAllAsync();

            var finReportsDto = finReports.Select(x => new FinancialReportDto()
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Name = x.Name,
                Description = x.Description,
                Period = x.Period,
                Revenue = x.Revenue,
                EBITDA = x.EBITDA,
                NetProfit = x.NetProfit,
                CAPEX = x.CAPEX,
                FCF = x.FCF,
                Debt = x.Debt,
                TotalAssets = x.TotalAssets,
                NonCurrentAssets = x.NonCurrentAssets,
                CurrentAssets = x.CurrentAssets,
                Inventories = x.Inventories,
                AccountsReceivable = x.AccountsReceivable,
                CashAndEquivalents = x.CashAndEquivalents,
                NonCurrentLiabilities = x.NonCurrentLiabilities,
                CurrentLiabilities = x.CurrentLiabilities
            }).ToList();

            return finReportsDto;
        }

        /// <summary>
        /// Получить все финансовые отчеты одной акции
        /// </summary>
        /// <param name="id">Id акции</param>
        /// <returns></returns>
        public async Task<IEnumerable<FinancialReportDto>> GetAllByShareCardIdAsync(Guid id)
        {
            var finReports = await _financialReportRepository.GetWhere(x => x.ParentId == id);

            var finReportsDto = finReports.Select(x => new FinancialReportDto()
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Name = x.Name,
                Description = x.Description,
                Period = x.Period,
                Revenue = x.Revenue,
                EBITDA = x.EBITDA,
                NetProfit = x.NetProfit,
                CAPEX = x.CAPEX,
                FCF = x.FCF,
                Debt = x.Debt,
                TotalAssets = x.TotalAssets,
                NonCurrentAssets = x.NonCurrentAssets,
                CurrentAssets = x.CurrentAssets,
                Inventories = x.Inventories,
                AccountsReceivable = x.AccountsReceivable,
                CashAndEquivalents = x.CashAndEquivalents,
                NonCurrentLiabilities = x.NonCurrentLiabilities,
                CurrentLiabilities = x.CurrentLiabilities
            }).ToList();

            return finReportsDto;
        }

        /// <summary>
        /// Получить финансовый отчет по Id
        /// </summary>
        /// <param name="id">Id финансового отчета</param>
        /// <returns>Dto финансового отчета</returns>
        public async Task<FinancialReportDto?> GetByIdAsync(Guid id)
        {
            var finReport = await _financialReportRepository.GetByIdAsync(id);

            if (finReport is null)
                return null;

            var finReportDto = new FinancialReportDto()
            {
                Id = finReport.Id,
                ParentId = finReport.ParentId,
                Name = finReport.Name,
                Description = finReport.Description,
                Period = finReport.Period,
                Revenue = finReport.Revenue,
                EBITDA = finReport.EBITDA,
                NetProfit = finReport.NetProfit,
                CAPEX = finReport.CAPEX,
                FCF = finReport.FCF,
                Debt = finReport.Debt,
                TotalAssets = finReport.TotalAssets,
                NonCurrentAssets = finReport.NonCurrentAssets,
                CurrentAssets = finReport.CurrentAssets,
                Inventories = finReport.Inventories,
                AccountsReceivable = finReport.AccountsReceivable,
                CashAndEquivalents = finReport.CashAndEquivalents,
                NonCurrentLiabilities = finReport.NonCurrentLiabilities,
                CurrentLiabilities = finReport.CurrentLiabilities
            };

            return finReportDto;
        }

        /// <summary>
        /// Обновить финансовый отчет
        /// </summary>
        /// <param name="updatingFinancialReportDto">Dto измененного финансового отчета</param>
        /// <returns></returns>
        public async Task UpdateAsync(UpdatingFinancialReportDto updatingFinancialReportDto)
        {
            var finReport = await _financialReportRepository.GetByIdAsync(updatingFinancialReportDto.Id);

            finReport.Name = updatingFinancialReportDto.Name;
            finReport.Description = updatingFinancialReportDto.Description;
            finReport.Period = updatingFinancialReportDto.Period;
            finReport.Revenue = updatingFinancialReportDto.Revenue;
            finReport.EBITDA = updatingFinancialReportDto.EBITDA;
            finReport.NetProfit = updatingFinancialReportDto.NetProfit;
            finReport.CAPEX = updatingFinancialReportDto.CAPEX;
            finReport.FCF = updatingFinancialReportDto.FCF;
            finReport.Debt = updatingFinancialReportDto.Debt;
            finReport.TotalAssets = updatingFinancialReportDto.TotalAssets;
            finReport.NonCurrentAssets = updatingFinancialReportDto.NonCurrentAssets;
            finReport.CurrentAssets = updatingFinancialReportDto.CurrentAssets;
            finReport.Inventories = updatingFinancialReportDto.Inventories;
            finReport.AccountsReceivable = updatingFinancialReportDto.AccountsReceivable;
            finReport.CashAndEquivalents = updatingFinancialReportDto.CashAndEquivalents;
            finReport.NonCurrentLiabilities = updatingFinancialReportDto.NonCurrentLiabilities;
            finReport.CurrentLiabilities = updatingFinancialReportDto.CurrentLiabilities;

            await _financialReportRepository.UpdateAsync(finReport);
        }
    }
}
