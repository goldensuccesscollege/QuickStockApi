using QuickStock.CQRS;
using QuickStock.Domain.Apparel;

namespace QuickStock.Applications.Apparel.Queries
{
    public class GetApparelItemByQrQuery : IRequest<ApparelItem?>
    {
        public string Qr { get; set; }

        public GetApparelItemByQrQuery(string qr)
        {
            Qr = qr;
        }
    }
}
