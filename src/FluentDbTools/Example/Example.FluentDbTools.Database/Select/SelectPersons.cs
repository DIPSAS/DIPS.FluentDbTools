using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using FluentDbTools.Extensions.SqlBuilder;
using FluentDbTools.Common.Abstractions;
using Example.FluentDbTools.Database.Entities;
using FluentDbTools.SqlBuilder.Abstractions.Common;

namespace Example.FluentDbTools.Database.Select
{
    public static class SelectPersons
    {
        public async static Task<IEnumerable<Person>> Execute(
            IDbConnection dbConnection,
            IDbConfigDatabaseTargets dbConfig,
            Guid[] ids)
        {
            var sql = dbConfig.BuildSql(ids, out var @params);
            var res = await dbConnection.QueryAsync<Person>(sql, @params);
            return res;
        }
        
        private static string BuildSql(this IDbConfigDatabaseTargets dbConfig, Guid[] ids, out DynamicParameters @params)
        {
            @params = new DynamicParameters();
            var inSelections = dbConfig.CreateParameterResolver().AddArrayParameter(@params, nameof(Person.PersonId), ids);
            var sql = dbConfig.CreateSqlBuilder().Select()
                .OnSchema()
                .Fields<Person>(x => x.F(item => item.PersonId))
                .Fields<Person>(x => x.F(item => item.SequenceNumber))
                .Fields<Person>(x => x.F(item => item.Alive))
                .Fields<Person>(x => x.F(item => item.Username))
                .Fields<Person>(x => x.F(item => item.Password))
                .Where<Person>(x => x.WP(item => item.PersonId, inSelections))
                .Build();
                
            return sql;
        }
    }
}