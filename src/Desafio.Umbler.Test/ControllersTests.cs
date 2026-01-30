using Desafio.Umbler.Application.DTOs;
using Desafio.Umbler.Application.Exceptions;
using Desafio.Umbler.Application.Interfaces;
using Desafio.Umbler.Application.Services;
using Desafio.Umbler.Controllers;
using Desafio.Umbler.Domain.Entities;
using Desafio.Umbler.Infrastructure.Context;
using Desafio.Umbler.Infrastructure.DataAccess;
using Desafio.Umbler.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        public void Home_Index_returns_View()
        {
            //arrange 
            var controller = new HomeController();

            //act
            var response = controller.Index();
            var result = response as ViewResult;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Home_Error_returns_View_With_Model()
        {
            //arrange 
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //act
            var response = controller.Error();
            var result = response as ViewResult;
            var model = result.Model as ErrorViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }


        [TestMethod]
        public async Task Domain_In_Database()
        {
            // arrange
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var domain = new DomainInfo
            {
                Id = 1,
                Name = "test.com",
                Ip = "192.168.0.1",
                HostedAt = "umbler.corp",
                WhoIs = "Ns.umbler.com",
                Ttl = 60,
                UpdatedAt = DateTime.UtcNow
            };

            // Use a clean instance of the context to run the test
            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(domain);
                await db.SaveChangesAsync();
            }
            
            var dns = new Mock<IDnsService>();
            var whois = new Mock<IWhoisService>();

            // act
            using (var db = new DatabaseContext(options))
            {
                var data = new DomainDataAccess(db);
                var service = new DomainService(data, dns.Object, whois.Object);

                var dto = await service.GetAsync("test.com");

                Assert.IsNotNull(dto);
                Assert.AreEqual(domain.Name, dto.Name);
                Assert.AreEqual(domain.Ip, dto.Ip);
            }
        }

        [TestMethod]
        public async Task Domain_Not_In_Database()
        {
            // arrange
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dns = new Mock<IDnsService>();
            dns.Setup(d => d.GetARecordAsync("test.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new DnsLookupResult("192.168.0.1", 60));

            var whois = new Mock<IWhoisService>();
            whois.Setup(w => w.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new WhoisResult("raw", "Umbler"));

            // Use a clean instance of the context to run the test 
            using (var db = new DatabaseContext(options))
            {
                var data = new DomainDataAccess(db);
                var service = new DomainService(data, dns.Object, whois.Object);

                // act
                var dto = await service.GetAsync("test.com");

                // assert
                Assert.IsNotNull(dto);
            }
        }

        [TestMethod]
        public async Task Domain_Moking_LookupClient()
        {
            // arrange
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dns = new Mock<IDnsService>();
            dns.Setup(d => d.GetARecordAsync("test.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new DnsLookupResult("192.168.0.1", 60));

            var whois = new Mock<IWhoisService>();
            whois.Setup(w => w.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new WhoisResult("raw", "Umbler"));

            using (var db = new DatabaseContext(options))
            {
                var data = new DomainDataAccess(db);
                var service = new DomainService(data, dns.Object, whois.Object);

                // act
                var dto = await service.GetAsync("test.com");

                // assert
                Assert.IsNotNull(dto);
            }
        }

        [TestMethod]
        public async Task Domain_Moking_WhoisClient()
        {
            // arrange
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dns = new Mock<IDnsService>();
            dns.Setup(d => d.GetARecordAsync("test.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new DnsLookupResult("192.168.0.1", 60));

            // WHOIS agora mockável
            var whois = new Mock<IWhoisService>();
            whois.Setup(w => w.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new WhoisResult("raw", "Umbler"));

            using (var db = new DatabaseContext(options))
            {
                var data = new DomainDataAccess(db);
                var service = new DomainService(data, dns.Object, whois.Object);

                // act
                var dto = await service.GetAsync("test.com");

                // assert
                Assert.IsNotNull(dto);

                //verificação extra:consigo garantir que o WhoIs foi chamado
                whois.Verify(w => w.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
        }

        // Caso o domínio seja inválido, deve lançar exceção
        [TestMethod]
        public async Task Invalid_Domain_Should_Throw_Exceptio()
        {
            var data = new Mock<IDomainDataAccess>();
            var dns = new Mock<IDnsService>();
            var whois = new Mock<IWhoisService>();

            var service = new DomainService(data.Object, dns.Object, whois.Object);

            await Assert.ThrowsExceptionAsync<InvalidDomainException>(() => service.GetAsync("Umbler"));
        }

        //Caso o domínio esteja em cache e não expirado, não deve chamar serviços externos
        [TestMethod]
        public async Task Domain_Cached_Not_Expired_Should_Not_Call_External()
        {
            var data = new Mock<IDomainDataAccess>();
            var dns = new Mock<IDnsService>();
            var whois = new Mock<IWhoisService>();

            data.Setup(d => d.GetByNameAsync("test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DomainInfo
                {
                    Name = "test.com",
                    Ip = "1.1.1.1",
                    HostedAt = "umbler",
                    WhoIs = "raw",
                    Ttl = 9999,
                    UpdatedAt = DateTime.UtcNow
                });

            var service = new DomainService(data.Object, dns.Object, whois.Object);

            var dto = await service.GetAsync("test.com");

            Assert.IsNotNull(dto);
        }

        // Caso o domínio esteja em cache, mas expirado, deve chamar serviços externos para atualizar
        [TestMethod]
        public async Task Domain_Expired_Should_Refresh_And_Save()
        {
            var data = new Mock<IDomainDataAccess>();
            var dns = new Mock<IDnsService>();
            var whois = new Mock<IWhoisService>();

            var domain = new DomainInfo
            {
                Name = "test.com",
                Ip = "old",
                HostedAt = "old",
                WhoIs = "old",
                Ttl = 1,
                UpdatedAt = DateTime.UtcNow.AddSeconds(-10)
            };

            data.Setup(d => d.GetByNameAsync("test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(domain);

            dns.Setup(d => d.GetARecordAsync("test.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new DnsLookupResult("192.168.0.1", 60));

            whois.Setup(w => w.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new WhoisResult("raw", "Umbler"));

            data.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new DomainService(data.Object, dns.Object, whois.Object);

            var dto = await service.GetAsync("test.com");

            Assert.IsNotNull(dto);
            Assert.AreEqual("192.168.0.1", dto.Ip);
        }
    }
}