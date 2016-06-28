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

        [Fact]
        public void callCompNoParam()
        {
            ParameterExpression local = Expression.Parameter(typeof(Company), "company");

            Expression paramid = Expression.Property(local, typeof(Company).GetProperty("Id"));
            Expression assignid = Expression.Assign(paramid, Expression.Constant(3));

            Expression paramName = Expression.Property(local, typeof(Company).GetProperty("Name"));
            Expression assignname = Expression.Assign(paramName, Expression.Constant("Bupa"));

            NewExpression newEx = Expression.New(typeof(Company).GetConstructor(System.Type.EmptyTypes));

            var returnTarget = Expression.Label(typeof(Company));
            var returnExpression = Expression.Return(returnTarget, local, typeof(Company));
            var returnLabel = Expression.Label(returnTarget, Expression.Default(typeof(Company)));

            var bloc = Expression.Block(
                new[] { local },
                Expression.Assign(local, newEx),
                assignid,
                assignname,

                returnExpression,
                returnLabel
                );



           var res = Expression.Lambda<Func<Company>>(bloc);
          var comany =  res.Compile()();
            Assert.NotNull(comany);
        }

        [Fact]
        public void callCompWithParam()
        {
            var companyType = typeof(Company);

            var local = Expression.Parameter(companyType, "company");
            var newlocal = Expression.New(companyType);

            var idparam = Expression.Parameter(typeof(int), "Id");
            var idPro = Expression.Property(local, companyType.GetProperty("Id"));
            var idAssign = Expression.Assign(idPro, idparam);

            var nameParam = Expression.Parameter(typeof(string), "Name");
            var namePro = Expression.Property(local, companyType.GetProperty("Name"));
            var nameAssign = Expression.Assign(namePro, nameParam);

            var returntarget = Expression.Label(companyType);
            var returnexpression = Expression.Return(returntarget, local, companyType);
            var returnlabel = Expression.Label(returntarget, Expression.Default(companyType));

            var block = Expression.Block(
                new[] { local },
                Expression.Assign(local, newlocal),
                idAssign,
                nameAssign,


                returnexpression,
                returnlabel
                );



            var funcres = Expression.Lambda<Func<int, string, Company>>(block, idparam, nameParam).Compile();
          var comp =  funcres(12, "Bupa");
            Assert.NotNull(comp);
        }


        Expression<Func<int,string, Company>> BuildLambda()
        {
            var createdType = typeof(Company);
         
            var ctor = Expression.New(createdType);

            var idParam = Expression.Parameter(typeof(int), "Id");
            var idValueAssignment = Expression.Bind(createdType.GetProperty("Id"), idParam);

            var NameParam = Expression.Parameter(typeof(string), "Name");
            var NameValueAssignment = Expression.Bind(createdType.GetProperty("Name"), NameParam);

            

            var memberInitId = Expression.MemberInit(ctor, idValueAssignment);

            var memberInitName = Expression.MemberInit(ctor, NameValueAssignment);

            //LambdaExpression sss= Expression.Lambda
            var exxx = Expression.Block(
                ctor,
               
                memberInitId,
                memberInitName
                );
            
            return
                Expression.Lambda<Func<int,string, Company>>(exxx, idParam,NameParam);
        }

        [Fact]
        public void newnew()
        {

          var qqq=  BuildLambda().Compile()(12,"Bupa");
            Assert.NotNull(qqq);
        }


        [Fact]
        public void newnewnew()
        {

            var expectedType = typeof(Company);
            var ctor = Expression.New(expectedType);
            var local = Expression.Parameter(expectedType, "obj");

            var NameParam = Expression.Parameter(typeof(string), "Name");
            var NameProperty = Expression.Property(local, "Name");

            var idParam = Expression.Parameter(typeof(int), "Id");
            var idProperty = Expression.Property(local, "Id");

            var returnTarget = Expression.Label(expectedType);
            var returnExpression = Expression.Return(returnTarget, local, expectedType);
            var returnLabel = Expression.Label(returnTarget, Expression.Default(expectedType));

            var block = Expression.Block(
                new[] { local },
                Expression.Assign(local, ctor),
                Expression.Assign(NameProperty, NameParam),

                Expression.Assign(idProperty, idParam),
                returnExpression,
                returnLabel
                );
            var comp =Expression.Lambda<Func<string,int, Company>>(block, NameParam, idParam).Compile();
            //var comp = Expression.Lambda<Func<string, int, dynamic>>(block, NameParam, idParam).Compile();

            dynamic res = comp("Bupa",12);

            Assert.NotNull(res);

        }
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
