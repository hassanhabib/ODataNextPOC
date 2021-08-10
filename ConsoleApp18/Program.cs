using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace ConsoleApp18
{
    class Program
    {
        static void Main(string[] args)
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

            Uri requestUri = new Uri("Products?$select=ID&$expand=ProductDetail&$filter=ID eq 2", UriKind.Relative);

            ODataUriParser parser = new ODataUriParser(model, serviceRoot, requestUri);
            SelectExpandClause expand = parser.ParseSelectAndExpand(); // parse $select, $expand
            FilterClause filter = parser.ParseFilter();                // parse $filter
            OrderByClause orderby = parser.ParseOrderBy();             // parse $orderby
            //SearchClause search = parser.ParseSearch();              // parse $search
            long? top = parser.ParseTop();                             // parse $top
            long? skip = parser.ParseSkip();                           // parse $skip
            bool? count = parser.ParseCount();                         // parse $count
            IQueryable<Product> products = new List<Product>().AsQueryable();
            IServiceCollection collection = new ServiceCollection();
            var serviceProvider = collection.BuildServiceProvider();
            FilterBinder filterBinder = new FilterBinder(serviceProvider);
            Expression exp = filterBinder.Bind(filter.Expression);
            var query = products.Where(ex => ex.ID == 2);
            Console.WriteLine("");


            /*
             * "?$select=Id, Name".AsIQueryable();
             * iquerable.ToODataQuery();
             * 
             */
        }

        public class Product
        {
            public int ID { get; set; }
            public ProductDetail ProductDetail { get; set; }
        }

        public class ProductDetail
        {
            public int ID { get; set; }
        }
    }
}
