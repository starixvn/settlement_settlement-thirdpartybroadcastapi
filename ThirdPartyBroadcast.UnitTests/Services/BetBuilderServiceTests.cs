using AutoMapper;
using AutoMapper.Configuration;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Kaden.XmlConverter;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Criteria.BetBuilder;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;
using Xunit;

namespace ThirdPartyBroadcast.UnitTests.Services
{
    public class BetBuilderServiceTests
    {
        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly ScaleOutSettleService scaleOutSettleService;

        private readonly CalculateService calculateService;

        private readonly IMapper mapper;

        private readonly XmlConvertService xmlConvertService;

        private readonly IDynamicRepository repository;

        private readonly BetBuilderService service;

        public BetBuilderServiceTests()
        {
            scaleOutSettleThirdPartyService = Substitute.For<ScaleOutSettleThirdPartyService>();
            scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
            calculateService = Substitute.For<CalculateService>();
            xmlConvertService = Substitute.For<XmlConvertService>();
            repository = Substitute.For<IDynamicRepository>();
            var mapperConfig = new MapperConfigurationExpression();
            mapperConfig.AddProfile<ThirdPartyBroadcastMappingProfile>();
            var config = new MapperConfiguration(mapperConfig);
            mapper = new Mapper(config);
            service = new BetBuilderServiceImp(scaleOutSettleThirdPartyService, scaleOutSettleService, calculateService,
                mapper, xmlConvertService, repository);
        }

        [Fact]
        public async Task Settle_ValidateFailed_StopProcessAndReturn()
        {
            var resultFailed = new ResultMessage
            {
                ErrorCode = 1
            };
            scaleOutSettleThirdPartyService.ValidateBetBuilder(Arg.Any<BetBuilderRequest>())
                .Returns((resultFailed, "1234"));

            var result = await service.Settle(new AutoBetBuilderRequest());

            Assert.Equal(resultFailed.ErrorCode, result.ErrorCode);

            //Assert
            repository.DidNotReceive().Execute(Arg.Any<WriteLogCriteria>());
        }

        [Fact]
        public void Settle_ValidatePassedAndNotExistInvalidResult_SettleResult()
        {
            var resources = new ResourcesModel
            {
                Items = new List<ResourceItemModel>
                {
                    new() { MarketId = "1" }
                }
            };
            scaleOutSettleThirdPartyService.ValidateBetBuilder(Arg.Any<BetBuilderRequest>())
                .Returns((new ResultMessage(), null));
            xmlConvertService.Deserialize<ResourcesModel>(Arg.Any<string>()).Returns(resources);
            xmlConvertService.Serialize(Arg.Any<ResourcesModel>()).Returns("test");

            service.Settle(new AutoBetBuilderRequest());

            //Assert
            scaleOutSettleService.Received().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
            calculateService.Received().Execute(Arg.Any<BetBuilderCriteria>());
        }

        [Theory]
        [InlineData(ActionType.Calculate, ActionId.BetBuilderCalc)]
        [InlineData(ActionType.Refund, ActionId.BetBuilderRefund)]
        public void Settle_ValidatePassedAndExistInvalidResult_SettleCorrectResult(string action, int actionId)
        {
            var resources = new ResourcesModel
            {
                Items = new List<ResourceItemModel>
                {
                    new() { MarketId = "123" },
                    new() { MarketId = "234" }
                }
            };

            var request = new AutoBetBuilderRequest
            {
                Action = action,
                Ip = "1",
                AdminId = 1,
                MatchId = 1234
            };

            var message = $"MarketIDs cannot be auto settled since they have been settled already: 123";

            scaleOutSettleThirdPartyService.ValidateBetBuilder(Arg.Any<BetBuilderRequest>())
                .Returns((new ResultMessage(), "123"));
            xmlConvertService.Deserialize<ResourcesModel>(Arg.Any<string>()).Returns(resources);
            xmlConvertService.Serialize(Arg.Any<ResourcesModel>()).Returns("2");

            service.Settle(request);

            //Assert
            repository.Received().Execute(Arg.Is<WriteLogCriteria>(item => item.ActionId == actionId && item.Remark == message));
            scaleOutSettleService.Received().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
            calculateService.Received().Execute(Arg.Any<BetBuilderCriteria>());
        }

        [Fact]
        public async Task Settle_ValidatePassedButAllStepAfterReturnError_SettleCorrectResult()
        {
            var resources = new ResourcesModel
            {
                Items = new List<ResourceItemModel>
                {
                    new() { MarketId = "123" },
                    new() { MarketId = "234" }
                }
            };

            var request = new AutoBetBuilderRequest
            {
                Ip = "1",
                AdminId = 1,
                MatchId = 1234
            };

            var resultFailed = new Result
            {
                ErrorCode = 1,
                ErrorMessage = "error"
            };

            var message = "MarketIDs cannot be auto settled since they have been settled already: 123| error";

            scaleOutSettleThirdPartyService.ValidateBetBuilder(Arg.Any<BetBuilderRequest>())
                .Returns((new ResultMessage(), "123"));
            xmlConvertService.Deserialize<ResourcesModel>(Arg.Any<string>()).Returns(resources);
            xmlConvertService.Serialize(Arg.Any<ResourcesModel>()).Returns("2");
            calculateService.Execute(Arg.Any<BetBuilderCriteria>()).Returns(resultFailed);

            var result = await service.Settle(request);

            //Assert
            await scaleOutSettleService.Received().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
            await calculateService.Received().Execute(Arg.Any<BetBuilderCriteria>());
            Assert.Equal(message, result.ErrorMessage);
        }
    }
}