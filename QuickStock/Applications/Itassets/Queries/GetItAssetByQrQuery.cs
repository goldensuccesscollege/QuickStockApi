using MediatR;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;

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
