using ChatApi.Domain;
using ChatApi.Domain.Enum;
using ChatApi.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ChatSessionsControllerTests
{
    private readonly Mock<IChatSessionService> _mockChatSessionService;

    private readonly ChatSessionsController _controller;

    public ChatSessionsControllerTests()
    {
        _mockChatSessionService = new Mock<IChatSessionService>();
        _controller = new ChatSessionsController(_mockChatSessionService.Object);
    }

    [Fact]
    public async Task StartChatSession_ReturnsChatSession_WhenChatSessionIsCreated()
    {
        // Arrange
        var chatSession = new ChatSession { Id = 1 };
        _mockChatSessionService.Setup(s => s.CreateSessionAsync()).ReturnsAsync(chatSession);

        // Act
        var result = await _controller.StartChatSession();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ChatSession>(okResult.Value);
        Assert.Equal(chatSession.Id, returnValue.Id);
    }

    [Fact]
    public async Task StartChatSession_ReturnsServiceUnavailable_WhenChatSessionIsNotCreated()
    {
        // Arrange
        _mockChatSessionService.Setup(s => s.CreateSessionAsync()).ReturnsAsync((ChatSession)null);

        // Act
        var result = await _controller.StartChatSession();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
        var error = Assert.IsType<string>(objectResult.Value);
        Assert.Equal("Chat session could not be started due to server conditions.", error);
    }

    [Fact]
    public async Task Poll_UpdatesAndReturnsOk_WhenChatSessionExists()
    {
        // Arrange
        var chatSession = new ChatSession { Id = 1 };
        _mockChatSessionService.Setup(s => s.GetSessionAsync(1)).ReturnsAsync(chatSession);

        // Act
        var result = await _controller.Poll(1);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        _mockChatSessionService.Verify(s => s.UpdateSessionAsync(It.IsAny<ChatSession>()), Times.Once);
    }

    [Fact]
    public async Task Poll_ReturnsNotFound_WhenChatSessionDoesNotExist()
    {
        // Arrange
        _mockChatSessionService.Setup(s => s.GetSessionAsync(1)).ReturnsAsync((ChatSession)null);

        // Act
        var result = await _controller.Poll(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
   
}
