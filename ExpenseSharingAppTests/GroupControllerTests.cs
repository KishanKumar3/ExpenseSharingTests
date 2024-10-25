using ExpenseSharingApp.Controllers;
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

        public class GroupControllerTests
        {
            private readonly Mock<IGroupService> _mockGroupService;
            private readonly GroupController _controller;

            public GroupControllerTests()
            {
                _mockGroupService = new Mock<IGroupService>();
                _controller = new GroupController(_mockGroupService.Object);
            }

            [Fact]
            public void GetAllGroups_ReturnsOkResultWithGroups()
            {
                // Arrange
                var groups = new List<Group>
            {
                new Group { GroupId = "1", Name = "Group 1" },
                new Group { GroupId = "2", Name = "Group 2" }
            };

                _mockGroupService.Setup(s => s.GetAllGroups()).Returns(groups);

                // Act
                var result = _controller.GetAllGroups();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var returnedGroups = Assert.IsAssignableFrom<IEnumerable<Group>>(okResult.Value);
                Assert.Equal(2, returnedGroups.Count());
            }

            [Fact]
            public void GetGroupById_ExistingId_ReturnsOkResultWithGroup()
            {
                // Arrange
                var groupId = "1";
                var group = new Group { GroupId = groupId, Name = "Test Group" };

                _mockGroupService.Setup(s => s.GetGroupById(groupId)).Returns(group);

                // Act
                var result = _controller.GetGroupById(groupId);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var returnedGroup = Assert.IsType<Group>(okResult.Value);
                Assert.Equal(groupId, returnedGroup.GroupId);
            }

            [Fact]
            public void GetGroupById_NonExistingId_ReturnsNotFound()
            {
                // Arrange
                var groupId = "999";

                _mockGroupService.Setup(s => s.GetGroupById(groupId)).Returns((Group)null);

                // Act
                var result = _controller.GetGroupById(groupId);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public void GetGroupsByUserId_ExistingUserId_ReturnsOkResultWithGroups()
            {
                // Arrange
                var userId = "user1";
                var groups = new List<Group>
            {
                new Group { GroupId = "1", Name = "Group 1" },
                new Group { GroupId = "2", Name = "Group 2" }
            };

                _mockGroupService.Setup(s => s.GetGroupsByUserId(userId)).Returns(groups);

                // Act
                var result = _controller.GetGroupsByUserId(userId);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var returnedGroups = Assert.IsAssignableFrom<IEnumerable<Group>>(okResult.Value);
                Assert.Equal(2, returnedGroups.Count());
            }

            [Fact]
            public void GetGroupsByUserId_NonExistingUserId_ReturnsInternalServerError()
            {
                // Arrange
                var userId = "nonexistinguser";

                _mockGroupService.Setup(s => s.GetGroupsByUserId(userId)).Throws(new Exception("Internal server error"));

                // Act
                var result = _controller.GetGroupsByUserId(userId);

                // Assert
                var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
                Assert.Equal(500, statusCodeResult.StatusCode);
            }

            [Fact]
            public void AddGroup_ValidGroup_ReturnsCreatedAtActionResult()
            {
                // Arrange
                var groupCreationModel = new GroupCreationModel
                {
                    Name = "New Group",
                    Description = "Test Description",
                    CreatedDate = DateTime.Now,
                    MemberEmails = new List<string> { "user1@example.com", "user2@example.com" }
                };

                var createdGroup = new Group { GroupId = "1", Name = "New Group" };

                _mockGroupService.Setup(s => s.CreateGroup(groupCreationModel)).Returns(createdGroup);

                // Act
                var result = _controller.AddGroup(groupCreationModel);

                // Assert
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal("GetGroupById", createdAtActionResult.ActionName);
                Assert.Equal("1", createdAtActionResult.RouteValues["id"]);
                Assert.Equal(createdGroup, createdAtActionResult.Value);
            }

            [Fact]
            public void AddGroup_InvalidGroup_ReturnsBadRequest()
            {
                // Arrange
                var groupCreationModel = new GroupCreationModel
                {
                    Name = "New Group",
                    Description = "Test Description",
                    CreatedDate = DateTime.Now,
                    MemberEmails = new List<string> { "user1@example.com", "user2@example.com" }
                };

                _mockGroupService.Setup(s => s.CreateGroup(groupCreationModel)).Throws(new ArgumentException("Invalid group details"));

                // Act
                var result = _controller.AddGroup(groupCreationModel);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Invalid group details", badRequestResult.Value);
            }

        [Fact]
        public void DeleteGroup_ExistingGroup_ReturnsOkResult()
        {
            // Arrange
            var groupId = "test-group-id";
            _mockGroupService.Setup(s => s.DeleteGroup(groupId)).Verifiable();

            // Act
            var result = _controller.DeleteGroup(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Group and related expenses deleted successfully.", okResult.Value);
            _mockGroupService.Verify(s => s.DeleteGroup(groupId), Times.Once);
        }

        [Fact]
        public void DeleteGroup_NonExistingGroup_ReturnsNotFoundResult()
        {
            // Arrange
            var groupId = "non-existing-group-id";
            _mockGroupService.Setup(s => s.DeleteGroup(groupId))
                .Throws(new KeyNotFoundException("Group not found"));

            // Act
            var result = _controller.DeleteGroup(groupId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            // Use reflection to get the Message property value
            var resultValue = notFoundResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("Message");
            var messageValue = messageProperty.GetValue(resultValue, null) as string;

            Assert.Equal("Group not found", messageValue);
        }


    }
}
