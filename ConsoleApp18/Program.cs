using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace ConsoleApp18
{
    class Program
    {
        public static string ExpressionVisitor(Expression expression)
        {
            if(expression.NodeType == ExpressionType.Call)
            {
                MethodCallExpression constantExp =
                    (MethodCallExpression)expression;
            }

            if (expression.NodeType == ExpressionType.Lambda)
            {
                LambdaExpression lambdaExpression =
                    (LambdaExpression)expression;

                BinaryExpression binaryExpression =
                    (BinaryExpression)lambdaExpression.Body;

                MemberExpression leftMemberExpression =
                    (MemberExpression) binaryExpression.Left;

                PropertyInfo leftMemberPropertyInfo =
                    (PropertyInfo)leftMemberExpression.Member;

                // first
                string leftMemberName =
                    leftMemberPropertyInfo.Name;

                string method =
                    binaryExpression.NodeType == ExpressionType.Equal 
                    ? " eq "
                    : "unknown";

                ConstantExpression rightMemberExpression =
                    (ConstantExpression)binaryExpression.Right;

                string rightMember =
                    $"'{rightMemberExpression.Value.ToString()}'";

                return $"{leftMemberName}{method}{rightMember}";
            }

            return default;
        }

        static void Main(string[] args)
        {
            IQueryable<Product> queryableProducts =
                new List<Product>().AsQueryable();

            IQueryable<Product> productsNamedSam =
                queryableProducts.Where(p => p.Name == "Sam").AsQueryable();

            string odataQuery = ExpressionVisitor(productsNamedSam.Expression);

            Console.WriteLine(odataQuery);
        }

        public void ODataToExpression()
        {
            Uri serviceRoot = new Uri("https://localhost");
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntitySet<ProductDetail>("ProductDetails");
            IEdmModel model = builder.GetEdmModel();

            //EdmEntityType productEdmEntityType = new EdmEntityType("AnyNamespace", "Product");
            //EdmEntityType productDetailsEntityType = new EdmEntityType("AnyNamespace", "ProductDetail");
            //var propertyInfo = new EdmNavigationPropertyInfo();
            //propertyInfo.Name = "ProductDetail";
            //propertyInfo.TargetMultiplicity = EdmMultiplicity.ZeroOrOne;
            //propertyInfo.Target = productDetailsEntityType;

            //var productId = productEdmEntityType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            //var productDetail = productEdmEntityType.AddUnidirectionalNavigation(propertyInfo);
            //productEdmEntityType.AddKeys(productId);
            //productDetailsEntityType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            //model.AddElement(productEdmEntityType);

            //EdmEntityContainer container = new EdmEntityContainer("AnyNamespace", "Default");
            //model.AddElement(container);
            //var productEntitySet = container.AddEntitySet("Products", productEdmEntityType);
            //var productDetailEntitySet = container.AddEntitySet("ProductDetails", productDetailsEntityType);
            //productEntitySet.AddNavigationTarget(productDetail, productDetailEntitySet);

            // QUERY -> EXP

            Uri requestUri = new Uri("Products?$select=ID&$expand=ProductDetail&$filter=Name eq 'Hassan'", UriKind.Relative);

            ODataUriParser parser = new ODataUriParser(model, serviceRoot, requestUri);
            ODataPath odataPath = parser.ParsePath();
            SelectExpandClause expand = parser.ParseSelectAndExpand(); // parse $select, $expand
            FilterClause filter = parser.ParseFilter();                // parse $filter
            OrderByClause orderby = parser.ParseOrderBy();             // parse $orderby
            //SearchClause search = parser.ParseSearch();              // parse $search
            long? top = parser.ParseTop();                             // parse $top
            long? skip = parser.ParseSkip();                           // parse $skip
            bool? count = parser.ParseCount();                         // parse $count
            IQueryable<Product> products = new List<Product>().AsQueryable();
            IServiceCollection collection = new ServiceCollection();
            collection.AddScoped<ODataQuerySettings>();
            collection.AddSingleton(sp => model);
            var serviceProvider = collection.BuildServiceProvider();
            FilterBinder filterBinder = new FilterBinder(serviceProvider);

            MethodInfo bindMethodInfo = typeof(FilterBinder)
                .GetMethod("Bind", BindingFlags.NonPublic | BindingFlags.Static,
                new Type[] {
                    typeof(IQueryable),
                    typeof(FilterClause),
                    typeof(Type),
                    typeof(ODataQueryContext),
                    typeof(ODataQuerySettings)
                });

            var odataQueryContext = new ODataQueryContext(model, typeof(Product), odataPath);

            var expression = bindMethodInfo.Invoke(null,
                new object[] { products, filter, typeof(Product), odataQueryContext, new ODataQuerySettings() });

            Expression exp = (Expression)expression;

            var productList = new List<Product>()
            {
                new Product
                {
                    Name = "Hassan"
                },
                new Product
                {
                    Name = "Hassan"
                },
                new Product
                {
                    Name = "Sam"
                }
            };

            var results = Where(productList.AsQueryable(), exp, typeof(Product));

            var samProduct = results.Cast<Product>().FirstOrDefault();

            Console.WriteLine(results.Cast<Product>().Count());


            /// EXP -> QUERY
            IQueryable<Product> hassanProducts =
                productList.Where(p => p.Name == "Hassan").AsQueryable();

            var odataQueryBuilder = new ODataXQueryBuilder();

            string query = odataQueryBuilder.AsODataQuery(hassanProducts);

            // $filter=Name eq 'Hassan'






            // INPUT: OData Query
            // Converting to Expression/LINQ
            // OUTPUT: OData Query

            /*
             * "?$select=Id, Name".AsIQueryable();
             * iquerable.ToODataQuery();
             * 
             */
        }

        public static IQueryable Where(IQueryable query, Expression where, Type type)
        {
            MethodInfo whereMethod = GenericMethodOf(_ => Queryable.Where<int>(
                default(IQueryable<int>), default(Expression<Func<int, bool>>)));

            whereMethod = whereMethod.MakeGenericMethod(type);
        //MethodInfo whereMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(type);
            return whereMethod.Invoke(null, new object[] { query, where }) as IQueryable;
        }

        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return GenericMethodOf(expression as Expression);
        }



        private static MethodInfo GenericMethodOf(Expression expression)
        {
            LambdaExpression lambdaExpression = expression as LambdaExpression;

            return (lambdaExpression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        public class Product
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public ProductDetail ProductDetail { get; set; }
        }

        public class ProductDetail
        {
            public int ID { get; set; }
        }
    }
}
