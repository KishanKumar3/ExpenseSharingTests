using ExpenseSharingApp.Models;
using ExpenseSharingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ExpenseSharingAppTests
{
    public class GroupServiceTests : IDisposable
    {
        private readonly ExpenseSharingContext _context;
        private readonly GroupService _service;

        public GroupServiceTests()
        {
            var options = new DbContextOptionsBuilder<ExpenseSharingContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ExpenseSharingContext(options);
            _service = new GroupService(_context);
        }

        [Fact]
        public void CreateGroup_ValidModel_ReturnsCreatedGroup()
        {
            // Arrange
            var groupCreationModel = new GroupCreationModel
            {
                Name = "Test Group",
                Description = "Test Description",
                CreatedDate = DateTime.Now,
                MemberEmails = new List<string> { "test@example.com" }
            };

            var user = new User { Id = "test-user-id", Email = "test@example.com", Name = "Test User", Password = "password", Role = "User" };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _service.CreateGroup(groupCreationModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(groupCreationModel.Name, result.Name);
            Assert.Equal(groupCreationModel.Description, result.Description);
            Assert.Equal(groupCreationModel.CreatedDate, result.CreatedDate);
            Assert.Single(result.Members);
        }

        [Fact]
        public void GetAllGroups_ReturnsAllGroups()
        {
            // Arrange
            var groups = new List<Group>
            {
                new Group { GroupId = "1", Name = "Group 1",
        Description = "Test Description" },
                new Group { GroupId = "2", Name = "Group 2",
        Description = "Test Description" }
            };
            _context.Groups.AddRange(groups);
            _context.SaveChanges();

            // Act
            var result = _service.GetAllGroups();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, g => g.Name == "Group 1");
            Assert.Contains(result, g => g.Name == "Group 2");
        }

        [Fact]
        public void GetGroupById_ExistingId_ReturnsGroup()
        {
            // Arrange
            var groupId = "test-group-id";
            var group = new Group { GroupId = groupId, Name = "Test Group",
                Description = "Test Description"
            };
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Act
            var result = _service.GetGroupById(groupId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(groupId, result.GroupId);
            Assert.Equal("Test Group", result.Name);
        }

        [Fact]
        public void GetGroupsByUserId_ReturnsUserGroups()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Password = "password", Role = "User" };
            _context.Users.Add(user);

            var groups = new List<Group>
            {
                new Group { GroupId = "1", Name = "Group 1" ,
        Description = "Test Description"},
                new Group { GroupId = "2", Name = "Group 2",
        Description = "Test Description" }
            };
            _context.Groups.AddRange(groups);
            _context.SaveChanges();

            var userGroups = new List<UserGroup>
            {
                new UserGroup { UserId = userId, GroupId = "1" },
                new UserGroup { UserId = userId, GroupId = "2" }
            };
            _context.UserGroups.AddRange(userGroups);
            _context.SaveChanges();

            // Act
            var result = _service.GetGroupsByUserId(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, g => g.Name == "Group 1");
            Assert.Contains(result, g => g.Name == "Group 2");
        }

        [Fact]
        public void DeleteGroup_ExistingGroup_DeletesGroupAndRelatedData()
        {
            // Arrange
            var groupId = "test-group-id";
            var group = new Group
            {
                GroupId = groupId,
                Name = "Test Group",
                Description = "Test Description"
            };
            _context.Groups.Add(group);

            var expense = new Expense
            {
                Id = "1",
                GroupId = groupId,
                Amount = 100,
                Description = "Test Expense", 
                PaidById = "test-user-id"
            };
            _context.Expenses.Add(expense);
            _context.SaveChanges();

            // Act
            _service.DeleteGroup(groupId);

            // Assert
            Assert.Null(_context.Groups.Find(groupId));
            Assert.Empty(_context.Expenses.Where(e => e.GroupId == groupId));
        }


        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
