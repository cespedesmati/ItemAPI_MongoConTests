using FluentAssertions;
using FluentAssertions.Extensions;
using ItemAPI_MongoConTests.Api.Controllers;
using ItemAPI_MongoConTests.Api.DTOs;
using ItemAPI_MongoConTests.Api.Models;
using ItemAPI_MongoConTests.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemAPI_MongoConTests.UnitTests
{
    public class ItemControllerTests
    {
        private readonly Mock<IItemsRepository> repositoryStub = new();
        private readonly Random rand = new();

        [Fact]
        public void UnitOfWork_StateUnderTest_ExpectedBehavior()
        {
            /*
            Convencion para nombrar a los tests:
                UnitOfWork : La funcionalidad que voy a testear
                StateUnderTest :  Condiciones bajo las que estamos testeando este metodo
                ExpectedBehavior : Lo que esperamos despues de ejecutar este test
            */
        }

        [Fact]
        public async void GetItem_WithUnexistingItem_ReturnsNotFound()
        {
            //Arrange
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                !.ReturnsAsync((Item)null);

            var controller = new ItemsController(repositoryStub.Object);

            //Act
            var result = await controller.GetItem(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void GetItem_WithExistingItem_ReturnsExpectedItem()
        {

            //Arrange
            var expectedItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(expectedItem);

            var controller = new ItemsController(repositoryStub.Object);

            //Act
            var result = await controller.GetItem(Guid.NewGuid());

            //Assert
            result.Value.Should().BeEquivalentTo(
                expectedItem,
                options => options.ComparingByMembers<Item>());

            /*
                .Should().BeEquivalentTo()
                es de FluentAssertions me permite escribir en un solo meotodo 
                la comprobacion de cada uno de los atributos de los objetos
                de otra forma tendria que comparar result.Id vs expectedItem.Id
                asi con todos
                el parametro options es para que las comparaciones se haga atributo por atributo
                y lo necesito usar porque Item es un recordTypes con clases comunes no hace falta
                si quiero que solo compare con los atributos que tienen en comun tengo que agregar .ExcludingMissingMembers()        
            */
        }

        [Fact]
        public async void GetItemsAsync_WithExistingItems_ReturnsAllItems()
        {
            // Given
            var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };

            repositoryStub.Setup(repo => repo.GetItemsAsync())
                .ReturnsAsync(expectedItems);

            var controller = new ItemsController(repositoryStub.Object);

            // When
            var result = await controller.GetItems();
            
            // Then
            var resultItems = (result.Result as OkObjectResult)!.Value as IEnumerable<ItemDto>;
            resultItems.Should().BeEquivalentTo(
                expectedItems,
                options => options.ComparingByMembers<Item>()
            );
        }

        [Fact]
        public async void CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
        {
            // Given
            var itemToCreate = new CreateItemDto()
            {
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000)
            };

            var controller = new ItemsController(repositoryStub.Object);

            // When
            var result = await controller.CreateItem(itemToCreate);

            // Then
            var CreateItem = (result.Result as CreatedAtActionResult)!.Value as ItemDto;

            itemToCreate.Should().BeEquivalentTo(
                CreateItem,
                options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers()
            );

            CreateItem.Id.Should().NotBeEmpty();
            CreateItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, 1000.Milliseconds());
        }

        [Fact]
        public async void UpdateItemAsync_WithExistingItem_ReturnsNoContent()
        {
            // Given
            Item existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            var itemId = existingItem.Id;

            var itemToUpdate = new UpdateItemDto()
            {
                Name = Guid.NewGuid().ToString(),
                Price = existingItem.Price + 3
            };

            var controller = new ItemsController(repositoryStub.Object);

            // When
            var result = await controller.UpdateItem(itemId, itemToUpdate);
            // Then
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async void DeleteItem_WithExistingItem_ReturnsNoContent()
        {
            // Given
            Item existingITem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingITem);

            var controller = new ItemsController(repositoryStub.Object);

            // When
            var result = await controller.DeleteItem(existingITem.Id);

            // Then
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async void GetItems_WithMatchingItems_ReturnsMatchingItems()
        {
            // Given
            var allItems = new[]{
                new Item(){Name = "Potion"},
                new Item(){Name = "Antidote"},
                new Item(){Name = "Hi-Potion"},
            };

            var nameToMatch = "Potion";

            repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(allItems);

            var controller = new ItemsController(repositoryStub.Object);

            // When
            var result = await controller.GetItems(nameToMatch);

            // Then
            var resultItems = (result.Result as OkObjectResult)!.Value as IEnumerable<ItemDto>;
            resultItems.Should()
                .OnlyContain(item => item.Name == allItems[0].Name || item.Name == allItems[2].Name);
        }

        private Item CreateRandomItem()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreatedDate = DateTimeOffset.UtcNow
            };
        }
    }
}
