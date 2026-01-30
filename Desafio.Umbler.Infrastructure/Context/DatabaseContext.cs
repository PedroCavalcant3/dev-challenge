using System;
using System.ComponentModel.DataAnnotations;
using Desafio.Umbler.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Desafio.Umbler.Infrastructure.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
        {

        }
        //Renamed to avoid ambiguity between the Domain project and the Domain entity
        public DbSet<DomainInfo> Domains => Set<DomainInfo>();
    }
    
}