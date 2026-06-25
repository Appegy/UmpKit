# Appegy: UMP Kit

Thin, standalone Unity wrapper around the Google User Messaging Platform (UMP)
consent SDK. It lets you drive the GDPR / CMP consent form on your own schedule
instead of bolting it to an ad SDK's initialization.

> Status: work in progress. This is an initial scaffold - the API is being
> designed, no implementation yet.

## Why

Mediation SDKs (for example AppLovin MAX) can show the Google UMP consent form as
part of their own init, which couples consent display to their lifecycle. UMP Kit
wraps the same native UMP SDK directly so the app decides when consent is shown,
keeps the surface small (coarse status, no per-purpose TCF parsing), and reports
results as values rather than exceptions.

## Planned public surface

```csharp
public interface IUmpConsentService
{
    UniTask<UmpResult> InitializeAsync(UmpInitializeParameters parameters, CancellationToken ct);
    UniTask<UmpResult> ShowFormIfRequiredAsync(CancellationToken ct);
    UniTask<UmpResult> ShowPrivacyOptionsFormAsync(CancellationToken ct);
    UniTask ResetConsentAsync(CancellationToken ct);
    bool IsPrivacyOptionsRequired { get; }
}
```

## Layout

- `src/` - the UPM package (`com.appegy.ump-kit`)
- `Appegy.UmpKit.Lab/` - Unity project used to develop and test the package

## License

MIT. See [LICENSE](LICENSE).
