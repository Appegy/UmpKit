using System.Threading;
using Cysharp.Threading.Tasks;

namespace Appegy.Ump
{
    public interface IUmpConsentService
    {
        bool IsPrivacyOptionsRequired { get; }

        UniTask<UmpResult> InitializeAsync(UmpInitializeParameters parameters, CancellationToken ct);

        UniTask<UmpResult> ShowFormIfRequiredAsync(CancellationToken ct);

        UniTask<UmpResult> ShowPrivacyOptionsFormAsync(CancellationToken ct);

        UniTask ResetConsentAsync(CancellationToken ct);
    }
}
