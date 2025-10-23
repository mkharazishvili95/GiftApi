using MediatR;

namespace GiftApi.Application.Features.File.Queries.Get
{
    public class GetFileQuery : IRequest<GetFileResponse>
    {
        public int FileId { get; set; }
    }
}
