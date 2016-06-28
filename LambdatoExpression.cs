using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace MMercan.Common.Tests.ExpressionTests
{
    public class LambdatoExpression
    {

        [Fact]
        public void GT()
        {
            // Expression<Func<int, bool>> lambda = num1 => num1 < 5;


            ParameterExpression num = Expression.Parameter(typeof(int), "num");
            //Expression numvar = Expression.

            Expression cons = Expression.Constant(5);

            BinaryExpression gt = Expression.GreaterThan(num, cons);
            var ex = Expression.Lambda<Func<int, bool>>(gt, new ParameterExpression[] { num });

            var comp = ex.Compile();
            var res = comp.Invoke(9);
            Assert.True(res);

            var res2 = comp.Invoke(4);
            Assert.False(res2);

        }

        [Fact]
        public void whereorder()
        {
            //companies.Where(company => (company.ToLower() == "coho winery" || company.Length > 16)).OrderBy(company => company)

            string[] companies = { "Consolidated Messenger", "Alpine Ski House", "Southridge Video", "City Power & Light",
                   "Coho Winery", "Wide World Importers", "Graphic Design Institute", "Adventure Works",
                   "Humongous Insurance", "Woodgrove Bank", "Margie's Travel", "Northwind Traders",
                   "Blue Yonder Airlines", "Trey Research", "The Phone Company",
                   "Wingtip Toys", "Lucerne Publishing", "Fourth Coffee" };

            IQueryable<String> queryableData = companies.AsQueryable<string>();


            ParameterExpression company = Expression.Parameter(typeof(string), "company");
            Expression left1 = Expression.Call(company, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
            Expression right1 = Expression.Constant("coho winery");
            BinaryExpression ex1 = Expression.Equal(left1, right1);

            Expression left2 = Expression.Property(company, typeof(string).GetProperty("Length"));
            Expression right2 = Expression.Constant(16);
            BinaryExpression ex2 = Expression.GreaterThan(left2, right2);


            BinaryExpression or = Expression.OrElse(ex1, ex2);

            MethodCallExpression wherecallexpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryableData.ElementType },
                queryableData.Expression,
                Expression.Lambda<Func<string, bool>>(or, new ParameterExpression[] { company }));



            MethodCallExpression orderbyexpression = Expression.Call(typeof(Queryable),
                "OrderBy",
                new Type[] { queryableData.ElementType, queryableData.ElementType },
                wherecallexpression,
                Expression.Lambda<Func<string, string>>(company, new ParameterExpression[] { company }));


            IQueryable<string> results = queryableData.Provider.CreateQuery<string>(orderbyexpression);
            var items = results.ToList();
            Assert.NotEmpty(items);


        }

        [Fact]
        public void compareee()
        {
            //Func<int, int, bool> f = (a, b) => a < b;


            ParameterExpression p1 = Expression.Parameter(typeof(int), "p1");
            ParameterExpression p2 = Expression.Parameter(typeof(int), "p2");

            Expression com = Expression.Equal(p1, p2);

            Expression<Func<int, int, bool>> exp = Expression.Lambda<Func<int, int, bool>>(com, new ParameterExpression[] { p1, p2 });
            var rees = exp.Compile()(5, 10);
            Assert.False(rees);
        }
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
