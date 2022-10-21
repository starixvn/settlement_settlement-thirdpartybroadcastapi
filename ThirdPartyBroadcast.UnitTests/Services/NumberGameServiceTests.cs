using AutoMapper;
using AutoMapper.Configuration;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;
using Xunit;

namespace ThirdPartyBroadcast.UnitTests.Services
{
    public class NumberGameServiceTests
    {
        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly NumberGameService service;

        public NumberGameServiceTests()
        {
            calculateService = Substitute.For<CalculateService>();
            scaleOutSettleThirdPartyService = Substitute.For<ScaleOutSettleThirdPartyService>();
            var mapperConfig = new MapperConfigurationExpression();
            mapperConfig.AddProfile<ThirdPartyBroadcastMappingProfile>();
            var config = new MapperConfiguration(mapperConfig);
            mapper = new Mapper(config);
            service = new NumberGameServiceImp(mapper, scaleOutSettleThirdPartyService, calculateService);
        }

        [Fact]
        public async Task Settle_ValidateFailed_StopProcess()
        {
            var resultFailed = new Result
            {
                ErrorCode = 99,
            };

            calculateService.ValidationDeployFlag(SpName.NumberGame).Returns(resultFailed);

            var result = await service.Settle(new AutoNumberGameRequest());

            //Assert
            Assert.Equal(resultFailed.ErrorCode, result.ErrorCode);
            await scaleOutSettleThirdPartyService.DidNotReceive().UpdateMatchInfo3rd(Arg.Any<UpdateMatchInfo3rdRequest>());
        }

        [Fact]
        public async Task Settle_ValidatePassed_DoSettle()
        {
            calculateService.ValidationDeployFlag(SpName.NumberGame).Returns(new Result());

            await service.Settle(new AutoNumberGameRequest());

            //Assert
            await scaleOutSettleThirdPartyService.Received().UpdateMatchInfo3rd(Arg.Any<UpdateMatchInfo3rdRequest>());
            await calculateService.Received().Execute(Arg.Any<NumberGameCriteria>());
        }

        [Fact]
        public async Task Settle_ValidatePassedButMatchLive_DoSettleWithoutUpdateMatch()
        {
            calculateService.ValidationDeployFlag(SpName.NumberGame).Returns(new Result());

            await service.Settle(new AutoNumberGameRequest { IsLive = true });

            //Assert
            await scaleOutSettleThirdPartyService.DidNotReceive().UpdateMatchInfo3rd(Arg.Any<UpdateMatchInfo3rdRequest>());
            await calculateService.Received().Execute(Arg.Any<NumberGameCriteria>());
        }
    }
}