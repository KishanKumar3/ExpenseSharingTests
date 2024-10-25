using ExpenseSharingApp.Controllers;
using ExpenseSharingApp.DTOs;
using ExpenseSharingApp.Interfaces;
using ExpenseSharingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseSharingAppTests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _controller = new UserController(_mockUserService.Object, _mockAuthService.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var login = new Login { Email = "test@example.com", Password = "password" };
            var user = new User { Id = "test-id", Email = "test@example.com" };
            _mockAuthService.Setup(service => service.ValidateUserAsync(login.Email, login.Password)).ReturnsAsync(true);
            _mockUserService.Setup(service => service.GetUserByEmailAsync(login.Email)).ReturnsAsync(user);
            _mockAuthService.Setup(service => service.GenerateJwtTokenAsync(user)).ReturnsAsync("test-token");

            // Act
            var result = await _controller.Login(login);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = okResult.Value;

            // Use reflection to get the Token property value
            var tokenProperty = resultValue.GetType().GetProperty("Token");
            var tokenValue = tokenProperty.GetValue(resultValue, null) as string;

            Assert.Equal("test-token", tokenValue);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var login = new Login { Email = "test@example.com", Password = "wrong-password" };
            _mockAuthService.Setup(service => service.ValidateUserAsync(login.Email, login.Password)).ReturnsAsync(false);

            // Act
            var result = await _controller.Login(login);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void GetAllUsers_ReturnsOkWithUsers()
        {
            // Arrange
            var users = new List<UserDTO> { new UserDTO(), new UserDTO() };
            _mockUserService.Setup(service => service.GetAll()).Returns(users);

            // Act
            var result = _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDTO>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }

        [Fact]
        public void GetUserById_ExistingId_ReturnsOkWithUser()
        {
            // Arrange
            var userId = "test-id";
            var user = new UserDTO { Id = userId };
            _mockUserService.Setup(service => service.GetById(userId)).Returns(user);

            // Act
            var result = _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
        }

        [Fact]
        public void GetUserById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var userId = "non-existing-id";
            _mockUserService.Setup(service => service.GetById(userId)).Returns((UserDTO)null);

            // Act
            var result = _controller.GetUserById(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void UpdateUser_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            var userId = "test-id";
            var userDto = new UserDTO { Id = userId, Name = "Updated Name" };
            _mockUserService.Setup(service => service.Update(userId, userDto)).Returns(true);

            // Act
            var result = _controller.UpdateUser(userId, userDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void UpdateUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "non-existing-id";
            var userDto = new UserDTO { Id = userId, Name = "Updated Name" };
            _mockUserService.Setup(service => service.Update(userId, userDto)).Returns(false);

            // Act
            var result = _controller.UpdateUser(userId, userDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteUser_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            var userId = "test-id";
            _mockUserService.Setup(service => service.Delete(userId)).Returns(true);

            // Act
            var result = _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void DeleteUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "non-existing-id";
            _mockUserService.Setup(service => service.Delete(userId)).Returns(false);

            // Act
            var result = _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
