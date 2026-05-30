using QuickStock.CQRS;
using System.Collections.Generic;

namespace QuickStock.Applications.Users.Queries
{
    public class GetUsersQuery : IRequest<IEnumerable<object>>
    {
    }
}
