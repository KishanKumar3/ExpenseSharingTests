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
    public class ExpenseControllerTests
    {
        private readonly Mock<IExpenseService> _mockExpenseService;
        private readonly ExpenseController _controller;

        public ExpenseControllerTests()
        {
            _mockExpenseService = new Mock<IExpenseService>();
            _controller = new ExpenseController(_mockExpenseService.Object);
        }

        [Fact]
        public void AddExpense_ValidExpense_ReturnsOkResult()
        {
            // Arrange
            var expenseDetails = new ExpenseDetailsModel
            {
                GroupId = "group1",
                Description = "Test Expense",
                Amount = 100,
                PaidById = "user1",
                Date = DateTime.Now
            };

            _mockExpenseService.Setup(s => s.AddExpense(It.IsAny<string>(), It.IsAny<ExpenseDetailsModel>()));

            // Act
            var result = _controller.AddExpense(expenseDetails);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Expense added successfully.", okResult.Value);
        }

        [Fact]
        public void AddExpense_InvalidExpense_ReturnsBadRequest()
        {
            // Arrange
            var expenseDetails = new ExpenseDetailsModel
            {
                GroupId = "group1",
                Description = "Test Expense",
                Amount = 100,
                PaidById = "user1",
                Date = DateTime.Now
            };

            _mockExpenseService.Setup(s => s.AddExpense(It.IsAny<string>(), It.IsAny<ExpenseDetailsModel>()))
                .Throws(new ArgumentException("Invalid expense details"));

            // Act
            var result = _controller.AddExpense(expenseDetails);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid expense details", badRequestResult.Value);
        }

        [Fact]
        public void GetAllExpenses_ReturnsOkResultWithExpenses()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new Expense { Id = "1", Description = "Expense 1", Amount = 100 },
                new Expense { Id = "2", Description = "Expense 2", Amount = 200 }
            };

            _mockExpenseService.Setup(s => s.GetAllExpenses()).Returns(expenses);

            // Act
            var result = _controller.GetAllExpenses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedExpenses = Assert.IsAssignableFrom<IEnumerable<Expense>>(okResult.Value);
            Assert.Equal(2, returnedExpenses.Count());
        }

        [Fact]
        public void GetExpenseById_ExistingId_ReturnsOkResultWithExpense()
        {
            // Arrange
            var expenseId = "1";
            var expense = new Expense { Id = expenseId, Description = "Test Expense", Amount = 100 };

            _mockExpenseService.Setup(s => s.GetExpenseById(expenseId)).Returns(expense);

            // Act
            var result = _controller.GetExpenseById(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedExpense = Assert.IsType<Expense>(okResult.Value);
            Assert.Equal(expenseId, returnedExpense.Id);
        }

        [Fact]
        public void GetExpenseById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var expenseId = "999";

            _mockExpenseService.Setup(s => s.GetExpenseById(expenseId)).Throws(new KeyNotFoundException());

            // Act
            var result = _controller.GetExpenseById(expenseId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void GetAllExpensesOfGroup_ValidGroupId_ReturnsOkResultWithExpenses()
        {
            // Arrange
            var groupId = "group1";
            var expenses = new List<Expense>
            {
                new Expense { Id = "1", Description = "Expense 1", Amount = 100, GroupId = groupId },
                new Expense { Id = "2", Description = "Expense 2", Amount = 200, GroupId = groupId }
            };

            _mockExpenseService.Setup(s => s.GetAllExpensesOfGroup(groupId)).Returns(expenses);

            // Act
            var result = _controller.GetAllExpensesOfGroup(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedExpenses = Assert.IsAssignableFrom<IEnumerable<Expense>>(okResult.Value);
            Assert.Equal(2, returnedExpenses.Count());
        }

        [Fact]
        public void GetAllExpensesOfGroup_InvalidGroupId_ReturnsInternalServerError()
        {
            // Arrange
            var groupId = "invalidGroup";

            _mockExpenseService.Setup(s => s.GetAllExpensesOfGroup(groupId)).Throws(new Exception("Internal server error"));

            // Act
            var result = _controller.GetAllExpensesOfGroup(groupId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public void UpdateExpense_ValidExpense_ReturnsOkResult()
        {
            // Arrange
            var expenseId = "1";
            var updatedExpenseDetails = new ExpenseDetailsModel
            {
                GroupId = "group1",
                Description = "Updated Expense",
                Amount = 150,
                PaidById = "user1",
                Date = DateTime.Now
            };

            _mockExpenseService.Setup(s => s.UpdateExpense(expenseId, updatedExpenseDetails));

            // Act
            var result = _controller.UpdateExpense(expenseId, updatedExpenseDetails);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Expense updated successfully.", okResult.Value);
        }

        [Fact]
        public void UpdateExpense_InvalidExpense_ReturnsBadRequest()
        {
            // Arrange
            var expenseId = "1";
            var updatedExpenseDetails = new ExpenseDetailsModel
            {
                GroupId = "group1",
                Description = "Updated Expense",
                Amount = 150,
                PaidById = "user1",
                Date = DateTime.Now
            };

            _mockExpenseService.Setup(s => s.UpdateExpense(expenseId, updatedExpenseDetails))
                .Throws(new ArgumentException("Invalid expense details"));

            // Act
            var result = _controller.UpdateExpense(expenseId, updatedExpenseDetails);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid expense details", badRequestResult.Value);
        }

        [Fact]
        public void UpdateExpense_NonExistingExpense_ReturnsNotFound()
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

            _mockExpenseService.Setup(s => s.UpdateExpense(expenseId, updatedExpenseDetails))
                .Throws(new KeyNotFoundException("Expense not found"));

            // Act
            var result = _controller.UpdateExpense(expenseId, updatedExpenseDetails);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Expense not found", notFoundResult.Value);
        }

        [Fact]
        public void DeleteExpense_ExistingExpense_ReturnsOkResult()
        {
            // Arrange
            var expenseId = "1";

            _mockExpenseService.Setup(s => s.DeleteExpense(expenseId));

            // Act
            var result = _controller.DeleteExpense(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Expense deleted successfully.", okResult.Value);
        }

        [Fact]
        public void DeleteExpense_NonExistingExpense_ReturnsNotFound()
        {
            // Arrange
            var expenseId = "999";

            _mockExpenseService.Setup(s => s.DeleteExpense(expenseId))
                .Throws(new KeyNotFoundException("Expense not found"));

            // Act
            var result = _controller.DeleteExpense(expenseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Expense not found", notFoundResult.Value);
        }

        [Fact]
        public void SettleExpense_ExistingExpense_ReturnsOkResult()
        {
            // Arrange
            var expenseId = "1";

            _mockExpenseService.Setup(s => s.SettleExpense(expenseId));

            // Act
            var result = _controller.SettleExpense(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Expenses settled successfully.", okResult.Value);
        }

        [Fact]
        public void SettleExpense_NonExistingExpense_ReturnsNotFound()
        {
            // Arrange
            var expenseId = "999";

            _mockExpenseService.Setup(s => s.SettleExpense(expenseId))
                .Throws(new KeyNotFoundException("Expense not found"));

            // Act
            var result = _controller.SettleExpense(expenseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Expense not found", notFoundResult.Value);
        }

        [Fact]
        public void SettleExpense_InvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var expenseId = "1";

            _mockExpenseService.Setup(s => s.SettleExpense(expenseId))
                .Throws(new InvalidOperationException("Cannot settle this expense"));

            // Act
            var result = _controller.SettleExpense(expenseId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cannot settle this expense", badRequestResult.Value);
        }
    }
}
