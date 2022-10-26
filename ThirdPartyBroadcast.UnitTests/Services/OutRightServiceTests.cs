using AutoMapper;
using AutoMapper.Configuration;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.OutRight;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;
using Xunit;

namespace ThirdPartyBroadcast.UnitTests.Services
{
    public class OutRightServiceTests
    {
        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly OutrightService service;

        public OutRightServiceTests()
        {
            calculateService = Substitute.For<CalculateService>();
            scaleOutSettleThirdPartyService = Substitute.For<ScaleOutSettleThirdPartyService>();
            var mapperConfig = new MapperConfigurationExpression();
            mapperConfig.AddProfile<ThirdPartyBroadcastMappingProfile>();
            var config = new MapperConfiguration(mapperConfig);
            mapper = new Mapper(config);
            service = new OutrightServiceImp(mapper, calculateService, scaleOutSettleThirdPartyService);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public async Task Settle_ValidateFailed_StopProcess(int validateDeployFlag, int validateMoveBetDay)
        {
            calculateService.ValidationDeployFlag(SpName.OutRight)
                .Returns(new Result { ErrorCode = validateDeployFlag });
            scaleOutSettleThirdPartyService.ValidateMoveBetDay(Arg.Any<string>(), "outright").Returns(new ResultMessage { ErrorCode = validateMoveBetDay });

            var result = await service.Settle(new AutoOutrightRequest());

            Assert.Equal(1, result.ErrorCode);
            await calculateService.DidNotReceive().Execute(Arg.Any<OutRightCriteria>());
        }

        [Fact]
        public async Task Settle_ValidatePassed_DoSettle()
        {
            calculateService.ValidationDeployFlag(SpName.OutRight)
                .Returns(new Result());
            scaleOutSettleThirdPartyService.ValidateMoveBetDay(Arg.Any<string>(), "outright").Returns(new ResultMessage());

            await service.Settle(new AutoOutrightRequest());

            //Assert
            await calculateService.Received().Execute(Arg.Any<OutRightCriteria>());
        }
    }
}