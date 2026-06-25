package com.appegy.ump;

import com.google.android.ump.ConsentDebugSettings;
import com.google.android.ump.ConsentInformation;
import com.google.android.ump.ConsentRequestParameters;
import com.google.android.ump.FormError;
import com.google.android.ump.UserMessagingPlatform;
import com.unity3d.player.UnityPlayer;

import android.app.Activity;

public final class UmpKitConsent
{
    private static final String GAME_OBJECT = "UmpKit";
    private static final String METHOD = "OnUmpMessage";

    private static ConsentInformation consentInformation;
    private static boolean formAvailable = false;

    private UmpKitConsent() {}

    private static Activity activity()
    {
        return UnityPlayer.currentActivity;
    }

    private static int status()
    {
        try
        {
            return consentInformation == null ? 0 : consentInformation.getConsentStatus();
        }
        catch (Exception e)
        {
            return 0;
        }
    }

    private static void send(String op, int status, int error, int nativeCode, String message)
    {
        if (message == null)
        {
            message = "";
        }
        String payload = op + "|" + status + "|" + error + "|" + nativeCode + "|" + message;
        UnityPlayer.UnitySendMessage(GAME_OBJECT, METHOD, payload);
    }

    public static void Initialize(final int debugGeography, final String testDevice)
    {
        activity().runOnUiThread(() -> doInitialize(debugGeography, testDevice));
    }

    private static void doInitialize(int debugGeography, String testDevice)
    {
        ConsentRequestParameters.Builder builder = new ConsentRequestParameters.Builder()
            .setTagForUnderAgeOfConsent(false);

        if (debugGeography > 0 && testDevice != null && testDevice.length() > 0)
        {
            ConsentDebugSettings debug = new ConsentDebugSettings.Builder(activity())
                .setDebugGeography(debugGeography)
                .addTestDeviceHashedId(testDevice)
                .build();
            builder.setConsentDebugSettings(debug);
        }

        ConsentRequestParameters params = builder.build();
        consentInformation = UserMessagingPlatform.getConsentInformation(activity());
        consentInformation.requestConsentInfoUpdate(
            activity(),
            params,
            () ->
            {
                formAvailable = consentInformation.isConsentFormAvailable();
                send("init", status(), 0, 0, "");
            },
            formError ->
            {
                send("init", status(), 1, formError.getErrorCode(), formError.getMessage());
            });
    }

    public static void ShowFormIfRequired()
    {
        activity().runOnUiThread(UmpKitConsent::doShowFormIfRequired);
    }

    private static void doShowFormIfRequired()
    {
        if (consentInformation == null)
        {
            send("show", 0, 4, 0, "not initialized");
            return;
        }

        if (consentInformation.getConsentStatus() == ConsentInformation.ConsentStatus.REQUIRED && !formAvailable)
        {
            send("show", status(), 2, 0, "form not available");
            return;
        }

        UserMessagingPlatform.loadAndShowConsentFormIfRequired(
            activity(),
            formError ->
            {
                if (formError != null)
                {
                    send("show", status(), 3, formError.getErrorCode(), formError.getMessage());
                }
                else
                {
                    send("show", status(), 0, 0, "");
                }
            });
    }

    public static void ShowPrivacyOptionsForm()
    {
        activity().runOnUiThread(UmpKitConsent::doShowPrivacyOptions);
    }

    private static void doShowPrivacyOptions()
    {
        if (consentInformation == null)
        {
            send("privacy", 0, 4, 0, "not initialized");
            return;
        }

        UserMessagingPlatform.showPrivacyOptionsForm(
            activity(),
            formError ->
            {
                if (formError != null)
                {
                    send("privacy", status(), 3, formError.getErrorCode(), formError.getMessage());
                }
                else
                {
                    send("privacy", status(), 0, 0, "");
                }
            });
    }

    public static void ResetConsent()
    {
        if (consentInformation != null)
        {
            consentInformation.reset();
        }
        formAvailable = false;
    }

    public static boolean IsPrivacyOptionsRequired()
    {
        if (consentInformation == null)
        {
            return false;
        }
        return consentInformation.getPrivacyOptionsRequirementStatus()
            == ConsentInformation.PrivacyOptionsRequirementStatus.REQUIRED;
    }
}
