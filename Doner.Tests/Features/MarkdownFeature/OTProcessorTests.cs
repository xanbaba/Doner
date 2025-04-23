using Doner.Features.MarkdownFeature;
using Doner.Features.MarkdownFeature.OT;
using Doner.Features.MarkdownFeature.Repositories;
using FluentAssertions;
using Moq;

namespace Doner.Tests.Features.MarkdownFeature;

public class OTProcessorTests
{
    private readonly OTProcessor _processor;

    public OTProcessorTests()
    {
        var mockRepository = new Mock<IMarkdownRepository>();
        _processor = new OTProcessor(mockRepository.Object);
    }

    #region Retain vs Retain Tests

    [Fact]
    public void TransformComponents_RetainVsRetain_EqualLength_ShouldReturnSingleRetain()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<RetainComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(5);
    }

    [Fact]
    public void TransformComponents_RetainVsRetain_ClientLonger_ShouldReturnTwoRetains()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new RetainComponent { Count = 10 } };
        var serverComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<RetainComponent>();
        result[1].Should().BeOfType<RetainComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(5);
        (result[1] as RetainComponent)!.Count.Should().Be(5);
    }

    [Fact]
    public void TransformComponents_RetainVsRetain_ServerLonger_ShouldReturnSingleRetain()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new RetainComponent { Count = 10 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<RetainComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(5);
    }

    #endregion

    #region Retain vs Insert Tests

    [Fact]
    public void TransformComponents_RetainVsInsert_ShouldRetainInsertedText()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new InsertComponent { Text = "Hello" } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<RetainComponent>();
        result[1].Should().BeOfType<RetainComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(5); // Retain inserted text
        (result[1] as RetainComponent)!.Count.Should().Be(5); // Original retain
    }

    [Fact]
    public void TransformComponents_RetainVsInsert_WithTrailingOperation_ShouldPreserveOrder()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 5 },
            new InsertComponent { Text = "Client" }
        };
        var serverComponents = new List<OperationComponent> 
        { 
            new InsertComponent { Text = "Server" } 
        };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().BeOfType<RetainComponent>(); // Retain server insert
        result[1].Should().BeOfType<RetainComponent>(); // Original client retain
        result[2].Should().BeOfType<InsertComponent>(); // Client insert
        (result[0] as RetainComponent)!.Count.Should().Be(6); // "Server" length
        (result[1] as RetainComponent)!.Count.Should().Be(5); // Original retain
        (result[2] as InsertComponent)!.Text.Should().Be("Client");
    }

    #endregion

    #region Retain vs Delete Tests

    [Fact]
    public void TransformComponents_RetainVsDelete_EqualLength_ShouldRemoveRetain()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().BeEmpty(); // Cannot retain deleted text
    }

    [Fact]
    public void TransformComponents_RetainVsDelete_RetainLonger_ShouldRetainRemainder()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new RetainComponent { Count = 10 } };
        var serverComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<RetainComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(5); // Retain the remainder
    }

    [Fact]
    public void TransformComponents_RetainVsDelete_DeleteLonger_ShouldReturnNoComponents()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new DeleteComponent { Count = 10 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().BeEmpty(); // All retained text was deleted
    }

    [Fact]
    public void TransformComponents_RetainVsDelete_WithTrailingInsert_ShouldPreserveInsert()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 5 },
            new InsertComponent { Text = "Client" }
        };
        var serverComponents = new List<OperationComponent> 
        { 
            new DeleteComponent { Count = 5 } 
        };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<InsertComponent>();
        (result[0] as InsertComponent)!.Text.Should().Be("Client");
    }

    #endregion

    #region Insert Tests

    [Fact]
    public void TransformComponents_InsertVsRetain_ShouldPreserveInsert()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new InsertComponent { Text = "Client" } };
        var serverComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<InsertComponent>();
        (result[0] as InsertComponent)!.Text.Should().Be("Client");
    }

    [Fact]
    public void TransformComponents_InsertVsInsert_ShouldPositionAfterServerInsert()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new InsertComponent { Text = "Client" } };
        var serverComponents = new List<OperationComponent> { new InsertComponent { Text = "Server" } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<RetainComponent>();
        result[1].Should().BeOfType<InsertComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(6); // Retain server insert length
        (result[1] as InsertComponent)!.Text.Should().Be("Client");
    }

    [Fact]
    public void TransformComponents_InsertVsDelete_ShouldInsertAtSamePosition()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new InsertComponent { Text = "Client" } };
        var serverComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<InsertComponent>();
        (result[0] as InsertComponent)!.Text.Should().Be("Client");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void TransformComponents_DeleteVsRetain_EqualLength_ShouldPreserveDelete()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<DeleteComponent>();
        (result[0] as DeleteComponent)!.Count.Should().Be(5);
    }

    [Fact]
    public void TransformComponents_DeleteVsRetain_DeleteLonger_ShouldSplitDelete()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new DeleteComponent { Count = 10 } };
        var serverComponents = new List<OperationComponent> { new RetainComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<DeleteComponent>();
        result[1].Should().BeOfType<DeleteComponent>();
        (result[0] as DeleteComponent)!.Count.Should().Be(5);
        (result[1] as DeleteComponent)!.Count.Should().Be(5);
    }

    [Fact]
    public void TransformComponents_DeleteVsInsert_ShouldSkipInsertedText()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new InsertComponent { Text = "Server" } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<RetainComponent>();
        result[1].Should().BeOfType<DeleteComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(6); // Skip over server insert
        (result[1] as DeleteComponent)!.Count.Should().Be(5); // Original delete preserved
    }

    [Fact]
    public void TransformComponents_DeleteVsDelete_EqualLength_ShouldCancelOut()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().BeEmpty(); // No need to delete what server has already deleted
    }

    [Fact]
    public void TransformComponents_DeleteVsDelete_ClientLonger_ShouldDeleteRemainder()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new DeleteComponent { Count = 10 } };
        var serverComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<DeleteComponent>();
        (result[0] as DeleteComponent)!.Count.Should().Be(5); // Delete the remainder
    }

    [Fact]
    public void TransformComponents_DeleteVsDelete_ServerLonger_ShouldReturnNoComponents()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> { new DeleteComponent { Count = 5 } };
        var serverComponents = new List<OperationComponent> { new DeleteComponent { Count = 10 } };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().BeEmpty(); // Server already deleted everything client wanted to delete
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void TransformComponents_ComplexScenario1_MultipleComponentsWithOverlap()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 5 },
            new InsertComponent { Text = "Client" },
            new RetainComponent { Count = 10 },
            new DeleteComponent { Count = 7 }
        };
        
        var serverComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 3 },
            new DeleteComponent { Count = 4 },
            new InsertComponent { Text = "Server" },
            new RetainComponent { Count = 15 }
        };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Should().BeOfType<RetainComponent>();
        result[1].Should().BeOfType<InsertComponent>();
        result[2].Should().BeOfType<RetainComponent>();
        result[3].Should().BeOfType<RetainComponent>();
        result[4].Should().BeOfType<DeleteComponent>();
        
        (result[0] as RetainComponent)!.Count.Should().Be(3); // First retain
        (result[1] as InsertComponent)!.Text.Should().Be("Client"); // Client insert
        (result[2] as RetainComponent)!.Count.Should().Be(6); // Retain server insert
        (result[3] as RetainComponent)!.Count.Should().Be(8); // Transformed retain (10 - 2)
        (result[4] as DeleteComponent)!.Count.Should().Be(7); // Delete preserved
    }

    [Fact]
    public void TransformComponents_ComplexScenario2_EmptyClientComponents_ShouldReturnEmpty()
    {
        // Arrange
        OperationComponent[] clientComponents = [];
        var serverComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 5 },
            new InsertComponent { Text = "Server" } 
        };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TransformComponents_ComplexScenario3_EmptyServerComponents_ShouldPreserveClientComponents()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 5 },
            new InsertComponent { Text = "Client" } 
        };
        OperationComponent[] serverComponents = [];

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<RetainComponent>();
        result[1].Should().BeOfType<InsertComponent>();
        (result[0] as RetainComponent)!.Count.Should().Be(5);
        (result[1] as InsertComponent)!.Text.Should().Be("Client");
    }

    [Fact]
    public void TransformComponents_ComplexScenario4_OverlappingInserts()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 5 },
            new InsertComponent { Text = "Client1" },
            new RetainComponent { Count = 3 },
            new InsertComponent { Text = "Client2" }
        };
        
        var serverComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 5 },
            new InsertComponent { Text = "Server1" },
            new RetainComponent { Count = 3 },
            new InsertComponent { Text = "Server2" }
        };

        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();

        // Assert
        result.Should().HaveCount(7);
        result[0].Should().BeOfType<RetainComponent>(); // Retain first 5
        result[1].Should().BeOfType<RetainComponent>(); // Retain server1 insert
        result[2].Should().BeOfType<InsertComponent>(); // Client1 insert
        result[3].Should().BeOfType<RetainComponent>(); // Retain server component
        result[4].Should().BeOfType<RetainComponent>(); // Retain 3
        result[5].Should().BeOfType<RetainComponent>(); // Retain server2 insert
        result[6].Should().BeOfType<InsertComponent>(); // Client2 insert
        
        (result[0] as RetainComponent)!.Count.Should().Be(5);
        (result[1] as RetainComponent)!.Count.Should().Be(7); // "Server1" length
        (result[2] as InsertComponent)!.Text.Should().Be("Client1");
        (result[4] as RetainComponent)!.Count.Should().Be(3);
        (result[5] as RetainComponent)!.Count.Should().Be(7); // "Server2" length
        (result[6] as InsertComponent)!.Text.Should().Be("Client2");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TransformComponents_EdgeCase_ZeroLengthComponents_ShouldHandleGracefully()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 0 },
            new DeleteComponent { Count = 0 },
            new InsertComponent { Text = "" }
        };
        
        var serverComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 0 },
            new DeleteComponent { Count = 0 },
            new InsertComponent { Text = "" }
        };
    
        // Act
        var result = _processor.TransformComponents(clientComponents, serverComponents).ToList();
    
        // Assert
        // The current implementation preserves zero-length components
        // We just verify it doesn't crash and produces valid results
        result.ForEach(component => 
        {
            switch (component)
            {
                case RetainComponent retain:
                    retain.Count.Should().BeGreaterThanOrEqualTo(0);
                    break;
                case DeleteComponent delete:
                    delete.Count.Should().BeGreaterThanOrEqualTo(0);
                    break;
                case InsertComponent insert:
                    insert.Text.Should().NotBeNull();
                    break;
            }
        });
    }

    [Fact]
    public void TransformComponents_EdgeCase_NegativeCounts_ShouldThrowException()
    {
        // Arrange
        var clientComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = -5 }
        };
        
        var serverComponents = new List<OperationComponent> 
        { 
            new RetainComponent { Count = 10 }
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            _processor.TransformComponents(clientComponents, serverComponents).ToList());
    }

    [Fact]
    public void Transform_ShouldCreateNewOperationWithTransformedComponents()
    {
        // Arrange
        var clientOp = new Operation
        {
            Id = Guid.NewGuid(),
            MarkdownId = "markdown123",
            UserId = Guid.NewGuid(),
            BaseVersion = 5,
            Components = new List<OperationComponent> { new InsertComponent { Text = "Client" } }
        };
        
        var serverOp = new Operation
        {
            Id = Guid.NewGuid(),
            MarkdownId = "markdown123",
            UserId = Guid.NewGuid(),
            BaseVersion = 5,
            Components = new List<OperationComponent> { new InsertComponent { Text = "Server" } }
        };

        // Act
        var result = _processor.Transform(clientOp, serverOp);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(clientOp.Id);
        result.MarkdownId.Should().Be(clientOp.MarkdownId);
        result.UserId.Should().Be(clientOp.UserId);
        result.BaseVersion.Should().Be(serverOp.BaseVersion + 1);
        
        var components = result.Components.ToList();
        components.Should().HaveCount(2);
        components[0].Should().BeOfType<RetainComponent>();
        components[1].Should().BeOfType<InsertComponent>();
        (components[0] as RetainComponent)!.Count.Should().Be(6); // "Server" length
        (components[1] as InsertComponent)!.Text.Should().Be("Client");
    }

    #endregion
}
