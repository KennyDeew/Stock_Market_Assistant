using FluentValidation.TestHelper;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Application.Validators;
using StockMarketAssistant.AnalyticsService.Domain.Constants;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Tests.Application
{
    public class GetTopAssetsRequestValidatorTests
    {
        private readonly GetTopAssetsRequestValidator _validator;

        public GetTopAssetsRequestValidatorTests()
        {
            _validator = new GetTopAssetsRequestValidator();
        }

        [Fact]
        public void Validate_ValidRequest_ShouldNotHaveErrors()
        {
            // Arrange
            var request = new GetTopAssetsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Top = 10,
                Context = AnalysisContext.Global
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_StartDateAfterEndDate_ShouldHaveError()
        {
            // Arrange
            var request = new GetTopAssetsRequest
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-7),
                Top = 10,
                Context = AnalysisContext.Global
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }

        [Fact]
        public void Validate_TopZero_ShouldHaveError()
        {
            // Arrange
            var request = new GetTopAssetsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Top = 0,
                Context = AnalysisContext.Global
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Top);
        }

        [Fact]
        public void Validate_TopExceedsMax_ShouldHaveError()
        {
            // Arrange
            var request = new GetTopAssetsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Top = DomainConstants.Aggregation.MaxTopAssetsCount + 1,
                Context = AnalysisContext.Global
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Top);
        }

        [Fact]
        public void Validate_PortfolioContextWithoutPortfolioId_ShouldHaveError()
        {
            // Arrange
            var request = new GetTopAssetsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Top = 10,
                Context = AnalysisContext.Portfolio,
                PortfolioId = null
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PortfolioId);
        }

        [Fact]
        public void Validate_GlobalContextWithPortfolioId_ShouldHaveError()
        {
            // Arrange
            var request = new GetTopAssetsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Top = 10,
                Context = AnalysisContext.Global,
                PortfolioId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PortfolioId);
        }
    }

    public class ComparePortfoliosRequestValidatorTests
    {
        private readonly ComparePortfoliosRequestValidator _validator;

        public ComparePortfoliosRequestValidatorTests()
        {
            _validator = new ComparePortfoliosRequestValidator();
        }

        [Fact]
        public void Validate_ValidRequest_ShouldNotHaveErrors()
        {
            // Arrange
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_EmptyPortfolioIds_ShouldHaveError()
        {
            // Arrange
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid>()
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PortfolioIds);
        }

        [Fact]
        public void Validate_TooManyPortfolios_ShouldHaveError()
        {
            // Arrange
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = Enumerable.Range(0, DomainConstants.Validation.MaxPortfoliosInComparison + 1)
                    .Select(_ => Guid.NewGuid())
                    .ToList()
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PortfolioIds);
        }

        [Fact]
        public void Validate_DuplicatePortfolioIds_ShouldHaveError()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid> { portfolioId, portfolioId }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PortfolioIds);
        }

        [Fact]
        public void Validate_EmptyPortfolioId_ShouldHaveError()
        {
            // Arrange
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid> { Guid.Empty, Guid.NewGuid() }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PortfolioIds);
        }

        [Fact]
        public void Validate_InvalidPeriod_ShouldHaveError()
        {
            // Arrange
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid> { Guid.NewGuid() },
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-7)
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }
    }
}

