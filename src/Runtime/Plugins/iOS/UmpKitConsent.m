#import <UserMessagingPlatform/UserMessagingPlatform.h>
#import "UnityInterface.h"

extern void UnitySendMessage(const char *, const char *, const char *);

static const char *const kGameObject = "UmpKit";
static const char *const kMethod = "OnUmpMessage";

static int UmpCanonicalStatus(UMPConsentStatus status)
{
    switch (status)
    {
        case UMPConsentStatusNotRequired: return 1;
        case UMPConsentStatusRequired:    return 2;
        case UMPConsentStatusObtained:    return 3;
        default:                          return 0;
    }
}

static int UmpCurrentStatus(void)
{
    return UmpCanonicalStatus(UMPConsentInformation.sharedInstance.consentStatus);
}

static void UmpSend(const char *op, int status, int error, int nativeCode, NSString *message)
{
    if (message == nil)
    {
        message = @"";
    }
    NSString *payload = [NSString stringWithFormat:@"%s|%d|%d|%d|%@", op, status, error, nativeCode, message];
    UnitySendMessage(kGameObject, kMethod, [payload UTF8String]);
}

void _UmpInitialize(int debugGeography, const char *testDevice)
{
    UMPRequestParameters *parameters = [[UMPRequestParameters alloc] init];
    parameters.tagForUnderAgeOfConsent = NO;

    NSString *device = testDevice != NULL ? [NSString stringWithUTF8String:testDevice] : @"";
    if (debugGeography > 0 && device.length > 0)
    {
        UMPDebugSettings *debug = [[UMPDebugSettings alloc] init];
        debug.geography = (UMPDebugGeography)debugGeography;
        debug.testDeviceIdentifiers = @[ device ];
        parameters.debugSettings = debug;
    }

    [UMPConsentInformation.sharedInstance
        requestConsentInfoUpdateWithParameters:parameters
        completionHandler:^(NSError *_Nullable error)
        {
            if (error)
            {
                UmpSend("init", UmpCurrentStatus(), 1, (int)error.code, error.localizedDescription);
            }
            else
            {
                UmpSend("init", UmpCurrentStatus(), 0, 0, @"");
            }
        }];
}

void _UmpShowFormIfRequired(void)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController *controller = UnityGetGLViewController();

        if (UMPConsentInformation.sharedInstance.consentStatus == UMPConsentStatusRequired
            && UMPConsentInformation.sharedInstance.formStatus != UMPFormStatusAvailable)
        {
            UmpSend("show", UmpCurrentStatus(), 2, 0, @"form not available");
            return;
        }

        [UMPConsentForm loadAndPresentIfRequiredFromViewController:controller
            completionHandler:^(NSError *_Nullable error)
            {
                if (error)
                {
                    UmpSend("show", UmpCurrentStatus(), 3, (int)error.code, error.localizedDescription);
                }
                else
                {
                    UmpSend("show", UmpCurrentStatus(), 0, 0, @"");
                }
            }];
    });
}

void _UmpShowPrivacyOptionsForm(void)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController *controller = UnityGetGLViewController();
        [UMPConsentForm presentPrivacyOptionsFormFromViewController:controller
            completionHandler:^(NSError *_Nullable error)
            {
                if (error)
                {
                    UmpSend("privacy", UmpCurrentStatus(), 3, (int)error.code, error.localizedDescription);
                }
                else
                {
                    UmpSend("privacy", UmpCurrentStatus(), 0, 0, @"");
                }
            }];
    });
}

void _UmpResetConsent(void)
{
    [UMPConsentInformation.sharedInstance reset];
}

bool _UmpIsPrivacyOptionsRequired(void)
{
    return UMPConsentInformation.sharedInstance.privacyOptionsRequirementStatus
        == UMPPrivacyOptionsRequirementStatusRequired;
}
