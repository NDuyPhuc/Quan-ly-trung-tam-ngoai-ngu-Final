using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

public interface IContactMessageService
{
    Task<ContactDispatchResult> SendContactAsync(ContactMessageRequest request, CancellationToken cancellationToken = default);
    Task<ContactDispatchResult> SendNewsletterAsync(NewsletterSubscriptionRequest request, CancellationToken cancellationToken = default);
}
