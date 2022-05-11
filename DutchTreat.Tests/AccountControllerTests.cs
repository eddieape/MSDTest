using System;
using Xunit;
using AutoFixture;
using Moq;
using FluentAssertions;
using DutchTreat.Controllers;
using Microsoft.Extensions.Logging;
using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using DutchTreat.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;


namespace DutchTreat.Tests
{

    public class AccountControllerTests
    {
        private static IConfiguration _config;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<AccountController>> _loggerMock;
        private readonly Mock<UserManager<StoreUser>> _userManagerMock;
        private readonly Mock<SignInManager<StoreUser>> _signInManagerMock;
        private readonly AccountController _sut;

        private Mock<UserManager<StoreUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<StoreUser>>();
     
            return new Mock<UserManager<StoreUser>>(userStoreMock.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<StoreUser>>().Object,
                new IUserValidator<StoreUser>[0],
                new IPasswordValidator<StoreUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<StoreUser>>>().Object);
        }

        private Mock<SignInManager<StoreUser>> GetMockSignInManager(UserManager<StoreUser> userManager)
        {
            var _contextAccessor = new Mock<IHttpContextAccessor>();
            var _userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<StoreUser>>();
            return new Mock<SignInManager<StoreUser>>(userManager, _contextAccessor.Object, _userPrincipalFactory.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                     new Mock<ILogger<SignInManager<StoreUser>>>().Object,
                     new Mock<IAuthenticationSchemeProvider>().Object,
                     new Mock<IUserConfirmation<StoreUser>>().Object);
        }

        public AccountControllerTests()
        {
            _fixture = new Fixture();

            _loggerMock = _fixture.Freeze<Mock<ILogger<AccountController>>>();  
            _userManagerMock = GetMockUserManager();
            _signInManagerMock = GetMockSignInManager(_userManagerMock.Object);

            var inMemorySettings = new Dictionary<string, string> {
                {"Tokens:Key", "a;sdlkfja; lsdkfj ;alksdfj ;alksdfj; aiefj;lskij;fldsk"},
                {"Tokens:Issuer", "localhost"},
                {"Tokens:Audience", "localhost"}
            };

            if (_config == null)
            {
                _config = new ConfigurationBuilder()
                                .AddInMemoryCollection(inMemorySettings)
                                .Build();
            }

            _sut = new AccountController(_loggerMock.Object, _signInManagerMock.Object, _userManagerMock.Object, _config);
        }

        [Fact]
        public async Task CreateToken_ShouldReturnCreatedResponse_WhenValidRequest()
        {
            // Arrange
            var request = _fixture.Create<LoginViewModel>();
            var storeUser = _fixture.Create<StoreUser>();

            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(storeUser);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<StoreUser>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Success));

            // Act
            var actionResult = await _sut.CreateToken(request);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<Microsoft.AspNetCore.Mvc.CreatedResult>();
            actionResult.As<Microsoft.AspNetCore.Mvc.CreatedResult>().Value
                .Should()
                .NotBeNull();
        }

        [Fact]
        public async Task CreateToken_ShouldReturnCreatedResponse_WhenInValidRequest()
        {
            // Arrange
            var request = _fixture.Create<LoginViewModel>();
            _sut.ModelState.AddModelError("Username", "The Username field is required.");
            var storeUser = _fixture.Create<StoreUser>();
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(storeUser);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<StoreUser>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Success));

            // Act
            var actionResult = await _sut.CreateToken(request);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<Microsoft.AspNetCore.Mvc.BadRequestResult>();
        }
    }
}
