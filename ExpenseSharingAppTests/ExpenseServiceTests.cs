using ExpenseSharingApp.Models;
using ExpenseSharingApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ExpenseSharingAppTests
{
    public class ExpenseServiceTests: IDisposable
    {
        private readonly ExpenseSharingContext _context;
        private readonly ExpenseService _service;

        public ExpenseServiceTests()
        {
            var options = new DbContextOptionsBuilder<ExpenseSharingContext>()
                .UseInMemoryDatabase(databaseName: "ExpenseSharingTestDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            _context = new ExpenseSharingContext(options);
            _service = new ExpenseService(_context);
        }

        

        [Fact]
        public void AddExpense_ValidExpense_AddsExpenseSuccessfully()
        {
            // Arrange
            var groupId = "group1";
            var expenseDetails = new ExpenseDetailsModel
            {
                GroupId = groupId,
                Description = "Test Expense",
                Amount = 100,
                PaidById = "user1",
                Date = DateTime.Now
            };

            
            var group = new Group
            {
                GroupId = groupId,
                Name = "Test Group",
                Description = "Group Description",
                Members = new List<UserGroup>
        {
            new UserGroup { UserId = "user1", GroupId = groupId },
            new UserGroup { UserId = "user2", GroupId = groupId }
        }
            };

            
            var paidByUser = new User
            {
                Id = "user1",
                Email = "user1@example.com", 
                Name = "User One", 
                Password = "password", 
                Role = "User" 
            };

            _context.Groups.Add(group);
            _context.Users.Add(paidByUser);
            _context.SaveChanges(); 

            // Act
            _service.AddExpense(groupId, expenseDetails);

            // Assert
            var addedExpense = _context.Expenses.FirstOrDefault(e => e.Description == "Test Expense");
            Assert.NotNull(addedExpense);
            Assert.Equal(100, addedExpense.Amount);
        }


        [Fact]
        public void AddExpense_InvalidGroup_ThrowsArgumentException()
        {
            // Arrange
            var groupId = "invalidGroup";
            var expenseDetails = new ExpenseDetailsModel
            {
                GroupId = groupId,
                Description = "Test Expense",
                Amount = 100,
                PaidById = "user1",
                Date = DateTime.Now
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.AddExpense(groupId, expenseDetails));
        }

        [Fact]
        public void GetAllExpenses_ReturnsAllExpenses()
        {
            // Arrange
            var expenses = new List<Expense>
    {
        new Expense { Id = "1", Description = "Expense 1", Amount = 100, GroupId = "group1", PaidById = "user1" },
        new Expense { Id = "2", Description = "Expense 2", Amount = 200, GroupId = "group2", PaidById = "user2" }
    };

            _context.Expenses.AddRange(expenses);
            _context.SaveChanges();

            // Act
            var result = _service.GetAllExpenses();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1" && e.Description == "Expense 1" && e.Amount == 100);
            Assert.Contains(result, e => e.Id == "2" && e.Description == "Expense 2" && e.Amount == 200);
        }

        [Fact]
        public void GetExpenseById_ExistingExpense_ReturnsExpense()
        {
            // Arrange
            var expenseId = "1";
            var groupId = "group1";
            var paidById = "user1";

            // Create a group
            var group = new Group
            {
                GroupId = groupId,
                Name = "Group 1",
                Description = "This is group 1"
            };
            _context.Groups.Add(group);

            // Create a user
            var user = new User
            {
                Id = paidById,
                Name = "User 1",
                Email = "user1@example.com",
                Password = "password",
                Role = "Member"
            };
            _context.Users.Add(user);

            // Create an expense
            var expense = new Expense
            {
                Id = expenseId,
                Description = "Test Expense",
                Amount = 100,
                GroupId = groupId,
                PaidById = paidById
            };
            _context.Expenses.Add(expense);
            _context.SaveChanges();

            // Act
            var result = _service.GetExpenseById(expenseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expenseId, result.Id);
            Assert.Equal("Test Expense", result.Description);
            Assert.Equal(100, result.Amount);
        }


        [Fact]
        public void GetExpenseById_NonExistingExpense_ThrowsKeyNotFoundException()
        {
            // Arrange
            var expenseId = "999";

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => _service.GetExpenseById(expenseId));
        }

        [Fact]
        public void GetAllExpensesOfGroup_ValidGroupId_ReturnsExpenses()
        {
            // Arrange
            var groupId = "group1";

            
            var user1 = new User { Id = "user1", Name = "User 1", Email = "user1@example.com", Password = "password1", Role = "member" };
            var user2 = new User { Id = "user2", Name = "User 2", Email = "user2@example.com", Password = "password2", Role = "member" };

            
            var group = new Group
            {
                GroupId = groupId,
                Name = "Group 1",
                Description = "This is group 1",
                Members = new List<UserGroup>
        {
            new UserGroup { UserId = "user1", GroupId = groupId, User = user1 },
            new UserGroup { UserId = "user2", GroupId = groupId, User = user2 }
        }
            };

            _context.Users.AddRange(user1, user2);
            _context.Groups.Add(group);

            
            var expenses = new List<Expense>
    {
        new Expense { Id = "1", Description = "Expense 1", Amount = 100, GroupId = groupId, PaidById = "user1" },
        new Expense { Id = "2", Description = "Expense 2", Amount = 200, GroupId = groupId, PaidById = "user2" }
    };

            _context.Expenses.AddRange(expenses);
            _context.SaveChanges();

            // Act
            var result = _service.GetAllExpensesOfGroup(groupId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal(groupId, e.GroupId));
        }






        [Fact]
        public void UpdateExpense_ValidExpense_UpdatesExpenseSuccessfully()
        {
            // Arrange
            var groupId = "group1";
            var userId = "user1";

            var group = new Group
            {
                GroupId = groupId,
                Name = "Test Group",
                Description = "Group Description"
            };

            var user = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "testuser@example.com",
                Password = "password",
                Role = "member"
            };

            var existingExpense = new Expense
            {
                Id = "1",
                Description = "Old Expense",
                Amount = 100,
                GroupId = groupId,
                PaidById = userId
            };

            _context.Groups.Add(group);
            _context.Users.Add(user);
            _context.Expenses.Add(existingExpense);
            _context.SaveChanges();

            var updatedExpenseDetails = new ExpenseDetailsModel
            {
                GroupId = groupId,
                Description = "Updated Expense",
                Amount = 150,
                PaidById = userId,
                Date = DateTime.Now
            };

            // Act
            _service.UpdateExpense(existingExpense.Id, updatedExpenseDetails);

            // Assert
            var updatedExpense = _context.Expenses.Find(existingExpense.Id);
            Assert.NotNull(updatedExpense);
            Assert.Equal(updatedExpenseDetails.Description, updatedExpense.Description);
            Assert.Equal(updatedExpenseDetails.Amount, updatedExpense.Amount);
            Assert.Equal(updatedExpenseDetails.GroupId, updatedExpense.GroupId);
            Assert.Equal(updatedExpenseDetails.PaidById, updatedExpense.PaidById);
        }



        [Fact]
        public void UpdateExpense_NonExistingExpense_ThrowsKeyNotFoundException()
        {
            // Arrange
            var expenseId = "999";
            var updatedExpenseDetails = new ExpenseDetailsModel
            {
                GroupId = "group1",
                Description = "Updated Expense",
                Amount = 150,
                PaidById = "user1",
                Date = DateTime.Now
            };

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => _service.UpdateExpense(expenseId, updatedExpenseDetails));
        }

        [Fact]
        public void DeleteExpense_ExistingExpense_DeletesExpenseSuccessfully()
        {
            // Arrange
            var expenseId = "1";
            var groupId = "group1";
            var paidById = "user1";
            var existingExpense = new Expense
            {
                Id = expenseId,
                Description = "Test Expense",
                Amount = 100,
                GroupId = groupId,
                PaidById = paidById
            };

            _context.Expenses.Add(existingExpense);
            _context.SaveChanges();

            // Act
            _service.DeleteExpense(expenseId);

            // Assert
            var deletedExpense = _context.Expenses.Find(expenseId);
            Assert.Null(deletedExpense);
        }


        [Fact]
        public void DeleteExpense_NonExistingExpense_ThrowsKeyNotFoundException()
        {
            // Arrange
            var expenseId = "999";

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => _service.DeleteExpense(expenseId));
        }

        [Fact]
        public void SettleExpense_ExistingExpense_SettlesExpenseSuccessfully()
        {
            // Arrange
            var expenseId = "1";
            var expense = new Expense
            {
                Id = expenseId,
                Description = "Test Expense", 
                Amount = 300,
                GroupId = "group1",
                PaidById = "user1",
                IsSettled = false
            };

            var group = new Group
            {
                GroupId = "group1",
                Name = "Test Group", 
                Description = "This is a test group", 
                Members = new List<UserGroup>
        {
            new UserGroup { UserId = "user1", User = new User { Id = "user1", Balance = 0, Name = "User 1", Email = "user1@example.com", Password = "password", Role = "Member" } },
            new UserGroup { UserId = "user2", User = new User { Id = "user2", Balance = 0, Name = "User 2", Email = "user2@example.com", Password = "password", Role = "Member" } },
            new UserGroup { UserId = "user3", User = new User { Id = "user3", Balance = 0, Name = "User 3", Email = "user3@example.com", Password = "password", Role = "Member" } }
        }
            };

            _context.Expenses.Add(expense);
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Act
            _service.SettleExpense(expenseId);

            // Assert
            Assert.True(expense.IsSettled);
            Assert.Equal(200, group.Members.First(m => m.UserId == "user1").User.Balance);
            Assert.Equal(-100, group.Members.First(m => m.UserId == "user2").User.Balance);
            Assert.Equal(-100, group.Members.First(m => m.UserId == "user3").User.Balance);
            _context.SaveChanges();
        }


        [Fact]
        public void SettleExpense_NonExistingExpense_ThrowsException()
        {
            // Arrange
            var expenseId = "999";

            // Act & Assert
            Assert.Throws<Exception>(() => _service.SettleExpense(expenseId));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
