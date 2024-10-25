using ExpenseSharingApp.DTOs;
using ExpenseSharingApp.Models;
using ExpenseSharingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ExpenseSharingAppTests
{
    public class UserServiceTests
    {
        private readonly ExpenseSharingContext _context;
        private readonly UserService _service;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<ExpenseSharingContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ExpenseSharingContext(options);
            _service = new UserService(_context);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ExistingEmail_ReturnsUser()
        {
            // Arrange
            var userId = "test-id";
            var user = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                Password = "test-password", 
                Role = "User"               
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = await _service.GetUserByEmailAsync(user.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);

            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }

        [Fact]
        public void GetAll_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
    {
        new User {
            Id = "1",
            Name = "User 1",
            Email = "user1@example.com",
            Password = "test-password",
            Role = "User"
        },
        new User {
            Id = "2",
            Name = "User 2",
            Email = "user2@example.com",
            Password = "test-password",
            Role = "User"
        }
    };
            _context.Users.AddRange(users);
            _context.SaveChanges();

            // Debugging
            var savedUsers = _context.Users.ToList();
            //Console.WriteLine($"Number of users in database after save: {savedUsers.Count}");

            // Act
            var result = _service.GetAll();

            // Debugging
            //Console.WriteLine($"Number of users retrieved: {result.Count()}");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.Email == "user1@example.com");
            Assert.Contains(result, u => u.Email == "user2@example.com");

            // Cleanup
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }


        [Fact]
        public void GetById_ExistingId_ReturnsUser()
        {
            // Arrange
            var userId = "test-id";
            var user = new User { Id = userId, Name = "Test User", Email = "test@example.com" ,
                Password = "test-password",
                Role = "User"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _service.GetById(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("Test User", result.Name);
            Assert.Equal("test@example.com", result.Email);


            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }

        [Fact]
        public void Update_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var userId = "test-id";
            var user = new User { Id = userId, Name = "Old Name", Email = "test@example.com",
                Password = "test-password",
                Role = "User"
            };
            var updatedUserDto = new UserDTO { Id = userId, Name = "New Name", Email = "test@example.com",
                Role = "User"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _service.Update(userId, updatedUserDto);

            // Assert
            Assert.True(result);
            Assert.Equal("New Name", user.Name);
            _context.SaveChanges(); 


            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }

        [Fact]
        public void Update_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existing-id";
            var updatedUserDto = new UserDTO { Id = userId, Name = "New Name", Email = "test@example.com" };

            // Act
            var result = _service.Update(userId, updatedUserDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Delete_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var userId = "test-id";
            var user = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                Password = "test-password", 
                Role = "User"               
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _service.Delete(userId);

            // Assert
            Assert.True(result);
            Assert.Null(_context.Users.Find(userId)); 


            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }

        [Fact]
        public void Delete_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existing-id";

            // Act
            var result = _service.Delete(userId);

            // Assert
            Assert.False(result);
        }
    }
}
