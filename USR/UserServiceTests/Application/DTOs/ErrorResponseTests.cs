using Aristotle.Application.DTOs;
using Xunit;

namespace UserService.UnitTests.Application.DTOs;

/* TODO: Many test cases here and in other classes are not needed in real life, but are included to showcase thorough testing practices.
 In a real-world scenario, focus on meaningful tests that cover critical functionality and edge cases. */

/// <summary>
///     Unit tests for ErrorResponse DTO
/// </summary>
public class ErrorResponseTests
{
    [Fact]
    public void ErrorResponse_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var errorResponse = new ErrorResponse();

        // Assert
        Assert.Equal(string.Empty, errorResponse.Title);
        Assert.Equal(0, errorResponse.Status);
        Assert.Equal(string.Empty, errorResponse.Detail);
        Assert.Null(errorResponse.Extensions);
    }

    [Fact]
    public void ErrorResponse_SetTitle_ShouldUpdateTitle()
    {
        // Arrange
        var errorResponse = new ErrorResponse();
        const string title = "Validation Error";

        // Act
        errorResponse.Title = title;

        // Assert
        Assert.Equal(title, errorResponse.Title);
    }

    [Fact]
    public void ErrorResponse_SetStatus_ShouldUpdateStatus()
    {
        // Arrange
        const int status = 400;

        // Act
        var errorResponse = new ErrorResponse { Status = status };

        // Assert
        Assert.Equal(status, errorResponse.Status);
    }

    [Fact]
    public void ErrorResponse_SetDetail_ShouldUpdateDetail()
    {
        // Arrange
        var errorResponse = new ErrorResponse();
        const string detail = "The email field is required";

        // Act
        errorResponse.Detail = detail;

        // Assert
        Assert.Equal(detail, errorResponse.Detail);
    }

    [Fact]
    public void ErrorResponse_SetExtensions_ShouldUpdateExtensions()
    {
        // Arrange
        var errorResponse = new ErrorResponse();
        var extensions = new Dictionary<string, object?>
        {
            { "traceId", "trace-123" },
            { "errorCode", "ERR_001" }
        };

        // Act
        errorResponse.Extensions = extensions;

        // Assert
        Assert.NotNull(errorResponse.Extensions);
        Assert.Equal(2, errorResponse.Extensions.Count);
        Assert.Equal("trace-123", errorResponse.Extensions["traceId"]);
        Assert.Equal("ERR_001", errorResponse.Extensions["errorCode"]);
    }

    [Fact]
    public void ErrorResponse_ObjectInitializer_ShouldSetAllProperties()
    {
        // Act
        var errorResponse = new ErrorResponse
        {
            Title = "Not Found",
            Status = 404,
            Detail = "User not found",
            Extensions = new Dictionary<string, object?>
            {
                { "userId", "user-123" }
            }
        };

        // Assert
        Assert.Equal("Not Found", errorResponse.Title);
        Assert.Equal(404, errorResponse.Status);
        Assert.Equal("User not found", errorResponse.Detail);
        Assert.NotNull(errorResponse.Extensions);
        Assert.Single(errorResponse.Extensions);
    }

    [Fact]
    public void ErrorResponse_Status_WithHttpStatusCodes_ShouldAcceptValidStatusCodes()
    {
        // Arrange & Act
        var badRequest = new ErrorResponse { Status = 400 };
        var unauthorized = new ErrorResponse { Status = 401 };
        var notFound = new ErrorResponse { Status = 404 };
        var internalError = new ErrorResponse { Status = 500 };

        // Assert
        Assert.Equal(400, badRequest.Status);
        Assert.Equal(401, unauthorized.Status);
        Assert.Equal(404, notFound.Status);
        Assert.Equal(500, internalError.Status);
    }

    [Fact]
    public void ErrorResponse_Extensions_WithNullValues_ShouldAcceptNullValues()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Extensions = new Dictionary<string, object?>
            {
                { "nullableField", null },
                { "nonNullField", "value" }
            }
        };

        // Assert
        Assert.NotNull(errorResponse.Extensions);
        Assert.Null(errorResponse.Extensions["nullableField"]);
        Assert.Equal("value", errorResponse.Extensions["nonNullField"]);
    }

    [Fact]
    public void ErrorResponse_Extensions_WithEmptyDictionary_ShouldAcceptEmptyDictionary()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Extensions = new Dictionary<string, object?>()
        };

        // Assert
        Assert.NotNull(errorResponse.Extensions);
        Assert.Empty(errorResponse.Extensions);
    }

    [Fact]
    public void ErrorResponse_SetExtensionsToNull_ShouldAcceptNull()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Extensions = new Dictionary<string, object?> { { "key", "value" } }
        };

        // Act
        errorResponse.Extensions = null;

        // Assert
        Assert.Null(errorResponse.Extensions);
    }

    [Fact]
    public void ErrorResponse_WithLongTitle_ShouldAcceptLongStrings()
    {
        // Arrange
        var longTitle = new string('T', 500);

        // Act
        var errorResponse = new ErrorResponse
        {
            Title = longTitle
        };

        // Assert
        Assert.Equal(longTitle, errorResponse.Title);
        Assert.Equal(500, errorResponse.Title.Length);
    }

    [Fact]
    public void ErrorResponse_WithLongDetail_ShouldAcceptLongStrings()
    {
        // Arrange
        var longDetail = new string('D', 1000);

        // Act
        var errorResponse = new ErrorResponse
        {
            Detail = longDetail
        };

        // Assert
        Assert.Equal(longDetail, errorResponse.Detail);
        Assert.Equal(1000, errorResponse.Detail.Length);
    }

    [Fact]
    public void ErrorResponse_WithSpecialCharacters_ShouldPreserveSpecialCharacters()
    {
        // Arrange
        const string title = "Error<>@#$%";
        const string detail = "Details with special chars: <>\"'!@#";

        // Act
        var errorResponse = new ErrorResponse
        {
            Title = title,
            Detail = detail
        };

        // Assert
        Assert.Equal(title, errorResponse.Title);
        Assert.Equal(detail, errorResponse.Detail);
    }

    [Fact]
    public void ErrorResponse_Extensions_WithComplexObjects_ShouldAcceptComplexTypes()
    {
        // Arrange
        var complexObject = new
        {
            Timestamp = DateTime.UtcNow,
            UserId = Guid.NewGuid(),
            ErrorDetails = new List<string> { "Error1", "Error2" }
        };

        // Act
        var errorResponse = new ErrorResponse
        {
            Extensions = new Dictionary<string, object?>
            {
                { "metadata", complexObject },
                { "count", 42 },
                { "isRetryable", true }
            }
        };

        // Assert
        Assert.NotNull(errorResponse.Extensions);
        Assert.Equal(3, errorResponse.Extensions.Count);
        Assert.Equal(complexObject, errorResponse.Extensions["metadata"]);
        Assert.Equal(42, errorResponse.Extensions["count"]);
        Assert.True((bool)errorResponse.Extensions["isRetryable"]!);
    }

    [Fact]
    public void ErrorResponse_MultipleInstances_ShouldBeIndependent()
    {
        // Arrange & Act
        var error1 = new ErrorResponse
        {
            Title = "Error 1",
            Status = 400,
            Detail = "Detail 1"
        };

        var error2 = new ErrorResponse
        {
            Title = "Error 2",
            Status = 500,
            Detail = "Detail 2"
        };

        // Assert
        Assert.NotEqual(error1.Title, error2.Title);
        Assert.NotEqual(error1.Status, error2.Status);
        Assert.NotEqual(error1.Detail, error2.Detail);
    }

    [Fact]
    public void ErrorResponse_Status_WithNegativeValue_ShouldAcceptNegativeValues()
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse
        {
            Status = -1
        };

        // Assert
        Assert.Equal(-1, errorResponse.Status);
    }

    [Fact]
    public void ErrorResponse_Status_WithLargeValue_ShouldAcceptLargeValues()
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse
        {
            Status = int.MaxValue
        };

        // Assert
        Assert.Equal(int.MaxValue, errorResponse.Status);
    }

    [Fact]
    public void ErrorResponse_MultiplePropertiesSet_ShouldMaintainIndependence()
    {
        // Arrange & Act
        var error1 = new ErrorResponse { Title = "Title1", Status = 100, Detail = "Detail1" };
        var error2 = new ErrorResponse { Title = "Title2", Status = 200, Detail = "Detail2" };

        // Assert
        Assert.Equal("Title1", error1.Title);
        Assert.Equal(100, error1.Status);
        Assert.Equal("Title2", error2.Title);
        Assert.Equal(200, error2.Status);
    }

    [Fact]
    public void ErrorResponse_Extensions_WithDuplicateKeys_ShouldOverwriteExistingKey()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Extensions = new Dictionary<string, object?>
            {
                { "key1", "value1" }
            }
        };

        // Act
        errorResponse.Extensions["key1"] = "value2";

        // Assert
        Assert.Equal("value2", errorResponse.Extensions["key1"]);
    }

    [Fact]
    public void ErrorResponse_Title_CanBeSetToNull()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Title = "Initial Title"
        };

        // Act
        errorResponse.Title = null!;

        // Assert
        Assert.Null(errorResponse.Title);
    }

    [Fact]
    public void ErrorResponse_Detail_CanBeSetToNull()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Detail = "Initial Detail"
        };

        // Act
        errorResponse.Detail = null!;

        // Assert
        Assert.Null(errorResponse.Detail);
    }
}