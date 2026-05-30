using QuickStock.CQRS;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;

namespace QuickStock.Applications.Itassets.Queries
{
    public class GetItAssetByQrQuery : IRequest<ItAsset?>
    {
        public string QrCode { get; set; }

        public GetItAssetByQrQuery(string qrCode)
        {
            QrCode = qrCode;
        }
    }
}
