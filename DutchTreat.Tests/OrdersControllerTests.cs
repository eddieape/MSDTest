using System;
using Xunit;
using AutoFixture;
using Moq;
using FluentAssertions;
using DutchTreat.Data;
using DutchTreat.Controllers;
using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMapper;
using DutchTreat.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DutchTreat.Tests
{

    public class OrdersControllerTests
    {
        private static IMapper _mapper;
        private readonly IFixture _fixture;
        private readonly Mock<IDutchRepository> _repositoryMock;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;
        private readonly Mock<UserManager<StoreUser>> _userManagerMock;
        private readonly OrdersController _sut;

        private Mock<UserManager<StoreUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<StoreUser>>();
            return new Mock<UserManager<StoreUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        public OrdersControllerTests()
        {
            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new DutchMappingProfile());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }

            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _repositoryMock = _fixture.Freeze<Mock<IDutchRepository>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<OrdersController>>>();
            _userManagerMock = GetMockUserManager();
            _sut = new OrdersController(_repositoryMock.Object, _loggerMock.Object, _mapper, _userManagerMock.Object);
 
        }

        [Fact]
        public void GetOrderById_ShouldReturnOkResponse_WhenValidInput()
        {
            // Arrage
            var orderMock = _fixture.Create<Order>();
            var username = _fixture.Create<string>();
            var id = _fixture.Create<int>();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "someAuthTypeName"))
                }
            };
            _repositoryMock.Setup(x => x.GetOrderById(username, id)).Returns(orderMock);

            // Act
            var actionResult = _sut.Get(id);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<OkObjectResult>();
            actionResult.As<OkObjectResult>().Value
                .Should()
                .NotBeNull()
                .And.BeOfType(_mapper.Map<OrderViewModel>(orderMock).GetType());
            _repositoryMock.Verify(x => x.GetOrderById(username, id), Times.Once());
        }

        [Fact]
        public void GetOrderById_ShouldReturnNotFound_WhenNoDataFound()
        {
            // Arrage
            Order response = null;
            var username = _fixture.Create<string>();
            var id = _fixture.Create<int>();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "someAuthTypeName"))
                }
            };
            _repositoryMock.Setup(x => x.GetOrderById(username, id)).Returns(response);

            // Act
            var actionResult = _sut.Get(id);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<NotFoundResult>();
            _repositoryMock.Verify(x => x.GetOrderById(username, id), Times.Once());
        }


        [Fact]
        public void GetOrderById_ShouldReturnBadRequest_WhenThrowException()
        {
            // Arrage
            var username = _fixture.Create<string>();
            var id = _fixture.Create<int>();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "someAuthTypeName"))
                }
            };
            _repositoryMock.Setup(x => x.GetOrderById(username, id)).Throws(new Exception("My custom exception"));

            // Act
            var actionResult = _sut.Get(id);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<BadRequestObjectResult>();
            _repositoryMock.Verify(x => x.GetOrderById(username, id), Times.Once());
        }


        [Fact]
        public void GetOrdersByUser_ShouldReturnOkResponse_WhenValidInput()
        {
            // Arrage
            var ordersMock = _fixture.Create<IEnumerable<Order>>();
            var username = _fixture.Create<string>();
            var includeItems = _fixture.Create<bool>();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "someAuthTypeName"))
                }
            };
            _repositoryMock.Setup(x => x.GetOrdersByUser(username, includeItems)).Returns(ordersMock);

            // Act
            var actionResult = _sut.Get(includeItems);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<OkObjectResult>();
            actionResult.As<OkObjectResult>().Value
                .Should()
                .NotBeNull()
                .And.BeOfType(_mapper.Map<IEnumerable<OrderViewModel>>(ordersMock).GetType());
            _repositoryMock.Verify(x => x.GetOrdersByUser(username, includeItems), Times.Once());
        }


        [Fact]
        public void GetOrdersByUser_ShouldReturnBadRequest_WhenThrowException()
        {
            // Arrage
            var username = _fixture.Create<string>();
            var includeItems = _fixture.Create<bool>();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "someAuthTypeName"))
                }
            };
            _repositoryMock.Setup(x => x.GetOrdersByUser(username, includeItems)).Throws(new Exception("My custom exception"));

            // Act
            var actionResult = _sut.Get(includeItems);


            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<BadRequestObjectResult>();
            _repositoryMock.Verify(x => x.GetOrdersByUser(username, includeItems), Times.Once());
        }


        [Fact]
        public async Task CreateOrder_ShouldReturnCreatedResponse_WhenValidRequest()
        {
            // Arrage
            var username = _fixture.Create<string>();
            var request = _fixture.Create<OrderViewModel>();
            
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "someAuthTypeName"))
                }
            };
            _repositoryMock.Setup(x => x.SaveAll()).Returns(true);


            // Act
            var actionResult = await _sut.Post(request);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<CreatedResult>();
            actionResult.As<CreatedResult>().Location.Equals($"/api/orders/{request.OrderId}");
            actionResult.As<CreatedResult>().Value
                .Should()
                .NotBeNull()
                .And.BeOfType((request).GetType());
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreatedResponse_WhenInValidRequest()
        {
            // Arrage
            var request = _fixture.Create<OrderViewModel>();
            _sut.ModelState.AddModelError("OrderNumber", "The OrderNumber field is required.");

            // Act
            var actionResult = await _sut.Post(request);

            // Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeAssignableTo<BadRequestObjectResult>();
        }
    }
}
