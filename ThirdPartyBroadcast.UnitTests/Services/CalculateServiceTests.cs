using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Services;
using Xunit;

namespace ThirdPartyBroadcast.UnitTests.Services
{
    public class CalculateServiceTests
    {
        private readonly IDynamicRepository repository;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly ScaleOutSettleService scaleOutSettleService;

        private readonly CalculateService service;

        public CalculateServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
            scaleOutSettleThirdPartyService = Substitute.For<ScaleOutSettleThirdPartyService>();
            var appSettingsKey = new AppSettingsKey { SiteDbServer = new List<string> { "01", "02" } };
            var options = Options.Create(appSettingsKey);

            service = new CalculateServiceImp(repository, options, scaleOutSettleThirdPartyService, scaleOutSettleService);
        }

        [Fact]
        public async Task Execute_ExistError_ReturnCorrectMessage()
        {
            await repository.ExecuteAsync(Arg.Do<CalculateCriteria>(criteria => criteria.ErrMsg = "Test"));
            scaleOutSettleService.GetSiteGroupId(Arg.Any<string>()).Returns(1, 2);

            var result = await service.Execute(new CalculateCriteria());
            var expected = "SiteDbGroupId 1: Test | SiteDbGroupId 2: Test";

            //Assert
            Assert.Equal(2, result.ErrorCode);
            Assert.Equal(expected, result.ErrorMessage);
        }

        [Fact]
        public async Task Execute_NotError_ReturnResult()
        {
            await repository.ExecuteAsync(Arg.Any<CalculateCriteria>());

            var result = await service.Execute(new CalculateCriteria());

            //Assert
            Assert.Equal(0, result.ErrorCode);
            Assert.Empty(result.ErrorMessage);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(1, 1)]
        public async Task ExecuteValidationMultiServer_Always_ExecuteCorrectly(int errorCode, int executeTime)
        {
            await repository.ExecuteAsync(Arg.Do<BaseCriteria>(criteria => criteria.ErrCode = errorCode));

            await service.ExecuteValidationMultiServer(new BaseCriteria());

            await repository.Received(executeTime).ExecuteAsync(Arg.Any<BaseCriteria>());
        }

        [Fact]
        public void ValidationDeployFlag_Always_CallService3rd()
        {
            service.ValidationDeployFlag("Mock");

            scaleOutSettleThirdPartyService.Received()
                .GetDeploymentInfo(Arg.Is<GetDeploymentInfoRequest>(item => item.SPName == "Mock"));
        }
    }
}