using AutoMapper;
using AutoMapper.Configuration;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.VirtualSoccer;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;
using Xunit;

namespace ThirdPartyBroadcast.UnitTests.Services
{
    public class VirtualSoccerServiceTests
    {
        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly IDynamicRepository repository;

        private readonly VirtualSoccerService service;

        public VirtualSoccerServiceTests()
        {
            calculateService = Substitute.For<CalculateService>();
            scaleOutSettleThirdPartyService = Substitute.For<ScaleOutSettleThirdPartyService>();
            repository = Substitute.For<IDynamicRepository>();
            var mapperConfig = new MapperConfigurationExpression();
            mapperConfig.AddProfile<ThirdPartyBroadcastMappingProfile>();
            var config = new MapperConfiguration(mapperConfig);
            mapper = new Mapper(config);
            service = new VirtualSoccerServiceImp(scaleOutSettleThirdPartyService, repository, mapper, calculateService);
        }

        [Fact]
        public async Task Settle_ValidateFailed_StopProcess()
        {
            var resultFailed = new Result
            {
                ErrorCode = 99,
            };

            calculateService.ValidationDeployFlag(SpName.VirtualSports).Returns(resultFailed);

            var result = await service.Settle(new AutoVirtualSoccerRequest());

            //Assert
            Assert.Equal(resultFailed.ErrorCode, result.ErrorCode);
            await scaleOutSettleThirdPartyService.DidNotReceive().UpdateMatchInfo3rd(Arg.Any<UpdateMatchInfo3rdRequest>());
        }

        [Theory]
        [InlineData(ActionType.Calculate, 1, 1, SpName.VirtualSports + "-CAL")]
        [InlineData(ActionType.Refund, null, null, SpName.VirtualSports + "-REFUND")]
        public async Task Settle_ValidatePassed_DoSettle(string action, int? home, int? away, string command)
        {
            calculateService.ValidationDeployFlag(SpName.VirtualSports).Returns(new Result());
            var request = new AutoVirtualSoccerRequest
            {
                Action = action,
                Fta = away,
                Fth = home
            };

            await service.Settle(request);

            //Assert
            await calculateService.Received().Execute(Arg.Any<VirtualSoccerCriteria>());
            await repository.Received().ExecuteAsync(Arg.Any<VRSoccerUpdateMatchCriteria>());
            await scaleOutSettleThirdPartyService.Received()
                .UpdateMatchInfo3rd(Arg.Is<UpdateMatchInfo3rdRequest>(item => item.Command == command && item.FTAway == away && item.FTHome == home));
        }
    }
}