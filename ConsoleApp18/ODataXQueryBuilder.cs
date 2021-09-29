using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp18.Program;

namespace ConsoleApp18
{
    public class ODataXQueryBuilder
    {
        public string AsODataQuery(IQueryable queryable)
        {
            string queryExpression = ExpressionVisitor(queryable.Expression);

            return queryExpression;
        }

        public string ExpressionVisitor(Expression expression)
        {
            Expression<Func<Product, bool>> exp = p => p.Name == "Hassan";

            if (exp.NodeType == ExpressionType.Lambda)
            {
                LambdaExpression lambdaExpression =
                    (LambdaExpression)expression;
            }

            return default;
        }
    }
}
