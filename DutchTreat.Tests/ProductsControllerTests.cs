using System;
using Xunit;
using AutoFixture;
using Moq;
using FluentAssertions;
using DutchTreat.Data;
using DutchTreat.Controllers;
using System.Collections.Generic;
using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DutchTreat.Tests
{
    public class ProductsControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IDutchRepository> _repositoryMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly ProductsController _sut;

        public ProductsControllerTests()
        {
            _fixture = new Fixture();
            _repositoryMock = _fixture.Freeze<Mock<IDutchRepository>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ProductsController>>>();
            _sut = new ProductsController(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetAllProducts_ShouldReturnOkResponse_WhenDataFound()
        {
            // Arrage
            var productsMock = _fixture.Create<IEnumerable<Product>>();
            _repositoryMock.Setup(x => x.GetAllProducts()).Returns(productsMock);

            // Act
            var result = _sut.Get();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ActionResult<IEnumerable<Product>>>();
            result.Result.Should().BeAssignableTo<OkObjectResult>();
            result.Result.As<OkObjectResult>().Value
                .Should()
                .NotBeNull()
                .And.BeOfType(productsMock.GetType());
            _repositoryMock.Verify(x => x.GetAllProducts(), Times.Once());
        }

        [Fact]
        public void GetAllProducts_ShouldReturnOkResponse_WhenDataNotFound()
        {
            // Arrage
            IEnumerable<Product> productsMock = null;
            _repositoryMock.Setup(x => x.GetAllProducts()).Returns(productsMock);

            // Act
            var result = _sut.Get();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ActionResult<IEnumerable<Product>>>();
            result.Result.As<OkObjectResult>().Value
                .Should()
                .BeNull();
            _repositoryMock.Verify(x => x.GetAllProducts(), Times.Once());
        }

        [Fact]
        public void GetAllProducts_ShouldReturnBadRequest_WhenThrowException()
        {
            // Arrage
            _repositoryMock.Setup(x => x.GetAllProducts()).Throws(new Exception("My custom exception"));

            // Act
            var result = _sut.Get();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<BadRequestObjectResult>();
            _repositoryMock.Verify(x => x.GetAllProducts(), Times.Once());
        }

    }
}
