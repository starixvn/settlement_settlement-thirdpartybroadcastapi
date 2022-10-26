using AutoMapper;
using AutoMapper.Configuration;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.HappyFive;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;
using Xunit;

namespace ThirdPartyBroadcast.UnitTests.Services
{
    public class HappyFiveServiceTests
    {
        private readonly IDynamicRepository repository;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly HappyFiveService service;

        public HappyFiveServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            calculateService = Substitute.For<CalculateService>();
            scaleOutSettleThirdPartyService = Substitute.For<ScaleOutSettleThirdPartyService>();
            var mapperConfig = new MapperConfigurationExpression();
            mapperConfig.AddProfile<ThirdPartyBroadcastMappingProfile>();
            var config = new MapperConfiguration(mapperConfig);
            mapper = new Mapper(config);
            service = new HappyFiveServiceImp(mapper, calculateService, repository, scaleOutSettleThirdPartyService);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public async Task Settle_ValidateFailed_StopProcess(int validateDeployFlag, int validateTicket)
        {
            calculateService.ValidationDeployFlag(SpName.HappyFive)
                .Returns(new Result { ErrorCode = validateDeployFlag });
            await repository.ExecuteAsync(Arg.Do<ValidateHappyFive>(item => item.ErrCode = validateTicket));

            var result = await service.Settle(new AutoHappyFiveRequest());

            Assert.Equal(1, result.ErrorCode);
            await scaleOutSettleThirdPartyService.DidNotReceive().UpdateMatchInfo3rd(Arg.Any<UpdateMatchInfo3rdRequest>());
        }

        [Fact]
        public async Task Settle_ValidatePassed_DoSettle()
        {
            calculateService.ValidationDeployFlag(SpName.HappyFive)
                .Returns(new Result());
            await repository.ExecuteAsync(Arg.Do<ValidateHappyFive>(item => item.ErrCode = 0));

            await service.Settle(new AutoHappyFiveRequest());

            //Assert
            await scaleOutSettleThirdPartyService.Received().UpdateMatchInfo3rd(Arg.Any<UpdateMatchInfo3rdRequest>());
            await calculateService.Received().Execute(Arg.Any<HappyFiveCriteria>());
        }
    }
}